// Core_API.Application/Contracts/Services/ITaskService.cs
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Microsoft.AspNetCore.Http;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Application.Contracts.Services.Tasks
{
    public interface ITaskService
    {
        // Basic CRUD
        Task<OperationResult<TaskDto>> GetTaskByIdAsync(int id, OperationContext context);
        Task<OperationResult<IEnumerable<TaskDto>>> GetAllTasksAsync(OperationContext context, TaskFilterDto? filter = null);
        Task<OperationResult<TaskDto>> CreateTaskAsync(CreateTaskDto createDto, OperationContext context);
        Task<OperationResult<TaskDto>> UpdateTaskAsync(int id, UpdateTaskDto updateDto, OperationContext context);
        Task<OperationResult<bool>> DeleteTaskAsync(int id, OperationContext context);

        // Task management
        Task<OperationResult<TaskDto>> UpdateTaskStatusAsync(int id, TaskStatus status, OperationContext context);
        Task<OperationResult<TaskDto>> AssignTaskAsync(int id, string userId, OperationContext context);
        Task<OperationResult<bool>> CompleteTaskAsync(int id, OperationContext context);

        // Comments
        Task<OperationResult<TaskCommentDto>> AddCommentAsync(int taskId, CreateTaskCommentDto commentDto, OperationContext context);
        Task<OperationResult<IEnumerable<TaskCommentDto>>> GetCommentsAsync(int taskId, OperationContext context);

        // Attachments
        Task<OperationResult<TaskAttachmentDto>> AddAttachmentAsync(int taskId, IFormFile file, OperationContext context);
        Task<OperationResult<IEnumerable<TaskAttachmentDto>>> GetAttachmentsAsync(int taskId, OperationContext context);
        Task<OperationResult<bool>> DeleteAttachmentAsync(int taskId, int attachmentId, OperationContext context);

        // Statistics and Reports
        Task<OperationResult<TaskStatsDto>> GetTaskStatsAsync(OperationContext context);
        Task<OperationResult<IEnumerable<TaskDto>>> GetOverdueTasksAsync(OperationContext context);
        Task<OperationResult<IEnumerable<TaskDto>>> GetTasksDueSoonAsync(OperationContext context, int days = 3);
        Task<OperationResult<IEnumerable<TaskDto>>> GetTasksDueTodayAsync(OperationContext context);
        Task<OperationResult<IEnumerable<TaskDto>>> GetMyTasksAsync(OperationContext context, TaskFilterDto? filter = null);
    }
}