using AutoMapper;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Settings mappings.
    /// </summary>
    public class InvoiceSettingsProfile : Profile
    {
        public InvoiceSettingsProfile()
        {
            #region Entity to DTO

            CreateMap<InvoiceSettings, InvoiceSettingsDto>()
              .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
              .ForMember(dest => dest.InvoicePrefix, opt => opt.MapFrom(src => src.InvoicePrefix))
              .ForMember(dest => dest.InvoiceStartingNumber, opt => opt.MapFrom(src => src.InvoiceStartingNumber))
              .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
              // Discount settings
              .ForMember(dest => dest.EnableItemLevelDiscounts, opt => opt.MapFrom(src => src.EnableItemLevelDiscounts))
              .ForMember(dest => dest.EnableOverallDiscounts, opt => opt.MapFrom(src => src.EnableOverallDiscounts))
              .ForMember(dest => dest.DefaultDiscountType, opt => opt.MapFrom(src => src.DefaultDiscountType))
              .ForMember(dest => dest.MaxDiscountPercentage, opt => opt.MapFrom(src => src.MaxDiscountPercentage))
              .ForMember(dest => dest.MaxDiscountAmount, opt => opt.MapFrom(src => src.MaxDiscountAmount))
              .ForMember(dest => dest.AllowMultipleDiscounts, opt => opt.MapFrom(src => src.AllowMultipleDiscounts))
              .ForMember(dest => dest.ApplyDiscountBeforeTax, opt => opt.MapFrom(src => src.ApplyDiscountBeforeTax))
              .ForMember(dest => dest.ShowDiscountColumnOnInvoice, opt => opt.MapFrom(src => src.ShowDiscountColumnOnInvoice));

            #endregion

            #region DTO to Entity

            CreateMap<InvoiceSettingsDto, InvoiceSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsAutomated, opt => opt.MapFrom(src => src.IsAutomated))
                .ForMember(dest => dest.InvoicePrefix, opt => opt.MapFrom(src => src.InvoicePrefix))
                .ForMember(dest => dest.InvoiceStartingNumber, opt => opt.MapFrom(src => src.InvoiceStartingNumber))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                // Discount settings
                .ForMember(dest => dest.EnableItemLevelDiscounts, opt => opt.MapFrom(src => src.EnableItemLevelDiscounts))
                .ForMember(dest => dest.EnableOverallDiscounts, opt => opt.MapFrom(src => src.EnableOverallDiscounts))
                .ForMember(dest => dest.DefaultDiscountType, opt => opt.MapFrom(src => src.DefaultDiscountType))
                .ForMember(dest => dest.MaxDiscountPercentage, opt => opt.MapFrom(src => src.MaxDiscountPercentage))
                .ForMember(dest => dest.MaxDiscountAmount, opt => opt.MapFrom(src => src.MaxDiscountAmount))
                .ForMember(dest => dest.AllowMultipleDiscounts, opt => opt.MapFrom(src => src.AllowMultipleDiscounts))
                .ForMember(dest => dest.ApplyDiscountBeforeTax, opt => opt.MapFrom(src => src.ApplyDiscountBeforeTax))
                .ForMember(dest => dest.ShowDiscountColumnOnInvoice, opt => opt.MapFrom(src => src.ShowDiscountColumnOnInvoice));

            #endregion
        }
    }
}