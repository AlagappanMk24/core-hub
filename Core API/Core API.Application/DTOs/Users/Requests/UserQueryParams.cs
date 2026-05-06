using Core_API.Application.Common.Models;

namespace Core_API.Application.DTOs.Users.Request
{
    public class UserQueryParameters : PaginationParams
    {
        public int? CompanyId { get; set; }
        public string? Role { get; set; }
    }
}