using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            // Configure Company properties
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.TaxId)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(c => c.Email)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(c => c.PhoneNumber)
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.Property(c => c.CreatedByUserId)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            // Configure Address as an owned type
            builder.OwnsOne(c => c.Address, address =>
            {
                address.Property(a => a.Address1)
                       .HasColumnName("Address1")
                       .IsRequired(false);

                address.Property(a => a.Address2)
                       .HasColumnName("Address2")
                       .IsRequired(false);

                address.Property(a => a.City)
                       .HasColumnName("City")
                       .IsRequired(false);

                address.Property(a => a.State)
                       .HasColumnName("State")
                       .IsRequired(false);

                address.Property(a => a.Country)
                       .HasColumnName("Country")
                       .IsRequired(false);

                address.Property(a => a.ZipCode)
                       .HasColumnName("ZipCode")
                       .IsRequired(false);
            });

            // Company - Customers relationship
            builder.HasMany(c => c.Customers)
                   .WithOne(cu => cu.Company)
                   .HasForeignKey(cu => cu.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Company if Customers exist

            // Company - Invoices relationship
            builder.HasMany(c => c.Invoices)
                   .WithOne(i => i.Company)
                   .HasForeignKey(i => i.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Company if Invoices exist

            // Index for performance
            builder.HasIndex(c => c.Name)
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0"); // Unique constraint for non-deleted companies
        }
    }
}
