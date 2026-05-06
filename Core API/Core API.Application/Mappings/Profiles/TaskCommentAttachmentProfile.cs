using AutoMapper;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Task Comment and Task Attachment mappings.
    /// </summary>
    public class TaskCommentAttachmentProfile : Profile
    {
        public TaskCommentAttachmentProfile()
        {
            #region Task Comment Mappings

            CreateMap<TaskComment, TaskCommentDto>()
                      .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                      .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                      .ReverseMap();

            CreateMap<CreateTaskCommentDto, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            #endregion

            #region Task Attachment Mappings

            CreateMap<TaskAttachment, TaskAttachmentDto>()
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
             .ReverseMap();

            #endregion

        }
    }
}