using AutoMapper;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Invoice Attachment mappings.
    /// </summary>
    public class InvoiceAttachmentProfile : Profile
    {
        public InvoiceAttachmentProfile()
        {
            #region Entity to DTO
            CreateMap<InvoiceAttachment, InvoiceAttachmentDto>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
              .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.FileUrl))
              .ForMember(dest => dest.FileContent, opt => opt.Ignore()); // Not mapped from entity
            #endregion

            #region DTO to Entity
            CreateMap<InvoiceAttachmentDto, InvoiceAttachment>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
               .ForMember(dest => dest.Invoice, opt => opt.Ignore())
               .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
               .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // Set in service
               .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.FileUrl))
               .ForMember(dest => dest.ContentType, opt => opt.Ignore()) // Set from file
               .ForMember(dest => dest.FileSize, opt => opt.Ignore()) // Set from file
               .ForMember(dest => dest.Description, opt => opt.Ignore()) // Optional
               .ForMember(dest => dest.IsPublic, opt => opt.Ignore()) // Default true
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
               .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            #endregion
        }
    }
}