using AutoMapper;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Audit Log mappings.
    /// </summary>
    public class InvoiceAuditLogProfile : Profile
    {
        public InvoiceAuditLogProfile()
        {
            CreateMap<InvoiceAuditLog, InvoiceAuditLogDto>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                 .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes))
                 .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                 .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));
        }
    }
}