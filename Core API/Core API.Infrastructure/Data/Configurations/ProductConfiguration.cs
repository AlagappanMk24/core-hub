using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Product - Category (many-to-one) relationship
            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product - SubCategory relationship (optional)
            builder.HasOne(p => p.SubCategory)
                   .WithMany(sc => sc.Products)
                   .HasForeignKey(p => p.SubCategoryId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false); // SubCategoryId is optional

            // Product - Brand (many-to-one) relationship
            builder.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey("BrandId")
                .OnDelete(DeleteBehavior.Restrict);

            // Product - ProductImages (one-to-many) relationship
            builder.HasMany(p => p.ProductImages)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - ProductSpecifications (one-to-many) relationship
            builder.HasMany(p => p.Specifications)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - ProductVariants (one-to-many) relationship
            builder.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - ProductTags (one-to-many) relationship
            builder.HasMany(p => p.Tags)
                .WithOne(t => t.Product)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Properties
            builder.Property(p => p.Title)
                   .IsRequired()
                   .HasMaxLength(255);
            builder.Property(p => p.SKU)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.Property(p => p.Barcode)
                   .HasMaxLength(50);
            builder.Property(p => p.MetaTitle)
                   .HasMaxLength(100);
            builder.Property(p => p.MetaDescription)
                   .HasMaxLength(255);
        }
    }
}
