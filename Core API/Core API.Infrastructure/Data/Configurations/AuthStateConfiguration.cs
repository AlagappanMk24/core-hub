using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class AuthStateConfiguration : IEntityTypeConfiguration<AuthState>
    {
        public void Configure(EntityTypeBuilder<AuthState> builder)
        {
            builder.HasKey(ast => ast.Id);

            // AuthState - UserId index for faster lookups
            builder.HasIndex(ast => ast.UserId);

            // Configure the expiration period indexing for efficient querying
            builder.HasIndex(ast => ast.ExpiresAt);

            builder.Property(ast => ast.EmailOTP)
                .HasMaxLength(10);

            builder.Property(ast => ast.SmsOTP)
                .HasMaxLength(10);
        }
    }
}
