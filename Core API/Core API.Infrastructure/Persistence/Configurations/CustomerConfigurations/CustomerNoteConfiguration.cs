using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.CustomerConfigurations
{
    /// <summary>
    /// Entity configuration for the CustomerNote entity.
    /// </summary>
    public class CustomerNoteConfiguration : IEntityTypeConfiguration<CustomerNote>
    {
        public void Configure(EntityTypeBuilder<CustomerNote> builder)
        {
            // Table name
            builder.ToTable("CustomerNotes", "dbo");

            // Primary Key
            builder.HasKey(cn => cn.Id);
            builder.Property(cn => cn.Id).UseIdentityColumn(1, 1);

            // Properties
            builder.Property(cn => cn.Note)
                   .IsRequired()
                   .HasMaxLength(2000)
                   .HasColumnName("Note")
                   .HasColumnType("nvarchar(2000)");

            builder.Property(cn => cn.Type)
                   .HasColumnName("Type")
                   .HasColumnType("int")
                   .HasDefaultValue(NoteType.Internal)
                   .IsRequired();

            builder.Property(cn => cn.IsPinned)
                   .HasColumnName("IsPinned")
                   .HasDefaultValue(false)
                   .IsRequired();

            // Relationships
            builder.HasOne(cn => cn.Customer)
                   .WithMany(c => c.Notes)
                   .HasForeignKey(cn => cn.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CustomerNotes_Customers");

            // Indexes
            builder.HasIndex(cn => cn.CustomerId)
                   .HasDatabaseName("IX_CustomerNotes_CustomerId");

            builder.HasIndex(cn => cn.Type)
                   .HasDatabaseName("IX_CustomerNotes_Type");

            builder.HasIndex(cn => cn.IsPinned)
                   .HasDatabaseName("IX_CustomerNotes_IsPinned");

            builder.HasIndex(cn => cn.IsDeleted)
                   .HasDatabaseName("IX_CustomerNotes_IsDeleted");

            builder.HasIndex(cn => new { cn.CustomerId, cn.IsPinned })
                   .HasDatabaseName("IX_CustomerNotes_CustomerId_Pinned")
                   .HasFilter("[IsDeleted] = 0");

            // Soft delete filter
            builder.HasQueryFilter(cn => !cn.IsDeleted);
        }
    }
}