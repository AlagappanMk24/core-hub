using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            // Company - Invoices relationship
            builder.HasMany(c => c.Invoices)
                   .WithOne(i => i.Company)
                   .HasForeignKey(i => i.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Company - Locations relationship
            builder.HasMany(c => c.Locations)
                   .WithOne(l => l.Company)
                   .HasForeignKey(l => l.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Company - Vendors relationship
            builder.HasMany(c => c.Vendors)
                   .WithOne(v => v.Company)
                   .HasForeignKey(v => v.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Company - Customers relationship
            builder.HasMany<Customer>()
                   .WithOne(c => c.Company)
                   .HasForeignKey(c => c.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Properties
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);
        }
    }
}
