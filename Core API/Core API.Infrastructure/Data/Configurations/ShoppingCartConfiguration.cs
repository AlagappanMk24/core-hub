using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            // ShoppingCart - Product relationship
            builder.HasOne(sc => sc.Product)
                .WithMany()
                .HasForeignKey(sc => sc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShoppingCart - ApplicationUser relationship
            builder.HasOne(sc => sc.ApplicationUser)
                .WithMany()
                .HasForeignKey(sc => sc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
