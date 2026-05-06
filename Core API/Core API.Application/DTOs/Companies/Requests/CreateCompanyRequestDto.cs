namespace Core_API.Application.DTOs.Companies.Requests
{
    public class CreateCompanyRequestDto
    {
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string FullName { get; set; } // Add this to include requester's name
    }
}
