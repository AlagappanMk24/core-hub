using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class CompanyRequestRepository(CoreAPIDbContext context) : GenericRepository<CompanyRequest>(context), ICompanyRequestRepository
    {
        private readonly CoreAPIDbContext _context = context;

        public async Task<CompanyRequest> GetRequestByIdAsync(int id)
        {
            return await _context.CompanyRequests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<CompanyRequest> GetPendingRequestAsync(string email, string companyName)
        {
            return await _context.CompanyRequests
                .FirstOrDefaultAsync(r => r.Email == email &&
                    r.CompanyName == companyName &&
                    r.Status == CompanyRequestStatus.Pending);
        }

        public async Task<List<CompanyRequest>> GetRequestsByEmailAsync(string email)
        {
            return await _context.CompanyRequests
                .Where(r => r.Email == email)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<(List<CompanyRequest> Requests, int TotalCount, int PendingCount, int ApprovedCount, int RejectedCount)>
            GetPagedRequestsAsync(int page, int pageSize, string status, string search)
        {
            var query = _context.CompanyRequests.AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<CompanyRequestStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            // Search by name, email, or company
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.FullName.ToLower().Contains(search) ||
                    r.Email.ToLower().Contains(search) ||
                    r.CompanyName.ToLower().Contains(search));
            }

            // Get counts
            var totalCount = await query.CountAsync();
            var pendingCount = await query.CountAsync(r => r.Status == CompanyRequestStatus.Pending);
            var approvedCount = await query.CountAsync(r => r.Status == CompanyRequestStatus.Approved);
            var rejectedCount = await query.CountAsync(r => r.Status == CompanyRequestStatus.Rejected);

            // Paginate
            var requests = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (requests, totalCount, pendingCount, approvedCount, rejectedCount);
        }

        public async Task<bool> CompanyExistsAsync(string companyName)
        {
            return await _context.Companies
                .AnyAsync(c => c.Name.ToLower() == companyName.ToLower());
        }

        public async Task<bool> HasPendingRequestAsync(string email, string companyName)
        {
            return await _context.CompanyRequests
                .AnyAsync(r => r.Email == email &&
                    r.CompanyName == companyName &&
                    r.Status == CompanyRequestStatus.Pending);
        }
    }
}
