using AutoMapper;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice main entity mappings.
    /// </summary>
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            #region Create DTO to Invoice

            CreateMap<CreateInvoiceDto, Invoice>()
                // Identity fields (ignored - system generated)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber))

                // Header fields from InvoiceHeaderBase
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Set from context
                .ForMember(dest => dest.BillingAddressId, opt => opt.MapFrom(src => src.BillingAddressId))
                .ForMember(dest => dest.ShippingAddressId, opt => opt.MapFrom(src => src.ShippingAddressId))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.CurrencyRate, opt => opt.MapFrom(src => src.CurrencyRate))
                .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PONumber))

                // Financial summary (calculated - ignore from DTO)
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountTotal, opt => opt.Ignore())
                .ForMember(dest => dest.TaxTotal, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())

                // Notes and terms - Map these from DTO
                .ForMember(dest => dest.CustomerNotes, opt => opt.MapFrom(src => src.CustomerNotes))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.TermsAndConditions, opt => opt.MapFrom(src => src.TermsAndConditions))
                .ForMember(dest => dest.FooterNote, opt => opt.MapFrom(src => src.FooterNote))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.ProjectDetail))

                // Payment fields - Map from DTO
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.PaymentTerms, opt => opt.MapFrom(src => src.PaymentTerms))
                .ForMember(dest => dest.PaymentGateway, opt => opt.MapFrom(src => src.PaymentGateway))

                // Invoice-specific fields
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.SentDate, opt => opt.Ignore())
                .ForMember(dest => dest.PaidDate, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.InvoiceStatus ?? InvoiceStatus.Draft))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus ?? PaymentStatus.Pending))
                .ForMember(dest => dest.InvoiceType, opt => opt.MapFrom(src => src.InvoiceType))

                // Financial adjustments - Map from DTO
                .ForMember(dest => dest.ShippingAmount, opt => opt.MapFrom(src => src.ShippingAmount))
                .ForMember(dest => dest.AdjustmentAmount, opt => opt.MapFrom(src => src.AdjustmentAmount))
                .ForMember(dest => dest.AdjustmentDescription, opt => opt.MapFrom(src => src.AdjustmentDescription))

                .ForMember(dest => dest.AmountPaid, opt => opt.Ignore()) // Default 0
                .ForMember(dest => dest.AmountDue, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.AmountRefunded, opt => opt.Ignore()) // Default 0
                .ForMember(dest => dest.PaymentTransactionId, opt => opt.Ignore())
                .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
                .ForMember(dest => dest.RecurringInvoiceId, opt => opt.MapFrom(src => src.RecurringInvoiceId))
                .ForMember(dest => dest.SourceSystem, opt => opt.MapFrom(src => src.SourceSystem))

                // Collections
                .ForMember(dest => dest.InvoiceItems, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TaxDetails, opt => opt.MapFrom(src => src.TaxDetails))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts))
                .ForMember(dest => dest.InvoiceAttachments, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())

                // BaseEntity fields
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())

                // Navigation properties
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringInvoice, opt => opt.Ignore())

                .ForMember(dest => dest.BaseCurrencySubtotal, opt => opt.Ignore())
                .ForMember(dest => dest.BaseCurrencyTotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.BaseCurrencyAmountPaid, opt => opt.Ignore())
                .ForMember(dest => dest.BaseCurrencyAmountDue, opt => opt.Ignore());

            #endregion

            #region Update DTO to Invoice
            CreateMap<UpdateInvoiceDto, Invoice>()
                          .IncludeBase<CreateInvoiceDto, Invoice>()
                          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                          .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.InvoiceStatus))
                          .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus));
            #endregion

            #region Invoice to Response DTO
            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber))
                .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PONumber))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.SentDate, opt => opt.MapFrom(src => src.SentDate))
                .ForMember(dest => dest.PaidDate, opt => opt.MapFrom(src => src.PaidDate))
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.InvoiceStatus.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.InvoiceType.ToString()))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.CurrencyRate, opt => opt.MapFrom(src => src.CurrencyRate))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
                .ForMember(dest => dest.DiscountTotal, opt => opt.MapFrom(src => src.DiscountTotal))
                .ForMember(dest => dest.TaxTotal, opt => opt.MapFrom(src => src.TaxTotal))
                .ForMember(dest => dest.ShippingAmount, opt => opt.MapFrom(src => src.ShippingAmount))
                .ForMember(dest => dest.AdjustmentAmount, opt => opt.MapFrom(src => src.AdjustmentAmount))
                .ForMember(dest => dest.AdjustmentDescription, opt => opt.MapFrom(src => src.AdjustmentDescription))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.AmountDue, opt => opt.MapFrom(src => src.AmountDue))
                .ForMember(dest => dest.AmountRefunded, opt => opt.MapFrom(src => src.AmountRefunded))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.PaymentGateway, opt => opt.MapFrom(src => src.PaymentGateway))
                .ForMember(dest => dest.PaymentTerms, opt => opt.MapFrom(src => src.PaymentTerms))
                .ForMember(dest => dest.CustomerNotes, opt => opt.MapFrom(src => src.CustomerNotes))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.TermsAndConditions, opt => opt.MapFrom(src => src.TermsAndConditions))
                .ForMember(dest => dest.FooterNote, opt => opt.MapFrom(src => src.FooterNote))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
                .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(src => src.IsRecurring))
                .ForMember(dest => dest.RecurringInvoiceId, opt => opt.MapFrom(src => src.RecurringInvoiceId))
                .ForMember(dest => dest.SourceSystem, opt => opt.MapFrom(src => src.SourceSystem))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.InternalNotes ?? string.Empty))
                // Nested DTOs
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.InvoiceItems))
                .ForMember(dest => dest.TaxDetails, opt => opt.MapFrom(src => src.TaxDetails))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts))
                .ForMember(dest => dest.InvoiceAttachments, opt => opt.MapFrom(src => src.InvoiceAttachments))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments))
                .ForMember(dest => dest.AuditLogs, opt => opt.MapFrom(src => src.AuditLogs))

                // Special mappings
                .ForMember(dest => dest.PdfStream, opt => opt.Ignore());
            #endregion
        }
    }
}