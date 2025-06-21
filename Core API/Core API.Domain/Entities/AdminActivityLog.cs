namespace Core_API.Domain.Entities
{
    public class AdminActivityLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
