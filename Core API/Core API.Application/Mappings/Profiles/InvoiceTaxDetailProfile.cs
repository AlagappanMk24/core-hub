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
    /// AutoMapper profile for Invoice Tax Detail mappings.
    /// </summary>
    public class InvoiceTaxDetailProfile : Profile
    {
        public InvoiceTaxDetailProfile()
        {
            #region DTO to Entity
            CreateMap<InvoiceTaxDetailDto, InvoiceTaxDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.TaxName, opt => opt.MapFrom(src => src.TaxName))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore()) // Calculated
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            #endregion

            #region Entity to DTO
            CreateMap<InvoiceTaxDetail, InvoiceTaxDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaxName, opt => opt.MapFrom(src => src.TaxName))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount));
            #endregion
        }
    }
}