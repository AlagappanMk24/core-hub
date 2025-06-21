using AutoMapper;
using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDto, ApplicationUser>()
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map UserName from Email
             .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()).ReverseMap();

            CreateMap<LoginDto, ApplicationUser>().ReverseMap();
        }
    }
}
