using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class TimezoneConfiguration : IEntityTypeConfiguration<Timezone>
    {
        public void Configure(EntityTypeBuilder<Timezone> builder)
        {
            // Ensure Name is unique
            builder.HasIndex(t => t.Name)
                   .IsUnique();

            // Timezone - Locations relationship (optional, if you want to define the inverse)
            builder.HasMany<Location>()
                   .WithOne(l => l.Timezone)
                   .HasForeignKey(l => l.TimezoneId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
