using Core_API.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // ApplicationUser - Company relationship
            builder.HasOne(au => au.Company)
                .WithMany()
                .HasForeignKey(au => au.CompanyId)
                .IsRequired(false);

            // ApplicationUser - Customer relationship
            builder.HasOne(au => au.Customer)
                .WithMany()
                .HasForeignKey(au => au.CustomerId)
                .IsRequired(false);

            // Optimize the query with an index on IsDeleted and DeletedDate
            builder.HasIndex(u => new { u.IsDeleted, u.DeletedDate })
              .HasFilter("IsDeleted = 1")
              .HasDatabaseName("IX_AspNetUsers_IsDeleted_DeletedDate");

            // ApplicationUser - RefreshToken relationship
            builder.HasMany(u => u.RefreshTokens)       // An ApplicationUser has many RefreshTokens
             .WithOne(rt => rt.ApplicationUser)   // Each RefreshToken has one ApplicationUser
             .HasForeignKey(rt => rt.ApplicationUserId) // The foreign key is ApplicationUserId in RefreshToken
             .OnDelete(DeleteBehavior.Cascade);   // Optional: What happens when a user is deleted
        }
    }
}