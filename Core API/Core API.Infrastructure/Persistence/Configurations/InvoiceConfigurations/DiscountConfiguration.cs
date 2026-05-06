using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations
{
    public class DiscountConfiguration : IEntityTypeConfiguration<InvoiceDiscount>
    {
        public void Configure(EntityTypeBuilder<InvoiceDiscount> builder)
        {
            builder.Property(td => td.Amount)
              .HasColumnType("decimal(18,2)");
        }
    }
}