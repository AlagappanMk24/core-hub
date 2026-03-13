namespace Core_API.Application.DTOs.Authentication.Request.CompanyRequest
{
    public class RejectCompanyRequestDto
    {
        public int RequestId { get; set; }
        public string Reason { get; set; }
    }
}