using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerStats
{
    /// <summary>
    /// Handler for getting customer statistics
    /// </summary>
    public class GetCustomerStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCustomerStatsQueryHandler> logger) : IRequestHandler<GetCustomerStatsQuery, OperationResult<CustomerStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetCustomerStatsQueryHandler> _logger = logger;

        public async Task<OperationResult<CustomerStatsDto>> Handle(
            GetCustomerStatsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                int allCount, activeCount, inactiveCount;
                int historicalAllCount, historicalActiveCount, historicalInactiveCount;
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // ✅ Super Admin: Can see stats across ALL companies
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving customer stats across all companies");

                    allCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted);
                    activeCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted && c.Status == CustomerStatus.Active);
                    inactiveCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted && c.Status != CustomerStatus.Active);

                    historicalAllCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted && c.CreatedDate <= thirtyDaysAgo);
                    historicalActiveCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted && c.Status == CustomerStatus.Active && c.CreatedDate <= thirtyDaysAgo);
                    historicalInactiveCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted && c.Status != CustomerStatus.Active && c.CreatedDate <= thirtyDaysAgo);
                }
                // ✅ Admin/User: Can only see stats for their company
                else if (request.Context.CompanyId.HasValue)
                {
                    var companyId = request.Context.CompanyId.Value;
                    _logger.LogInformation("Admin/User retrieving customer stats for company {CompanyId}", companyId);

                    allCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted);
                    activeCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.Status == CustomerStatus.Active);
                    inactiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.Status != CustomerStatus.Active);

                    historicalAllCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.CreatedDate <= thirtyDaysAgo);
                    historicalActiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.Status == CustomerStatus.Active && c.CreatedDate <= thirtyDaysAgo);
                    historicalInactiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.Status != CustomerStatus.Active && c.CreatedDate <= thirtyDaysAgo);
                }
                else
                {
                    return OperationResult<CustomerStatsDto>.FailureResult("Unable to retrieve customer statistics.");
                }

                // Calculate percentage changes
                double allChange = historicalAllCount > 0 ? ((double)(allCount - historicalAllCount) / historicalAllCount * 100) : 0;
                double activeChange = historicalActiveCount > 0 ? ((double)(activeCount - historicalActiveCount) / historicalActiveCount * 100) : 0;
                double inactiveChange = historicalInactiveCount > 0 ? ((double)(inactiveCount - historicalInactiveCount) / historicalInactiveCount * 100) : 0;

                var stats = new CustomerStatsDto
                {
                    AllCount = allCount,
                    AllChange = Math.Round(allChange, 1),
                    ActiveCount = activeCount,
                    ActiveChange = Math.Round(activeChange, 1),
                    InactiveCount = inactiveCount,
                    InactiveChange = Math.Round(inactiveChange, 1)
                };

                return OperationResult<CustomerStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer stats");
                return OperationResult<CustomerStatsDto>.FailureResult("Failed to retrieve customer statistics.");
            }
        }
    }
}
