using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class TaxTypeRepository(CoreInvoiceDbContext dbContext) : GenericRepository<TaxType>(dbContext), ITaxTypeRepository
    {
    }
}