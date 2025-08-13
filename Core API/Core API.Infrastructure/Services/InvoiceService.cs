using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Domain.Models.Email;
using Core_API.Application.DTOs.EmailDto;

namespace Core_API.Infrastructure.Services
{
    public class InvoiceService(IUnitOfWork unitOfWork, IPdfService pdfService, IEmailSendingService emailSendingService, ILogger<InvoiceService> logger, IMapper mapper) : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        private readonly IEmailSendingService _emailSendingService = emailSendingService ?? throw new ArgumentNullException(nameof(emailSendingService));
        private readonly ILogger<InvoiceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly string[] _supportedCurrencies = ["USD", "EUR", "INR"];

        public async Task<OperationResult<InvoiceResponseDto>> CreateAsync(InvoiceCreateDto dto, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for creating an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                // Validate customer and company
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

                // Validate invoice type
                if (!Enum.TryParse<InvoiceType>(dto.Type, true, out var invoiceType))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invalid invoice type.");
                }

                // Validate currency
                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                // Validate tax types
                var taxTypes = await _unitOfWork.TaxTypes.Query().Where(t => t.CompanyId == companyId && !t.IsDeleted).ToListAsync();
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
                invoice.InvoiceNumber = dto.IsAutomated ? (await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId)) : dto.InvoiceNumber;
                invoice.CompanyId = companyId;
                //invoice.Status = InvoiceStatus.Draft;
                //invoice.PaymentStatus = PaymentStatus.Pending;
                invoice.CreatedBy = operationContext.UserId;
                invoice.CreatedDate = DateTime.UtcNow;

                // Add invoice items
                invoice.InvoiceItems = dto.Items.Select(item => new InvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Quantity * item.UnitPrice,
                    TaxType = item.TaxType,
                    TaxAmount = item.TaxAmount,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                // Add tax details
                invoice.TaxDetails = dto.TaxDetails.Select(tax => new TaxDetail
                {
                    TaxType = tax.TaxType,
                    Rate = tax.Rate,
                    Amount = tax.Amount,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                // Add discounts
                invoice.Discounts = dto.Discounts.Select(discount => new Discount
                {
                    Description = discount.Description,
                    Amount = discount.Amount,
                    IsPercentage = discount.IsPercentage,
                    CreatedBy = operationContext.UserId,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                // Calculate totals
                invoice.Subtotal = invoice.InvoiceItems.Sum(i => i.Amount);
                invoice.Tax = invoice.TaxDetails.Sum(t => t.Amount);
                var discountAmount = invoice.Discounts.Sum(d => d.IsPercentage ? (invoice.Subtotal * d.Amount / 100) : d.Amount);
                invoice.TotalAmount = invoice.Subtotal + invoice.Tax - discountAmount;

                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                var response = await MapToInvoiceResponseDto(invoice);
                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to create invoice.");
            }
        }
        public async Task<OperationResult<InvoiceResponseDto>> UpdateAsync(InvoiceUpdateDto dto, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for updating an invoice.");
                    return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                var invoice = await _unitOfWork.Invoices.GetAsync(i => i.Id == dto.Id && i.CompanyId == companyId && !i.IsDeleted, "InvoiceItems,TaxDetails,Customer");
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

                // Validate invoice number
                if (!dto.IsAutomated && await _unitOfWork.Invoices.InvoiceNumberExistsAsync(companyId, dto.InvoiceNumber, dto.Id))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice number already exists.");
                }

                // Validate invoice type
                if (!Enum.TryParse<InvoiceType>(dto.Type, true, out var invoiceType))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invalid invoice type.");
                }

                // Validate invoice status
                if (!Enum.TryParse<InvoiceStatus>(dto.InvoiceStatus, true, out var invoiceStatus))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invalid invoice status.");
                }

                // Validate payment status
                if (!Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var paymentStatus))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invalid payment status.");
                }

                if (!_supportedCurrencies.Contains(dto.Currency))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid currency. Supported currencies: {string.Join(", ", _supportedCurrencies)}.");
                }

                var taxTypes = await _unitOfWork.TaxTypes.Query().Where(t => t.CompanyId == companyId && !t.IsDeleted).ToListAsync();
                foreach (var item in dto.Items)
                {
                    if (!string.IsNullOrEmpty(item.TaxType) && !taxTypes.Any(t => t.Name == item.TaxType))
                    {
                        return OperationResult<InvoiceResponseDto>.FailureResult($"Invalid tax type: {item.TaxType}.");
                    }
                }

                if (dto.Discounts.Any(d => d.Amount < 0))
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Discount amount cannot be negative.");
                }

                // Map DTO to entity
                _mapper.Map(dto, invoice);
                invoice.InvoiceNumber = dto.IsAutomated ? (await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId)) : dto.InvoiceNumber;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                // Update invoice items
                invoice.InvoiceItems.Clear();
                invoice.InvoiceItems = _mapper.Map<List<InvoiceItem>>(dto.Items);
                foreach (var item in invoice.InvoiceItems)
                {
                    item.Amount = item.Quantity * item.UnitPrice;
                    item.CreatedBy = operationContext.UserId;
                    item.CreatedDate = DateTime.UtcNow;
                }

                // Update tax details
                invoice.TaxDetails.Clear();
                invoice.TaxDetails = _mapper.Map<List<TaxDetail>>(dto.TaxDetails);
                foreach (var tax in invoice.TaxDetails)
                {
                    tax.CreatedBy = operationContext.UserId;
                    tax.CreatedDate = DateTime.UtcNow;
                }

                // Update discount details
                invoice.Discounts.Clear();
                invoice.Discounts = _mapper.Map<List<Discount>>(dto.Discounts);
                foreach (var discount in invoice.Discounts)
                {
                    discount.CreatedBy = operationContext.UserId;
                    discount.CreatedDate = DateTime.UtcNow;
                }

                // Recalculate totals
                invoice.Subtotal = invoice.InvoiceItems.Sum(i => i.Amount);
                invoice.Tax = invoice.TaxDetails.Sum(t => t.Amount);
                var discountAmount = invoice.Discounts.Sum(d => d.IsPercentage ? (invoice.Subtotal * d.Amount / 100) : d.Amount);
                invoice.TotalAmount = invoice.Subtotal + invoice.Tax - discountAmount;

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<InvoiceResponseDto>(invoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(customer);
                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId} for company {CompanyId}", dto.Id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to update invoice.");
            }
        }
        public async Task<OperationResult<bool>> DeleteAsync(int id, OperationContext operationContext)
        {
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

                invoice.IsDeleted = true;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
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

                var invoice = await _unitOfWork.Invoices.GetAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, "Customer,InvoiceItems,TaxDetails,Discounts");
                if (invoice == null)
                {
                    return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
                }

                var response = _mapper.Map<InvoiceResponseDto>(invoice);
                response.Customer = _mapper.Map<CustomerResponseDto>(invoice.Customer);
                return OperationResult<InvoiceResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Failed to retrieve invoice.");
            }
        }
        //public async Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(OperationContext operationContext, int pageNumber, int pageSize, string search = null, string status = null)
        //{
        //    try
        //    {
        //        // Validate CompanyId
        //        if (!operationContext.CompanyId.HasValue)
        //        {
        //            _logger.LogWarning("Company ID is required for retrieving paged invoices.");
        //            return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Company ID is required.");
        //        }
        //        int companyId = operationContext.CompanyId.Value;

        //        var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, pageNumber, pageSize, search, status);
        //        var mappedItems = new List<InvoiceResponseDto>();
        //        foreach (var invoice in result.Items)
        //        {
        //            var dto = await MapToInvoiceResponseDto(invoice);
        //            mappedItems.Add(dto);
        //        }
        //        var response = new PaginatedResult<InvoiceResponseDto>
        //        {
        //            Items = mappedItems,
        //            TotalCount = result.TotalCount,
        //            PageNumber = result.PageNumber,
        //            PageSize = result.PageSize
        //        };
        //        return OperationResult<PaginatedResult<InvoiceResponseDto>>.SuccessResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving paged invoices for company {CompanyId}", operationContext.CompanyId);
        //        return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Failed to retrieve invoices.");
        //    }
        //}

        public async Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> GetPagedAsync(
       OperationContext operationContext,
       int pageNumber,
       int pageSize,
       string? search = null,
       string? invoiceStatus = null,
       string? paymentStatus = null,
       int? customerId = null,
       int? taxType = null,
       decimal? minAmount = null,
       decimal? maxAmount = null,
       string? invoiceNumberFrom = null,
       string? invoiceNumberTo = null,
       DateTime? issueDateFrom = null,
       DateTime? issueDateTo = null,
       DateTime? dueDateFrom = null,
       DateTime? dueDateTo = null)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving paged invoices.");
                    return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                var result = await _unitOfWork.Invoices.GetPagedAsync(
                    companyId,
                    pageNumber,
                    pageSize,
                    search,
                    invoiceStatus,
                    paymentStatus,
                    customerId,
                    taxType,
                    minAmount,
                    maxAmount,
                    invoiceNumberFrom,
                    invoiceNumberTo,
                    issueDateFrom,
                    issueDateTo,
                    dueDateFrom,
                    dueDateTo);

                var mappedItems = new List<InvoiceResponseDto>();
                foreach (var invoice in result.Items)
                {
                    var dto = await MapToInvoiceResponseDto(invoice);
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
                    return OperationResult<InvoiceSettingsDto>.FailureResult("Invoice settings not found for this company.");
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
                var settings = _mapper.Map<InvoiceSettings>(dto);
                settings.CompanyId = companyId;
                settings.CreatedBy = operationContext.UserId;
                settings.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.InvoiceSettings.SaveAsync(settings);
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
                    Cc = emailData.Cc.Where(e => !string.IsNullOrWhiteSpace(e)).ToList(),
                    Subject = emailData.Subject,
                    HtmlMessage = emailData.Message,
                    InvoiceNumber = invoice.InvoiceNumber,
                    AmountDue = invoice.TotalAmount,
                    DueDate = invoice.PaymentDue
                };

                // Send email
                await _emailSendingService.SendInvoiceEmailAsync(invoiceEmailRequest, operationContext, pdfStream, $"Invoice_{invoice.InvoiceNumber}.pdf");

                // Update invoice status
                invoice.InvoiceStatus = InvoiceStatus.Sent;
                invoice.PaymentStatus = invoice.PaymentStatus == PaymentStatus.Completed || invoice.PaymentStatus == PaymentStatus.Refunded ? invoice.PaymentStatus : PaymentStatus.Pending;
                invoice.UpdatedBy = operationContext.UserId;
                invoice.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {InvoiceId} sent successfully to {Email} for company {CompanyId}", id, invoiceEmailRequest.Email, companyId);
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
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving invoice stats.");
                    return OperationResult<InvoiceStatsDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;

                var invoices = await _unitOfWork.Invoices.Query()
                    .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                    .ToListAsync();

                var stats = new InvoiceStatsDto
                {
                    All = new StatsItem
                    {
                        Count = invoices.Count,
                        Amount = invoices.Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices)
                    },
                    Draft = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Draft),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Draft).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Draft).ToList())
                    },
                    Sent = new StatsItem // Same as Open
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Sent),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Sent).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Sent).ToList())
                    },
                    Approved = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Approved),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Approved).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Approved).ToList())
                    },
                    Cancelled = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Cancelled),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Cancelled).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Cancelled).ToList())
                    },
                    Pending = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Pending),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Pending).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.Pending).ToList())
                    },
                    Processing = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Processing),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Processing).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.Processing).ToList())
                    },
                    Completed = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Completed),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Completed).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.Completed).ToList())
                    },
                    PartiallyPaid = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.PartiallyPaid),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.PartiallyPaid).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.PartiallyPaid).ToList())
                    },
                    Overdue = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Overdue),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Overdue).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.Overdue).ToList())
                    },
                    Refunded = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Refunded),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Refunded).Sum(i => i.TotalAmount),
                        Change = CalculateChange(invoices.Where(i => i.PaymentStatus == PaymentStatus.Refunded).ToList())
                    }
                };

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

                // customers need to access company-wide invoices
                //var query = _unitOfWork.Invoices.Query()
                //    .Where(i => !i.IsDeleted && (i.CustomerId == context.CustomerId || (i.CompanyId == context.CompanyId && context.CompanyId != 0)))
                //    .Include(i => i.Customer)
                //    .Include(i => i.Company)
                //    .Include(i => i.InvoiceItems)
                //    .Include(i => i.TaxDetails)
                //    .Include(i => i.Discounts);

                var query = _unitOfWork.Invoices.Query()
                    .Where(i => !i.IsDeleted && i.CustomerId == customerId)
                    .Include(i => i.Customer)
                    .Include(i => i.Company)
                    .Include(i => i.InvoiceItems)
                    .Include(i => i.TaxDetails)
                    .Include(i => i.Discounts);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
                {
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Invoice, List<Discount>>)query.Where(i => i.InvoiceStatus == invoiceStatus);
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
                    var dto = await MapToInvoiceResponseDto(invoice);
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

                var invoiceDto = await MapToInvoiceResponseDto(invoice);
                _logger.LogInformation("Retrieved invoice {InvoiceId} for customer {CustomerId}.", id, customerId);
                return OperationResult<InvoiceResponseDto>.SuccessResult(invoiceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for customer {CustomerId}", id, context.CustomerId);
                return OperationResult<InvoiceResponseDto>.FailureResult("An error occurred while retrieving the invoice.");
            }
        }
        private static decimal CalculateChange(List<Invoice> invoices)
        {
            // Placeholder: Calculate percentage change (e.g., compared to previous period)
            // This is a simplified example; replace with actual business logic
            return invoices.Count != 0 ? 0 : 0;
        }
        private async Task<InvoiceResponseDto> MapToInvoiceResponseDto(Invoice invoice)
        {
            var response = _mapper.Map<InvoiceResponseDto>(invoice);
            var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == invoice.CustomerId && c.CompanyId == invoice.CompanyId && !c.IsDeleted);
            response.Customer = _mapper.Map<CustomerResponseDto>(customer);
            return response;
        }
    }
}