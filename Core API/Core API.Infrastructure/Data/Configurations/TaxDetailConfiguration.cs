using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
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
