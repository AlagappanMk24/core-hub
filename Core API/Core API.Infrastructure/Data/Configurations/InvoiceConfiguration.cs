using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // Invoice - Customer relationship
        builder.HasOne(i => i.Customer)
               .WithMany(c => c.Invoices)
               .HasForeignKey(i => i.CustomerId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Customer if Invoices exist

        // Invoice - Company relationship
        builder.HasOne(i => i.Company)
               .WithMany(c => c.Invoices)
               .HasForeignKey(i => i.CompanyId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Company if Invoices exist

        // Invoice - InvoiceItems relationship
        builder.HasMany(i => i.InvoiceItems)
               .WithOne(ii => ii.Invoice)
               .HasForeignKey(ii => ii.InvoiceId)
               .OnDelete(DeleteBehavior.Cascade); // Delete InvoiceItems when Invoice is deleted

        // Invoice - TaxDetails relationship
        builder.HasMany(i => i.TaxDetails)
               .WithOne(td => td.Invoice)
               .HasForeignKey(td => td.InvoiceId)
               .OnDelete(DeleteBehavior.Cascade); // Delete Discounts when Invoice is deleted

        // Invoice - Discount relationship
        builder.HasMany(i => i.Discounts)
              .WithOne(d => d.Invoice)
              .HasForeignKey(d => d.InvoiceId)
              .OnDelete(DeleteBehavior.Cascade);

        // Unique Index
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        // String Constraints
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.PONumber).HasMaxLength(50);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.Property(i => i.PaymentMethod).HasMaxLength(100);
        builder.Property(i => i.PaymentTerms).HasMaxLength(100);
        builder.Property(i => i.PaymentGateway).HasMaxLength(50);
        builder.Property(i => i.PaymentTransactionId).HasMaxLength(500);
        builder.Property(i => i.AdjustmentDescription).HasMaxLength(200);
        builder.Property(i => i.SourceSystem).HasMaxLength(200);
        builder.Property(i => i.CustomerNotes).HasMaxLength(1000);
        builder.Property(i => i.InternalNotes).HasMaxLength(1000);
        builder.Property(i => i.TermsAndConditions).HasMaxLength(500);
        builder.Property(i => i.FooterNote).HasMaxLength(500);

        // Decimals (Base fields included)
        var decimalProps = new[] {
            "Subtotal", "DiscountTotal", "TaxTotal", "ShippingAmount", "TotalAmount",
            "CurrencyRate", "AdjustmentAmount", "AmountPaid", "AmountDue", "AmountRefunded"
        };
        foreach (var prop in decimalProps)
            builder.Property(prop).HasColumnType("decimal(18,2)");
    }
}