using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class InvoiceAttachmentsConfiguration : IEntityTypeConfiguration<InvoiceAttachments>
    {
        public void Configure(EntityTypeBuilder<InvoiceAttachments> builder)
        {
            // InvoiceAttachments - Invoice relationship
            builder.HasOne(ia => ia.Invoice)
                   .WithMany(i => i.InvoiceAttachments)
                   .HasForeignKey(ia => ia.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade); // Delete InvoiceAttachments when Invoice is deleted
        }
    }
}
