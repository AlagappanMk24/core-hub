using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.Contracts.Services.Invoices;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Invoicing.CustomerInvoice
{
    public class CustomerInvoiceService(IUnitOfWork unitOfWork,IInvoiceService invoiceService, ILogger<CustomerInvoiceService> logger, IMapper mapper) : ICustomerInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IInvoiceService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));   
        private readonly ILogger<CustomerInvoiceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

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
                    var dto = _invoiceService.MapToInvoiceResponseDto(invoice);
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

                var invoiceDto = _invoiceService.MapToInvoiceResponseDto(invoice);
                _logger.LogInformation("Retrieved invoice {InvoiceId} for customer {CustomerId}.", id, customerId);
                return OperationResult<InvoiceResponseDto>.SuccessResult(invoiceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for customer {CustomerId}", id, context.CustomerId);
                return OperationResult<InvoiceResponseDto>.FailureResult("An error occurred while retrieving the invoice.");
            }
        }
    }
}