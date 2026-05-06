using AutoMapper;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Domain.Entities.Settings;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Email Settings mappings.
    /// </summary>
    public class EmailSettingsProfile : Profile
    {
        public EmailSettingsProfile()
        {
            CreateMap<EmailSettings, EmailSettingsDto>();

            CreateMap<EmailSettingsDto, EmailSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}
