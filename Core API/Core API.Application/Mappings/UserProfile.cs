using AutoMapper;
using Core_API.Application.DTOs.User.Request;
using Core_API.Application.DTOs.User.Response;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Exceptions;
using Core_API.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom<EmailValueResolver>())
                .ForMember(dest => dest.Email, opt => opt.MapFrom<EmailValueResolver>())
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom<PhoneNumberValueResolver>())
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom<CountryCodeValueResolver>())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ReverseMap();

            // New mapping for UserUpsertVM
            CreateMap<UserDto, UserUpsertResponse>()
                .ForMember(dest => dest.SelectedRoles, opt => opt.MapFrom(src => src.Roles ?? new List<string>()))
                .ForMember(dest => dest.RoleList, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyList, opt => opt.Ignore())
                .ReverseMap();
        }
    }
    public class EmailValueResolver : IValueResolver<UserDto, ApplicationUser, string>
    {
        public string Resolve(UserDto source, ApplicationUser destination, string destMember, ResolutionContext context)
        {
            try
            {
                var validatedEmail = new Domain.ValueObjects.Email(source.Email);
                return validatedEmail.Value;
            }
            catch (InvalidEmailException ex)
            {
                throw new ValidationException($"Email validation failed: {ex.Message}");
            }
        }
    }
    public class PhoneNumberValueResolver : IValueResolver<UserDto, ApplicationUser, string>
    {
        public string Resolve(UserDto source, ApplicationUser destination, string destMember, ResolutionContext context)
        {
            try
            {
                var validatedPhone = new PhoneNumber(source.PhoneNumber, source.CountryCode);
                return validatedPhone.NationalNumber;
            }
            catch (InvalidPhoneException ex)
            {
                throw new ValidationException($"Phone validation failed: {ex.Message}");
            }
        }
    }

    public class CountryCodeValueResolver : IValueResolver<UserDto, ApplicationUser, string>
    {
        public string Resolve(UserDto source, ApplicationUser destination, string destMember, ResolutionContext context)
        {
            try
            {
                var validatedPhone = new PhoneNumber(source.PhoneNumber, source.CountryCode);
                return validatedPhone.CountryCode;
            }
            catch (InvalidPhoneException ex)
            {
                throw new ValidationException($"Phone validation failed: {ex.Message}");
            }
        }
    }
}
