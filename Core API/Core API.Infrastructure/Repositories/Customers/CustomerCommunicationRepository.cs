using Core_API.Application.Contracts.Persistence.Customers;
using Core_API.Domain.Entities.Customers;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Customers
{
    /// <summary>
    /// Repository for managing customer communication records
    /// </summary>
    public class CustomerCommunicationRepository(CoreInvoiceDbContext context) : GenericRepository<CustomerCommunication>(context), ICustomerCommunicationRepository
    {

        /// <summary>
        /// Gets all communications for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="limit">Maximum number of records to return (default 50)</param>
        /// <returns>List of customer communications</returns>
        public async Task<List<CustomerCommunication>> GetByCustomerIdAsync(int customerId, int limit = 50)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId && !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Gets communications by type (email, sms, call, meeting)
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="type">Communication type</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <returns>List of filtered communications</returns>
        public async Task<List<CustomerCommunication>> GetByTypeAsync(int customerId, string type, int limit = 50)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId && c.Type == type && !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Gets communications within a date range
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of communications in date range</returns>
        public async Task<List<CustomerCommunication>> GetByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId &&
                            c.SentAt >= startDate &&
                            c.SentAt <= endDate &&
                            !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets unread communications for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of unread communications</returns>
        public async Task<List<CustomerCommunication>> GetUnreadByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId &&
                            c.Status == "sent" &&
                            c.ReadAt == null &&
                            !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the latest communication for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The latest communication or null</returns>
        public async Task<CustomerCommunication?> GetLatestByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId && !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Marks a communication as delivered
        /// </summary>
        /// <param name="communicationId">The communication ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkAsDeliveredAsync(int communicationId)
        {
            var communication = await GetByIdAsync(communicationId);
            if (communication == null)
                return false;

            communication.Status = "delivered";
            communication.DeliveredAt = DateTime.UtcNow;
            Update(communication);
            return true;
        }

        /// <summary>
        /// Marks a communication as read
        /// </summary>
        /// <param name="communicationId">The communication ID</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkAsReadAsync(int communicationId)
        {
            var communication = await GetByIdAsync(communicationId);
            if (communication == null)
                return false;

            communication.Status = "read";
            communication.ReadAt = DateTime.UtcNow;
            Update(communication);
            return true;
        }

        /// <summary>
        /// Marks a communication as failed
        /// </summary>
        /// <param name="communicationId">The communication ID</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkAsFailedAsync(int communicationId, string errorMessage)
        {
            var communication = await GetByIdAsync(communicationId);
            if (communication == null)
                return false;

            communication.Status = "failed";
            communication.ErrorMessage = errorMessage;
            Update(communication);
            return true;
        }

        /// <summary>
        /// Gets communication count by status for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>Dictionary with status as key and count as value</returns>
        public async Task<Dictionary<string, int>> GetCommunicationStatsAsync(int customerId)
        {
            var communications = await _dbSet
                .Where(c => c.CustomerId == customerId && !c.IsDeleted)
                .ToListAsync();

            return communications
                .GroupBy(c => c.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Gets communications by direction (inbound/outbound)
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="direction">Communication direction</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <returns>List of communications by direction</returns>
        public async Task<List<CustomerCommunication>> GetByDirectionAsync(int customerId, string direction, int limit = 50)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId && c.Direction == direction && !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Deletes old communications (soft delete)
        /// </summary>
        /// <param name="daysOld">Delete communications older than this many days</param>
        /// <returns>Number of records deleted</returns>
        public async Task<int> DeleteOldCommunicationsAsync(int daysOld)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldCommunications = await _dbSet
                .Where(c => c.SentAt < cutoffDate && !c.IsDeleted)
                .ToListAsync();

            foreach (var communication in oldCommunications)
            {
                communication.IsDeleted = true;
                communication.UpdatedDate = DateTime.UtcNow;
            }

            return oldCommunications.Count;
        }

        /// <summary>
        /// Gets communications by sent by user
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="sentBy">User ID who sent the communication</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <returns>List of communications sent by specific user</returns>
        public async Task<List<CustomerCommunication>> GetBySentByAsync(int customerId, string sentBy, int limit = 50)
        {
            return await _dbSet
                .Where(c => c.CustomerId == customerId && c.SentBy == sentBy && !c.IsDeleted)
                .OrderByDescending(c => c.SentAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Searches communications by subject or content
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching communications</returns>
        public async Task<List<CustomerCommunication>> SearchCommunicationsAsync(int customerId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetByCustomerIdAsync(customerId);

            return await _dbSet
                .Where(c => c.CustomerId == customerId &&
                            !c.IsDeleted &&
                            (c.Subject.Contains(searchTerm) ||
                             c.Content.Contains(searchTerm)))
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();
        }
    }
}