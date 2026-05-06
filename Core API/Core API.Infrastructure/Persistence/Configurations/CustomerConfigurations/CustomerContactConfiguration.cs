using Core_API.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.CustomerConfigurations
{
    /// <summary>
    /// Entity configuration for the CustomerContact entity.
    /// </summary>
    public class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
    {
        public void Configure(EntityTypeBuilder<CustomerContact> builder)
        {
            // Table name
            builder.ToTable("CustomerContacts", "dbo");

            // Primary Key
            builder.HasKey(cc => cc.Id);
            builder.Property(cc => cc.Id).UseIdentityColumn(1, 1);

            // Properties
            builder.Property(cc => cc.Name)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("Name")
                   .HasColumnType("nvarchar(100)");

            builder.Property(cc => cc.Email)
                   .HasMaxLength(100)
                   .HasColumnName("Email")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired(false);

            builder.Property(cc => cc.PhoneNumber)
                   .HasMaxLength(20)
                   .HasColumnName("PhoneNumber")
                   .HasColumnType("nvarchar(20)")
                   .IsRequired(false);

            builder.Property(cc => cc.Designation)
                   .HasMaxLength(100)
                   .HasColumnName("Designation")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired(false);

            builder.Property(cc => cc.IsPrimary)
                   .HasColumnName("IsPrimary")
                   .HasDefaultValue(false)
                   .IsRequired();

            // Relationships
            builder.HasOne(cc => cc.Customer)
                   .WithMany(c => c.Contacts)
                   .HasForeignKey(cc => cc.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CustomerContacts_Customers");

            // Indexes
            builder.HasIndex(cc => cc.CustomerId)
                   .HasDatabaseName("IX_CustomerContacts_CustomerId");

            builder.HasIndex(cc => cc.Email)
                   .HasDatabaseName("IX_CustomerContacts_Email")
                   .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");

            builder.HasIndex(cc => cc.IsDeleted)
                   .HasDatabaseName("IX_CustomerContacts_IsDeleted");

            builder.HasIndex(cc => new { cc.CustomerId, cc.IsPrimary })
                   .HasDatabaseName("IX_CustomerContacts_CustomerId_Primary")
                   .HasFilter("[IsDeleted] = 0");

            // Soft delete filter
            builder.HasQueryFilter(cc => !cc.IsDeleted);
        }
    }
}