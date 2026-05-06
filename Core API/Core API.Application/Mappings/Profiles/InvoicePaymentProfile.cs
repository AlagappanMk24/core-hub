using AutoMapper;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Payment mappings.
    /// </summary>
    public class InvoicePaymentProfile : Profile
    {
        public InvoicePaymentProfile()
        {
            CreateMap<InvoicePayment, InvoicePaymentDto>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
              .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => src.ReferenceNumber))
              .ForMember(dest => dest.IsRefund, opt => opt.MapFrom(src => src.IsRefund));
        }
    }
}
