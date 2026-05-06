using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.Contracts.Services.Invoices;
using Core_API.Application.Contracts.Services.RecurringInvoices;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Application.DTOs.RecurringInvoice.Response;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.RealTime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.Services.Invoicing.RecurringInvoice
{
    /// <summary>
    /// Service implementation for recurring invoice management
    /// REUSES IInvoiceService for actual invoice creation
    /// </summary>
    public class RecurringInvoiceService : IRecurringInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceService _invoiceService; // REUSED!
        private readonly IEmailServiceProvider _emailServiceProvider;
        private readonly ILogger<RecurringInvoiceService> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly string[] _supportedCurrencies = new[] { "USD", "EUR", "INR", "GBP", "CAD", "AUD" };

        public RecurringInvoiceService(
            IUnitOfWork unitOfWork,
            IInvoiceService invoiceService, // INJECTED and REUSED
            IEmailServiceProvider emailServiceProvider,
            ILogger<RecurringInvoiceService> logger,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _emailServiceProvider = emailServiceProvider ?? throw new ArgumentNullException(nameof(emailServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        #region CRUD Operations

        public async Task<OperationResult<RecurringInvoiceResponseDto>> CreateAsync(CreateRecurringInvoiceDto dto, OperationContext context)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate CompanyId
                if (!context.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for creating recurring invoice.");
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                // Validate name uniqueness
                var existing = await _unitOfWork.RecurringInvoices
                    .Query()
                    .AnyAsync(r => r.CompanyId == companyId && r.Name == dto.Name && !r.IsDeleted);
                if (existing)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("A recurring invoice with this name already exists.");
                }

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Validate currency
                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                // Validate source invoice if provided
                Domain.Entities.Invoices.Invoice? sourceInvoice = null;
                if (dto.SourceInvoiceId.HasValue)
                {
                    sourceInvoice = await _unitOfWork.Invoices.GetAsync(
                        i => i.Id == dto.SourceInvoiceId.Value && i.CompanyId == companyId && !i.IsDeleted,
                        includeProperties: "InvoiceItems,TaxDetails,Discounts");

                    if (sourceInvoice == null)
                    {
                        return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Source invoice not found or does not belong to your company.");
                    }
                }

                // Validate schedule
                if (!ValidateSchedule(dto, out var scheduleError))
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult(scheduleError);
                }

                // Map DTO to entity
                var recurringInvoice = _mapper.Map<Domain.Entities.RecurringInvoices.RecurringInvoice>(dto);

                // Set system fields
                recurringInvoice.CompanyId = companyId;
                recurringInvoice.CreatedBy = context.UserId;
                recurringInvoice.CreatedDate = DateTime.UtcNow;
                recurringInvoice.Status = RecurringInvoiceStatus.Draft;
                recurringInvoice.OccurrencesGenerated = 0;

                // Calculate financial totals from source invoice or set defaults
                if (sourceInvoice != null)
                {
                    recurringInvoice.Subtotal = sourceInvoice.Subtotal;
                    recurringInvoice.TaxTotal = sourceInvoice.TaxTotal;
                    recurringInvoice.DiscountTotal = sourceInvoice.DiscountTotal;
                    recurringInvoice.ShippingAmount = sourceInvoice.ShippingAmount;
                    recurringInvoice.TotalAmount = sourceInvoice.TotalAmount;
                    recurringInvoice.PaymentTerms = sourceInvoice.PaymentTerms;
                }
                else
                {
                    // If no source invoice, these will be 0 and need to be set via line items
                    // This would require additional DTO properties for line items
                }

                // Calculate next invoice date
                recurringInvoice.NextInvoiceDate = CalculateNextDate(recurringInvoice, recurringInvoice.StartDate);

                // Save recurring invoice
                await _unitOfWork.RecurringInvoices.AddAsync(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = "Created",
                    Description = $"Recurring invoice template created by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { dto }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = await MapToResponseDto(recurringInvoice);

                // Send notification
                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceCreated",
                    new { recurringInvoice.Id, recurringInvoice.Name });

                _logger.LogInformation("Recurring invoice template {RecurringInvoiceId} created successfully for company {CompanyId}",
                    recurringInvoice.Id, companyId);

                return OperationResult<RecurringInvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating recurring invoice for company {CompanyId}", context.CompanyId);
                return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Failed to create recurring invoice: " + ex.Message);
            }
        }

        public async Task<OperationResult<RecurringInvoiceResponseDto>> UpdateAsync(UpdateRecurringInvoiceDto dto, OperationContext context)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                // Get existing recurring invoice
                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetWithDetailsAsync(dto.Id, companyId);
                if (recurringInvoice == null)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Recurring invoice not found or does not belong to your company.");
                }

                // Check if can modify based on status
                if (recurringInvoice.Status == RecurringInvoiceStatus.Completed ||
                    recurringInvoice.Status == RecurringInvoiceStatus.Cancelled ||
                    recurringInvoice.Status == RecurringInvoiceStatus.Expired)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult($"Cannot update a {recurringInvoice.Status} recurring invoice.");
                }

                // Validate name uniqueness
                var nameExists = await _unitOfWork.RecurringInvoices
                    .Query()
                    .AnyAsync(r => r.CompanyId == companyId && r.Name == dto.Name && r.Id != dto.Id && !r.IsDeleted);
                if (nameExists)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("A recurring invoice with this name already exists.");
                }

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Validate schedule
                if (!ValidateSchedule(dto, out var scheduleError))
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult(scheduleError);
                }

                // Store old values for audit
                var oldValues = JsonSerializer.Serialize(new
                {
                    recurringInvoice.Name,
                    recurringInvoice.Frequency,
                    recurringInvoice.FrequencyInterval,
                    recurringInvoice.Status,
                    recurringInvoice.NextInvoiceDate
                });

                // Map DTO to entity
                _mapper.Map(dto, recurringInvoice);
                recurringInvoice.UpdatedBy = context.UserId;
                recurringInvoice.UpdatedDate = DateTime.UtcNow;

                // Recalculate next invoice date if schedule changed
                if (HasScheduleChanged(dto, recurringInvoice))
                {
                    DateTime baseDate = recurringInvoice.LastInvoiceDate ?? DateTime.UtcNow;
                    recurringInvoice.NextInvoiceDate = CalculateNextDate(recurringInvoice, baseDate);
                }

                _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = "Updated",
                    Description = $"Recurring invoice template updated by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { Old = oldValues, New = dto }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = await MapToResponseDto(recurringInvoice);

                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceUpdated",
                    new { recurringInvoice.Id, recurringInvoice.Name });

                _logger.LogInformation("Recurring invoice template {RecurringInvoiceId} updated successfully", recurringInvoice.Id);

                return OperationResult<RecurringInvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating recurring invoice {RecurringInvoiceId} for company {CompanyId}", dto.Id, context.CompanyId);
                return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Failed to update recurring invoice: " + ex.Message);
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id, OperationContext context)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetAsync(
                    r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted,
                    includeProperties: "GeneratedInvoices");

                if (recurringInvoice == null)
                {
                    return OperationResult<bool>.FailureResult("Recurring invoice not found or does not belong to your company.");
                }

                // Check if any invoices have been generated
                bool hasGeneratedInvoices = recurringInvoice.GeneratedInvoices?.Any(g => !g.IsDeleted) ?? false;

                if (hasGeneratedInvoices)
                {
                    // Soft delete if invoices exist
                    recurringInvoice.IsDeleted = true;
                    recurringInvoice.UpdatedBy = context.UserId;
                    recurringInvoice.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                }
                else
                {
                    // Hard delete if no invoices generated
                    _unitOfWork.RecurringInvoices.Delete(recurringInvoice);
                }

                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = "Deleted",
                    Description = $"Recurring invoice template deleted by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { DeletedDate = DateTime.UtcNow }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceDeleted",
                    new { recurringInvoice.Id });

                _logger.LogInformation("Recurring invoice template {RecurringInvoiceId} deleted successfully", id);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to delete recurring invoice.");
            }
        }

        public async Task<OperationResult<RecurringInvoiceResponseDto>> GetByIdAsync(int id, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetWithDetailsAsync(id, companyId);
                if (recurringInvoice == null)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Recurring invoice not found or does not belong to your company.");
                }

                // Check customer access
                if (context.CustomerId.HasValue && recurringInvoice.CustomerId != context.CustomerId.Value)
                {
                    return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Access denied to this recurring invoice.");
                }

                var response = await MapToResponseDto(recurringInvoice);
                return OperationResult<RecurringInvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return OperationResult<RecurringInvoiceResponseDto>.FailureResult("Failed to retrieve recurring invoice.");
            }
        }

        public async Task<OperationResult<PaginatedResult<RecurringInvoiceResponseDto>>> GetPagedAsync(
            OperationContext context,
            RecurringInvoiceFilterDto filter)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<PaginatedResult<RecurringInvoiceResponseDto>>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var result = await _unitOfWork.RecurringInvoices.GetPagedAsync(companyId, filter);

                var mappedItems = new List<RecurringInvoiceResponseDto>();
                foreach (var invoice in result.Items)
                {
                    var dto = await MapToResponseDto(invoice);
                    mappedItems.Add(dto);
                }

                var response = new PaginatedResult<RecurringInvoiceResponseDto>
                {
                    Items = mappedItems,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                return OperationResult<PaginatedResult<RecurringInvoiceResponseDto>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged recurring invoices for company {CompanyId}", context.CompanyId);
                return OperationResult<PaginatedResult<RecurringInvoiceResponseDto>>.FailureResult("Failed to retrieve recurring invoices.");
            }
        }

        #endregion

        #region Status Management

        public async Task<OperationResult<bool>> ActivateAsync(int id, OperationContext context)
        {
            return await UpdateStatusAsync(id, RecurringInvoiceStatus.Active, context, "activate");
        }

        public async Task<OperationResult<bool>> PauseAsync(int id, OperationContext context)
        {
            return await UpdateStatusAsync(id, RecurringInvoiceStatus.Paused, context, "pause");
        }

        public async Task<OperationResult<bool>> ResumeAsync(int id, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetAsync(
                    r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted);

                if (recurringInvoice == null)
                {
                    return OperationResult<bool>.FailureResult("Recurring invoice not found.");
                }

                if (recurringInvoice.Status != RecurringInvoiceStatus.Paused)
                {
                    return OperationResult<bool>.FailureResult("Can only resume paused recurring invoices.");
                }

                // Store old values for audit
                var oldStatus = recurringInvoice.Status;

                recurringInvoice.Status = RecurringInvoiceStatus.Active;
                recurringInvoice.PausedDate = null;
                recurringInvoice.UpdatedBy = context.UserId;
                recurringInvoice.UpdatedDate = DateTime.UtcNow;

                // Recalculate next invoice date if needed
                if (recurringInvoice.NextInvoiceDate < DateTime.UtcNow)
                {
                    recurringInvoice.NextInvoiceDate = CalculateNextDate(recurringInvoice, DateTime.UtcNow);
                }

                _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = "Resumed",
                    Description = $"Recurring invoice resumed by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { OldStatus = oldStatus.ToString(), NewStatus = "Active" }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceResumed",
                    new { Id = id, recurringInvoice.NextInvoiceDate });

                _logger.LogInformation("Recurring invoice {RecurringInvoiceId} resumed for company {CompanyId}", id, companyId);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to resume recurring invoice.");
            }
        }

        public async Task<OperationResult<bool>> CancelAsync(int id, OperationContext context)
        {
            return await UpdateStatusAsync(id, RecurringInvoiceStatus.Cancelled, context, "cancel");
        }

        #endregion

        #region Generation Operations

        public async Task<OperationResult<InvoiceResponseDto>> GenerateInvoiceManuallyAsync(GenerateManualDto dto, OperationContext context)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetWithDetailsAsync(dto.RecurringInvoiceId, companyId);
                if (recurringInvoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Recurring invoice not found.");
                }

                // Check if generation is allowed
                if (recurringInvoice.Status != RecurringInvoiceStatus.Active &&
                    recurringInvoice.Status != RecurringInvoiceStatus.Draft)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Cannot generate invoice from {recurringInvoice.Status} recurring invoice.");
                }

                // Check max occurrences
                if (recurringInvoice.MaxOccurrences.HasValue &&
                    recurringInvoice.OccurrencesGenerated >= recurringInvoice.MaxOccurrences.Value)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Maximum occurrences already reached for this recurring invoice.");
                }

                // Determine invoice date
                var invoiceDate = dto.InvoiceDate ?? DateTime.UtcNow;

                // Create invoice DTO from template
                var invoiceCreateDto = await CreateInvoiceFromTemplate(recurringInvoice, invoiceDate, dto.DueDate, context);

                // REUSE InvoiceService to create the invoice!
                var invoiceResult = await _invoiceService.CreateAsync(invoiceCreateDto, context);
                if (!invoiceResult.IsSuccess)
                {
                    return invoiceResult;
                }

                // Get sequence number
                var sequenceNumber = await _unitOfWork.RecurringInvoices.GetNextSequenceNumberAsync(recurringInvoice.Id);

                // Create instance record
                var instance = new RecurringInvoiceInstance
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    InvoiceId = invoiceResult.Data.Id,
                    GeneratedDate = DateTime.UtcNow,
                    ScheduledGenerationDate = dto.InvoiceDate ?? DateTime.UtcNow,
                    SequenceNumber = sequenceNumber,
                    GeneratedInvoiceNumber = invoiceResult.Data.InvoiceNumber,
                    Amount = invoiceResult.Data.TotalAmount,
                    Notes = dto.GenerationNotes,
                    GenerationStatus = GenerationStatus.Success,
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.RecurringInvoiceInstances.AddAsync(instance);

                // Update recurring invoice counters
                recurringInvoice.OccurrencesGenerated++;
                recurringInvoice.LastInvoiceDate = DateTime.UtcNow;

                if (dto.OverrideNextDate)
                {
                    recurringInvoice.NextInvoiceDate = CalculateNextDate(recurringInvoice, DateTime.UtcNow);
                }

                // Check if max occurrences reached
                if (recurringInvoice.MaxOccurrences.HasValue &&
                    recurringInvoice.OccurrencesGenerated >= recurringInvoice.MaxOccurrences.Value)
                {
                    recurringInvoice.Status = RecurringInvoiceStatus.Completed;

                    // Create completion audit log
                    var completionLog = new RecurringInvoiceAuditLog
                    {
                        RecurringInvoiceId = recurringInvoice.Id,
                        Action = "Completed",
                        Description = "Recurring invoice completed - max occurrences reached",
                        Changes = JsonSerializer.Serialize(new { recurringInvoice.OccurrencesGenerated }),
                        IpAddress = "system",
                        UserAgent = "system",
                        CreatedBy = context.UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(completionLog);
                }

                _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Auto-send if configured
                if (recurringInvoice.AutoSend && dto.SendImmediately)
                {
                    // You would trigger email sending here
                    _logger.LogInformation("Auto-send enabled for generated invoice {InvoiceId}", invoiceResult.Data.Id);
                }

                // Send notification
                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceGenerated",
                    new
                    {
                        RecurringInvoiceId = recurringInvoice.Id,
                        InvoiceId = invoiceResult.Data.Id,
                        invoiceResult.Data.InvoiceNumber,
                        SequenceNumber = sequenceNumber
                    });

                _logger.LogInformation("Manually generated invoice {InvoiceNumber} from recurring template {RecurringInvoiceId}",
                    invoiceResult.Data.InvoiceNumber, recurringInvoice.Id);

                return invoiceResult;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error manually generating invoice from recurring template {RecurringInvoiceId}", dto.RecurringInvoiceId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to generate invoice: " + ex.Message);
            }
        }

        public async Task<OperationResult<bool>> UpdateNextInvoiceDateAsync(int id, DateTime nextDate, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetAsync(
                    r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted);

                if (recurringInvoice == null)
                {
                    return OperationResult<bool>.FailureResult("Recurring invoice not found.");
                }

                if (nextDate < DateTime.UtcNow.Date)
                {
                    return OperationResult<bool>.FailureResult("Next invoice date cannot be in the past.");
                }

                var oldDate = recurringInvoice.NextInvoiceDate;

                recurringInvoice.NextInvoiceDate = nextDate;
                recurringInvoice.UpdatedBy = context.UserId;
                recurringInvoice.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = "DateUpdated",
                    Description = $"Next invoice date updated by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { OldDate = oldDate, NewDate = nextDate }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    "RecurringInvoiceDateUpdated",
                    new { Id = id, NextInvoiceDate = nextDate });

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating next invoice date for recurring invoice {RecurringInvoiceId}", id);
                return OperationResult<bool>.FailureResult("Failed to update next invoice date.");
            }
        }

        public async Task<List<Domain.Entities.RecurringInvoices.RecurringInvoice>> GetDueRecurringInvoicesAsync(DateTime asOfDate)
        {
            try
            {
                return await _unitOfWork.RecurringInvoices.GetDueInvoicesAsync(asOfDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due recurring invoices as of {AsOfDate}", asOfDate);
                return new List<Domain.Entities.RecurringInvoices.RecurringInvoice>();
            }
        }

        public async Task<OperationResult<int>> ProcessDueInvoicesAsync(CancellationToken cancellationToken = default)
        {
            var processedCount = 0;
            var errors = new List<string>();

            try
            {
                var today = DateTime.UtcNow.Date;
                var dueInvoices = await GetDueRecurringInvoicesAsync(today);

                _logger.LogInformation("Found {Count} due recurring invoices to process", dueInvoices.Count);

                foreach (var recurring in dueInvoices)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Create system context for background processing
                        var systemContext = new OperationContext(
                            userId: "system",
                            companyId: recurring.CompanyId);

                        var generateDto = new GenerateManualDto
                        {
                            RecurringInvoiceId = recurring.Id,
                            InvoiceDate = DateTime.UtcNow,
                            OverrideNextDate = true,
                            SendImmediately = recurring.AutoSend,
                            GenerationNotes = "Auto-generated by system"
                        };

                        var result = await GenerateInvoiceManuallyAsync(generateDto, systemContext);

                        if (result.IsSuccess)
                        {
                            processedCount++;
                            _logger.LogInformation("Successfully generated invoice from recurring template {RecurringInvoiceId}",
                                recurring.Id);
                        }
                        else
                        {
                            errors.Add($"Failed to generate from recurring {recurring.Id}: {result.ErrorMessage}");
                            _logger.LogError("Failed to generate from recurring {RecurringInvoiceId}: {ErrorMessage}",
                                recurring.Id, result.ErrorMessage);

                            // Mark as failed for retry logic
                            await MarkGenerationFailed(recurring.Id, result.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Exception for recurring {recurring.Id}: {ex.Message}");
                        _logger.LogError(ex, "Exception processing recurring invoice {RecurringInvoiceId}", recurring.Id);
                        await MarkGenerationFailed(recurring.Id, ex.Message);
                    }
                }

                return OperationResult<int>.SuccessResult(processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessDueInvoicesAsync");
                return OperationResult<int>.FailureResult($"Failed to process due invoices: {ex.Message}");
            }
        }

        #endregion

        #region Customer-Specific Operations

        public async Task<OperationResult<List<RecurringInvoiceSummaryDto>>> GetCustomerRecurringInvoicesAsync(
            int customerId,
            OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<List<RecurringInvoiceSummaryDto>>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var invoices = await _unitOfWork.RecurringInvoices.GetByCustomerAsync(customerId, companyId);

                var summaries = invoices.Select(i => new RecurringInvoiceSummaryDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    CustomerName = i.Customer?.Name ?? "Unknown",
                    Frequency = i.Frequency.ToString(),
                    TotalAmount = i.TotalAmount,
                    NextInvoiceDate = i.NextInvoiceDate,
                    Status = i.Status.ToString(),
                    OccurrencesGenerated = i.OccurrencesGenerated,
                    MaxOccurrences = i.MaxOccurrences,
                    TotalGeneratedValue = i.GeneratedInvoices?
                        .Where(g => !g.IsDeleted)
                        .Sum(g => g.Invoice?.TotalAmount ?? 0) ?? 0,
                    AutoSend = i.AutoSend,
                    Currency = i.Currency
                }).ToList();

                return OperationResult<List<RecurringInvoiceSummaryDto>>.SuccessResult(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer recurring invoices for customer {CustomerId}", customerId);
                return OperationResult<List<RecurringInvoiceSummaryDto>>.FailureResult("Failed to retrieve customer recurring invoices.");
            }
        }

        #endregion

        #region Statistics and Dashboard

        public async Task<OperationResult<Dictionary<string, int>>> GetStatusCountsAsync(OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<Dictionary<string, int>>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var counts = await _unitOfWork.RecurringInvoices.GetStatusCountsAsync(companyId);
                return OperationResult<Dictionary<string, int>>.SuccessResult(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status counts for company {CompanyId}", context.CompanyId);
                return OperationResult<Dictionary<string, int>>.FailureResult("Failed to retrieve status counts.");
            }
        }

        public async Task<OperationResult<RecurringInvoiceStatsDto>> GetStatsAsync(OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<RecurringInvoiceStatsDto>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var today = DateTime.UtcNow.Date;
                var activeInvoices = await _unitOfWork.RecurringInvoices
                    .Query()
                    .Where(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Active)
                    .Include(r => r.Customer)
                    .ToListAsync();

                var stats = new RecurringInvoiceStatsDto
                {
                    TotalActive = activeInvoices.Count,
                    TotalDraft = await _unitOfWork.RecurringInvoices
                        .Query()
                        .CountAsync(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Draft),
                    TotalPaused = await _unitOfWork.RecurringInvoices
                        .Query()
                        .CountAsync(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Paused),
                    TotalCompleted = await _unitOfWork.RecurringInvoices
                        .Query()
                        .CountAsync(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Completed),
                    TotalCancelled = await _unitOfWork.RecurringInvoices
                        .Query()
                        .CountAsync(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Cancelled),
                    TotalExpired = await _unitOfWork.RecurringInvoices
                        .Query()
                        .CountAsync(r => r.CompanyId == companyId && !r.IsDeleted && r.Status == RecurringInvoiceStatus.Expired),

                    // Calculate recurring values
                    TotalMonthlyValue = activeInvoices
                        .Where(i => i.Frequency == RecurringFrequency.Monthly || i.Frequency == RecurringFrequency.BiMonthly)
                        .Sum(i => i.TotalAmount / (i.Frequency == RecurringFrequency.BiMonthly ? 2 : 1)),

                    TotalQuarterlyValue = activeInvoices
                        .Where(i => i.Frequency == RecurringFrequency.Quarterly)
                        .Sum(i => i.TotalAmount),

                    TotalAnnualValue = activeInvoices
                        .Where(i => i.Frequency == RecurringFrequency.Annually)
                        .Sum(i => i.TotalAmount) +
                        activeInvoices
                            .Where(i => i.Frequency == RecurringFrequency.Monthly)
                            .Sum(i => i.TotalAmount * 12) +
                        activeInvoices
                            .Where(i => i.Frequency == RecurringFrequency.Quarterly)
                            .Sum(i => i.TotalAmount * 4) +
                        activeInvoices
                            .Where(i => i.Frequency == RecurringFrequency.SemiAnnually)
                            .Sum(i => i.TotalAmount * 2),

                    // Due counts
                    DueThisWeek = activeInvoices
                        .Count(i => i.NextInvoiceDate.Date <= today.AddDays(7)),
                    DueThisMonth = activeInvoices
                        .Count(i => i.NextInvoiceDate.Date <= today.AddDays(30)),
                    DueNextMonth = activeInvoices
                        .Count(i => i.NextInvoiceDate.Date > today.AddDays(30) &&
                                    i.NextInvoiceDate.Date <= today.AddDays(60)),

                    // By frequency breakdown
                    ByFrequency = activeInvoices
                        .GroupBy(i => i.Frequency.ToString())
                        .Select(g => new RecurringInvoiceByFrequencyDto
                        {
                            Frequency = g.Key,
                            Count = g.Count(),
                            TotalValue = g.Sum(i => i.TotalAmount)
                        })
                        .OrderBy(f => f.Frequency)
                        .ToList(),

                    // Top customers
                    TopCustomersByValue = activeInvoices
                        .Where(i => i.Customer != null)
                        .GroupBy(i => new { i.CustomerId, i.Customer.Name })
                        .Select(g => new RecurringInvoiceByCustomerDto
                        {
                            CustomerId = g.Key.CustomerId,
                            CustomerName = g.Key.Name ?? "Unknown",
                            Count = g.Count(),
                            TotalValue = g.Sum(i => i.TotalAmount),
                            AverageValue = g.Average(i => i.TotalAmount)
                        })
                        .OrderByDescending(c => c.TotalValue)
                        .Take(5)
                        .ToList()
                };

                return OperationResult<RecurringInvoiceStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for company {CompanyId}", context.CompanyId);
                return OperationResult<RecurringInvoiceStatsDto>.FailureResult("Failed to retrieve stats.");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<OperationResult<bool>> UpdateStatusAsync(
            int id,
            RecurringInvoiceStatus newStatus,
            OperationContext context,
            string action)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!context.CompanyId.HasValue)
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = context.CompanyId.Value;

                var recurringInvoice = await _unitOfWork.RecurringInvoices.GetAsync(
                    r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted);

                if (recurringInvoice == null)
                {
                    return OperationResult<bool>.FailureResult("Recurring invoice not found.");
                }

                // Validate status transition
                if (!CanTransitionToStatus(recurringInvoice.Status, newStatus, out var errorMessage))
                {
                    return OperationResult<bool>.FailureResult(errorMessage);
                }

                var oldStatus = recurringInvoice.Status;

                recurringInvoice.Status = newStatus;
                recurringInvoice.UpdatedBy = context.UserId;
                recurringInvoice.UpdatedDate = DateTime.UtcNow;

                // Set date fields based on new status
                if (newStatus == RecurringInvoiceStatus.Paused)
                {
                    recurringInvoice.PausedDate = DateTime.UtcNow;
                }
                else if (newStatus == RecurringInvoiceStatus.Cancelled)
                {
                    recurringInvoice.CancelledDate = DateTime.UtcNow;
                }

                _unitOfWork.RecurringInvoices.Update(recurringInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurringInvoice.Id,
                    Action = char.ToUpper(action[0]) + action.Substring(1),
                    Description = $"Recurring invoice {action}d by {context.UserId}",
                    Changes = JsonSerializer.Serialize(new { OldStatus = oldStatus.ToString(), NewStatus = newStatus.ToString() }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.RecurringInvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                await _hubContext.Clients.Group($"company-{companyId}").SendAsync(
                    $"RecurringInvoice{char.ToUpper(action[0]) + action.Substring(1)}d",
                    new { Id = id, Status = newStatus.ToString() });

                _logger.LogInformation("Recurring invoice {RecurringInvoiceId} {Action}d for company {CompanyId}",
                    id, action, companyId);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error {Action}ing recurring invoice {RecurringInvoiceId} for company {CompanyId}",
                    action, id, context.CompanyId);
                return OperationResult<bool>.FailureResult($"Failed to {action} recurring invoice.");
            }
        }

        private bool CanTransitionToStatus(RecurringInvoiceStatus current, RecurringInvoiceStatus target, out string errorMessage)
        {
            errorMessage = null;

            // Define allowed transitions
            var allowedTransitions = new Dictionary<RecurringInvoiceStatus, List<RecurringInvoiceStatus>>
            {
                { RecurringInvoiceStatus.Draft, new List<RecurringInvoiceStatus>
                    { RecurringInvoiceStatus.Active, RecurringInvoiceStatus.Cancelled } },
                { RecurringInvoiceStatus.Active, new List<RecurringInvoiceStatus>
                    { RecurringInvoiceStatus.Paused, RecurringInvoiceStatus.Cancelled, RecurringInvoiceStatus.Completed, RecurringInvoiceStatus.Expired } },
                { RecurringInvoiceStatus.Paused, new List<RecurringInvoiceStatus>
                    { RecurringInvoiceStatus.Active, RecurringInvoiceStatus.Cancelled } },
                { RecurringInvoiceStatus.Completed, new List<RecurringInvoiceStatus>() },
                { RecurringInvoiceStatus.Cancelled, new List<RecurringInvoiceStatus>() },
                { RecurringInvoiceStatus.Expired, new List<RecurringInvoiceStatus>() }
            };

            if (!allowedTransitions.ContainsKey(current))
            {
                errorMessage = "Invalid current status.";
                return false;
            }

            if (!allowedTransitions[current].Contains(target) && current != target)
            {
                errorMessage = $"Cannot transition from {current} to {target}.";
                return false;
            }

            return true;
        }

        private bool ValidateSchedule(CreateRecurringInvoiceDto dto, out string errorMessage)
        {
            errorMessage = null;

            switch (dto.Frequency)
            {
                case RecurringFrequency.Daily:
                    // No additional validation needed
                    break;

                case RecurringFrequency.Weekly:
                case RecurringFrequency.BiWeekly:
                    if (!dto.DayOfWeek.HasValue)
                    {
                        errorMessage = "Day of week is required for weekly frequencies.";
                        return false;
                    }
                    break;

                case RecurringFrequency.Monthly:
                case RecurringFrequency.BiMonthly:
                    if (!dto.DayOfMonth.HasValue && !dto.WeekOfMonth.HasValue)
                    {
                        errorMessage = "Either day of month or week of month is required for monthly frequencies.";
                        return false;
                    }

                    if (dto.DayOfMonth.HasValue && (dto.DayOfMonth < 1 || dto.DayOfMonth > 31))
                    {
                        errorMessage = "Day of month must be between 1 and 31.";
                        return false;
                    }

                    if (dto.WeekOfMonth.HasValue && (dto.WeekOfMonth < 1 || dto.WeekOfMonth > 5))
                    {
                        errorMessage = "Week of month must be between 1 and 5.";
                        return false;
                    }
                    break;

                case RecurringFrequency.Quarterly:
                case RecurringFrequency.SemiAnnually:
                    if (!dto.DayOfMonth.HasValue && !dto.MonthOfYear.HasValue)
                    {
                        errorMessage = "Month of year is recommended for quarterly/semi-annual frequencies.";
                    }
                    break;

                case RecurringFrequency.Annually:
                    if (!dto.MonthOfYear.HasValue && !dto.DayOfMonth.HasValue)
                    {
                        errorMessage = "Month of year and day of month are recommended for annual frequency.";
                    }
                    break;
            }

            // Validate end date vs max occurrences
            if (dto.EndDate.HasValue && dto.MaxOccurrences.HasValue)
            {
                errorMessage = "Cannot specify both end date and max occurrences.";
                return false;
            }

            // Validate start date
            if (dto.StartDate < DateTime.UtcNow.Date.AddDays(-1))
            {
                errorMessage = "Start date cannot be in the past.";
                return false;
            }

            // Validate end date
            if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate)
            {
                errorMessage = "End date must be after start date.";
                return false;
            }

            return true;
        }

        private DateTime CalculateNextDate(Domain.Entities.RecurringInvoices.RecurringInvoice invoice, DateTime fromDate)
        {
            var nextDate = fromDate;

            switch (invoice.Frequency)
            {
                case RecurringFrequency.Daily:
                    nextDate = fromDate.AddDays(invoice.FrequencyInterval);
                    break;

                case RecurringFrequency.Weekly:
                    nextDate = fromDate.AddDays(7 * invoice.FrequencyInterval);
                    if (invoice.DayOfWeek.HasValue)
                    {
                        // Adjust to specific day of week
                        while (nextDate.DayOfWeek != invoice.DayOfWeek.Value)
                        {
                            nextDate = nextDate.AddDays(1);
                        }
                    }
                    break;

                case RecurringFrequency.BiWeekly:
                    nextDate = fromDate.AddDays(14 * invoice.FrequencyInterval);
                    if (invoice.DayOfWeek.HasValue)
                    {
                        while (nextDate.DayOfWeek != invoice.DayOfWeek.Value)
                        {
                            nextDate = nextDate.AddDays(1);
                        }
                    }
                    break;

                case RecurringFrequency.Monthly:
                case RecurringFrequency.BiMonthly:
                    nextDate = fromDate.AddMonths(invoice.FrequencyInterval * (invoice.Frequency == RecurringFrequency.BiMonthly ? 2 : 1));

                    if (invoice.DayOfMonth.HasValue)
                    {
                        // Adjust to specific day of month
                        var targetDay = Math.Min(invoice.DayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay);
                    }
                    else if (invoice.WeekOfMonth.HasValue && invoice.DayOfWeek.HasValue)
                    {
                        // Adjust to specific weekday of month (e.g., second Monday)
                        nextDate = GetNthWeekdayOfMonth(nextDate.Year, nextDate.Month,
                            invoice.WeekOfMonth.Value, invoice.DayOfWeek.Value);
                    }
                    break;

                case RecurringFrequency.Quarterly:
                    nextDate = fromDate.AddMonths(3 * invoice.FrequencyInterval);
                    break;

                case RecurringFrequency.SemiAnnually:
                    nextDate = fromDate.AddMonths(6 * invoice.FrequencyInterval);
                    break;

                case RecurringFrequency.Annually:
                    nextDate = fromDate.AddYears(invoice.FrequencyInterval);
                    if (invoice.MonthOfYear.HasValue && invoice.DayOfMonth.HasValue)
                    {
                        try
                        {
                            nextDate = new DateTime(nextDate.Year, invoice.MonthOfYear.Value,
                                Math.Min(invoice.DayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, invoice.MonthOfYear.Value)));
                        }
                        catch
                        {
                            // Fallback to same month/day as source
                        }
                    }
                    break;
            }

            return nextDate;
        }

        private DateTime GetNthWeekdayOfMonth(int year, int month, int n, DayOfWeek dayOfWeek)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var daysToAdd = ((int)dayOfWeek - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            var firstOccurrence = firstDayOfMonth.AddDays(daysToAdd);

            var nthOccurrence = firstOccurrence.AddDays(7 * (n - 1));

            // If we've gone past the month, go back to the last occurrence
            if (nthOccurrence.Month != month)
            {
                nthOccurrence = firstOccurrence.AddDays(7 * 4); // Last occurrence (5th week)
                if (nthOccurrence.Month != month)
                {
                    nthOccurrence = firstOccurrence.AddDays(7 * 3); // 4th week
                }
            }

            return nthOccurrence;
        }

        private bool HasScheduleChanged(UpdateRecurringInvoiceDto dto, Domain.Entities.RecurringInvoices.RecurringInvoice existing)
        {
            return dto.Frequency != existing.Frequency ||
                   dto.FrequencyInterval != existing.FrequencyInterval ||
                   dto.DayOfMonth != existing.DayOfMonth ||
                   dto.DayOfWeek != existing.DayOfWeek ||
                   dto.WeekOfMonth != existing.WeekOfMonth ||
                   dto.MonthOfYear != existing.MonthOfYear ||
                   dto.StartDate != existing.StartDate ||
                   dto.EndDate != existing.EndDate ||
                   dto.MaxOccurrences != existing.MaxOccurrences;
        }

        private async Task<CreateInvoiceDto> CreateInvoiceFromTemplate(
            Domain.Entities.RecurringInvoices.RecurringInvoice template,
            DateTime invoiceDate,
            DateTime? dueDate,
            OperationContext context)
        {
            // Create invoice DTO using template values
            var invoiceDto = new CreateInvoiceDto
            {
                CustomerId = template.CustomerId,
                InvoiceType = InvoiceType.Recurring,
                Currency = template.Currency,
                IssueDate = invoiceDate,
                DueDate = dueDate ?? (template.OverridePaymentTerms.HasValue
                    ? invoiceDate.AddDays(template.OverridePaymentTerms.Value)
                    : invoiceDate.AddDays(30)),
                PONumber = template.OverridePONumber ?? template.PONumber,
                ProjectDetail = template.OverrideProjectDetail ?? template.ProjectDetail,
                CustomerNotes = template.OverrideCustomerNotes ?? template.CustomerNotes,
                PaymentMethod = template.OverridePaymentMethod ?? template.PaymentMethod,
                IsAutomated = true,
                // Items would need to be populated from template's recurring items
                // This requires additional DTO properties or fetching from source invoice
            };

            // If source invoice exists, copy its items
            if (template.SourceInvoiceId.HasValue && template.SourceInvoice != null)
            {
                invoiceDto.Items = template.SourceInvoice.InvoiceItems.Select(item => new InvoiceItemDto
                {
                    Description = item.Description,
                    Quantity = (int)item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxType = item.TaxType,
                    TaxAmount = item.TaxAmount,
                    Amount = item.Amount
                }).ToList();

                invoiceDto.TaxDetails = template.SourceInvoice.TaxDetails.Select(tax => new InvoiceTaxDetailDto
                {
                    TaxName = tax.TaxName,
                    Rate = tax.Rate,
                    TaxAmount = tax.TaxAmount
                }).ToList();

                invoiceDto.Discounts = template.SourceInvoice.Discounts.Select(discount => new InvoiceDiscountDto
                {
                    Description = discount.Description,
                    Amount = discount.Amount,
                    DiscountType = DiscountType.Percentage
                }).ToList();
            }

            return invoiceDto;
        }

        private async System.Threading.Tasks.Task MarkGenerationFailed(int recurringInvoiceId, string errorMessage)
        {
            try
            {
                // You could implement a retry counter or notification system here
                _logger.LogWarning("Generation failed for recurring invoice {RecurringInvoiceId}: {ErrorMessage}",
                    recurringInvoiceId, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking generation failed for recurring invoice {RecurringInvoiceId}",
                    recurringInvoiceId);
            }
        }

        private async Task<RecurringInvoiceResponseDto> MapToResponseDto(Domain.Entities.RecurringInvoices.RecurringInvoice invoice)
        {
            var response = _mapper.Map<RecurringInvoiceResponseDto>(invoice);

            if (invoice.Customer != null)
            {
                response.Customer = _mapper.Map<CustomerResponseDto>(invoice.Customer);
            }

            if (invoice.SourceInvoice != null)
            {
                response.SourceInvoiceNumber = invoice.SourceInvoice.InvoiceNumber;
            }

            if (invoice.GeneratedInvoices != null)
            {
                response.GeneratedInvoices = invoice.GeneratedInvoices
                    .Where(g => !g.IsDeleted)
                    .Select(g => new RecurringInvoiceInstanceDto
                    {
                        Id = g.Id,
                        InvoiceId = g.InvoiceId,
                        InvoiceNumber = g.Invoice?.InvoiceNumber ?? g.GeneratedInvoiceNumber,
                        GeneratedDate = g.GeneratedDate,
                        ScheduledGenerationDate = g.ScheduledGenerationDate,
                        SequenceNumber = g.SequenceNumber,
                        Amount = g.Invoice?.TotalAmount ?? g.Amount,
                        InvoiceStatus = g.Invoice?.InvoiceStatus.ToString() ?? "Unknown",
                        PaymentStatus = g.Invoice?.PaymentStatus.ToString() ?? "Unknown",
                        IssueDate = g.Invoice?.IssueDate,
                        DueDate = g.Invoice?.DueDate,
                        GenerationStatus = g.GenerationStatus.ToString(),
                        ErrorMessage = g.ErrorMessage,
                        RetryCount = g.RetryCount,
                        Notes = g.Notes
                    })
                    .OrderByDescending(g => g.GeneratedDate)
                    .ToList();

                response.TotalGeneratedCount = response.GeneratedInvoices.Count;
                response.TotalGeneratedValue = response.GeneratedInvoices.Sum(g => g.Amount);
                response.AverageInvoiceValue = response.TotalGeneratedCount > 0
                    ? response.TotalGeneratedValue / response.TotalGeneratedCount
                    : 0;
            }

            return response;
        }

        #endregion
    }
}