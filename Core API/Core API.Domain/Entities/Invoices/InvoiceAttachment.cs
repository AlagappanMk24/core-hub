using Core_API.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Represents a file attached to an invoice, such as a supporting document,
    /// signed copy, or proof of delivery.
    /// </summary>
    public class InvoiceAttachment : BaseEntity
    {
        // ── Invoice Reference ─────────────────────────────────────────────────
        /// <summary>
        /// Foreign key to the invoice this attachment belongs to.
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        // ── File Identity ─────────────────────────────────────────────────────

        /// <summary>
        /// Original name of the uploaded file including extension (e.g., "invoice_signed.pdf").
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        /// <summary>
        /// Server-side file system path where the file is stored.
        /// </summary>
        [StringLength(1000)]
        public string? FilePath { get; set; }

        /// <summary>
        /// Publicly accessible URL for cloud-hosted files (e.g., S3, Azure Blob). Null for local storage.
        /// </summary>
        [StringLength(500)]
        public string? FileUrl { get; set; }

        // ── File Metadata ─────────────────────────────────────────────────────

        /// <summary>
        /// MIME type of the file (e.g., "application/pdf", "image/png").
        /// </summary>
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        [Required]
        public long FileSize { get; set; }

        /// <summary>
        /// Optional description or label for the attachment (e.g., "Signed Invoice Copy").
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        // ── Visibility ────────────────────────────────────────────────────────

        /// <summary>
        /// Controls whether the customer can view this attachment via the customer portal.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool IsPublic { get; set; } = true;

    }
}
