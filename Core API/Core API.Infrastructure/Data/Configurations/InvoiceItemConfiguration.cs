using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            // InvoiceItem - Invoice relationship
            builder.HasOne(ii => ii.Invoice)
                   .WithMany(i => i.InvoiceItems)
                   .HasForeignKey(ii => ii.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade); // Delete InvoiceItem when Invoice is deleted

            // Configure decimal properties
            builder.Property(ii => ii.Price).HasColumnType("decimal(18,2)");
            builder.Property(ii => ii.Amount).HasColumnType("decimal(18,2)");
        }
    }
}
