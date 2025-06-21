using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class OrderActivityLogConfiguration : IEntityTypeConfiguration<OrderActivityLog>
    {
        public void Configure(EntityTypeBuilder<OrderActivityLog> builder)
        {
            builder.ToTable("OrderActivityLogs");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Timestamp).IsRequired();
            builder.Property(l => l.User).HasMaxLength(255);
            builder.Property(l => l.ActivityType).IsRequired(); // Enum stored as INT by default
            builder.Property(l => l.Description).HasMaxLength(1000);
            builder.Property(l => l.Details).HasMaxLength(4000);

            // Optional: Store ActivityType as string
            // builder.Property(l => l.ActivityType).HasConversion<string>().HasMaxLength(50);

            // Index for performance
            builder.HasIndex(l => new { l.OrderHeaderId, l.Timestamp });
        }
    }
}
