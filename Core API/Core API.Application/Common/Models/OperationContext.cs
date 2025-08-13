namespace Core_API.Application.Common.Models
{
    public class OperationContext
    {
        public int? CompanyId { get; set; }
        public string UserId { get; set; }
        public int? CustomerId { get; set; }

        // Single constructor with optional parameters
        public OperationContext(string userId, int? companyId = null, int? customerId = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (companyId.HasValue && companyId <= 0)
                throw new ArgumentException("CompanyId must be greater than 0.", nameof(companyId));
            if (customerId.HasValue && customerId <= 0)
                throw new ArgumentException("CustomerId must be greater than 0.", nameof(customerId));

            UserId = userId;
            CompanyId = companyId;
            CustomerId = customerId;
        }
    }
}