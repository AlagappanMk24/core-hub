using Core_API.Domain.Entities.Customers;

namespace Core_API.Application.Contracts.Persistence.Customers
{
    /// <summary>
    /// Repository interface for customer communication operations
    /// </summary>
    public interface ICustomerCommunicationRepository : IGenericRepository<CustomerCommunication>
    {
        /// <summary>
        /// Gets all communications for a specific customer
        /// </summary>
        Task<List<CustomerCommunication>> GetByCustomerIdAsync(int customerId, int limit = 50);

        /// <summary>
        /// Gets communications by type (email, sms, call, meeting)
        /// </summary>
        Task<List<CustomerCommunication>> GetByTypeAsync(int customerId, string type, int limit = 50);

        /// <summary>
        /// Gets communications within a date range
        /// </summary>
        Task<List<CustomerCommunication>> GetByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets unread communications for a customer
        /// </summary>
        Task<List<CustomerCommunication>> GetUnreadByCustomerIdAsync(int customerId);

        /// <summary>
        /// Gets the latest communication for a customer
        /// </summary>
        Task<CustomerCommunication?> GetLatestByCustomerIdAsync(int customerId);

        /// <summary>
        /// Marks a communication as delivered
        /// </summary>
        Task<bool> MarkAsDeliveredAsync(int communicationId);

        /// <summary>
        /// Marks a communication as read
        /// </summary>
        Task<bool> MarkAsReadAsync(int communicationId);

        /// <summary>
        /// Marks a communication as failed
        /// </summary>
        Task<bool> MarkAsFailedAsync(int communicationId, string errorMessage);

        /// <summary>
        /// Gets communication count by status for a customer
        /// </summary>
        Task<Dictionary<string, int>> GetCommunicationStatsAsync(int customerId);

        /// <summary>
        /// Gets communications by direction (inbound/outbound)
        /// </summary>
        Task<List<CustomerCommunication>> GetByDirectionAsync(int customerId, string direction, int limit = 50);

        /// <summary>
        /// Deletes old communications (soft delete)
        /// </summary>
        Task<int> DeleteOldCommunicationsAsync(int daysOld);

        /// <summary>
        /// Gets communications by sent by user
        /// </summary>
        Task<List<CustomerCommunication>> GetBySentByAsync(int customerId, string sentBy, int limit = 50);

        /// <summary>
        /// Searches communications by subject or content
        /// </summary>
        Task<List<CustomerCommunication>> SearchCommunicationsAsync(int customerId, string searchTerm);
    }
}