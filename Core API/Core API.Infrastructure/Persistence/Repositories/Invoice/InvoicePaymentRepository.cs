using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoicePaymentRepository(CoreInvoiceDbContext dbContext) : GenericRepository<InvoicePayment>(dbContext), IInvoicePaymentRepository
    {
        public async Task<List<InvoicePayment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        public async Task<decimal> GetTotalPaidByInvoiceIdAsync(int invoiceId)
        {
            return await dbset
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted && !p.IsRefund)
                .SumAsync(p => p.Amount);
        }
    }
}