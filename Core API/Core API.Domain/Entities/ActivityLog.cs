namespace Core_API.Domain.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // The user who performed the action (e.g., the admin)
        public string TargetUserId { get; set; } // The user affected by the action (e.g., the user being edited)
        public string Action { get; set; } // e.g., "User Created", "User Updated", "Roles Assigned"
        public string Details { get; set; } // Additional details (e.g., "Assigned roles: Admin, Manager")
        public DateTime Timestamp { get; set; } // When the action occurred
    }
}
