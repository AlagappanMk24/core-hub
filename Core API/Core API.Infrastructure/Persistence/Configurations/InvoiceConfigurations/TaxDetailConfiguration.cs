using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations
{
    public class TaxDetailConfiguration : IEntityTypeConfiguration<InvoiceTaxDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceTaxDetail> builder)
        {
            builder.Property(td => td.Rate)
              .HasColumnType("decimal(5,2)");
            builder.Property(td => td.TaxAmount)
              .HasColumnType("decimal(18,2)");
        }
    }
}
