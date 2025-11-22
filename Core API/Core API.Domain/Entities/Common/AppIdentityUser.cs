using Microsoft.AspNetCore.Identity;

namespace Core_API.Domain.Entities.Common;
public class AppIdentityUser : IdentityUser
{
    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
