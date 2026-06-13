using AutoMapper;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Discount mappings.
    /// </summary>
    public class InvoiceDiscountProfile : Profile
    {
        public InvoiceDiscountProfile()
        {
            #region DTO to Entity
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
            #endregion

            #region Entity to DTO
            CreateMap<InvoiceDiscount, InvoiceDiscountDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));
            #endregion
        }
    }
}