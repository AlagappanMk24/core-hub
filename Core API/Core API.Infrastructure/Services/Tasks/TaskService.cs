using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Tasks;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Domain.Entities.Tasks;
using Core_API.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Infrastructure.Services.Tasks
{
    public class TaskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaskService> logger) : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<TaskService> _logger = logger;

        public async Task<OperationResult<TaskDto>> GetTaskByIdAsync(int id, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetTaskWithDetailsAsync(id);

                if (task == null)
                    return OperationResult<TaskDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskDto>.FailureResult("You don't have permission to view this task");

                var taskDto = _mapper.Map<TaskDto>(task);
                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {TaskId}", id);
                return OperationResult<TaskDto>.FailureResult("Failed to retrieve task");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskDto>>> GetAllTasksAsync(OperationContext context, TaskFilterDto? filter = null)
        {
            try
            {
                var query = _unitOfWork.TaskItems.Query()
                    .Where(t => !t.IsDeleted);

                // Apply user-based filtering
                if (!context.IsSuperAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId);
                }

                // Apply filters
                query = ApplyFilters(query, filter);

                // Apply sorting
                query = ApplySorting(query, filter);

                // Apply pagination
                if (filter != null)
                {
                    query = query.Skip((filter.Page - 1) * filter.PageSize)
                                 .Take(filter.PageSize);
                }

                var tasks = await query
                 .Include(t => t.AssignedToUser)
                 .Include(t => t.CreatedByUser)
                 .Include(t => t.ParentTask)
                 .Include(t => t.Subtasks.Where(s => !s.IsDeleted))
                 .Include(t => t.Comments.Where(c => !c.IsDeleted))
                     .ThenInclude(c => c.User)
                 .Include(t => t.Attachments.Where(a => !a.IsDeleted))
                 .ToListAsync();

                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tasks");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks");
            }
        }
        public async Task<OperationResult<TaskDto>> CreateTaskAsync(CreateTaskDto createDto, OperationContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(context.UserId))
                    return OperationResult<TaskDto>.FailureResult("Invalid user context");

                var task = _mapper.Map<TaskItem>(createDto);
                task.CreatedByUserId = context.UserId;
                task.CreatedDate = DateTime.UtcNow;

                // Set default assigned user if not specified
                if (string.IsNullOrEmpty(task.AssignedToUserId))
                {
                    task.AssignedToUserId = context.UserId;
                }

                await _unitOfWork.TaskItems.AddAsync(task);
                await _unitOfWork.SaveChangesAsync();

                await LogTaskAction(task.Id, "Created", $"Task created by {context.UserId}", context);

                var createdTask = await _unitOfWork.TaskItems.GetTaskWithDetailsAsync(task.Id);
                var taskDto = _mapper.Map<TaskDto>(createdTask);
                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return OperationResult<TaskDto>.FailureResult("Failed to create task");
            }
        }
        public async Task<OperationResult<TaskDto>> UpdateTaskAsync(int id, UpdateTaskDto updateDto, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetTaskWithDetailsAsync(id);

                if (task == null)
                    return OperationResult<TaskDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskDto>.FailureResult("You don't have permission to update this task");

                // Track changes for audit log - collect only changed fields
                var changedFields = new List<string>();

                if (updateDto.Title != null && task.Title != updateDto.Title)
                {
                    changedFields.Add($"Title: '{task.Title}' → '{updateDto.Title}'");
                    task.Title = updateDto.Title;
                }

                if (updateDto.Description != null && task.Description != updateDto.Description)
                {
                    changedFields.Add($"Description updated");
                    task.Description = updateDto.Description;
                }

                if (updateDto.Priority.HasValue && task.Priority != updateDto.Priority.Value)
                {
                    changedFields.Add($"Priority: {task.Priority} → {updateDto.Priority.Value}");
                    task.Priority = updateDto.Priority.Value;
                }

                if (updateDto.Status.HasValue && task.Status != updateDto.Status.Value)
                {
                    changedFields.Add($"Status: {task.Status} → {updateDto.Status.Value}");
                    task.Status = updateDto.Status.Value;

                    if (updateDto.Status.Value == TaskStatus.Completed)
                        task.CompletedAt = DateTime.UtcNow;
                }

                if (updateDto.DueDate.HasValue && task.DueDate != updateDto.DueDate.Value)
                {
                    changedFields.Add($"DueDate: {task.DueDate?.ToString("yyyy-MM-dd")} → {updateDto.DueDate.Value.ToString("yyyy-MM-dd")}");
                    task.DueDate = updateDto.DueDate.Value;
                }

                if (updateDto.ReminderDate.HasValue && task.ReminderDate != updateDto.ReminderDate.Value)
                {
                    changedFields.Add($"ReminderDate: {task.ReminderDate?.ToString("yyyy-MM-dd HH:mm")} → {updateDto.ReminderDate.Value.ToString("yyyy-MM-dd HH:mm")}");
                    task.ReminderDate = updateDto.ReminderDate.Value;
                }

                if (updateDto.Category != null && task.Category != updateDto.Category)
                {
                    changedFields.Add($"Category: '{task.Category}' → '{updateDto.Category}'");
                    task.Category = updateDto.Category;
                }

                if (updateDto.Tag != null && task.Tag != updateDto.Tag)
                {
                    changedFields.Add($"Tag: '{task.Tag}' → '{updateDto.Tag}'");
                    task.Tag = updateDto.Tag;
                }

                if (!string.IsNullOrEmpty(updateDto.AssignedToUserId) && task.AssignedToUserId != updateDto.AssignedToUserId)
                {
                    changedFields.Add($"AssignedTo: {task.AssignedToUserId} → {updateDto.AssignedToUserId}");
                    task.AssignedToUserId = updateDto.AssignedToUserId;
                }

                if (updateDto.EstimatedHours.HasValue && task.EstimatedHours != updateDto.EstimatedHours.Value)
                {
                    changedFields.Add($"EstimatedHours: {task.EstimatedHours} → {updateDto.EstimatedHours.Value}");
                    task.EstimatedHours = updateDto.EstimatedHours.Value;
                }

                if (updateDto.ActualHours.HasValue && task.ActualHours != updateDto.ActualHours.Value)
                {
                    changedFields.Add($"ActualHours: {task.ActualHours} → {updateDto.ActualHours.Value}");
                    task.ActualHours = updateDto.ActualHours.Value;
                }

                task.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.TaskItems.Update(task);
                await _unitOfWork.SaveChangesAsync();

                // Log only if there were changes
                if (changedFields.Any())
                {
                    var changeDescription = $"Task updated. Changes: {string.Join("; ", changedFields)}";
                    await LogTaskAction(task.Id, "Updated", changeDescription, context);
                }

                var updatedTask = await _unitOfWork.TaskItems.GetTaskWithDetailsAsync(task.Id);
                var taskDto = _mapper.Map<TaskDto>(updatedTask);
                return OperationResult<TaskDto>.SuccessResult(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return OperationResult<TaskDto>.FailureResult("Failed to update task");
            }
        }
        public async Task<OperationResult<bool>> DeleteTaskAsync(int id, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == id);

                if (task == null)
                    return OperationResult<bool>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<bool>.FailureResult("You don't have permission to delete this task");

                var hasSubtasks = await _unitOfWork.TaskItems.HasSubtasksAsync(id);
                if (hasSubtasks)
                {
                    return OperationResult<bool>.FailureResult("Cannot delete task with subtasks. Delete subtasks first.");
                }

                task.IsDeleted = true;
                task.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.TaskItems.Update(task);
                await _unitOfWork.SaveChangesAsync();

                await LogTaskAction(id, "Deleted", $"Task deleted by {context.UserId}", context);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return OperationResult<bool>.FailureResult("Failed to delete task");
            }
        }
        public async Task<OperationResult<TaskDto>> UpdateTaskStatusAsync(int id, TaskStatus status, OperationContext context)
        {
            var updateDto = new UpdateTaskDto { Status = status };
            return await UpdateTaskAsync(id, updateDto, context);
        }
        public async Task<OperationResult<TaskDto>> AssignTaskAsync(int id, string userId, OperationContext context)
        {
            var updateDto = new UpdateTaskDto { AssignedToUserId = userId };
            return await UpdateTaskAsync(id, updateDto, context);
        }
        public async Task<OperationResult<bool>> CompleteTaskAsync(int id, OperationContext context)
        {
            var result = await UpdateTaskStatusAsync(id, TaskStatus.Completed, context);
            return OperationResult<bool>.SuccessResult(result.IsSuccess);
        }
        public async Task<OperationResult<TaskCommentDto>> AddCommentAsync(int taskId, CreateTaskCommentDto commentDto, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == taskId);

                if (task == null)
                    return OperationResult<TaskCommentDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskCommentDto>.FailureResult("You don't have permission to comment on this task");

                var comment = _mapper.Map<TaskComment>(commentDto);
                comment.TaskId = taskId;
                comment.UserId = context.UserId;
                comment.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.TaskComments.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                await LogTaskAction(taskId, "Comment", $"Comment added: {commentDto.Comment}", context);

                var commentDtoResult = _mapper.Map<TaskCommentDto>(comment);
                return OperationResult<TaskCommentDto>.SuccessResult(commentDtoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task {TaskId}", taskId);
                return OperationResult<TaskCommentDto>.FailureResult("Failed to add comment");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskCommentDto>>> GetCommentsAsync(int taskId, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == taskId);

                if (task == null)
                    return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("You don't have permission to view comments");


                var comments = await _unitOfWork.TaskComments.GetCommentsByTaskAsync(taskId);
                var commentDtos = _mapper.Map<IEnumerable<TaskCommentDto>>(comments);

                return OperationResult<IEnumerable<TaskCommentDto>>.SuccessResult(commentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for task {TaskId}", taskId);
                return OperationResult<IEnumerable<TaskCommentDto>>.FailureResult("Failed to retrieve comments");
            }
        }
        public async Task<OperationResult<TaskAttachmentDto>> AddAttachmentAsync(int taskId, IFormFile file, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == taskId);

                if (task == null)
                    return OperationResult<TaskAttachmentDto>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<TaskAttachmentDto>.FailureResult("You don't have permission to add attachments to this task");

                if (file == null || file.Length == 0)
                    return OperationResult<TaskAttachmentDto>.FailureResult("No file provided");

                var maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                    return OperationResult<TaskAttachmentDto>.FailureResult($"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Tasks", taskId.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new TaskAttachment
                {
                    TaskId = taskId,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileUrl = $"/api/task/{taskId}/attachments/download/{Guid.NewGuid()}",
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.TaskAttachments.AddAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                await LogTaskAction(taskId, "Attachment", $"Attachment added: {file.FileName}", context);

                var attachmentDto = _mapper.Map<TaskAttachmentDto>(attachment);
                return OperationResult<TaskAttachmentDto>.SuccessResult(attachmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attachment to task {TaskId}", taskId);
                return OperationResult<TaskAttachmentDto>.FailureResult("Failed to add attachment");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskAttachmentDto>>> GetAttachmentsAsync(int taskId, OperationContext context)
        {
            try
            {
                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == taskId);

                if (task == null)
                    return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("Task not found");

                if (!CanAccessTask(task, context))
                    return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("You don't have permission to view attachments");

                var attachments = await _unitOfWork.TaskAttachments.GetAttachmentsByTaskAsync(taskId);

                var attachmentDtos = _mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments);

                return OperationResult<IEnumerable<TaskAttachmentDto>>.SuccessResult(attachmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments for task {TaskId}", taskId);
                return OperationResult<IEnumerable<TaskAttachmentDto>>.FailureResult("Failed to retrieve attachments");
            }
        }
        public async Task<OperationResult<bool>> DeleteAttachmentAsync(int taskId, int attachmentId, OperationContext context)
        {
            try
            {
                var attachment = await _unitOfWork.TaskAttachments.GetAsync(a => a.Id == attachmentId && a.TaskId == taskId);

                if (attachment == null)
                    return OperationResult<bool>.FailureResult("Attachment not found");

                var task = await _unitOfWork.TaskItems.GetAsync(t => t.Id == taskId);

                if (!CanAccessTask(task, context))
                    return OperationResult<bool>.FailureResult("You don't have permission to delete this attachment");

                if (System.IO.File.Exists(attachment.FilePath))
                {
                    System.IO.File.Delete(attachment.FilePath);
                }

                attachment.IsDeleted = true;
                attachment.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.TaskAttachments.Update(attachment);
                await _unitOfWork.SaveChangesAsync();

                await LogTaskAction(taskId, "Attachment", $"Attachment deleted: {attachment.FileName}", context);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} from task {TaskId}", attachmentId, taskId);
                return OperationResult<bool>.FailureResult("Failed to delete attachment");
            }
        }
        public async Task<OperationResult<TaskStatsDto>> GetTaskStatsAsync(OperationContext context)
        {
            try
            {
                var query = _unitOfWork.TaskItems.Query().Where(t => !t.IsDeleted);

                if (!context.IsSuperAdmin)
                {
                    query = query.Where(t => t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId);
                }

                var tasks = await query.ToListAsync();
                var today = DateTime.UtcNow.Date;
                var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);
                var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                var stats = new TaskStatsDto
                {
                    TotalTasks = tasks.Count,
                    TasksAssignedToMe = tasks.Count(t => t.AssignedToUserId == context.UserId),
                    TasksCreatedByMe = tasks.Count(t => t.CreatedByUserId == context.UserId),
                    MyTasks = tasks.Count(t => t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId),
                    PendingTasks = tasks.Count(t => t.Status == TaskStatus.Pending),
                    InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
                    CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                    CancelledTasks = tasks.Count(t => t.Status == TaskStatus.Cancelled),
                    OnHoldTasks = tasks.Count(t => t.Status == TaskStatus.OnHold),
                    OverdueTasks = tasks.Count(t => t.Status != TaskStatus.Completed && t.DueDate.HasValue && t.DueDate.Value.Date < today),
                    HighPriorityTasks = tasks.Count(t => t.Priority == TaskPriority.High),
                    UrgentPriorityTasks = tasks.Count(t => t.Priority == TaskPriority.Urgent),
                    TasksDueToday = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date == today),
                    TasksDueThisWeek = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= endOfWeek),
                    TasksDueThisMonth = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= endOfMonth),
                    TasksByCategory = tasks.GroupBy(t => t.Category ?? "Uncategorized").ToDictionary(g => g.Key, g => g.Count()),
                    TasksByStatus = tasks.GroupBy(t => t.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                    TasksByPriority = tasks.GroupBy(t => t.Priority.ToString()).ToDictionary(g => g.Key, g => g.Count())
                };

                return OperationResult<TaskStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task stats");
                return OperationResult<TaskStatsDto>.FailureResult("Failed to retrieve task statistics");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskDto>>> GetOverdueTasksAsync(OperationContext context)
        {
            try
            {
                var overdueTasks = await _unitOfWork.TaskItems.GetOverdueTasksAsync();
                var filteredTasks = overdueTasks.Where(t => CanAccessTask(t, context));
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);
                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue tasks");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve overdue tasks");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskDto>>> GetTasksDueSoonAsync(OperationContext context, int days = 3)
        {
            try
            {
                var dueSoonTasks = await _unitOfWork.TaskItems.GetTasksDueSoonAsync(days);
                var filteredTasks = dueSoonTasks.Where(t => CanAccessTask(t, context));
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);
                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks due soon");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks due soon");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskDto>>> GetTasksDueTodayAsync(OperationContext context)
        {
            try
            {
                var dueTodayTasks = await _unitOfWork.TaskItems.GetTasksDueTodayAsync();
                var filteredTasks = dueTodayTasks.Where(t => CanAccessTask(t, context));
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(filteredTasks);
                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks due today");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks due today");
            }
        }
        public async Task<OperationResult<IEnumerable<TaskDto>>> GetMyTasksAsync(OperationContext context, TaskFilterDto? filter = null)
        {
            try
            {

                var query = _unitOfWork.TaskItems.Query()
                    .Where(t => !t.IsDeleted && (t.AssignedToUserId == context.UserId || t.CreatedByUserId == context.UserId));

                query = ApplyFilters(query, filter);
                query = ApplySorting(query, filter);

                if (filter != null)
                {
                    query = query.Skip((filter.Page - 1) * filter.PageSize)
                                 .Take(filter.PageSize);
                }

                var tasks = await query
                    .Include(t => t.AssignedToUser)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.ParentTask)
                    .Include(t => t.Subtasks.Where(s => !s.IsDeleted))
                    .Include(t => t.Comments.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.User)
                    .Include(t => t.Attachments.Where(a => !a.IsDeleted))
                    .ToListAsync();

                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
                return OperationResult<IEnumerable<TaskDto>>.SuccessResult(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my tasks");
                return OperationResult<IEnumerable<TaskDto>>.FailureResult("Failed to retrieve tasks");
            }
        }

        #region Private Helper Methods
        private IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null) return query;

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
            {
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);
            }

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(t => t.Category == filter.Category);

            if (!string.IsNullOrEmpty(filter.Tag))
                query = query.Where(t => t.Tag == filter.Tag);

            if (filter.DueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);

            if (filter.DueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);

            if (filter.Overdue == true)
                query = query.Where(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(filter.SearchTerm) ||
                                         t.Description != null && t.Description.Contains(filter.SearchTerm));
            }

            return query;
        }
        private IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, TaskFilterDto? filter)
        {
            if (filter == null || string.IsNullOrEmpty(filter.SortBy))
                return query.OrderBy(t => t.DueDate);

            return filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "priority" => filter.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "duedate" => filter.SortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                "createdat" => filter.SortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate),
                _ => filter.SortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate)
            };
        }
        private bool CanAccessTask(TaskItem task, OperationContext context)
        {
            if (context.IsSuperAdmin) return true;
            if (context.IsAdmin) return true;

            return task.AssignedToUserId == context.UserId || task.CreatedByUserId == context.UserId;
        }
        private async System.Threading.Tasks.Task LogTaskAction(int taskId, string action, string description, OperationContext context)
        {
            var auditLog = new TaskAuditLog
            {
                TaskId = taskId,
                Action = action,
                Description = description,
                PerformedByUserId = context.UserId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.TaskAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion
    }
}