using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            // OrderDetail - OrderHeader relationship
            builder.HasOne(od => od.OrderHeader)
                    .WithMany(oh => oh.OrderDetails)
                    .HasForeignKey(od => od.OrderHeaderId)
                    .OnDelete(DeleteBehavior.Cascade);

            // OrderDetail - Product relationship
            builder.HasOne(od => od.Product)
                    .WithMany()
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
