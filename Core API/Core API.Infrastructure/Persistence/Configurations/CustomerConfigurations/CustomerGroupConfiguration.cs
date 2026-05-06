using Core_API.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.CustomerConfigurations
{
    /// <summary>
    /// Entity configuration for the CustomerGroup entity.
    /// </summary>
    public class CustomerGroupConfiguration : IEntityTypeConfiguration<CustomerGroup>
    {
        public void Configure(EntityTypeBuilder<CustomerGroup> builder)
        {
            // Table name
            builder.ToTable("CustomerGroups", "dbo");

            // Primary Key
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id).UseIdentityColumn(1, 1);

            // Properties
            builder.Property(g => g.Name)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("Name")
                   .HasColumnType("nvarchar(100)");

            builder.Property(g => g.Description)
                   .HasMaxLength(500)
                   .HasColumnName("Description")
                   .HasColumnType("nvarchar(500)")
                   .IsRequired(false);

            builder.Property(g => g.CompanyId)
                   .IsRequired()
                   .HasColumnName("CompanyId");

            // Relationships
            builder.HasOne(g => g.Company)
                   .WithMany(c => c.CustomerGroups)
                   .HasForeignKey(g => g.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CustomerGroups_Companies");

            // Indexes
            builder.HasIndex(g => new { g.CompanyId, g.Name })
                   .HasDatabaseName("IX_CustomerGroups_CompanyId_Name")
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(g => g.IsDeleted)
                   .HasDatabaseName("IX_CustomerGroups_IsDeleted");

            // Soft delete filter
            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
