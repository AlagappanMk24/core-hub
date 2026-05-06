using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Companies.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.CompanyRequests.Queries.GetRequestStatus
{
    public class GetCompanyRequestStatusQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCompanyRequestStatusQueryHandler> logger)
        : IRequestHandler<GetCompanyRequestStatusQuery, OperationResult<List<RequestStatusResponseDto>>>
    {
        public async Task<OperationResult<List<RequestStatusResponseDto>>> Handle(
            GetCompanyRequestStatusQuery request, CancellationToken cancellationToken)
        {
            var requests = await unitOfWork.CompanyRequests.GetRequestsByEmailAsync(request.Email);

            var response = requests.Select(r => new RequestStatusResponseDto
            {
                Id = r.Id,
                CompanyName = r.CompanyName,
                RequestedAt = r.RequestedAt,
                Status = r.Status.ToString(),
                ProcessedAt = r.ProcessedAt,
                RejectionReason = r.RejectionReason
            }).ToList();

            return OperationResult<List<RequestStatusResponseDto>>.SuccessResult(response);
        }
    }
}