using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Customer - Invoices relationship
            builder.HasMany(c => c.Invoices)
                   .WithOne(i => i.Customer)
                   .HasForeignKey(i => i.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Customer if Invoices exist

            // Customer - Company relationship
            builder.HasOne(c => c.Company)
                   .WithMany()
                   .HasForeignKey(c => c.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Configure Address as an owned type
            builder.OwnsOne(c => c.Address, address =>
            {
                address.Property(a => a.Address1).HasColumnName("Address1").IsRequired();
                address.Property(a => a.Address2).HasColumnName("Address2").IsRequired(false);
                address.Property(a => a.City).HasColumnName("City").IsRequired();
                address.Property(a => a.State).HasColumnName("State").IsRequired(false);
                address.Property(a => a.Country).HasColumnName("Country").IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("ZipCode").IsRequired();
            });

            // Configure Customer properties
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(c => c.Email)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(c => c.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);
            builder.Property(c => c.CompanyId)
                   .IsRequired();
        }
    }
}