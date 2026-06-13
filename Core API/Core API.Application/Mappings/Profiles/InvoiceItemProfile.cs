using AutoMapper;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Item mappings.
    /// </summary>
    public class InvoiceItemProfile : Profile
    {
        public InvoiceItemProfile()
        {
            #region DTO to Entity
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
            #endregion

            #region Entity to DTO
            CreateMap<InvoiceItem, InvoiceItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TaxType, opt => opt.MapFrom(src => src.TaxType))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount));
            #endregion
        }
    }
}
