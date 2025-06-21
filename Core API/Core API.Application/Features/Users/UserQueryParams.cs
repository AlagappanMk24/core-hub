using Core_API.Application.Common.QueryParams;

namespace Core_API.Application.Features.Users
{
    public class UserQueryParameters : QueryParameters
    {
        public int? CompanyId { get; set; }
        public string? Role { get; set; }
    }
}
