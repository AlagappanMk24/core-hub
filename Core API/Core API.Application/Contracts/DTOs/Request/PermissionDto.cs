namespace Core_API.Application.Contracts.DTOs.Request
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public string Action { get; set; }
    }
}
