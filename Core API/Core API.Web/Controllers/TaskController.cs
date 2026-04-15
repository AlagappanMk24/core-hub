// Core_API.Web/Controllers/TaskController.cs
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = Core_API.Domain.Enums.TaskStatus;

namespace Core_API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController(ITaskService taskService, ILogger<TaskController> logger) : BaseApiController
    {
        private readonly ITaskService _taskService = taskService;
        private readonly ILogger<TaskController> _logger = logger;

        /// <summary>
        /// Get all tasks with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllTasks([FromQuery] TaskFilterDto filter)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetAllTasksAsync(context, filter);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }

        /// <summary>
        /// Get my tasks (tasks assigned to or created by current user)
        /// </summary>
        [HttpGet("my-tasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTasks([FromQuery] TaskFilterDto filter)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetMyTasksAsync(context, filter);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my tasks");
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetTaskByIdAsync(id, context);

                if (!result.IsSuccess)
                    return NotFound(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task");
            }
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createDto)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.CreateTaskAsync(createDto, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return CreatedAtAction(nameof(GetTaskById), new { id = result.Data.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "An error occurred while creating the task");
            }
        }

        /// <summary>
        /// Update an existing task
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.UpdateTaskAsync(id, updateDto, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task");
            }
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.DeleteTaskAsync(id, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }

        /// <summary>
        /// Update task status
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromQuery] TaskStatus status)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.UpdateTaskStatusAsync(id, status, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status for {TaskId}", id);
                return StatusCode(500, "An error occurred while updating task status");
            }
        }

        /// <summary>
        /// Complete a task
        /// </summary>
        [HttpPatch("{id}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.CompleteTaskAsync(id, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(new { message = "Task completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", id);
                return StatusCode(500, "An error occurred while completing the task");
            }
        }

        /// <summary>
        /// Assign task to user
        /// </summary>
        [HttpPatch("{id}/assign/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignTask(int id, string userId)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.AssignTaskAsync(id, userId, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task {TaskId} to user {UserId}", id, userId);
                return StatusCode(500, "An error occurred while assigning the task");
            }
        }

        /// <summary>
        /// Add comment to task
        /// </summary>
        [HttpPost("{id}/comments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddComment(int id, [FromBody] CreateTaskCommentDto commentDto)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.AddCommentAsync(id, commentDto, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return CreatedAtAction(nameof(GetComments), new { id = result.Data.TaskId }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task {TaskId}", id);
                return StatusCode(500, "An error occurred while adding comment");
            }
        }

        /// <summary>
        /// Get comments for task
        /// </summary>
        [HttpGet("{id}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComments(int id)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetCommentsAsync(id, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving comments");
            }
        }

        /// <summary>
        /// Upload attachment to task
        /// </summary>
        [HttpPost("{id}/attachments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.AddAttachmentAsync(id, file, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return CreatedAtAction(nameof(GetAttachments), new { id = result.Data.TaskId }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment to task {TaskId}", id);
                return StatusCode(500, "An error occurred while uploading attachment");
            }
        }

        /// <summary>
        /// Get all attachments for a task
        /// </summary>
        [HttpGet("{id}/attachments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttachments(int id)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetAttachmentsAsync(id, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving attachments");
            }
        }

        /// <summary>
        /// Delete attachment from task
        /// </summary>
        [HttpDelete("{taskId}/attachments/{attachmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttachment(int taskId, int attachmentId)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.DeleteAttachmentAsync(taskId, attachmentId, context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} from task {TaskId}", attachmentId, taskId);
                return StatusCode(500, "An error occurred while deleting attachment");
            }
        }

        /// <summary>
        /// Get task statistics
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTaskStats()
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetTaskStatsAsync(context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task stats");
                return StatusCode(500, "An error occurred while retrieving task statistics");
            }
        }

        /// <summary>
        /// Get overdue tasks
        /// </summary>
        [HttpGet("overdue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverdueTasks()
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetOverdueTasksAsync(context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue tasks");
                return StatusCode(500, "An error occurred while retrieving overdue tasks");
            }
        }

        /// <summary>
        /// Get tasks due soon
        /// </summary>
        [HttpGet("due-soon")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTasksDueSoon([FromQuery] int days = 3)
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetTasksDueSoonAsync(context, days);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks due soon");
                return StatusCode(500, "An error occurred while retrieving tasks due soon");
            }
        }

        /// <summary>
        /// Get tasks due today
        /// </summary>
        [HttpGet("due-today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTasksDueToday()
        {
            try
            {
                var context = CurrentContext;
                if (context == null)
                    return Unauthorized();

                var result = await _taskService.GetTasksDueTodayAsync(context);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks due today");
                return StatusCode(500, "An error occurred while retrieving tasks due today");
            }
        }
    }
}