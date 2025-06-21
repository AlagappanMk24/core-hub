namespace Core_API.Domain.Entities
{
    public class ExportLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // The admin or manager who initiated the export
        public int ExportedUserCount { get; set; } // Number of users exported
        public DateTime ExportTime { get; set; } // When the export occurred
        public string QueryParameters { get; set; } // JSON string of the query parameters used
    }
}
