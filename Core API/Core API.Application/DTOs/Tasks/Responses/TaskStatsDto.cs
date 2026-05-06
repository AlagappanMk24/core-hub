namespace Core_API.Application.DTOs.Tasks.Responses
{
    public class TaskStatsDto
    {
        public int TotalTasks { get; set; }
        public int MyTasks { get; set; }
        public int TasksAssignedToMe { get; set; }
        public int TasksCreatedByMe { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int UrgentPriorityTasks { get; set; }
        public int TasksDueToday { get; set; }
        public int TasksDueThisWeek { get; set; }
        public int TasksDueThisMonth { get; set; }
        public Dictionary<string, int> TasksByCategory { get; set; } = new();
        public Dictionary<string, int> TasksByStatus { get; set; } = new();
        public Dictionary<string, int> TasksByPriority { get; set; } = new();
    }
}
