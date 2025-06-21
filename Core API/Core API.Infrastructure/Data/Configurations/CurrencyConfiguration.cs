using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            // Ensure Code is unique
            builder.HasIndex(c => c.Code)
                   .IsUnique();

            // Currency - Locations relationship (optional, if you want to define the inverse)
            builder.HasMany<Location>()
                   .WithOne(l => l.Currency)
                   .HasForeignKey(l => l.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
