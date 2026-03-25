using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core_API.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for the <see cref="RecurringInvoice"/> entity.
    /// Defines table mappings, relationships, indexes, and property constraints.
    /// </summary>
    public class RecurringInvoiceConfiguration : IEntityTypeConfiguration<RecurringInvoice>
    {
        public void Configure(EntityTypeBuilder<RecurringInvoice> builder)
        {
            // ── Table Name (Optional if you want to customize) ─────────────────
            // builder.ToTable("RecurringInvoices");

            // ── Primary Key ────────────────────────────────────────────────────
            // Inherited from BaseEntity, already configured

            // ── Indexes ────────────────────────────────────────────────────────

            // Index for faster lookups by company and name (unique constraint)
            builder.HasIndex(r => new { r.CompanyId, r.Name })
                   .IsUnique()
                   .HasDatabaseName("IX_RecurringInvoices_CompanyId_Name")
                   .HasFilter("[IsDeleted] = 0"); // Only enforce on non-deleted records

            // Index for finding due invoices (background job optimization)
            builder.HasIndex(r => new { r.Status, r.NextInvoiceDate, r.IsDeleted })
                   .HasDatabaseName("IX_RecurringInvoices_Status_NextDate");

            // Index for customer-specific queries
            builder.HasIndex(r => new { r.CustomerId, r.Status, r.IsDeleted })
                   .HasDatabaseName("IX_RecurringInvoices_CustomerId_Status");

            // Index for frequency-based reporting
            builder.HasIndex(r => new { r.Frequency, r.Status, r.CompanyId })
                   .HasDatabaseName("IX_RecurringInvoices_Frequency_Status");

            // ── Relationships ───────────────────────────────────────────────────

            // RecurringInvoice -> Customer (Many-to-One)
            builder.HasOne(r => r.Customer)
                   .WithMany(c => c.RecurringInvoices) // You'll need to add this navigation property to Customer entity
                   .HasForeignKey(r => r.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict) // Prevent deletion of customer with active recurring invoices
                   .IsRequired();

            // RecurringInvoice -> Company (Many-to-One)
            builder.HasOne(r => r.Company)
                   .WithMany(c => c.RecurringInvoices) // You'll need to add this navigation property to Company entity
                   .HasForeignKey(r => r.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            // RecurringInvoice -> SourceInvoice (Optional One-to-One)
            builder.HasOne(r => r.SourceInvoice)
                   .WithMany() // A source invoice can be template for multiple recurring invoices
                   .HasForeignKey(r => r.SourceInvoiceId)
                   .OnDelete(DeleteBehavior.SetNull); // Set to null if source invoice is deleted

            // RecurringInvoice -> GeneratedInvoices (One-to-Many)
            builder.HasMany(r => r.GeneratedInvoices)
                   .WithOne(ri => ri.RecurringInvoice)
                   .HasForeignKey(ri => ri.RecurringInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade); // Delete instances if template is deleted

            // RecurringInvoice -> AuditLogs (One-to-Many)
            builder.HasMany(r => r.AuditLogs)
                   .WithOne(al => al.RecurringInvoice)
                   .HasForeignKey(al => al.RecurringInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Property Configurations ─────────────────────────────────────────

            // String Length Constraints
            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(r => r.Description)
                   .HasMaxLength(1000);

            builder.Property(r => r.OverridePONumber)
                   .HasMaxLength(50);

            builder.Property(r => r.OverrideCustomerNotes)
                   .HasMaxLength(1000);

            builder.Property(r => r.OverrideTermsAndConditions)
                   .HasMaxLength(500);

            builder.Property(r => r.OverrideFooterNote)
                   .HasMaxLength(500);

            builder.Property(r => r.OverrideProjectDetail)
                   .HasMaxLength(200);

            builder.Property(r => r.OverridePaymentMethod)
                   .HasMaxLength(100);

            builder.Property(r => r.OverrideAdjustmentDescription)
                   .HasMaxLength(200);

            // ── Decimal Precision ───────────────────────────────────────────────

            builder.Property(r => r.OverrideShippingAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(r => r.OverrideAdjustmentAmount)
                   .HasColumnType("decimal(18,2)");

            // Note: Financial fields (Subtotal, TaxTotal, etc.) are inherited from InvoiceHeaderBase
            // and should be configured there, but you can override if needed:

            // ── Enum Conversions (Store as strings for readability) ─────────────

            builder.Property(r => r.Frequency)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(r => r.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            // ── Required Fields ─────────────────────────────────────────────────

            builder.Property(r => r.FrequencyInterval)
                   .IsRequired();

            builder.Property(r => r.StartDate)
                   .IsRequired();

            builder.Property(r => r.NextInvoiceDate)
                   .IsRequired();

            builder.Property(r => r.GenerateInAdvanceDays)
                   .HasDefaultValue(0);

            builder.Property(r => r.AutoSend)
                   .HasDefaultValue(false);

            builder.Property(r => r.AutoEmail)
                   .HasDefaultValue(false);

            builder.Property(r => r.AutoCharge)
                   .HasDefaultValue(false);

            builder.Property(r => r.ReminderBeforeDue)
                   .HasDefaultValue(false);

            builder.Property(r => r.ReminderDaysBefore)
                   .HasDefaultValue(3);

            // ── Query Filters ───────────────────────────────────────────────────

            // Global query filter to exclude soft-deleted records
            builder.HasQueryFilter(r => !r.IsDeleted);

            // ── Seed Data (Optional - for demo/testing) ─────────────────────────
            /*
            builder.HasData(
                new RecurringInvoice
                {
                    Id = 1,
                    Name = "Monthly Subscription - Basic",
                    Description = "Basic monthly service subscription",
                    CustomerId = 1,
                    CompanyId = 1,
                    Frequency = RecurringFrequency.Monthly,
                    FrequencyInterval = 1,
                    DayOfMonth = 1,
                    StartDate = DateTime.UtcNow,
                    NextInvoiceDate = DateTime.UtcNow.AddMonths(1),
                    Status = RecurringInvoiceStatus.Active,
                    AutoSend = true,
                    AutoEmail = true,
                    Subtotal = 99.99m,
                    TaxTotal = 8.00m,
                    TotalAmount = 107.99m,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system",
                    IsDeleted = false
                }
            );
            */
        }
    }
}
