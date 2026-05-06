using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.CustomerConfigurations
{
    /// <summary>
    /// Entity configuration for the CustomerDocument entity.
    /// </summary>
    public class CustomerDocumentConfiguration : IEntityTypeConfiguration<CustomerDocument>
    {
        public void Configure(EntityTypeBuilder<CustomerDocument> builder)
        {
            // Table name
            builder.ToTable("CustomerDocuments", "dbo");

            // Primary Key
            builder.HasKey(cd => cd.Id);
            builder.Property(cd => cd.Id).UseIdentityColumn(1, 1);

            // Properties
            builder.Property(cd => cd.DocumentName)
                   .IsRequired()
                   .HasMaxLength(200)
                   .HasColumnName("DocumentName")
                   .HasColumnType("nvarchar(200)");

            builder.Property(cd => cd.FileUrl)
                   .IsRequired()
                   .HasMaxLength(500)
                   .HasColumnName("FileUrl")
                   .HasColumnType("nvarchar(500)");

            builder.Property(cd => cd.FileType)
                   .HasMaxLength(100)
                   .HasColumnName("FileType")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired(false);

            builder.Property(cd => cd.FileSize)
                   .HasColumnName("FileSize")
                   .HasColumnType("bigint")
                   .IsRequired();

            builder.Property(cd => cd.Type)
                   .HasColumnName("Type")
                   .HasColumnType("int")
                   .HasDefaultValue(DocumentType.Other)
                   .IsRequired();

            // Relationships
            builder.HasOne(cd => cd.Customer)
                   .WithMany(c => c.Documents)
                   .HasForeignKey(cd => cd.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CustomerDocuments_Customers");

            // Indexes
            builder.HasIndex(cd => cd.CustomerId)
                   .HasDatabaseName("IX_CustomerDocuments_CustomerId");

            builder.HasIndex(cd => cd.Type)
                   .HasDatabaseName("IX_CustomerDocuments_Type");

            builder.HasIndex(cd => cd.IsDeleted)
                   .HasDatabaseName("IX_CustomerDocuments_IsDeleted");

            builder.HasIndex(cd => new { cd.CustomerId, cd.Type })
                   .HasDatabaseName("IX_CustomerDocuments_CustomerId_Type")
                   .HasFilter("[IsDeleted] = 0");

            // Soft delete filter
            builder.HasQueryFilter(cd => !cd.IsDeleted);
        }
    }
}