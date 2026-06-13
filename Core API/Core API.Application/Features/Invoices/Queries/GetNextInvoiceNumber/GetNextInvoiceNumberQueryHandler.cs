using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetNextInvoiceNumber;

/// <summary>
/// Handler for GetNextInvoiceNumberQuery
/// </summary>
public sealed class GetNextInvoiceNumberQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetNextInvoiceNumberQueryHandler> logger) : IRequestHandler<GetNextInvoiceNumberQuery, OperationResult<string>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GetNextInvoiceNumberQueryHandler> _logger = logger;

    public async Task<OperationResult<string>> Handle(
        GetNextInvoiceNumberQuery request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        try
        {
            _logger.LogInformation("Retrieving next invoice number for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}",
                context.UserId, context.IsSuperAdmin, context.CompanyId);

            // Validate CompanyId - Required for non-super admin users
            if (!context.IsSuperAdmin && !context.CompanyId.HasValue)
            {
                _logger.LogWarning("Company ID is required for retrieving next invoice number. UserId: {UserId}", context.UserId);
                return OperationResult<string>.FailureResult("Company ID is required.");
            }

            // For Super Admin, we need to determine which company they want to get the next number for
            // Since Super Admin may have access to multiple companies, we'll need companyId from context or a parameter
            // For now, we'll check if CompanyId is provided in context
            if (context.IsSuperAdmin && !context.CompanyId.HasValue)
            {
                _logger.LogWarning("Super Admin attempting to get next invoice number without CompanyId. UserId: {UserId}", context.UserId);
                return OperationResult<string>.FailureResult("Company ID is required to generate next invoice number.");
            }

            var companyId = context.CompanyId!.Value;

            // Get the current settings to check if automated numbering is enabled
            var settings = await _unitOfWork.InvoiceSettings.GetByCompanyIdAsync(companyId);

            if (settings == null || !settings.IsAutomated)
            {
                _logger.LogWarning("Automated numbering is not enabled for company {CompanyId}", companyId);
                return OperationResult<string>.FailureResult("Automated numbering is not enabled for this company.");
            }

            // Get the next invoice number
            var nextNumber = await _unitOfWork.InvoiceSettings.GetNextInvoiceNumberAsync(companyId);

            _logger.LogInformation("Next invoice number retrieved successfully for company {CompanyId}: {NextNumber}",
                companyId, nextNumber);

            return OperationResult<string>.SuccessResult(nextNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving next invoice number for UserId: {UserId}, CompanyId: {CompanyId}",
                context.UserId, context.CompanyId);
            return OperationResult<string>.FailureResult("Failed to retrieve next invoice number: " + ex.Message);
        }
    }
}