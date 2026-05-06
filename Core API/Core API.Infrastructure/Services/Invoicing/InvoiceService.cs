using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Application.Contracts.Services.Exchange;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.Contracts.Services.Invoices;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using Core_API.Domain.Models.Email;
using Core_API.Infrastructure.RealTime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.Services.Invoicing
{
    public class InvoiceService(IUnitOfWork unitOfWork, IInvoiceNumberService invoiceNumberService, IInvoiceCalculationService invoiceCalculationService, IInvoiceAttachmentService invoiceAttachmentService, IInvoiceEmailService invoiceEmailService, IExchangeRateService exchangeRateService, ILogger<InvoiceService> logger, IMapper mapper, IHubContext<NotificationHub> hubContext) : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IInvoiceNumberService _invoiceNumberService = invoiceNumberService ?? throw new ArgumentNullException(nameof(invoiceNumberService));
        private readonly IInvoiceCalculationService _invoiceCalculationService = invoiceCalculationService ?? throw new ArgumentNullException(nameof(invoiceCalculationService));
        private readonly IInvoiceAttachmentService _invoiceAttachmentService = invoiceAttachmentService ?? throw new ArgumentNullException(nameof(invoiceAttachmentService));
        private readonly IInvoiceEmailService _invoiceEmailService = invoiceEmailService ?? throw new ArgumentNullException(nameof(invoiceEmailService));
        private readonly IExchangeRateService _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        private readonly ILogger<InvoiceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly string[] _supportedCurrencies = { "USD", "EUR", "INR" };
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        public async Task<OperationResult<InvoiceResponseDto>> CreateAsync(CreateInvoiceDto dto, OperationContext operationContext)
        {
            // Validate CompanyId
            if (!operationContext.CompanyId.HasValue)
            {
                _logger.LogWarning("Company ID is required for creating an invoice.");
                return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
            }
            int companyId = operationContext.CompanyId.Value;

            // Generate invoice number BEFORE transaction
            string invoiceNumber;
            if (dto.IsAutomated)
            {
                invoiceNumber = await _invoiceNumberService.GenerateUniqueInvoiceNumberAsync(companyId);
            }
            else
            {
                invoiceNumber = dto.InvoiceNumber ?? $"INV{DateTime.UtcNow.Ticks}";
            }

            // Start transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get company to get base currency
                var company = await _unitOfWork.Companies.GetAsync(c => c.Id == companyId);
                if (company == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company not found.");
                }

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted, tracked: true);
                if (customer == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Validate invoice number
                if (!dto.IsAutomated)
                {
                    if (string.IsNullOrWhiteSpace(invoiceNumber))
                        return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number is required for non-automated creation.");

                    if (await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, invoiceNumber))
                        return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number already exists.");
                }
                // Validate currency
                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                // Fetch current exchange rate from API
                decimal exchangeRate = await _exchangeRateService.GetExchangeRateAsync(company.BaseCurrency, dto.Currency);
                if (exchangeRate <= 0)
                {
                    _logger.LogError("Invalid exchange rate returned: {Rate} for {Base} to {Target}", exchangeRate, company.BaseCurrency, dto.Currency);
                    return OperationResult<InvoiceResponseDto>.FailureResult("Unable to process invoice: Invalid exchange rate received.");
                }
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
                var invoice = _mapper.Map<Domain.Entities.Invoices.Invoice>(dto);

                invoice.InvoiceNumber = invoiceNumber;
                invoice.CompanyId = companyId;
                invoice.CreatedBy = operationContext.UserId;
                invoice.CreatedDate = DateTime.UtcNow;
                invoice.InvoiceStatus = InvoiceStatus.Draft;
                invoice.PaymentStatus = PaymentStatus.Pending;
                invoice.AmountPaid = 0;
                invoice.AmountRefunded = 0;
                invoice.AmountDue = 0;
                invoice.SourceSystem = dto.SourceSystem ?? "Manual";

                invoice.Customer = customer;

                // Calculate item amounts and tax amounts
                _invoiceCalculationService.CalculateItemAmounts(invoice.InvoiceItems, taxTypes);

                // Calculate tax details from items
                _invoiceCalculationService.CalculateTaxDetails(invoice.InvoiceItems, invoice.TaxDetails, taxTypes);

                // Calculate discounts
                _invoiceCalculationService.CalculateDiscount(invoice.Discounts, invoice.Subtotal);

                // Calculate all totals
                _invoiceCalculationService.CalculateInvoiceTotals(invoice);

                // Convert to base currency for reporting
                invoice.BaseCurrencySubtotal = invoice.Subtotal / exchangeRate;
                invoice.BaseCurrencyTotalAmount = invoice.TotalAmount / exchangeRate;

                // Calculate AmountDue
                invoice.AmountDue = _invoiceCalculationService.CalculateAmountDue(invoice.TotalAmount, invoice.AmountPaid);
                invoice.BaseCurrencyAmountDue = invoice.BaseCurrencyTotalAmount - invoice.BaseCurrencyAmountPaid;

                // Add invoice 
                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Handle attachments after invoice has ID
                if (dto.Attachments?.Any() == true)
                {
                    invoice.InvoiceAttachments = await _invoiceAttachmentService.HandleAttachmentsAsync(dto.Attachments, companyId, invoice.Id, operationContext.UserId);
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
                if (_hubContext != null && invoice.Customer != null)
                {
                    var customerGroupId = customer.Id.ToString();
                    await SendNotificationAsync(customerGroupId, new
                    {
                        InvoiceId = invoice.Id,
                        invoice.InvoiceNumber,
                        invoice.Currency,
                        Message = $"New invoice {invoice.InvoiceNumber} has been created",
                        Amount = invoice.TotalAmount,
                        DueDate = invoice.DueDate.ToString("dd MMM yyyy"),
                        CustomerName = customer.Name,
                        Timestamp = DateTime.UtcNow
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
        public async Task<OperationResult<InvoiceResponseDto>> UpdateAsync(UpdateInvoiceDto dto, OperationContext operationContext)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                // Super Admin can update any invoice
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin updating invoice {InvoiceId}", dto.Id);
                    // No company filter for Super Admin
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }

                // Get existing invoice with all related data
                var invoice = await query
                   .Include(i => i.InvoiceItems)
                   .Include(i => i.TaxDetails)
                   .Include(i => i.Discounts)
                   .Include(i => i.InvoiceAttachments)
                   .Include(i => i.Customer)
                   .FirstOrDefaultAsync(i => i.Id == dto.Id);

                if (invoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }

                int companyId = invoice.CompanyId;

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId && !c.IsDeleted, tracked:true);
                if (customer == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }
                invoice.Customer = customer;
                // Validate invoice number uniqueness
                if (!dto.IsAutomated)
                {
                    if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
                        return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number is required for non-automated creation.");

                    if (await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, dto.InvoiceNumber))
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
                _invoiceCalculationService.CalculateItemAmounts(mappedItems, taxTypes);
                foreach (var item in mappedItems)
                {
                    item.CreatedBy = operationContext.UserId;
                    item.CreatedDate = DateTime.UtcNow;
                    invoice.InvoiceItems.Add(item);
                }

                // Update tax details
                invoice.TaxDetails.Clear();
                var mappedTaxDetails = _mapper.Map<List<InvoiceTaxDetail>>(dto.TaxDetails);
                _invoiceCalculationService.CalculateTaxDetails(invoice.InvoiceItems, mappedTaxDetails, taxTypes);
                foreach (var tax in mappedTaxDetails)
                {
                    tax.CreatedBy = operationContext.UserId;
                    tax.CreatedDate = DateTime.UtcNow;
                    invoice.TaxDetails.Add(tax);
                }

                // Update discount details
                invoice.Discounts.Clear();
                var mappedDiscounts = _mapper.Map<List<InvoiceDiscount>>(dto.Discounts);
                _invoiceCalculationService.CalculateDiscount(mappedDiscounts, invoice.Subtotal);
                foreach (var discount in mappedDiscounts)
                {
                    discount.CreatedBy = operationContext.UserId;
                    discount.CreatedDate = DateTime.UtcNow;
                    invoice.Discounts.Add(discount);
                }

                // Recalculate totals
                _invoiceCalculationService.CalculateInvoiceTotals(invoice);
                invoice.AmountDue = _invoiceCalculationService.CalculateAmountDue(invoice.TotalAmount, invoice.AmountPaid);

                // Handle attachments
                invoice.InvoiceAttachments.Clear();

                if (dto.Attachments?.Any() == true)
                {
                    var existingAttachmentIds = invoice.InvoiceAttachments.Select(a => a.Id).ToHashSet();

                    var newAttachments = dto.Attachments.Where(a => a.FileContent != null).ToList();

                    if (newAttachments.Any())
                    {
                        var addedAttachments = await _invoiceAttachmentService.HandleAttachmentsAsync(
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
                        var customerGroupId = customer.Id.ToString();
                        await SendNotificationAsync(customerGroupId, new
                        {
                            InvoiceId = invoice.Id,
                            invoice.InvoiceNumber,
                            Message = $"Invoice {invoice.InvoiceNumber} has been updated. Amount due: {invoice.Currency} {invoice.TotalAmount:N2}",
                            Amount = invoice.TotalAmount,
                            invoice.Currency,
                            DueDate = invoice.DueDate.ToString("dd MMM yyyy"),
                            CustomerName = customer.Name,
                            Timestamp = DateTime.UtcNow
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
                IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                // Super Admin can delete any invoice
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin deleting invoice {InvoiceId}", id);
                    // No company filter for Super Admin
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    return OperationResult<bool>.FailureResult("Company ID is required.");
                }

                var invoice = await query
                    .Include(i => i.InvoiceAttachments)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return OperationResult<bool>.FailureResult("Invoice not found.");
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
                                invoice.InvoiceNumber,
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
                IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
                    .Where(i => !i.IsDeleted);

                // Super Admin can access any invoice
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving invoice {InvoiceId}", id);
                    // No company filter for Super Admin
                }
                // Company user (Admin/User) - must have CompanyId
                else if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    _logger.LogWarning("Company ID is required for non-super admin users.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }

                // Apply customer filter if user is a customer
                if (operationContext.CustomerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }

                var invoice = await query
                     .Include(i => i.Customer)
                     .Include(i => i.InvoiceItems)
                     .Include(i => i.TaxDetails)
                     .Include(i => i.Discounts)
                     .Include(i => i.InvoiceAttachments)
                     .Include(i => i.Payments)
                     .Include(i => i.AuditLogs)
                     .FirstOrDefaultAsync(i => i.Id == id);

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
                int? companyId = null;
                if (!operationContext.IsSuperAdmin)
                {
                    if (!operationContext.CompanyId.HasValue)
                        return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Company ID is required.");

                    companyId = operationContext.CompanyId.Value;
                }

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
        public async Task<OperationResult<InvoiceResponseDto>> DuplicateAsync(int id, OperationContext operationContext)
        {
            // 1️. Fetch the original invoice (without company filter for Super Admin)
            var invoiceQuery = _unitOfWork.Invoices.Query()
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts)
                .Include(i => i.InvoiceAttachments)
                .Include(i => i.Customer);

            Domain.Entities.Invoices.Invoice? originalInvoice = null;

            // ✅ Super Admin can duplicate any invoice across all companies
            if (operationContext.IsSuperAdmin)
            {
                _logger.LogInformation("Super Admin attempting to duplicate invoice {InvoiceId}", id);
                originalInvoice = await invoiceQuery.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

                if (originalInvoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found for Super Admin duplication", id);
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found.");
                }
            }
            else
            {
                // Regular user must have CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for duplicating an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                originalInvoice = await invoiceQuery.FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted);

                if (originalInvoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}", id, companyId);
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }
            }

            // Get the company ID from the original invoice (needed for settings and attachment paths)
            int targetCompanyId = originalInvoice.CompanyId;

            // 2. Generate duplicate invoice number BEFORE transaction
            var duplicateInvoiceNumber = await _invoiceNumberService.GenerateDuplicateInvoiceNumberAsync(targetCompanyId, originalInvoice);

            // 3. Start transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 4. Create a deep copy of the invoice
                var duplicatedInvoice = new Domain.Entities.Invoices.Invoice
                {
                    // Copy basic properties (excluding ID and system-generated fields)
                    InvoiceNumber = duplicateInvoiceNumber,
                    IssueDate = DateTime.UtcNow,
                    DueDate = _invoiceCalculationService.CalculateDueDateFromPaymentTerms(DateTime.UtcNow, originalInvoice.PaymentTerms),
                    PONumber = originalInvoice.PONumber,
                    ProjectDetail = originalInvoice.ProjectDetail,
                    InvoiceType = originalInvoice.InvoiceType,
                    Currency = originalInvoice.Currency,
                    CurrencyRate = originalInvoice.CurrencyRate,
                    CustomerId = originalInvoice.CustomerId,
                    CustomerNotes = originalInvoice.CustomerNotes,
                    InternalNotes = originalInvoice.InternalNotes,
                    TermsAndConditions = originalInvoice.TermsAndConditions,
                    FooterNote = originalInvoice.FooterNote,
                    PaymentMethod = originalInvoice.PaymentMethod,
                    PaymentTerms = originalInvoice.PaymentTerms,
                    ShippingAmount = originalInvoice.ShippingAmount,
                    AdjustmentAmount = originalInvoice.AdjustmentAmount,
                    AdjustmentDescription = originalInvoice.AdjustmentDescription,

                    // Set status as Draft (not sent)
                    InvoiceStatus = InvoiceStatus.Draft,
                    PaymentStatus = PaymentStatus.Pending,

                    // Reset payment-related fields
                    AmountPaid = 0,
                    AmountRefunded = 0,
                    AmountDue = 0,
                    SentDate = null,
                    PaidDate = null,
                    PaymentGateway = null,
                    PaymentTransactionId = null,

                    // Set system fields
                    CompanyId = targetCompanyId,
                    IsAutomated = originalInvoice.IsAutomated,
                    SourceSystem = "Duplicate",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                // 5. Copy line items
                foreach (var item in originalInvoice.InvoiceItems)
                {
                    duplicatedInvoice.InvoiceItems.Add(new InvoiceItem
                    {
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TaxType = item.TaxType,
                        TaxPercentage = item.TaxPercentage,
                        TaxAmount = item.TaxAmount,
                        Amount = item.Amount,
                        TotalAmount = item.TotalAmount,
                        IsTaxable = item.IsTaxable,
                        CreatedBy = operationContext.UserId,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // 6. Copy tax details
                foreach (var tax in originalInvoice.TaxDetails)
                {
                    duplicatedInvoice.TaxDetails.Add(new InvoiceTaxDetail
                    {
                        TaxName = tax.TaxName,
                        Rate = tax.Rate,
                        TaxAmount = tax.TaxAmount,
                        CreatedBy = operationContext.UserId,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // 7. Copy discounts
                foreach (var discount in originalInvoice.Discounts)
                {
                    duplicatedInvoice.Discounts.Add(new InvoiceDiscount
                    {
                        Description = discount.Description,
                        DiscountType = discount.DiscountType,
                        Amount = discount.Amount,
                        CreatedBy = operationContext.UserId,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // 8. Calculate all financial totals
                _invoiceCalculationService.CalculateInvoiceTotals(duplicatedInvoice);
                duplicatedInvoice.AmountDue = duplicatedInvoice.TotalAmount - duplicatedInvoice.AmountPaid;

                // 9. Handle attachments (copy files if needed)
                if (originalInvoice.InvoiceAttachments.Any())
                {
                    duplicatedInvoice.InvoiceAttachments = _invoiceAttachmentService.CopyAttachments(
                        originalInvoice.InvoiceAttachments,
                        targetCompanyId,
                        operationContext.UserId);
                }

                // 10. Save duplicated invoice
                await _unitOfWork.Invoices.AddAsync(duplicatedInvoice);
                await _unitOfWork.SaveChangesAsync();

                // 11. Update attachment InvoiceIds after save
                if (duplicatedInvoice.InvoiceAttachments.Any())
                {
                    foreach (var attachment in duplicatedInvoice.InvoiceAttachments)
                    {
                        attachment.InvoiceId = duplicatedInvoice.Id;
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 12. Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = duplicatedInvoice.Id,
                    Action = "Duplicated",
                    Description = $"Invoice duplicated from original invoice {originalInvoice.InvoiceNumber} (ID: {originalInvoice.Id}) by {operationContext.UserId}",
                    Changes = JsonSerializer.Serialize(new
                    {
                        OriginalInvoiceId = originalInvoice.Id,
                        OriginalInvoiceNumber = originalInvoice.InvoiceNumber,
                        DuplicatedDate = DateTime.UtcNow
                    }),
                    IpAddress = "system",
                    UserAgent = "system",
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.InvoiceAuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // 13. Map to response DTO
                var response = MapToInvoiceResponseDto(duplicatedInvoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(originalInvoice.Customer);

                _logger.LogInformation("Invoice {OriginalInvoiceNumber} duplicated successfully as {NewInvoiceNumber} (ID: {NewInvoiceId})",
                    originalInvoice.InvoiceNumber, duplicatedInvoice.InvoiceNumber, duplicatedInvoice.Id);

                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error duplicating invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to duplicate invoice: " + ex.Message);
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
                    "InvoiceItems,TaxDetails,Discounts,Customer", tracked: true
                );
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found or does not belong to company {CompanyId}", id, companyId);
                    return OperationResult<bool>.FailureResult("Invoice not found or does not belong to your company.");
                }

                // Log invoice details for debugging
                _logger.LogDebug("Fetched invoice: Id={Id}, CustomerId={CustomerId}, InvoiceNumber={InvoiceNumber}, DueDate={DueDate}, TotalAmount={TotalAmount}",
                    invoice.Id, invoice.CustomerId, invoice.InvoiceNumber, invoice.DueDate, invoice.TotalAmount);

                // Validate customer
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == invoice.CustomerId && c.CompanyId == companyId && !c.IsDeleted, tracked:true);
                if (customer == null)
                {
                    return OperationResult<bool>.FailureResult("Invoice has an invalid CustomerId.");
                }

                // Send email
                await _invoiceEmailService.SendInvoiceEmailAsync(invoice, emailData, operationContext);

                // Update invoice
                invoice.SentDate = DateTime.UtcNow;
                invoice.InvoiceStatus = InvoiceStatus.Sent;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                // ✅ Save changes - EF will detect changes automatically
                await _unitOfWork.SaveChangesAsync();

                // Create audit log
                var auditLog = new InvoiceAuditLog
                {
                    InvoiceId = invoice.Id,
                    Action = "Sent",
                    Description = $"Invoice sent to {string.Join(", ", emailData.To)}",
                    Changes = JsonSerializer.Serialize(new { SentDate = DateTime.UtcNow, Recipients = emailData.To }),
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
                    if (_hubContext != null && invoice.Customer != null)
                    {
                        var customerGroupId = customer.Id.ToString();
                        await SendNotificationAsync(customerGroupId, new
                        {
                            InvoiceId = invoice.Id,
                            invoice.InvoiceNumber,
                            Message = $"Invoice {invoice.InvoiceNumber} has been sent to you. Amount due: {invoice.Currency} {invoice.TotalAmount:N2}",
                            Amount = invoice.TotalAmount,
                            invoice.Currency,
                            DueDate = invoice.DueDate.ToString("dd MMM yyyy"),
                            CustomerName = customer.Name,
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    _logger.LogInformation("Sent SignalR notification for invoice {InvoiceId} to customer {CustomerId}", id, invoice.CustomerId);

                }
                catch (Exception signalREx)
                {
                    _logger.LogWarning(signalREx, "Failed to send SignalR notification for invoice {InvoiceId} to customer {CustomerId}", id, invoice.CustomerId);
                }

                _logger.LogInformation("Invoice {InvoiceId} sent successfully to {Email} for company {CompanyId}", id, emailData.To.FirstOrDefault(), companyId);
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to send invoice.");
            }
        }
        public InvoiceResponseDto MapToInvoiceResponseDto(Domain.Entities.Invoices.Invoice invoice)
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

        #region Private Helper Methods
        private static InvoiceAuditLog CreateAuditLog(int invoiceId, CreateInvoiceDto dto, Domain.Entities.Invoices.Invoice invoice, OperationContext operationContext)
        {
            // Create a summary of the invoice for audit log (reduced size)
            var auditSummary = new
            {
                invoice.InvoiceNumber,
                dto.CustomerId,
                dto.IssueDate,
                dto.DueDate,
                dto.Currency,
                dto.CurrencyRate,
                invoice.Subtotal,
                invoice.DiscountTotal,
                invoice.TaxTotal,
                dto.ShippingAmount,
                dto.AdjustmentAmount,
                invoice.TotalAmount,
                ItemsCount = dto.Items?.Count ?? 0,
                DiscountsCount = dto.Discounts?.Count ?? 0,
                TaxDetailsCount = dto.TaxDetails?.Count ?? 0,
                dto.IsAutomated,
                dto.PaymentMethod,
                dto.PaymentTerms,
                dto.SourceSystem
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