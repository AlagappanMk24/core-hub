namespace Core_API.Application.DTOs.User.Response
{
    public class UserIndexResponse
    {
        public UserQueryParameters QueryParameters { get; set; }
        public List<CompanyDto> Companies { get; set; }
        public List<string> Roles { get; set; }
    }
    public class CompanyDto
    {

    }
}