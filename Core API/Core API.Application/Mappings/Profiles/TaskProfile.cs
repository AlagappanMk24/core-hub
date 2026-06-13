using AutoMapper;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;

namespace Core_API.Application.Mappings.Profiles
{
    /// <summary>
    /// AutoMapper profile for Task entity mappings.
    /// </summary>
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            #region Entity to Response DTO

            CreateMap<TaskItem, TaskDto>()
              .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
              .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
              .ForMember(dest => dest.AssignedToUserName, opt => opt.MapFrom(src => src.AssignedToUser != null ? src.AssignedToUser.UserName : null))
              .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName : null))
              .ForMember(dest => dest.ParentTaskTitle, opt => opt.MapFrom(src => src.ParentTask != null ? src.ParentTask.Title : null))
              .ForMember(dest => dest.SubtaskCount, opt => opt.MapFrom(src => src.Subtasks != null ? src.Subtasks.Count(s => !s.IsDeleted) : 0))
              .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count(c => !c.IsDeleted) : 0))
              .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments != null ? src.Attachments.Count(a => !a.IsDeleted) : 0))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
              .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedDate))
              .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Where(c => !c.IsDeleted)))
              .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments.Where(a => !a.IsDeleted)))
              .ForMember(dest => dest.Subtasks, opt => opt.MapFrom(src => src.Subtasks.Where(s => !s.IsDeleted)))
              .ReverseMap();

            #endregion

            #region Create DTO to Entity
            CreateMap<CreateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.TaskStatus.Pending))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.Subtasks, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
                .ForMember(dest => dest.ActualHours, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag))
                .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId))
                .ForMember(dest => dest.ParentTaskId, opt => opt.MapFrom(src => src.ParentTaskId))
                .ForMember(dest => dest.ReminderDate, opt => opt.MapFrom(src => src.ReminderDate))
                .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(src => src.IsRecurring))
                .ForMember(dest => dest.RecurrencePattern, opt => opt.MapFrom(src => src.RecurrencePattern))
                .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours));
            #endregion

            #region Update DTO to Entity

            CreateMap<UpdateTaskDto, TaskItem>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
               .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
               .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
               .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
               .ForMember(dest => dest.Subtasks, opt => opt.Ignore())
               .ForMember(dest => dest.Comments, opt => opt.Ignore())
               .ForMember(dest => dest.Attachments, opt => opt.Ignore())
               .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
               .ForMember(dest => dest.ReminderDate, opt => opt.MapFrom(src => src.ReminderDate))
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

        }
    }
}