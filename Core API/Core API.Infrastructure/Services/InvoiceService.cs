using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Email;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Core_API.Domain.Models.Email;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.Services
{
    public class InvoiceService(IUnitOfWork unitOfWork, IPdfService pdfService, IEmailSendingService emailSendingService, IExchangeRateService exchangeRateService, ILogger<InvoiceService> logger, IMapper mapper, IHubContext<NotificationHub> hubContext) : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        private readonly IEmailSendingService _emailSendingService = emailSendingService ?? throw new ArgumentNullException(nameof(emailSendingService));
        private readonly IExchangeRateService _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        private readonly ILogger<InvoiceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly string[] _supportedCurrencies = ["USD", "EUR", "INR"];
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        public async Task<OperationResult<InvoiceResponseDto>> CreateAsync(InvoiceCreateDto dto, OperationContext operationContext)
        {
            // Start transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for creating an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Get company to get base currency
                var company = await _unitOfWork.Companies.GetAsync(c => c.Id == companyId);
                if (company == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company not found.");
                }

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Validate invoice number
                if (!dto.IsAutomated && await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, dto.InvoiceNumber))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number already exists.");
                }

                // Validate currency
                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                // Fetch current exchange rate from API
                decimal exchangeRate = await _exchangeRateService.GetExchangeRateAsync(company.BaseCurrency, dto.Currency);
                _logger.LogInformation("Fetched exchange rate from {BaseCurrency} to {InvoiceCurrency}: {Rate}",
                    company.BaseCurrency, dto.Currency, exchangeRate);

                // Validate tax types
                var taxTypes = await _unitOfWork.TaxTypes.Query()
                    .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                    .ToListAsync();

                foreach (var item in dto.Items)
                {
                    if (!string.IsNullOrEmpty(item.TaxType) && !taxTypes.Any(t => t.Name == item.TaxType))
                    {
                        return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {item.TaxType}.");
                    }
                }

                // Validate discounts
                if (dto.Discounts.Any(d => d.Amount < 0))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Discount amount cannot be negative.");
                }

                // Map DTO to entity
                var invoice = _mapper.Map<Invoice>(dto);

                // Set system-generated fields
                invoice.InvoiceNumber = dto.IsAutomated
                        ? await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId)
                        : dto.InvoiceNumber ?? $"INV{DateTime.UtcNow.Ticks}";

                invoice.CompanyId = companyId;
                invoice.CreatedBy = operationContext.UserId;
                invoice.CreatedDate = DateTime.UtcNow;
                invoice.InvoiceStatus = InvoiceStatus.Draft;
                invoice.PaymentStatus = PaymentStatus.Pending;
                invoice.AmountPaid = 0;
                invoice.AmountRefunded = 0;
                invoice.AmountDue = 0;
                invoice.SourceSystem = dto.SourceSystem ?? "Manual";

                // Calculate item amounts and tax amounts
                CalculateItemAmounts(invoice.InvoiceItems, taxTypes);

                // Calculate tax details from items
                CalculateTaxDetails(invoice.InvoiceItems, invoice.TaxDetails, taxTypes);

                // Calculate discounts
                CalculateDiscount(invoice.Discounts, invoice.Subtotal);

                // Calculate all totals
                CalculateInvoiceTotals(invoice);

                // Convert to base currency for reporting
                invoice.BaseCurrencySubtotal = invoice.Subtotal / exchangeRate;
                invoice.BaseCurrencyTotalAmount = invoice.TotalAmount / exchangeRate;

                // Calculate AmountDue
                invoice.AmountDue = invoice.TotalAmount - invoice.AmountPaid;
                invoice.BaseCurrencyAmountDue = invoice.BaseCurrencyTotalAmount - invoice.BaseCurrencyAmountPaid;

                // Add invoice 
                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Handle attachments after invoice has ID
                if (dto.Attachments?.Any() == true)
                {
                    invoice.InvoiceAttachments = await HandleAttachmentsAsync(dto.Attachments, companyId, invoice.Id, operationContext.UserId);
                    // Update the invoice with attachments
                    _unitOfWork.Invoices.Update(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Create audit log
                var auditLog = CreateAuditLog(invoice.Id, dto, invoice, operationContext);
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Map to response DTO
                var response = MapToInvoiceResponseDto(invoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(customer);

                // Send notification
                if (_hubContext != null && customer != null)
                {
                    await SendNotificationAsync(customer.Id.ToString(), new
                    {
                        InvoiceId = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        Currency = invoice.Currency,
                        TotalAmount = invoice.TotalAmount,
                        Message = $"New invoice {invoice.InvoiceNumber} has been created"
                    });
                }

                _logger.LogInformation("Invoice {InvoiceNumber} created successfully with ID {InvoiceId}",
                    invoice.InvoiceNumber, invoice.Id);

                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to create invoice: " + ex.Message);
            }
        }
        public async Task<OperationResult<InvoiceResponseDto>> UpdateAsync(InvoiceUpdateDto dto, OperationContext operationContext)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for updating an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Get existing invoice with all related data
                var invoice = await _unitOfWork.Invoices.GetAsync(
                    i => i.Id == dto.Id && i.CompanyId == companyId && !i.IsDeleted,
                    "InvoiceItems,TaxDetails,Discounts,InvoiceAttachments,Customer");

                if (invoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Validate invoice number uniqueness
                if (!dto.IsAutomated && !string.IsNullOrEmpty(dto.InvoiceNumber) &&
                    await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, dto.InvoiceNumber, dto.Id))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number already exists.");
                }

                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                // Validate tax types
                var taxTypes = await _unitOfWork.TaxTypes.Query()
                    .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                    .ToListAsync();

                foreach (var item in dto.Items.Where(i => !string.IsNullOrEmpty(i.TaxType)))
                {
                    if (!string.IsNullOrEmpty(item.TaxType) && !taxTypes.Any(t => t.Name == item.TaxType))
                    {
                        return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {item.TaxType}.");
                    }
                }
                foreach (var tax in dto.TaxDetails.Where(t => !string.IsNullOrEmpty(t.TaxName)))
                {
                    if (!taxTypes.Any(t => t.Name == tax.TaxName))
                    {
                        return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {tax.TaxName}.");
                    }
                }

                // Validate discounts
                if (dto.Discounts.Any(d => d.Amount < 0))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Discount amount cannot be negative.");
                }

                // Store old values for audit
                var oldInvoice = JsonSerializer.Serialize(new
                {
                    invoice.InvoiceNumber,
                    invoice.InvoiceStatus,
                    invoice.PaymentStatus,
                    invoice.TotalAmount,
                    invoice.AmountPaid,
                });

                // Preserve payment information
                var existingAmountPaid = invoice.AmountPaid;
                var existingAmountRefunded = invoice.AmountRefunded;

                // Map DTO to existing entity (AutoMapper updates simple properties)
                _mapper.Map(dto, invoice);

                // Restore payment information
                invoice.AmountPaid = existingAmountPaid;
                invoice.AmountRefunded = existingAmountRefunded;

                // Generate invoice number if automated
                invoice.InvoiceNumber = dto.IsAutomated
                     ? await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId)
                     : dto.InvoiceNumber ?? invoice.InvoiceNumber;

                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                // Update collections using AutoMapper
                // Items
                invoice.InvoiceItems.Clear();
                var mappedItems = _mapper.Map<List<InvoiceItem>>(dto.Items);
                CalculateItemAmounts(mappedItems, taxTypes);
                foreach (var item in mappedItems)
                {
                    item.CreatedBy = operationContext.UserId;
                    item.CreatedDate = DateTime.UtcNow;
                    invoice.InvoiceItems.Add(item);
                }

                // Update tax details
                invoice.TaxDetails.Clear();
                var mappedTaxDetails = _mapper.Map<List<InvoiceTaxDetail>>(dto.TaxDetails);
                CalculateTaxDetails(invoice.InvoiceItems, mappedTaxDetails, taxTypes);
                foreach (var tax in mappedTaxDetails)
                {
                    tax.CreatedBy = operationContext.UserId;
                    tax.CreatedDate = DateTime.UtcNow;
                    invoice.TaxDetails.Add(tax);
                }

                // Update discount details
                invoice.Discounts.Clear();
                var mappedDiscounts = _mapper.Map<List<InvoiceDiscount>>(dto.Discounts);
                CalculateDiscount(mappedDiscounts, invoice.Subtotal);
                foreach (var discount in mappedDiscounts)
                {
                    discount.CreatedBy = operationContext.UserId;
                    discount.CreatedDate = DateTime.UtcNow;
                    invoice.Discounts.Add(discount);
                }

                // Recalculate totals
                CalculateInvoiceTotals(invoice);
                invoice.AmountDue = invoice.TotalAmount - invoice.AmountPaid;

                // Handle attachments
                invoice.InvoiceAttachments.Clear();

                if (dto.Attachments?.Any() == true)
                {
                    var existingAttachmentIds = invoice.InvoiceAttachments.Select(a => a.Id).ToHashSet();

                    var newAttachments = dto.Attachments.Where(a => a.FileContent != null).ToList();

                    if (newAttachments.Any())
                    {
                        var addedAttachments = await HandleAttachmentsAsync(
                            newAttachments, companyId, invoice.Id, operationContext.UserId);
                        invoice.InvoiceAttachments.AddRange(addedAttachments);
                    }
                }
                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoice.Id,
                    Action = "Updated",
                    Description = $"Invoice updated by {operationContext.UserId}",
                    Changes = JsonSerializer.Serialize(new { Old = oldInvoice, New = dto }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = _mapper.Map<InvoiceResponseDto>(invoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(customer);

                // Send notification to the CUSTOMER
                if (_hubContext != null && invoice.Customer != null)
                {
                    try
                    {
                        await _hubContext.Clients.Group(invoice.CustomerId.ToString()).SendAsync(
                            "ReceiveInvoiceNotification",
                            new
                            {
                                InvoiceId = invoice.Id,
                                InvoiceNumber = invoice.InvoiceNumber,
                                Message = $"Invoice {invoice.InvoiceNumber} has been updated."
                            });

                        _logger.LogInformation("Sent SignalR notification for invoice update {InvoiceId} to customer {CustomerId}",
                            invoice.Id, invoice.CustomerId);
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogWarning(signalREx, "Failed to send SignalR notification for invoice update {InvoiceId} to customer {CustomerId}",
                            invoice.Id, invoice.CustomerId);
                    }
                }

                _logger.LogInformation("Invoice {InvoiceId} updated successfully", invoice.Id);

                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating invoice {InvoiceId} for company {CompanyId}", dto.Id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to update invoice.");
            }
        }
        public async Task<OperationResult<bool>> DeleteAsync(int id, OperationContext operationContext)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for deleting an invoice.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var invoice = await _unitOfWork.Invoices.GetAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted);
                if (invoice == null)
                {
                    return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Soft delete invoice
                invoice.IsDeleted = true;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                // Soft delete attachments
                foreach (var attachment in invoice.InvoiceAttachments)
                {
                    attachment.IsDeleted = true;
                    attachment.UpdatedBy = operationContext.UserId;
                    attachment.UpdatedDate = DateTime.UtcNow;
                }

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoice.Id,
                    Action = "Deleted",
                    Description = $"Invoice deleted by {operationContext.UserId}",
                    Changes = JsonSerializer.Serialize(new { DeletedDate = DateTime.UtcNow }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Send notification to the CUSTOMER
                if (_hubContext != null && invoice.Customer != null)
                {
                    try
                    {
                        await _hubContext.Clients.Group(invoice.CustomerId.ToString()).SendAsync(
                            "ReceiveInvoiceNotification",
                            new
                            {
                                InvoiceId = invoice.Id,
                                InvoiceNumber = invoice.InvoiceNumber,
                                Message = $"Invoice {invoice.InvoiceNumber} has been deleted."
                            });

                        _logger.LogInformation("Sent SignalR notification for invoice deletion {InvoiceId} to customer {CustomerId}",
                            invoice.Id, invoice.CustomerId);
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogWarning(signalREx, "Failed to send SignalR notification for invoice deletion {InvoiceId} to customer {CustomerId}",
                            invoice.Id, invoice.CustomerId);
                    }
                }

                _logger.LogInformation("Invoice {InvoiceId} deleted successfully", id);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to delete invoice.");
            }
        }
        public async Task<OperationResult<InvoiceResponseDto>> GetByIdAsync(int id, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }

                int companyId = operationContext.CompanyId.Value;

                var invoice = await _unitOfWork.Invoices.GetAsync(
                  i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted,
                  includeProperties: "Customer,InvoiceItems,TaxDetails,Discounts,InvoiceAttachments,Payments,AuditLogs");

                if (invoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Check customer access if user is a customer
                if (operationContext.CustomerId.HasValue && invoice.CustomerId != operationContext.CustomerId.Value)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Access denied to this invoice.");
                }
                var response = _mapper.Map<InvoiceResponseDto>(invoice);
                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to retrieve invoice.");
            }
        }
        public async Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(OperationContext operationContext, InvoiceFilterRequestDto filter)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving paged invoices.");
                    return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                // Apply customer filter if user is a customer
                if (operationContext.CustomerId.HasValue)
                {
                    filter.CustomerId = operationContext.CustomerId.Value;
                }

                var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, filter);

                var mappedItems = new List<InvoiceResponseDto>();
                foreach (var invoice in result.Items)
                {
                    var dto = MapToInvoiceResponseDto(invoice);
                    mappedItems.Add(dto);
                }

                var response = new PaginatedResult<InvoiceResponseDto>
                {
                    Items = mappedItems,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                };

                return OperationResult<PaginatedResult<InvoiceResponseDto>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged invoices for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Failed to retrieve invoices.");
            }
        }
        public async Task<OperationResult<InvoiceSettingsDto>> GetInvoiceSettingsAsync(OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving invoice settings.");
                    return OperationResult<InvoiceSettingsDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(companyId);
                if (settings == null)
                {
                    // Create default settings if not exist
                    settings = new InvoiceSettings
                    {
                        CompanyId = companyId,
                        IsAutomated = true,
                        InvoicePrefix = "INV",
                        InvoiceStartingNumber = 1000,
                        CreatedBy = operationContext.UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.InvoiceSettings.AddAsync(settings);
                    await _unitOfWork.SaveChangesAsync();
                }
                var settingsDto = _mapper.Map<InvoiceSettingsDto>(settings);
                return OperationResult<InvoiceSettingsDto>.SuccessResult(settingsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceSettingsDto>.FailureResult("Failed to retrieve invoice settings.");
            }
        }
        public async Task<OperationResult<bool>> SaveInvoiceSettingsAsync(InvoiceSettingsDto dto, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for saving invoice settings.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(companyId);
                if (settings == null)
                {
                    settings = _mapper.Map<InvoiceSettings>(dto);
                    settings.CompanyId = companyId;
                    settings.CreatedBy = operationContext.UserId;
                    settings.CreatedDate = DateTime.UtcNow;
                    await _unitOfWork.InvoiceSettings.AddAsync(settings);
                }
                else
                {
                    _mapper.Map(dto, settings);
                    settings.UpdatedBy = operationContext.UserId;
                    settings.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.InvoiceSettings.Update(settings);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice settings saved successfully for company {CompanyId}", companyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to save invoice settings.");
            }
        }
        public async Task<OperationResult<string>> GetNextInvoiceNumberAsync(OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving next invoice number.");
                    return OperationResult<string>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var nextNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);
                return OperationResult<string>.SuccessResult(nextNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next invoice number for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<string>.FailureResult("Failed to retrieve next invoice number.");
            }
        }
        public async Task<OperationResult<bool>> SendInvoiceAsync(int id, OperationContext operationContext, EmailDataDto emailData)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for sending an invoice.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Validate email data
                if (emailData.To.Count == 0 || emailData.To.All(e => string.IsNullOrWhiteSpace(e)))
                {
                    _logger.LogWarning("No valid 'To' email addresses provided for invoice {InvoiceId}", id);
                    return OperationResult<bool>.FailureResult("At least one valid 'To' email address is required.");
                }
                // Fetch invoice with related data
                var invoice = await _unitOfWork.Invoices.GetAsync(
                    i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted,
                    "InvoiceItems,TaxDetails,Discounts,Customer"
                );
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}", id, companyId);
                    return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Log invoice details for debugging
                _logger.LogDebug("Fetched invoice: Id={Id}, CustomerId={CustomerId}, InvoiceNumber={InvoiceNumber}, DueDate={DueDate}, TotalAmount={TotalAmount}",
                    invoice.Id, invoice.CustomerId, invoice.InvoiceNumber, invoice.DueDate, invoice.TotalAmount);

                // Validate CustomerId
                if (invoice.CustomerId == 0)
                {
                    _logger.LogWarning("Invoice {InvoiceId} has an invalid CustomerId (0).", id);
                    return OperationResult<bool>.FailureResult("Invoice has an invalid CustomerId.");
                }
                // Generate PDF if required
                MemoryStream pdfStream = null;
                if (emailData.AttachPdf)
                {
                    var pdfResult = await _pdfService.GenerateInvoicePdfAsync(id, operationContext);
                    if (!pdfResult.IsSuccess)
                    {
                        _logger.LogWarning("Failed to generate PDF for invoice {InvoiceId}: {ErrorMessage}", id, pdfResult.ErrorMessage);
                        return OperationResult<bool>.FailureResult("Failed to generate invoice PDF.");
                    }
                    pdfStream = pdfResult.Data.PdfStream;
                }

                // Prepare email request
                var invoiceEmailRequest = new InvoiceEmailRequest
                {
                    To = emailData.To.Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
                    Cc = emailData.Cc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                    Bcc = emailData.Bcc?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new List<string>(),
                    Subject = emailData.Subject ?? $"Invoice {invoice.InvoiceNumber}",
                    HtmlMessage = emailData.Message ?? GenerateDefaultEmailMessage(invoice),
                    InvoiceNumber = invoice.InvoiceNumber,
                    AmountDue = invoice.TotalAmount,
                    DueDate = invoice.DueDate
                };

                // Send email
                await _emailSendingService.SendInvoiceEmailAsync(invoiceEmailRequest, operationContext, pdfStream, $"Invoice_{invoice.InvoiceNumber}.pdf");

                // Update invoice
                invoice.SentDate = DateTime.UtcNow;
                invoice.InvoiceStatus = InvoiceStatus.Sent;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoice.Id,
                    Action = "Sent",
                    Description = $"Invoice sent to {string.Join(", ", invoiceEmailRequest.To)}",
                    Changes = JsonSerializer.Serialize(new { SentDate = DateTime.UtcNow, Recipients = invoiceEmailRequest.To }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();


                // Send SignalR notification to the customer
                try
                {
                    if (_hubContext == null)
                    {
                        _logger.LogWarning("SignalR hub context is null for invoice {InvoiceId}", id);
                    }
                    else
                    {
                        await _hubContext.Clients.Group(invoice.CustomerId.ToString()).SendAsync(
                            "ReceiveInvoiceNotification",
                            new
                            {
                                InvoiceId = invoice.Id,
                                InvoiceNumber = invoice.InvoiceNumber,
                                Message = $"New invoice {invoice.InvoiceNumber} has been sent to you."
                            }
                        );
                        _logger.LogInformation("Sent SignalR notification for invoice {InvoiceId} to customer {CustomerId}", id, invoice.CustomerId);
                    }
                }
                catch (Exception signalREx)
                {
                    _logger.LogWarning(signalREx, "Failed to send SignalR notification for invoice {InvoiceId} to customer {CustomerId}", id, invoice.CustomerId);
                }

                _logger.LogInformation("Invoice {InvoiceId} sent successfully to {Email} for company {CompanyId}", id, invoiceEmailRequest.To.FirstOrDefault(), companyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to send invoice.");
            }
        }
        public async Task<OperationResult<InvoiceStatsDto>> GetStatsAsync(OperationContext operationContext)
        {
            try
            {
                // Build the base query
                var query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                // Apply company filter
                if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    _logger.LogWarning("No CompanyId provided for stats retrieval");
                    return OperationResult<InvoiceStatsDto>.FailureResult("Company ID is required.");
                }

                // Apply customer filter if this is a customer role
                if (operationContext.CustomerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }

                // For Admin/User roles, DO NOT filter by CreatedBy - show ALL company invoices
                // Only filter by CreatedBy if you specifically want user-specific invoices
                //if (!string.IsNullOrEmpty(operationContext.UserId) && operationContext.CustomerId == null)
                //    query = query.Where(i => i.CreatedBy == operationContext.UserId);

                // Get all invoices for the company/customer
                var invoices = await query.ToListAsync();

                _logger.LogInformation("Retrieved {Count} invoices for stats calculation", invoices.Count);

                // For debugging - log first few invoices if any
                if (invoices.Any())
                {
                    _logger.LogDebug("First invoice sample: Id={Id}, Number={Number}, Status={Status}, Amount={Amount}",
                        invoices.First().Id, invoices.First().InvoiceNumber, invoices.First().InvoiceStatus, invoices.First().TotalAmount);
                }

                // Calculate previous period for change (last 30 days vs previous 30 days)
                var today = DateTime.UtcNow.Date;
                var currentPeriodStart = today.AddDays(-30);
                var previousPeriodStart = today.AddDays(-60);
                var currentPeriodEnd = today;
                var previousPeriodEnd = currentPeriodStart;

                var currentPeriodInvoices = invoices.Where(i => i.IssueDate >= currentPeriodStart && i.IssueDate < currentPeriodEnd).ToList();
                var previousPeriodInvoices = invoices.Where(i => i.IssueDate >= previousPeriodStart && i.IssueDate < previousPeriodEnd).ToList();

                var currentPeriodAmount = currentPeriodInvoices.Sum(i => i.TotalAmount);
                var previousPeriodAmount = previousPeriodInvoices.Sum(i => i.TotalAmount);

                var stats = new InvoiceStatsDto
                {
                    All = new StatsItem
                    {
                        Count = invoices.Count,
                        Amount = invoices.Sum(i => i.TotalAmount),
                        Change = CalculateChange(previousPeriodAmount, currentPeriodAmount)
                    },
                    Draft = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Draft),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Draft).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Sent = new StatsItem // Same as Open
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Sent),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Sent).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Viewed = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Viewed),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Viewed).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    PartiallyPaid = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.PartiallyPaid),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.PartiallyPaid).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Paid = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Paid),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Paid).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Overdue = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Overdue),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Overdue).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Void = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Void),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Void).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Cancelled = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Void),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Void).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Refunded = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Refunded),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Refunded).Sum(i => i.TotalAmount),
                        Change = 0
                    },
                    Pending = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Pending),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Pending).Sum(i => i.TotalAmount),
                        Change = 0
                    }
                };
                _logger.LogInformation("Stats calculated: Total Invoices={Count}, Total Amount={Amount}",stats.All.Count, stats.All.Amount);

                return OperationResult<InvoiceStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceStatsDto>.FailureResult("Failed to retrieve stats.");
            }
        }
        public async Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetCustomerInvoicesAsync(OperationContext context, int pageNumber, int pageSize, string status)
        {
            try
            {
                // Validate CustomerId
                if (!context.CustomerId.HasValue || context.CustomerId <= 0)
                {
                    _logger.LogWarning("Valid Customer ID is required for retrieving customer invoices.");
                    return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Valid Customer ID is required.");
                }
                int customerId = context.CustomerId.Value;

                var query = _unitOfWork.Invoices.Query()
                    .Where(i => !i.IsDeleted && i.CustomerId == customerId)
                    .Include(i => i.Customer)
                    .Include(i => i.Company)
                    .Include(i => i.InvoiceItems)
                    .Include(i => i.TaxDetails)
                    .Include(i => i.Discounts);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Invoice, List<InvoiceDiscount>>)query.Where(i => i.InvoiceStatus == invoiceStatus);
                }

                var totalCount = await query.CountAsync();
                var invoices = await query
                    .OrderByDescending(i => i.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var invoiceDtos = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    var dto = MapToInvoiceResponseDto(invoice);
                    invoiceDtos.Add(dto);
                }

                var result = new PaginatedResult<InvoiceResponseDto>
                {
                    Items = invoiceDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieved {Count} invoices for customer {CustomerId}, page {PageNumber}, size {PageSize}, status: {Status}", totalCount, customerId, pageNumber, pageSize, status ?? "none");
                return OperationResult<PaginatedResult<InvoiceResponseDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer invoices for customer {CustomerId}", context.CustomerId);
                return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("An error occurred while retrieving invoices.");
            }
        }
        public async Task<OperationResult<InvoiceResponseDto>> GetCustomerInvoiceByIdAsync(int id, OperationContext context)
        {
            try
            {
                // Validate CustomerId
                if (!context.CustomerId.HasValue || context.CustomerId <= 0)
                {
                    _logger.LogWarning("Valid Customer ID is required for retrieving invoice {InvoiceId}.", id);
                    return OperationResult<InvoiceResponseDto>.FailureResult("Valid Customer ID is required.");
                }
                int customerId = context.CustomerId.Value;

                //var invoice = await _unitOfWork.Invoices.GetAsync(
                //    i => i.Id == id && !i.IsDeleted && (i.CustomerId == context.CustomerId || i.CompanyId == context.CompanyId),
                //    "Customer,InvoiceItems,TaxDetails,Discounts"
                //);

                var invoice = await _unitOfWork.Invoices.GetAsync(
                      i => i.Id == id && !i.IsDeleted && i.CustomerId == customerId,
                        "Customer,InvoiceItems,TaxDetails,Discounts"
                );

                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or access denied for customer {CustomerId}.", id, customerId);
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or access denied.");
                }

                var invoiceDto = MapToInvoiceResponseDto(invoice);
                _logger.LogInformation("Retrieved invoice {InvoiceId} for customer {CustomerId}.", id, customerId);
                return OperationResult<InvoiceResponseDto>.SuccessResult(invoiceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for customer {CustomerId}", id, context.CustomerId);
                return OperationResult<InvoiceResponseDto>.FailureResult("An error occurred while retrieving the invoice.");
            }
        }
        public async Task<OperationResult<bool>> DeleteAttachmentAsync(int invoiceId, int attachmentId, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for deleting an attachment.");
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Fetch the invoice to ensure it exists and belongs to the company
                var invoice = await _unitOfWork.Invoices.GetAsync(i => i.Id == invoiceId && i.CompanyId == companyId && !i.IsDeleted);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}.", invoiceId, companyId);
                    return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Fetch the attachment
                var attachment = await _unitOfWork.InvoiceAttachments.GetAsync(a => a.Id == attachmentId && a.InvoiceId == invoiceId && !a.IsDeleted);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found for invoice {InvoiceId}.", attachmentId, invoiceId);
                    return OperationResult<bool>.FailureResult("Attachment not found or does not belong to the specified invoice.");
                }

                // Delete file from storage
                var filePath = Path.Combine("wwwroot", attachment.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Perform soft delete
                attachment.IsDeleted = true;
                attachment.UpdatedBy = operationContext.UserId;
                attachment.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.InvoiceAttachments.Update(attachment);
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoiceId,
                    Action = "AttachmentDeleted",
                    Description = $"Attachment {attachment.FileName} deleted",
                    Changes = JsonSerializer.Serialize(new { attachmentId, attachment.FileName }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Attachment {AttachmentId} deleted successfully for invoice {InvoiceId} for company {CompanyId}.",
                    attachmentId, invoiceId, companyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for invoice {InvoiceId} for company {CompanyId}",
                    attachmentId, invoiceId, operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to delete attachment.");
            }
        }

        #region Private Helper Methods
        private static void CalculateItemAmounts(List<InvoiceItem> items, List<TaxType> taxTypes)
        {
            foreach (var item in items)
            {
                // Calculate base amount
                item.Amount = item.Quantity * item.UnitPrice;

                // Calculate tax amount
                if (!string.IsNullOrEmpty(item.TaxType))
                {
                    var taxType = taxTypes.FirstOrDefault(t => t.Name == item.TaxType);
                    if (taxType != null)
                    {
                        item.TaxAmount = (item.Amount * taxType.Rate) / 100;
                        item.TaxPercentage = taxType.Rate;
                    }
                }

                // Calculate total amount
                item.TotalAmount = item.Amount + item.TaxAmount;
                item.IsTaxable = !string.IsNullOrEmpty(item.TaxType);
            }
        }
        private static void CalculateTaxDetails(List<InvoiceItem> items, List<InvoiceTaxDetail> taxDetails, List<TaxType> taxTypes)
        {
            foreach (var taxDetail in taxDetails)
            {
                var taxableAmount = items
                    .Where(item => item.TaxType == taxDetail.TaxName)
                    .Sum(item => item.Amount);

                var taxType = taxTypes.FirstOrDefault(t => t.Name == taxDetail.TaxName);
                if (taxType != null)
                {
                    taxDetail.Rate = taxType.Rate;
                    taxDetail.TaxAmount = (taxableAmount * taxType.Rate) / 100;
                }
            }
        }
        private void CalculateDiscount(List<InvoiceDiscount> discounts, decimal subtotal)
        {
            foreach (var discount in discounts)
            {
                // No calculation needed, just validation
                if (discount.DiscountType == DiscountType.Percentage && discount.Amount > 100)
                {
                    _logger.LogWarning("Percentage discount {Amount}% exceeds 100%", discount.Amount);
                }
            }
        }
        private static void CalculateInvoiceTotals(Invoice invoice)
        {
            // Calculate Subtotal from items
            invoice.Subtotal = invoice.InvoiceItems?.Sum(i => i.Amount) ?? 0;

            // Calculate DiscountTotal 
            invoice.DiscountTotal = invoice.Discounts?.Sum(d =>
            {
                if (d.DiscountType == DiscountType.Percentage)
                {
                    // For percentage discount, Amount is the percentage value
                    return invoice.Subtotal * d.Amount / 100;
                }
                else
                {
                    // For fixed discount, Amount is the fixed amount
                    return d.Amount;
                }
            }) ?? 0;

            // Calculate TaxTotal
            invoice.TaxTotal = invoice.TaxDetails?.Sum(t => t.TaxAmount) ?? 0;

            // Calculate TotalAmount
            invoice.TotalAmount = invoice.Subtotal - invoice.DiscountTotal + invoice.TaxTotal + invoice.ShippingAmount + invoice.AdjustmentAmount;
        }
        private static async Task<List<InvoiceAttachment>> HandleAttachmentsAsync(List<InvoiceAttachmentDto> attachmentDtos, int companyId, int invoiceId, string userId)
        {
            var attachments = new List<InvoiceAttachment>();

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "Invoices",
                companyId.ToString(), invoiceId.ToString());

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var attachmentDto in attachmentDtos.Where(a => a.FileContent != null))
            {
                var file = attachmentDto.FileContent;
                var uniqueFileName = $"{Guid.NewGuid()}_{attachmentDto.FileName}";
                var filePath = Path.Combine(basePath, uniqueFileName);
                var relativePath = $"/Attachments/Invoices/{companyId}/{invoiceId}/{uniqueFileName}";

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new InvoiceAttachment
                {
                    InvoiceId = invoiceId,
                    FileName = attachmentDto.FileName,
                    FilePath = relativePath,
                    FileUrl = relativePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Description = attachmentDto.FileName,
                    IsPublic = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                attachments.Add(attachment);
            }

            return attachments;
        }
        private InvoiceResponseDto MapToInvoiceResponseDto(Invoice invoice)
        {
            var response = _mapper.Map<InvoiceResponseDto>(invoice);

            if (invoice.Customer != null)
            {
                response.Customer = _mapper.Map<CustomerResponseDto>(invoice.Customer);
            }

            // Map additional collections
            if (invoice.Payments != null)
            {
                response.Payments = _mapper.Map<List<InvoicePaymentDto>>(invoice.Payments);
            }

            if (invoice.AuditLogs != null)
            {
                response.AuditLogs = _mapper.Map<List<InvoiceAuditLogDto>>(invoice.AuditLogs);
            }

            // Set calculated fields
            response.IsRecurring = invoice.IsRecurring;

            return response;
        }
        private static string GenerateDefaultEmailMessage(Invoice invoice)
        {
            return $@"
                <html>
                <body>
                    <h3>Invoice {invoice.InvoiceNumber}</h3>
                    <p>Dear {invoice.Customer?.Name},</p>
                    <p>Please find attached invoice {invoice.InvoiceNumber} for your reference.</p>
                    <p><strong>Invoice Details:</strong></p>
                    <ul>
                        <li>Invoice Number: {invoice.InvoiceNumber}</li>
                        <li>Issue Date: {invoice.IssueDate:dd MMM yyyy}</li>
                        <li>Due Date: {invoice.DueDate:dd MMM yyyy}</li>
                        <li>Total Amount: {invoice.TotalAmount:C} {invoice.Currency}</li>
                    </ul>
                    <p>Thank you for your business!</p>
                </body>
                </html>";
        }
        private static decimal CalculateChange(decimal previous, decimal current)
        {
            if (previous == 0) return 0;
            return ((current - previous) / previous) * 100;
        }
        private static InvoiceAuditLog CreateAuditLog(int invoiceId, InvoiceCreateDto dto, Invoice invoice, OperationContext operationContext)
        {
            // Create a summary of the invoice for audit log (reduced size)
            var auditSummary = new
            {
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = dto.CustomerId,
                IssueDate = dto.IssueDate,
                DueDate = dto.DueDate,
                Currency = dto.Currency,
                CurrencyRate = dto.CurrencyRate,
                Subtotal = invoice.Subtotal,
                DiscountTotal = invoice.DiscountTotal,
                TaxTotal = invoice.TaxTotal,
                ShippingAmount = dto.ShippingAmount,
                AdjustmentAmount = dto.AdjustmentAmount,
                TotalAmount = invoice.TotalAmount,
                ItemsCount = dto.Items?.Count ?? 0,
                DiscountsCount = dto.Discounts?.Count ?? 0,
                TaxDetailsCount = dto.TaxDetails?.Count ?? 0,
                IsAutomated = dto.IsAutomated,
                PaymentMethod = dto.PaymentMethod,
                PaymentTerms = dto.PaymentTerms,
                SourceSystem = dto.SourceSystem
            };

            var changesJson = JsonSerializer.Serialize(auditSummary);

            // Ensure we don't exceed the maximum length (2000 characters)
            const int maxLength = 2000;
            if (changesJson.Length > maxLength)
            {
                // Truncate with ellipsis
                changesJson = changesJson.Substring(0, maxLength - 3) + "...";
            }

            return new InvoiceAuditLog
            {
                InvoiceId = invoiceId,
                Action = "Created",
                Description = $"Invoice created by {operationContext.UserId}",
                Changes = changesJson,
                IpAddress = "system",
                UserAgent = "system",
                CreatedBy = operationContext.UserId,
                CreatedDate = DateTime.UtcNow
            };
        }
        private async Task SendNotificationAsync(string groupId, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupId).SendAsync("ReceiveInvoiceNotification", data);
                _logger.LogInformation("Notification sent to group {GroupId}", groupId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification to group {GroupId}", groupId);
            }
        }
        #endregion
    }
}