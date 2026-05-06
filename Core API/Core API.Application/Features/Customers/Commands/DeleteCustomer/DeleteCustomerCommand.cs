using Core_API.Application.Common.Base;

namespace Core_API.Application.Features.Customers.Commands.DeleteCustomer
{
    /// <summary>
    /// Command to delete a customer (soft delete)
    /// </summary>
    public record DeleteCustomerCommand : BaseCommand<bool>
    {
        public int Id { get; init; }
    }
}
