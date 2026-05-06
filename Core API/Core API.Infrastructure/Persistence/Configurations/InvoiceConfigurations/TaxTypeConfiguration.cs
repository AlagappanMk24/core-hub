using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations
{
    public class TaxTypeConfiguration : IEntityTypeConfiguration<TaxType>
    {
        public void Configure(EntityTypeBuilder<TaxType> builder)
        {
            builder.Property(td => td.Rate)
              .HasColumnType("decimal(5,2)");
        }
    }
}