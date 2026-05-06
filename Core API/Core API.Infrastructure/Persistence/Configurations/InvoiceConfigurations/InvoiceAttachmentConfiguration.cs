using Core_API.Domain.Entities.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations;
public class InvoiceAttachmentConfiguration : IEntityTypeConfiguration<InvoiceAttachment>
{
    public void Configure(EntityTypeBuilder<InvoiceAttachment> builder)
    {
          builder.HasOne(a => a.Invoice)
                 .WithMany(i => i.InvoiceAttachments)
                 .HasForeignKey(a => a.InvoiceId);
    }
}
