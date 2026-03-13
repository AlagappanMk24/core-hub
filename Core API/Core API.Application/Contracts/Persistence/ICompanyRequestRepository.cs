using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ICompanyRequestRepository : IGenericRepository<CompanyRequest>
    {
        Task<CompanyRequest> GetRequestByIdAsync(int id);
        Task<CompanyRequest> GetPendingRequestAsync(string email, string companyName);
        Task<List<CompanyRequest>> GetRequestsByEmailAsync(string email);
        Task<(List<CompanyRequest> Requests, int TotalCount, int PendingCount, int ApprovedCount, int RejectedCount)>
            GetPagedRequestsAsync(int page, int pageSize, string status, string search);
        Task<bool> CompanyExistsAsync(string companyName);
        Task<bool> HasPendingRequestAsync(string email, string companyName);
    }
}