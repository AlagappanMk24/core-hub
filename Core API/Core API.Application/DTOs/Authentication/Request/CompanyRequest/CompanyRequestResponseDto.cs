namespace Core_API.Application.DTOs.Authentication.Request.CompanyRequest
{
    public class CompanyRequestResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string ProcessedBy { get; set; }
        public string RejectionReason { get; set; }
    }
}