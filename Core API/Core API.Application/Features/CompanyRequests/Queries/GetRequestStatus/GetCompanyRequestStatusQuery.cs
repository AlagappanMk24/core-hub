using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Companies.Responses;

namespace Core_API.Application.Features.CompanyRequests.Queries.GetRequestStatus
{
    public record GetCompanyRequestStatusQuery : BaseQuery<List<RequestStatusResponseDto>>
    {
        public string Email { get; init; } = string.Empty;
    }
}