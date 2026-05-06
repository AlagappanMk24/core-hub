using Core_API.Application.Common.Base;
using Core_API.Application.Common.Models;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerInvoices
{
    /// <summary>
    /// Query to get all invoices for a specific customer
    /// </summary>
    public record GetCustomerInvoicesQuery : BaseQuery<List<CustomerInvoiceDto>>
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public int CustomerId { get; init; }
        public OperationContext Context { get; init; } 
    }
}