using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class InvoiceAttachmentRepository(CoreAPIDbContext dbContext) : IInvoiceAttachmentRepository
    {
        private readonly CoreAPIDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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