using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class OrderHeaderConfiguration : IEntityTypeConfiguration<OrderHeader>
    {
        public void Configure(EntityTypeBuilder<OrderHeader> builder)
        {
            // OrderHeader - ApplicationUser relationship
            builder.HasOne(oh => oh.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(oh => oh.ApplicationUserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);

            // OrderHeader - Customer relationship
            builder.HasOne(oh => oh.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(oh => oh.CustomerId)
                    .IsRequired(false) // If order can exist without customer
                    .OnDelete(DeleteBehavior.NoAction);

            // OrderHeader - Invoices relationship
            builder.HasMany(oh => oh.Invoices)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.NoAction);

            // OrderHeader - ActivityLog relationship
            builder.HasMany(o => o.OrderActivityLog)
                .WithOne()
                .HasForeignKey(a => a.OrderHeaderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Property configurations
            builder.Property(oh => oh.Subtotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.Tax).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.Discount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.OrderTotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.AmountPaid).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.AmountDue).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.ShippingCharges).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(oh => oh.OrderStatus).HasMaxLength(50);
            builder.Property(oh => oh.PaymentStatus).HasMaxLength(50);
            builder.Property(oh => oh.DeliveryStatus).HasMaxLength(50);
            builder.Property(oh => oh.OrderDate).IsRequired();
            builder.Property(oh => oh.PaymentDueDate).IsRequired();
            builder.Property(oh => oh.ShippingContactName).HasMaxLength(100);
            builder.Property(oh => oh.ShippingContactPhone).HasMaxLength(20);
            builder.Property(oh => oh.TrackingNumber).HasMaxLength(50);
            builder.Property(oh => oh.TrackingUrl).HasMaxLength(500);
            builder.Property(oh => oh.TransactionId).HasMaxLength(100);
            builder.Property(oh => oh.SessionId).HasMaxLength(100);
            builder.Property(oh => oh.PaymentIntentId).HasMaxLength(100);
            builder.Property(oh => oh.EstimatedDelivery);
            builder.Property(oh => oh.DeliveryMethod).HasMaxLength(50);
            builder.Property(oh => oh.ShippingMethod).HasMaxLength(100);
            builder.Property(oh => oh.Carrier).HasMaxLength(100);
            builder.Property(oh => oh.PaymentMethod).HasMaxLength(50);
            builder.Property(oh => oh.CustomerNotes).HasMaxLength(1000);

            // Configure Shipping Address as an owned type
            builder.OwnsOne(l => l.ShipToAddress, shippingAddress =>
            {
                shippingAddress.Property(a => a.ShippingAddress1).HasColumnName("ShippingAddress1").IsRequired().HasMaxLength(100);
                shippingAddress.Property(a => a.ShippingAddress2).HasColumnName("ShippingAddress2").IsRequired(false).HasMaxLength(100);
                shippingAddress.Property(a => a.ShippingCity).HasColumnName("ShippingCity").IsRequired().HasMaxLength(50);
                shippingAddress.Property(a => a.ShippingState).HasColumnName("ShippingState").IsRequired(false).HasMaxLength(50);
                shippingAddress.Property(a => a.ShippingCountry).HasColumnName("ShippingCountry").IsRequired().HasMaxLength(50);
                shippingAddress.Property(a => a.ShippingZipCode).HasColumnName("ShippingZipCode").IsRequired().HasMaxLength(20);
            });

            // Configure Billing Address as an owned type
            builder.OwnsOne(l => l.BillToAddress, billingAddress =>
            {
                billingAddress.Property(a => a.BillingAddress1).HasColumnName("BillingAddress1").IsRequired().HasMaxLength(100);
                billingAddress.Property(a => a.BillingAddress2).HasColumnName("BillingAddress2").IsRequired(false).HasMaxLength(100);
                billingAddress.Property(a => a.BillingCity).HasColumnName("BillingCity").IsRequired().HasMaxLength(50);
                billingAddress.Property(a => a.BillingState).HasColumnName("BillingState").IsRequired(false).HasMaxLength(50);
                billingAddress.Property(a => a.BillingCountry).HasColumnName("BillingCountry").IsRequired().HasMaxLength(50);
                billingAddress.Property(a => a.BillingZipCode).HasColumnName("BillingZipCode").IsRequired().HasMaxLength(20);
            });
        }
    }
}
