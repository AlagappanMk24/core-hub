using AutoMapper;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Email.EmailSettings;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Application.DTOs.RecurringInvoice.Response;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;

namespace Core_API.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== USER & AUTH MAPPINGS =====
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            CreateMap<LoginDto, ApplicationUser>().ReverseMap();

            // ===== CUSTOMER & ADDRESS MAPPINGS =====
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<Address, AddressDto>();

            // ===== INVOICE MAPPINGS =====
            CreateMap<InvoiceCreateDto, Invoice>()
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
                .ForMember(dest => dest.RecurringInvoice, opt => opt.Ignore());

            CreateMap<InvoiceUpdateDto, Invoice>()
                      .IncludeBase<InvoiceCreateDto, Invoice>()
                      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                      .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.InvoiceStatus))
                      .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus));

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

            // ===== INVOICE ITEM MAPPINGS =====
            CreateMap<InvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Amount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.TaxType, opt => opt.MapFrom(src => src.TaxType))
                .ForMember(dest => dest.TaxPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.IsTaxable, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.TaxType)))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<InvoiceItem, InvoiceItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TaxType, opt => opt.MapFrom(src => src.TaxType))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount));

            // ===== INVOICE TAX DETAIL MAPPINGS =====
            CreateMap<InvoiceTaxDetailDto, InvoiceTaxDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.TaxName, opt => opt.MapFrom(src => src.TaxName))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<InvoiceTaxDetail, InvoiceTaxDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaxName, opt => opt.MapFrom(src => src.TaxName))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount));

            // ===== INVOICE DISCOUNT MAPPINGS =====
            CreateMap<InvoiceDiscountDto, InvoiceDiscount>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.DiscountType))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<InvoiceDiscount, InvoiceDiscountDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));

            // ===== INVOICE PAYMENT MAPPINGS =====
            CreateMap<InvoicePayment, InvoicePaymentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => src.PaymentReference))
                .ForMember(dest => dest.IsRefund, opt => opt.MapFrom(src => src.IsRefund));

            // ===== INVOICE ATTACHMENT MAPPINGS =====
            CreateMap<InvoiceAttachment, InvoiceAttachmentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.FileUrl))
                .ForMember(dest => dest.FileContent, opt => opt.Ignore()); // Not mapped from entity

            CreateMap<InvoiceAttachmentDto, InvoiceAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.FileUrl))
                .ForMember(dest => dest.ContentType, opt => opt.Ignore()) // Set from file
                .ForMember(dest => dest.FileSize, opt => opt.Ignore()) // Set from file
                .ForMember(dest => dest.Description, opt => opt.Ignore()) // Optional
                .ForMember(dest => dest.IsPublic, opt => opt.Ignore()) // Default true
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ===== INVOICE AUDIT LOG MAPPINGS =====
            CreateMap<InvoiceAuditLog, InvoiceAuditLogDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // ===== TAX TYPE MAPPINGS =====
            CreateMap<TaxType, TaxTypeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate));

            CreateMap<TaxTypeCreateDto, TaxType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Set from context
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ===== INVOICE SETTINGS MAPPINGS =====
            CreateMap<InvoiceSettings, InvoiceSettingsDto>()
                .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
                .ForMember(dest => dest.InvoicePrefix, opt => opt.MapFrom(src => src.InvoicePrefix))
                .ForMember(dest => dest.InvoiceStartingNumber, opt => opt.MapFrom(src => src.InvoiceStartingNumber))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));

            CreateMap<InvoiceSettingsDto, InvoiceSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
                .ForMember(dest => dest.InvoicePrefix, opt => opt.MapFrom(src => src.InvoicePrefix))
                .ForMember(dest => dest.InvoiceStartingNumber, opt => opt.MapFrom(src => src.InvoiceStartingNumber))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ===== EMAIL SETTINGS MAPPINGS =====
            CreateMap<EmailSettings, EmailSettingsDto>();
            CreateMap<EmailSettingsDto, EmailSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ===== RECURRING INVOICE MAPPINGS =====
            CreateMap<RecurringInvoiceCreateDto, RecurringInvoice>()
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

            CreateMap<RecurringInvoiceUpdateDto, RecurringInvoice>()
                .IncludeBase<RecurringInvoiceCreateDto, RecurringInvoice>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

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

            // ===== RECURRING INVOICE INSTANCE MAPPINGS =====
            CreateMap<RecurringInvoiceInstance, RecurringInvoiceInstanceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : src.GeneratedInvoiceNumber))
                .ForMember(dest => dest.GeneratedDate, opt => opt.MapFrom(src => src.GeneratedDate))
                .ForMember(dest => dest.ScheduledGenerationDate, opt => opt.MapFrom(src => src.ScheduledGenerationDate))
                .ForMember(dest => dest.SequenceNumber, opt => opt.MapFrom(src => src.SequenceNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.TotalAmount : src.Amount))
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceStatus.ToString() : "Unknown"))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.PaymentStatus.ToString() : "Unknown"))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.IssueDate : (DateTime?)null))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.DueDate : (DateTime?)null))
                .ForMember(dest => dest.GenerationStatus, opt => opt.MapFrom(src => src.GenerationStatus.ToString()))
                .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage));

            // ===== RECURRING INVOICE SUMMARY DTO =====
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
        }
    }
}