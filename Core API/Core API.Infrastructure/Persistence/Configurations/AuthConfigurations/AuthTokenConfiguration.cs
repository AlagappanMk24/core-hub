using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Infrastructure.Persistence.Configurations.AuthConfigurations
{
    public class AuthTokenConfiguration : IEntityTypeConfiguration<AuthToken>
    {
        public void Configure(EntityTypeBuilder<AuthToken> builder)
        {

            builder.HasKey(at => at.Id);

            builder.HasIndex(at => at.Token)
                .IsUnique();
            // AuthToken - User relationship (implied by UserId)
            builder.HasIndex(at => at.UserId);

            builder.Property(at => at.Token)
                .IsRequired();

            builder.Property(at => at.UserId)
                .IsRequired();
        }
    }
}
