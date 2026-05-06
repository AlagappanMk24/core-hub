namespace Core_API.Application.Common.Models
{
    public class OperationContext
    {
        public string UserId { get; set; }
        public int? CompanyId { get; set; }
        public int? CustomerId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }  
        public List<string> Roles { get; set; }

        // Helper properties
        public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
        public bool IsCompanyUser => CompanyId.HasValue && !IsSuperAdmin;
        public bool IsCustomerUser => CustomerId.HasValue && Roles.Contains("Customer");

        // Single constructor with optional parameters
        public OperationContext(string userId, int? companyId = null, int? customerId = null, bool isSuperAdmin = false, List<string> roles = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (companyId.HasValue && companyId < 0)
                throw new ArgumentException("CompanyId cannot be negative.", nameof(companyId));

            if (customerId.HasValue && customerId < 0)
                throw new ArgumentException("CustomerId cannot be negative.", nameof(customerId));

            UserId = userId;
            CompanyId = companyId;
            CustomerId = customerId;
            IsSuperAdmin = isSuperAdmin;
            Roles = roles ?? new List<string>();
        }

        // Helper methods
        public bool HasRole(string role) => Roles.Contains(role);

        public bool HasAnyRole(params string[] roles) => roles.Any(r => Roles.Contains(r));

        public bool CanAccessCompany(int companyId) =>
            IsSuperAdmin || CompanyId == companyId;

        public bool CanAccessCustomer(int customerId) =>
            IsSuperAdmin || CustomerId == customerId ||
            (CompanyId.HasValue && Roles.Contains("Admin"));

        public override string ToString()
        {
            return $"UserId: {UserId}, CompanyId: {CompanyId}, CustomerId: {CustomerId}, " +
                   $"IsSuperAdmin: {IsSuperAdmin}, Roles: [{string.Join(",", Roles)}]";
        }
    }
}