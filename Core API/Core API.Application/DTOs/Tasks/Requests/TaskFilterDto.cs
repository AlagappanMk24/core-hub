using Core_API.Domain.Enums;

namespace Core_API.Application.DTOs.Tasks.Requests
{
    public class TaskFilterDto
    {
        // Filters
        public Domain.Enums.TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? Overdue { get; set; }
        public bool? MyTasks { get; set; }
        public string? SearchTerm { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; } = "DueDate";
        public bool SortDescending { get; set; } = false;
    }
}
