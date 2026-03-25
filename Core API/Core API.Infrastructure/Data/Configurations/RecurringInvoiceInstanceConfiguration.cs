using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Data.Configurations
{

    /// <summary>
    /// Configuration for the <see cref="RecurringInvoiceInstance"/> entity.
    /// </summary>
    public class RecurringInvoiceInstanceConfiguration : IEntityTypeConfiguration<RecurringInvoiceInstance>
    {
        public void Configure(EntityTypeBuilder<RecurringInvoiceInstance> builder)
        {
            // ── Table Name ─────────────────────────────────────────────────────
            // builder.ToTable("RecurringInvoiceInstances");

            // ── Indexes ────────────────────────────────────────────────────────

            // Index for finding instances by recurring invoice
            builder.HasIndex(i => new { i.RecurringInvoiceId, i.SequenceNumber })
                   .IsUnique()
                   .HasDatabaseName("IX_RecurringInstances_RecurringId_Sequence");

            // Index for finding instances by invoice
            builder.HasIndex(i => i.InvoiceId)
                   .IsUnique()
                   .HasDatabaseName("IX_RecurringInstances_InvoiceId"); // One-to-one with Invoice

            // Index for generation status (for retry jobs)
            builder.HasIndex(i => new { i.GenerationStatus, i.ScheduledGenerationDate })
                   .HasDatabaseName("IX_RecurringInstances_Status_ScheduledDate");

            // ── Relationships ───────────────────────────────────────────────────

            // RecurringInvoiceInstance -> RecurringInvoice (Many-to-One)
            builder.HasOne(i => i.RecurringInvoice)
                   .WithMany(r => r.GeneratedInvoices)
                   .HasForeignKey(i => i.RecurringInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // RecurringInvoiceInstance -> Invoice (One-to-One)
            builder.HasOne(i => i.Invoice)
                   .WithOne() // Invoice doesn't have a navigation back to instance
                   .HasForeignKey<RecurringInvoiceInstance>(i => i.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Property Configurations ─────────────────────────────────────────

            builder.Property(i => i.GeneratedInvoiceNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Notes)
                   .HasMaxLength(500);

            builder.Property(i => i.ErrorMessage)
                   .HasMaxLength(1000);

            builder.Property(i => i.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(i => i.SequenceNumber)
                   .IsRequired();

            builder.Property(i => i.GeneratedDate)
                   .IsRequired();

            builder.Property(i => i.GenerationStatus)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(i => i.RetryCount)
                   .HasDefaultValue(0);

            // ── Query Filters ───────────────────────────────────────────────────

            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }
}
