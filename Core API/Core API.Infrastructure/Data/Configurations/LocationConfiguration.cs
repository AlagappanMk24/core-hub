using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            // Configure Address as an owned type
            builder.OwnsOne(l => l.Address, address =>
            {
                address.Property(a => a.Address1).HasColumnName("Address1").IsRequired();
                address.Property(a => a.Address2).HasColumnName("Address2").IsRequired(false);
                address.Property(a => a.City).HasColumnName("City").IsRequired();
                address.Property(a => a.State).HasColumnName("State").IsRequired(false);
                address.Property(a => a.Country).HasColumnName("Country").IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("ZipCode").IsRequired();
            });

            // Location - Company relationship
            builder.HasOne(l => l.Company)
                   .WithMany(c => c.Locations)
                   .HasForeignKey(l => l.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Company if Locations exist

            // Location - Currency relationship
            builder.HasOne(l => l.Currency)
                   .WithMany()// No navigation property in Currency
                   .HasForeignKey(l => l.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Currency if Locations exist

            // Location - Timezone relationship
            builder.HasOne(l => l.Timezone)
                   .WithMany() // No navigation property in Timezone
                   .HasForeignKey(l => l.TimezoneId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Timezone if Locations exist

            // Location - Invoices relationship
            builder.HasMany(l => l.Invoices)
                   .WithOne(i => i.Location)
                   .HasForeignKey(i => i.LocationId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Location if Invoices exist
        }
    }
}
