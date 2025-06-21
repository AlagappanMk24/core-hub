using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class TaxDetailConfiguration : IEntityTypeConfiguration<TaxDetail>
    {
        public void Configure(EntityTypeBuilder<TaxDetail> builder)
        {
            // TaxDetail - Invoice relationship
            builder.HasOne(td => td.Invoice)
                   .WithMany(i => i.TaxDetails)
                   .HasForeignKey(td => td.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade); // Delete TaxDetail when Invoice is deleted

            // Already defined in the class, but ensure column types
            builder.Property(td => td.Rate).HasColumnType("decimal(5,2)");
            builder.Property(td => td.Amount).HasColumnType("decimal(18,2)");
        }
    }
}
