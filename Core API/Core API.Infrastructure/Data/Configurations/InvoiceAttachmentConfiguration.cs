using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Data.Configurations;
public class InvoiceAttachmentConfiguration : IEntityTypeConfiguration<InvoiceAttachment>
{
    public void Configure(EntityTypeBuilder<InvoiceAttachment> builder)
    {
          builder.HasOne(a => a.Invoice)
                 .WithMany(i => i.InvoiceAttachments)
                 .HasForeignKey(a => a.InvoiceId);
    }
}
