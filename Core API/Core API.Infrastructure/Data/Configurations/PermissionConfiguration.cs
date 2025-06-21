using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<RoleMenuPermission>
    {
        // Define many-to-many relationship between roles and menu permissions
        public void Configure(EntityTypeBuilder<RoleMenuPermission> builder)
        {
            // Primary Key
            builder.HasKey(rp => rp.Id);

            // Properties
            builder.Property(rp => rp.RoleId)
                .IsRequired()
                .HasMaxLength(450); // Matches AspNetRoles.Id

            builder.Property(rp => rp.MenuName)
                .IsRequired()
                .HasMaxLength(50); // Consistent with frontend menu names

            builder.Property(rp => rp.PermissionId)
                .IsRequired();

            builder.Property(rp => rp.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true); // Default to enabled

            // Relationships
            // RoleId -> AspNetRoles (no navigation in IdentityRole)
            builder.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Delete permissions if role is deleted

            // PermissionId -> Permission
            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RoleMenuPermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting permissions in use

            // Index for performance
            builder.HasIndex(rp => new { rp.RoleId, rp.MenuName, rp.PermissionId })
                .IsUnique(); // Ensure unique role-menu-permission combinations
        }
    }
}