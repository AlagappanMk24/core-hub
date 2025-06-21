using Microsoft.AspNetCore.Identity;

namespace Core_API.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string EntityName { get; set; } // e.g., Orders, Invoices
        public string Action { get; set; } // e.g., View, Create
        public ICollection<RoleMenuPermission>? RoleMenuPermissions { get; set; }
    }
    public class RoleMenuPermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; } // AspNetRoles Id
        public string MenuName { get; set; } // e.g., Orders
        public int PermissionId { get; set; }
        public bool IsEnabled { get; set; }
        public Permission Permission { get; set; }
    }
}
