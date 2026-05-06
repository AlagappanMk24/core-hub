using Core_API.Application.Contracts.Persistence.Taxes;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Infrastructure.Repositories;

namespace Core_API.Infrastructure.Repositories.Invoice
{
    public class TaxTypeRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaxType>(dbContext), ITaxTypeRepository
    {
    }
}