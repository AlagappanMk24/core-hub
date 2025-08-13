using AutoMapper;
using Core_API.Application.DTOs.Authentication.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.EmailDto.EmailSettings;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;

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

            // Invoice mappings
            CreateMap<InvoiceCreateDto, Invoice>()
                .ForMember(dest => dest.InvoiceItems, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TaxDetails, opt => opt.MapFrom(src => src.TaxDetails))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts))
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.Tax, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceStatus, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore());

            CreateMap<InvoiceUpdateDto, Invoice>()
                .IncludeBase<InvoiceCreateDto, Invoice>()
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => Enum.Parse<InvoiceStatus>(src.InvoiceStatus, true)))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => Enum.Parse<PaymentStatus>(src.PaymentStatus, true)));

            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.InvoiceType.ToString()))
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => src.InvoiceStatus.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.PaymentDue))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.InvoiceItems))
                .ForMember(dest => dest.TaxDetails, opt => opt.MapFrom(src => src.TaxDetails)) 
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts)) 
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.PdfStream, opt => opt.Ignore());

            // Invoice item mappings
            CreateMap<InvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<InvoiceItem, InvoiceItemDto>();

            // Tax detail mappings
            CreateMap<TaxDetailDto, TaxDetail>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<TaxDetail, TaxDetailDto>();

            // Discount mappings
            CreateMap<DiscountDto, Discount>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<Discount, DiscountDto>();

            // Customer mappings
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<Address, AddressDto>();

            // Tax type mappings
            CreateMap<TaxType, TaxTypeDto>();
            CreateMap<TaxTypeCreateDto, TaxType>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            // Invoice settings mappings
            CreateMap<InvoiceSettings, InvoiceSettingsDto>();
            CreateMap<InvoiceSettingsDto, InvoiceSettings>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            // Email settings mappings
            CreateMap<EmailSettings, EmailSettingsDto>();
            CreateMap<EmailSettingsDto, EmailSettings>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}