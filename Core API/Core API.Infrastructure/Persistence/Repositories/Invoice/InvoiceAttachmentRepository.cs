using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceAttachmentRepository(CoreInvoiceDbContext dbContext) : IInvoiceAttachmentRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        public async Task<InvoiceAttachment> GetAsync(Expression<Func<InvoiceAttachment, bool>> predicate)
        {
            return await _dbContext.InvoiceAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }
        public void Update(InvoiceAttachment attachment)
        {
            _dbContext.InvoiceAttachments.Update(attachment);
        }
    }
}