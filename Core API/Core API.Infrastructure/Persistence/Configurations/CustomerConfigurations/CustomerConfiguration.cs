using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.CustomerConfigurations
{
    /// <summary>
    /// Entity configuration for the Customer entity.
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Table name
            builder.ToTable("Customers", "dbo");

            // Primary Key
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseIdentityColumn(1, 1);

            // Properties
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("Name")
                   .HasColumnType("nvarchar(100)");

            builder.Property(c => c.TaxId)
                   .HasMaxLength(50)
                   .HasColumnName("TaxId")
                   .HasColumnType("nvarchar(50)")
                   .IsRequired(false);

            builder.Property(c => c.Website)
                   .HasMaxLength(200)
                   .HasColumnName("Website")
                   .HasColumnType("nvarchar(200)")
                   .IsRequired(false);

            builder.Property(c => c.CreditLimit)
                   .HasColumnName("CreditLimit")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(c => c.DefaultPaymentTerms)
                   .HasMaxLength(50)
                   .HasColumnName("DefaultPaymentTerms")
                   .HasColumnType("nvarchar(50)")
                   .IsRequired(false);

            builder.Property(c => c.DefaultCurrency)
                   .HasMaxLength(3)
                   .HasColumnName("DefaultCurrency")
                   .HasColumnType("nvarchar(3)")
                   .HasDefaultValue("USD")
                   .IsRequired(false);

            builder.Property(c => c.Status)
                   .HasColumnName("Status")
                   .HasColumnType("int")
                   .HasDefaultValue(CustomerStatus.Active)
                   .IsRequired();

            builder.Property(c => c.ActiveSince)
                   .HasColumnName("ActiveSince")
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(c => c.LastPurchaseDate)
                   .HasColumnName("LastPurchaseDate")
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(c => c.TotalPurchases)
                   .HasColumnName("TotalPurchases")
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(c => c.AveragePaymentDays)
                   .HasColumnName("AveragePaymentDays")
                   .HasColumnType("int")
                   .IsRequired(false);

            // Owned types (Value Objects)
            builder.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .IsRequired()
                     .HasMaxLength(100)
                     .HasColumnType("nvarchar(100)");

                email.Property(e => e.Domain)
                     .HasColumnName("EmailDomain")
                     .HasMaxLength(100)
                     .HasColumnType("nvarchar(100)");

                email.Property(e => e.LocalPart)
                     .HasColumnName("EmailLocalPart")
                     .HasMaxLength(100)
                     .HasColumnType("nvarchar(100)");

                // ✅ Add index on the Value property
                email.HasIndex(e => e.Value)
                     .HasDatabaseName("IX_Customers_Email")
                     .IsUnique()
                     .HasFilter("[IsDeleted] = 0");
            });

            builder.OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                     .HasColumnName("PhoneNumber")
                     .IsRequired()
                     .HasMaxLength(20)
                     .HasColumnType("nvarchar(20)");

                phone.Property(p => p.CountryCode)
                     .HasColumnName("PhoneCountryCode")
                     .HasMaxLength(10)
                     .HasColumnType("nvarchar(10)");

                phone.Property(p => p.NationalNumber)
                     .HasColumnName("PhoneNationalNumber")
                     .HasMaxLength(20)
                     .HasColumnType("nvarchar(20)");

                phone.Property(p => p.CountryCodeAlpha2)
                     .HasColumnName("PhoneCountryCodeAlpha2")
                     .HasMaxLength(2)
                     .HasColumnType("nvarchar(2)");

                // ✅ Add index on the Value property
                phone.HasIndex(p => p.Value)
                     .HasDatabaseName("IX_Customers_PhoneNumber")
                     .HasFilter("[IsDeleted] = 0");
            });

            builder.OwnsOne(c => c.Address, address =>
            {
                address.Property(a => a.AddressLine1)
                       .HasColumnName("AddressLine1")
                       .IsRequired()
                       .HasMaxLength(200)
                       .HasColumnType("nvarchar(200)");

                address.Property(a => a.AddressLine2)
                       .HasColumnName("AddressLine2")
                       .IsRequired(false)
                       .HasMaxLength(200)
                       .HasColumnType("nvarchar(200)");

                address.Property(a => a.City)
                       .HasColumnName("City")
                       .IsRequired()
                       .HasMaxLength(100)
                       .HasColumnType("nvarchar(100)");

                address.Property(a => a.State)
                       .HasColumnName("State")
                       .IsRequired(false)
                       .HasMaxLength(100)
                       .HasColumnType("nvarchar(100)");

                address.Property(a => a.CountryCode)
                       .HasColumnName("CountryCode")
                       .IsRequired()
                       .HasMaxLength(2)
                       .HasColumnType("nvarchar(2)");

                address.Property(a => a.CountryName)
                       .HasColumnName("CountryName")
                       .IsRequired()
                       .HasMaxLength(100)
                       .HasColumnType("nvarchar(100)");

                address.Property(a => a.ZipCode)
                       .HasColumnName("ZipCode")
                       .IsRequired()
                       .HasMaxLength(20)
                       .HasColumnType("nvarchar(20)");
            });

            // Relationships
            // Customer - Customer Group relationship
            builder.HasOne(c => c.CustomerGroup)
                   .WithMany(g => g.Customers)
                   .HasForeignKey(c => c.CustomerGroupId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .HasConstraintName("FK_Customers_CustomerGroups");

            // Customer - Company relationship
            builder.HasOne(c => c.Company)
                   .WithMany(c => c.Customers)
                   .HasForeignKey(c => c.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Customers_Companies");

            // Customer - Invoices relationship
            builder.HasMany(c => c.Invoices)
                   .WithOne(i => i.Customer)
                   .HasForeignKey(i => i.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Customer if Invoices exist

            // Customer - Recurring Invoices relationship
            builder.HasMany(c => c.RecurringInvoices)
                 .WithOne(ri => ri.Customer)
                 .HasForeignKey(ri => ri.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

            // Indexes for regular properties
            builder.HasIndex(c => c.Name)
                   .HasDatabaseName("IX_Customers_Name")
                   .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.CompanyId)
                   .HasDatabaseName("IX_Customers_CompanyId");

            builder.HasIndex(c => c.CustomerGroupId)
                   .HasDatabaseName("IX_Customers_CustomerGroupId");

            builder.HasIndex(c => c.Status)
                   .HasDatabaseName("IX_Customers_Status");

            builder.HasIndex(c => c.IsDeleted)
                   .HasDatabaseName("IX_Customers_IsDeleted");

            builder.HasIndex(c => new { c.CompanyId, c.Status, c.IsDeleted })
                   .HasDatabaseName("IX_Customers_Company_Status_Deleted");

            // Soft delete filter
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}