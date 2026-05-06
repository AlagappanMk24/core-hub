using AutoMapper;
using Core_API.Application.DTOs.RecurringInvoice.Response;
using Core_API.Domain.Entities.RecurringInvoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Recurring Invoice Instance mappings.
    /// </summary>
    public class RecurringInvoiceInstanceProfile : Profile
    {
        public RecurringInvoiceInstanceProfile()
        {
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
        }
    }
}