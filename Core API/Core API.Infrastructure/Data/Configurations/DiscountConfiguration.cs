using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
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