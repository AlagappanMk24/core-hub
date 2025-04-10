using AutoMapper;
using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Domain.Models.Entities;

namespace Core_API.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDto, ApplicationUser>()
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map UserName from Email
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
             .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src=> src.StreetAddress))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
             .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
             .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()).ReverseMap();

            CreateMap<LoginDto, ApplicationUser>().ReverseMap();
        }
    }
}
