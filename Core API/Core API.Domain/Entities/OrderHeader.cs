using Core_API.Domain.Entities.Common;
using Core_API.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities
{
    public class OrderHeader : BaseEntity
    {
        // Identifiers
        public string? ApplicationUserId { get; set; }
        public int? CustomerId { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? TransactionId { get; set; }

        // Dates
        public DateTime OrderDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateOnly PaymentDueDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }

        // Monetary Values
        public decimal Subtotal { get; set; } // Before tax and discount
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal OrderTotal { get; set; } // Subtotal + Tax - Discount + ShippingFee
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; } // OrderTotal - AmountPaid
        public decimal ShippingCharges { get; set; }

        // Statuses
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? DeliveryStatus { get; set; }

        // Shipping Details
        public string? ShippingMethod { get; set; }
        public string? DeliveryMethod { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? TrackingUrl { get; set; }

        // Payment Details
        public string? PaymentMethod { get; set; }

        // Contact Information
        public string? ShippingContactName { get; set; }
        public string? ShippingContactPhone { get; set; }

        // Notes
        public string? CustomerNotes { get; set; }

        // Addresses
        public ShippingAddress ShipToAddress { get; set; }
        public BillingAddress BillToAddress { get; set; }

        // Navigation Properties
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("CustomerId")]
        [ValidateNever]
        public Customer Customer { get; set; }

        public List<OrderActivityLog> OrderActivityLog { get; set; } = new List<OrderActivityLog>();

        [ValidateNever]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        [ValidateNever]
        public ICollection<Invoice>? Invoices { get; set; }
    }
}
