using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.Property(ii => ii.UnitPrice)
              .HasColumnType("decimal(18,2)");
            builder.Property(ii => ii.Amount)
              .HasColumnType("decimal(18,2)");
            builder.Property(ii => ii.TaxAmount)
              .HasColumnType("decimal(18,2)");
        }
    }
}
