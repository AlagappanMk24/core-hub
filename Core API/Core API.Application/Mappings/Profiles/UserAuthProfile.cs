using AutoMapper;
using Core_API.Application.DTOs.Auth.Requests;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for User and Authentication related mappings.
    /// </summary>
    public class UserAuthProfile : Profile
    {
        public UserAuthProfile()
        {
            #region Register Mapping

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

            #endregion

            #region Login Mapping

            CreateMap<LoginDto, ApplicationUser>().ReverseMap();

            #endregion
        }
    }
}
