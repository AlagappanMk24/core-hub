using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;

/// <summary>
/// Handler for GetPagedInvoicesQuery
/// </summary>
public class GetPagedInvoicesQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetPagedInvoicesQueryHandler> logger) : IRequestHandler<GetPagedInvoicesQuery, OperationResult<PaginatedResult<InvoiceResponseDto>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<GetPagedInvoicesQueryHandler> _logger = logger;

    public async Task<OperationResult<PaginatedResult<InvoiceResponseDto>>> Handle(
        GetPagedInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Determine company ID based on user role
            int? companyId = null;

            if (request.Context.IsSuperAdmin)
            {
                // ✅ SUPER ADMIN: Return invoices from ALL companies
                _logger.LogInformation("Super Admin retrieving paged invoices from ALL companies. Page: {PageNumber}, Size: {PageSize}, Search: {Search}, Status: {Status}",
                    request.PageNumber, request.PageSize, request.Search ?? "none", request.InvoiceStatus ?? "none");

                // companyId stays NULL to get invoices from all companies
            }
            else if (request.Context.IsCustomerUser)
            {
                // ✅ CUSTOMER: Only see their own invoices
                _logger.LogInformation("Customer retrieving own invoices. CustomerId: {CustomerId}, Page: {PageNumber}, Size: {PageSize}",
                    request.Context.CustomerId, request.PageNumber, request.PageSize);  
                request.CustomerId = request.Context.CustomerId;
                // companyId stays NULL for customer
            }
            else
            {
                // ✅ REGULAR USER (Company Admin/User): Only see their company's invoices
                if (!request.Context.CompanyId.HasValue)
                {
                    return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult(
                        "Company ID is required for non-admin users."
                    );
                }

                companyId = request.Context.CompanyId.Value;

                _logger.LogInformation("Company user retrieving invoices. CompanyId: {CompanyId}, Page: {PageNumber}, Size: {PageSize}, Search: {Search}, Status: {Status}",
                    companyId, request.PageNumber, request.PageSize, request.Search ?? "none", request.InvoiceStatus ?? "none");
            }

            // Get paged data from repository
            var result = await _unitOfWork.Invoices.GetPagedAsync(companyId, request, cancellationToken);

            // Map entities to DTOs
            var mappedItems = _mapper.Map<List<InvoiceResponseDto>>(result.Items);

            var response = new PaginatedResult<InvoiceResponseDto>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            // ✅ Add role information to response for debugging (optional)
            var roleInfo = request.Context.IsSuperAdmin ? "SuperAdmin" :
                          request.Context.IsCustomerUser ? "Customer" : "CompanyUser";

            _logger.LogInformation("Successfully retrieved {Count} invoices out of {TotalCount} total for {Role}",
                mappedItems.Count, result.TotalCount, roleInfo);

            return OperationResult<PaginatedResult<InvoiceResponseDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged invoices for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}",
                request.Context.UserId, request.Context.IsSuperAdmin);
            return OperationResult<PaginatedResult<InvoiceResponseDto>>.FailureResult("Failed to retrieve invoices.");
        }
    }
}