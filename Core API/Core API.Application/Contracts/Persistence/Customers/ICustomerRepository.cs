using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Contracts.Persistence.Customers
{
    /// <summary>
    /// Defines data access operations for Customer entities, extending the generic repository 
    /// with domain-specific queries and reporting.
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Gets a paginated list of customers belonging to a specific company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="filter">The filtering, searching, and pagination parameters.</param>
        /// <returns>A paginated result containing the list of customers and metadata.</returns>
        Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, CustomerFilterRequestDto filter);

        /// <summary>
        /// Gets a paginated list of all customers across all companies. Primarily used for Super Admin views.
        /// </summary>
        /// <param name="filter">The filtering, searching, and pagination parameters.</param>
        /// <returns>A paginated result containing customers from all organizations.</returns>
        Task<PaginatedResult<Customer>> GetAllCustomersPagedAsync(CustomerFilterRequestDto filter);

        /// <summary>
        /// Checks if a customer with the specified email exists within a specific company context.
        /// </summary>
        /// <param name="context">The current operation context containing CompanyId.</param>
        /// <param name="email">The email address to check (case-insensitive).</param>
        /// <returns><c>true</c> if an active customer exists with the given email; otherwise, <c>false</c>.</returns>
        Task<bool> ExistsAsync(OperationContext context, string email);

        /// <summary>
        /// Determines whether a specific customer has any associated invoices.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <returns><c>true</c> if the customer has at least one non-deleted invoice; otherwise, <c>false</c>.</returns>
        Task<bool> HasInvoicesAsync(int customerId);

        /// <summary>
        /// Retrieves all non-deleted invoices associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <returns>A list of invoice entities ordered by issue date descending.</returns>
        Task<List<Domain.Entities.Invoices.Invoice>> GetCustomerInvoicesAsync(int customerId);

        /// <summary>
        /// Retrieves all non-deleted payments made by a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <returns>A list of invoice payments ordered by payment date descending.</returns>
        Task<List<InvoicePayment>> GetCustomerPaymentsAsync(int customerId);

        /// <summary>
        /// Retrieves a history of recent actions related to the customer, such as invoice creation or payments.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="count">The maximum number of activity records to return. Defaults to 10.</param>
        /// <returns>A list of customer activity DTOs.</returns>
        Task<List<CustomerActivityDto>> GetCustomerActivitiesAsync(int customerId, int count = 10);

        /// <summary>
        /// Calculates the monthly spending trend for a customer over the last 12 months.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <returns>A list of spending trends grouped by month and year.</returns>
        Task<List<SpendingTrendDto>> GetCustomerSpendingTrendAsync(int customerId);
    }
}