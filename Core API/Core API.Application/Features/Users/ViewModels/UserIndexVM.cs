using Core_API.Application.Features.Companies.DTOs;

namespace Core_API.Application.Features.Users.ViewModels
{
    public class UserIndexVM
    {
        public UserQueryParameters QueryParameters { get; set; }
        public List<CompanyDto> Companies { get; set; }
        public List<string> Roles { get; set; }
    }
}