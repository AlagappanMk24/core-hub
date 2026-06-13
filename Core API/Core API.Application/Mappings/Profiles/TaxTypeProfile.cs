using AutoMapper;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Tax Type mappings.
    /// </summary>
    public class TaxTypeProfile : Profile
    {
        public TaxTypeProfile()
        {
            #region Entity to DTO

            CreateMap<TaxType, TaxTypeDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate));

            #endregion

            #region Create DTO to Entity

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

            #endregion
        }
    }
}