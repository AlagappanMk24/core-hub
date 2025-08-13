using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core_API.Application.DTOs.User.Response
{
    // VIEW MODEL - For UI presentation and form handling
    public class UserUpsertResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public int CompanyId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<string> SelectedRoles { get; set; } = new List<string>();
        public List<SelectListItem>? CompanyList { get; set; }
        public List<SelectListItem>? RoleList { get; set; }
    }
}
