namespace Core_API.Application.DTOs.Companies.Requests
{
    public class RejectCompanyRequestDto
    {
        public int RequestId { get; set; }
        public string Reason { get; set; }
    }
}