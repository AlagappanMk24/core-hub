using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
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
