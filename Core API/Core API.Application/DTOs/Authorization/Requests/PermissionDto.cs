namespace Core_API.Application.DTOs.Authorization.Request;
public class PermissionDto
{
    public int Id { get; set; }
    public string EntityName { get; set; }
    public string Action { get; set; }
}
