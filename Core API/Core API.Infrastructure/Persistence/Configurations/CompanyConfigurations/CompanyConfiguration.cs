using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Companies;

namespace Core_API.Infrastructure.Persistence.Configurations.CompanyConfigurations
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

            builder.Property(c => c.CreatedByUserId)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            // Configure Address as an owned type (Value Object)
            builder.OwnsOne(c => c.Address, address =>
            {
                address.Property(a => a.AddressLine1)
                       .HasColumnName("AddressLine1")
                       .IsRequired()
                       .HasMaxLength(200);

                address.Property(a => a.AddressLine2)
                       .HasColumnName("AddressLine2")
                       .IsRequired(false)
                       .HasMaxLength(200);

                address.Property(a => a.City)
                       .HasColumnName("City")
                       .IsRequired()
                       .HasMaxLength(100);

                address.Property(a => a.State)
                       .HasColumnName("State")
                       .IsRequired(false)
                       .HasMaxLength(100);

                address.Property(a => a.CountryCode)
                       .HasColumnName("CountryCode")
                       .IsRequired()
                       .HasMaxLength(2);

                address.Property(a => a.CountryName)
                       .HasColumnName("CountryName")
                       .IsRequired()
                       .HasMaxLength(100);

                address.Property(a => a.ZipCode)
                       .HasColumnName("ZipCode")
                       .IsRequired()
                       .HasMaxLength(20);
            });

            // Configure Email Value Object
            builder.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .IsRequired()
                     .HasMaxLength(100);
            });

            // Configure PhoneNumber Value Object
            builder.OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                     .HasColumnName("PhoneNumber")
                     .IsRequired()
                     .HasMaxLength(20);
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