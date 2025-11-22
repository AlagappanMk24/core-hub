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

        // Ensure InvoiceNumber is unique
        builder.HasIndex(i => i.InvoiceNumber)
               .IsUnique();

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.PONumber).HasMaxLength(50);
        builder.Property(i => i.IssueDate).IsRequired();
        builder.Property(i => i.DueDate).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.PaymentMethod).HasMaxLength(100);
        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Tax).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
    }
}