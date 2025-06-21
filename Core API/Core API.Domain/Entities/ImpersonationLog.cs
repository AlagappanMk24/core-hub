namespace Core_API.Domain.Entities
{
    public class ImpersonationLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; }
        public string ImpersonatedUserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
