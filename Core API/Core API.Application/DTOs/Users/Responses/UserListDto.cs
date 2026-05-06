namespace Core_API.Application.DTOs.Users.Response
{
    public class UserListDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}
