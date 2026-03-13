namespace Core_API.Application.DTOs.Authentication.Request.CompanyRequest
{
    public class RequestStatusResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string RejectionReason { get; set; }
    }
}