using Core_API.Application.DTOs.Companies.Requests;
using Core_API.Application.DTOs.Companies.Responses;
namespace Core_API.Application.Contracts.Services.Companies
{
    public interface ICompanyRequestService
    {
        // Public endpoints
        Task<(bool Success, string Message, int? RequestId)> CreateRequestAsync(CreateCompanyRequestDto dto);
        Task<List<RequestStatusResponseDto>> GetRequestStatusAsync(string email);

        // Admin endpoints
        Task<CompanyRequestListResponseDto> GetPagedRequestsAsync(int page, int pageSize, string status, string search);
        Task<CompanyRequestResponseDto> GetRequestByIdAsync(int id);
        Task<(bool Success, string Message, int? CompanyId, string UserId)> ApproveRequestAsync(int id, string adminId);
        Task<(bool Success, string Message)> RejectRequestAsync(int id, string reason, string adminId);

        // User company update
        Task<(bool Success, string Message, string Token)> UpdateUserCompanyAsync(string userId, int companyId);
    }
}