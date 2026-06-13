using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceById;

/// <summary>
/// Handler for GetInvoiceByIdQuery
/// </summary>
public sealed class GetInvoiceByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetInvoiceByIdQueryHandler> logger) : IRequestHandler<GetInvoiceByIdQuery, OperationResult<InvoiceResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger = logger;

    public async Task<OperationResult<InvoiceResponseDto>> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        try
        {
            _logger.LogInformation("Retrieving invoice {InvoiceId} for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}",
                request.Id, context.UserId, context.IsSuperAdmin, context.CompanyId);

            // Build query with proper filtering
            IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query()
                .Where(i => !i.IsDeleted);

            // Super Admin can access any invoice (no company filter)
            if (context.IsSuperAdmin)
            {
                _logger.LogInformation("Super Admin retrieving invoice {InvoiceId}", request.Id);
                // No company filter for Super Admin
            }
            // Company user (Admin/User) - must have CompanyId
            else if (context.CompanyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == context.CompanyId.Value);
            }
            else
            {
                _logger.LogWarning("Company ID is required for non-super admin users.");
                return OperationResult<InvoiceResponseDto>.FailureResult("Company ID is required.");
            }

            // Apply customer filter if user is a customer
            if (context.CustomerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == context.CustomerId.Value);
            }

            // Include all related data
            var invoice = await query
                .Include(i => i.Customer)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts)
                .Include(i => i.InvoiceAttachments)
                .Include(i => i.Payments)
                .Include(i => i.AuditLogs)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found", request.Id);
                return OperationResult<InvoiceResponseDto>.FailureResult("Invoice not found or does not belong to your company.");
            }

            // Additional security check for customer users
            if (context.CustomerId.HasValue && invoice.CustomerId != context.CustomerId.Value)
            {
                _logger.LogWarning("Customer {CustomerId} attempted to access invoice {InvoiceId} belonging to customer {InvoiceCustomerId}",
                    context.CustomerId, request.Id, invoice.CustomerId);
                return OperationResult<InvoiceResponseDto>.FailureResult("Access denied to this invoice.");
            }

            // Map to response DTO
            var response = _mapper.Map<InvoiceResponseDto>(invoice);

            _logger.LogInformation("Invoice {InvoiceId} retrieved successfully for UserId: {UserId}",
                invoice.Id, context.UserId);

            return OperationResult<InvoiceResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for UserId: {UserId}, CompanyId: {CompanyId}",
                request.Id, context.UserId, context.CompanyId);
            return OperationResult<InvoiceResponseDto>.FailureResult("Failed to retrieve invoice: " + ex.Message);
        }
    }
}