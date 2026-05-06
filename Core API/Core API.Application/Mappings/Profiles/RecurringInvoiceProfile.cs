using AutoMapper;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Application.DTOs.RecurringInvoice.Response;
using Core_API.Domain.Entities.RecurringInvoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Recurring Invoice main entity mappings.
    /// </summary>
    public class RecurringInvoiceProfile : Profile
    {
        public RecurringInvoiceProfile()
        {
            #region Create DTO to Entity

            CreateMap<CreateRecurringInvoiceDto, RecurringInvoice>()
                // Identity fields
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))

                // Header fields from InvoiceHeaderBase
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Set from context
                .ForMember(dest => dest.BillingAddressId, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingAddressId, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.CurrencyRate, opt => opt.Ignore()) // Default 1.0
                .ForMember(dest => dest.PONumber, opt => opt.Ignore()) // Optional

                // Financial summary (calculated)
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountTotal, opt => opt.Ignore())
                .ForMember(dest => dest.TaxTotal, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())

                // Notes and terms
                .ForMember(dest => dest.CustomerNotes, opt => opt.Ignore())
                .ForMember(dest => dest.InternalNotes, opt => opt.Ignore())
                .ForMember(dest => dest.TermsAndConditions, opt => opt.Ignore())
                .ForMember(dest => dest.FooterNote, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectDetail, opt => opt.Ignore())

                // Payment fields
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())

                // Recurring-specific fields
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.FrequencyInterval, opt => opt.MapFrom(src => src.FrequencyInterval))
                .ForMember(dest => dest.DayOfMonth, opt => opt.MapFrom(src => src.DayOfMonth))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.WeekOfMonth, opt => opt.MapFrom(src => src.WeekOfMonth))
                .ForMember(dest => dest.MonthOfYear, opt => opt.MapFrom(src => src.MonthOfYear))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PausedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CancelledDate, opt => opt.Ignore())
                .ForMember(dest => dest.MaxOccurrences, opt => opt.MapFrom(src => src.MaxOccurrences))
                .ForMember(dest => dest.OccurrencesGenerated, opt => opt.Ignore()) // Default 0
                .ForMember(dest => dest.NextInvoiceDate, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.LastInvoiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Default Draft
                .ForMember(dest => dest.GenerateInAdvanceDays, opt => opt.Ignore()) // Default 0
                .ForMember(dest => dest.AutoSend, opt => opt.MapFrom(src => src.AutoSend))
                .ForMember(dest => dest.AutoEmail, opt => opt.MapFrom(src => src.AutoEmail))
                .ForMember(dest => dest.AutoCharge, opt => opt.Ignore()) // Default false
                .ForMember(dest => dest.ReminderBeforeDue, opt => opt.Ignore()) // Default false
                .ForMember(dest => dest.ReminderDaysBefore, opt => opt.Ignore()) // Default 3
                .ForMember(dest => dest.SourceInvoiceId, opt => opt.MapFrom(src => src.SourceInvoiceId))

                // Override fields
                .ForMember(dest => dest.OverridePONumber, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideCustomerNotes, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideTermsAndConditions, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideFooterNote, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideProjectDetail, opt => opt.Ignore())
                .ForMember(dest => dest.OverridePaymentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.OverridePaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideShippingAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideAdjustmentAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OverrideAdjustmentDescription, opt => opt.Ignore())

                // Collections
                .ForMember(dest => dest.GeneratedInvoices, opt => opt.Ignore())
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
                .ForMember(dest => dest.SourceInvoice, opt => opt.Ignore());

            #endregion

            #region Update DTO to Entity

            CreateMap<UpdateRecurringInvoiceDto, RecurringInvoice>()
                .IncludeBase<CreateRecurringInvoiceDto, RecurringInvoice>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            #endregion

            #region Entity to Response DTO

            CreateMap<RecurringInvoice, RecurringInvoiceResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.CurrencyRate, opt => opt.MapFrom(src => src.CurrencyRate))
                .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PONumber))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
                .ForMember(dest => dest.DiscountTotal, opt => opt.MapFrom(src => src.DiscountTotal))
                .ForMember(dest => dest.TaxTotal, opt => opt.MapFrom(src => src.TaxTotal))
                .ForMember(dest => dest.ShippingAmount, opt => opt.MapFrom(src => src.ShippingAmount))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.CustomerNotes, opt => opt.MapFrom(src => src.CustomerNotes))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.TermsAndConditions, opt => opt.MapFrom(src => src.TermsAndConditions))
                .ForMember(dest => dest.FooterNote, opt => opt.MapFrom(src => src.FooterNote))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.PaymentTerms, opt => opt.MapFrom(src => src.PaymentTerms))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency.ToString()))
                .ForMember(dest => dest.FrequencyInterval, opt => opt.MapFrom(src => src.FrequencyInterval))
                .ForMember(dest => dest.DayOfMonth, opt => opt.MapFrom(src => src.DayOfMonth))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek.HasValue ? src.DayOfWeek.Value.ToString() : null))
                .ForMember(dest => dest.WeekOfMonth, opt => opt.MapFrom(src => src.WeekOfMonth))
                .ForMember(dest => dest.MonthOfYear, opt => opt.MapFrom(src => src.MonthOfYear))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PausedDate, opt => opt.MapFrom(src => src.PausedDate))
                .ForMember(dest => dest.CancelledDate, opt => opt.MapFrom(src => src.CancelledDate))
                .ForMember(dest => dest.MaxOccurrences, opt => opt.MapFrom(src => src.MaxOccurrences))
                .ForMember(dest => dest.OccurrencesGenerated, opt => opt.MapFrom(src => src.OccurrencesGenerated))
                .ForMember(dest => dest.NextInvoiceDate, opt => opt.MapFrom(src => src.NextInvoiceDate))
                .ForMember(dest => dest.LastInvoiceDate, opt => opt.MapFrom(src => src.LastInvoiceDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.GenerateInAdvanceDays, opt => opt.MapFrom(src => src.GenerateInAdvanceDays))
                .ForMember(dest => dest.AutoSend, opt => opt.MapFrom(src => src.AutoSend))
                .ForMember(dest => dest.AutoEmail, opt => opt.MapFrom(src => src.AutoEmail))
                .ForMember(dest => dest.AutoCharge, opt => opt.MapFrom(src => src.AutoCharge))
                .ForMember(dest => dest.ReminderBeforeDue, opt => opt.MapFrom(src => src.ReminderBeforeDue))
                .ForMember(dest => dest.ReminderDaysBefore, opt => opt.MapFrom(src => src.ReminderDaysBefore))
                .ForMember(dest => dest.SourceInvoiceId, opt => opt.MapFrom(src => src.SourceInvoiceId))
                .ForMember(dest => dest.SourceInvoiceNumber, opt => opt.MapFrom(src => src.SourceInvoice != null ? src.SourceInvoice.InvoiceNumber : null))
                .ForMember(dest => dest.OverridePONumber, opt => opt.MapFrom(src => src.OverridePONumber))
                .ForMember(dest => dest.OverrideCustomerNotes, opt => opt.MapFrom(src => src.OverrideCustomerNotes))
                .ForMember(dest => dest.OverrideTermsAndConditions, opt => opt.MapFrom(src => src.OverrideTermsAndConditions))
                .ForMember(dest => dest.OverrideFooterNote, opt => opt.MapFrom(src => src.OverrideFooterNote))
                .ForMember(dest => dest.OverrideProjectDetail, opt => opt.MapFrom(src => src.OverrideProjectDetail))
                .ForMember(dest => dest.OverridePaymentMethod, opt => opt.MapFrom(src => src.OverridePaymentMethod))
                .ForMember(dest => dest.OverridePaymentTerms, opt => opt.MapFrom(src => src.OverridePaymentTerms))
                .ForMember(dest => dest.OverrideShippingAmount, opt => opt.MapFrom(src => src.OverrideShippingAmount))
                .ForMember(dest => dest.OverrideAdjustmentAmount, opt => opt.MapFrom(src => src.OverrideAdjustmentAmount))
                .ForMember(dest => dest.OverrideAdjustmentDescription, opt => opt.MapFrom(src => src.OverrideAdjustmentDescription))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))

                // Nested DTOs
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.GeneratedInvoices, opt => opt.MapFrom(src => src.GeneratedInvoices))

                // Calculated fields
                .ForMember(dest => dest.TotalGeneratedCount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalGeneratedValue, opt => opt.Ignore())
                .ForMember(dest => dest.AverageInvoiceValue, opt => opt.Ignore())
                .ForMember(dest => dest.EstimatedCompletionDate, opt => opt.Ignore());

            #endregion

            #region Entity to Summary DTO

            CreateMap<RecurringInvoice, RecurringInvoiceSummaryDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
               .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency.ToString()))
               .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
               .ForMember(dest => dest.NextInvoiceDate, opt => opt.MapFrom(src => src.NextInvoiceDate))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
               .ForMember(dest => dest.OccurrencesGenerated, opt => opt.MapFrom(src => src.OccurrencesGenerated))
               .ForMember(dest => dest.MaxOccurrences, opt => opt.MapFrom(src => src.MaxOccurrences))
               .ForMember(dest => dest.TotalGeneratedValue, opt => opt.Ignore()) // Calculated
               .ForMember(dest => dest.AutoSend, opt => opt.MapFrom(src => src.AutoSend));

            #endregion
        }
    }
}