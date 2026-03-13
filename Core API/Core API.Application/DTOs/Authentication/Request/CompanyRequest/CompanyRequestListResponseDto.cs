namespace Core_API.Application.DTOs.Authentication.Request.CompanyRequest
{
    public class CompanyRequestListResponseDto
    {
        public List<CompanyRequestResponseDto> Requests { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}