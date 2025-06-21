using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Core_API.Domain.Entities;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId).IsRequired();

            builder.Property(e => e.TargetUserId).IsRequired();

            builder.Property(e => e.Action).IsRequired().HasMaxLength(100);

            builder.Property(e => e.Details).HasMaxLength(500);

            builder.Property(e => e.Timestamp).IsRequired();

            builder.HasIndex(log => new { log.TargetUserId, log.Timestamp });

            builder.HasIndex(log => new { log.Action, log.Timestamp });
        }
    }
}
