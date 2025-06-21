using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {
            // SubCategory has one Category (many-to-one)
            builder.HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories) // Assuming Category has a collection of SubCategories
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubCategory has many Products (one-to-many)
            builder.HasMany(sc => sc.Products)
                .WithOne(p => p.SubCategory)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Or .SetNull if SubCategoryId is nullable in Product

            // Configure properties
            builder.Property(sc => sc.Name)
                .HasMaxLength(255);
        }
    }
}
