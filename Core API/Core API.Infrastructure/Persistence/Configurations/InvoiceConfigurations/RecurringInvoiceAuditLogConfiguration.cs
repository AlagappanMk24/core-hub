using Core_API.Domain.Entities.RecurringInvoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Persistence.Configurations.InvoiceConfigurations
{
    /// <summary>
    /// Configuration for the <see cref="RecurringInvoiceAuditLog"/> entity.
    /// </summary>
    public class RecurringInvoiceAuditLogConfiguration : IEntityTypeConfiguration<RecurringInvoiceAuditLog>
    {
        public void Configure(EntityTypeBuilder<RecurringInvoiceAuditLog> builder)
        {
            // ── Table Name ─────────────────────────────────────────────────────
            // builder.ToTable("RecurringInvoiceAuditLogs");

            // ── Indexes ────────────────────────────────────────────────────────

            // Index for chronological queries
            builder.HasIndex(al => new { al.RecurringInvoiceId, al.CreatedDate })
                   .HasDatabaseName("IX_RecurringAuditLogs_RecurringId_CreatedDate");

            // Index for action-based filtering
            builder.HasIndex(al => new { al.RecurringInvoiceId, al.Action })
                   .HasDatabaseName("IX_RecurringAuditLogs_RecurringId_Action");

            // ── Relationships ───────────────────────────────────────────────────

            builder.HasOne(al => al.RecurringInvoice)
                   .WithMany(r => r.AuditLogs)
                   .HasForeignKey(al => al.RecurringInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Property Configurations ─────────────────────────────────────────

            builder.Property(al => al.Action)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(al => al.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(al => al.Changes)
                   .HasMaxLength(4000); // Increased for JSON data

            builder.Property(al => al.IpAddress)
                   .HasMaxLength(45); // IPv6 can be up to 45 chars

            builder.Property(al => al.UserAgent)
                   .HasMaxLength(500);

            // ── Query Filters ───────────────────────────────────────────────────

            builder.HasQueryFilter(al => !al.IsDeleted);

            // ── Automatic CreatedDate (handled by BaseEntity) ───────────────────
        }
    }
}
