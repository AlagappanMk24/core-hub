namespace Core_API.Application.DTOs.Customer.Response
{
    /// <summary>
    /// DTO for customer activity feed
    /// </summary>
    public class CustomerActivityDto
    {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
        public string Color { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}