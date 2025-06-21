using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Configurations
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            // Vendor - Products relationship
            builder.HasMany(v => v.Products)
                   .WithOne(p => p.Vendor)
                   .HasForeignKey(p => p.VendorId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Vendor - Company relationship
            builder.HasOne(v => v.Company)
                   .WithMany(c => c.Vendors)
                   .HasForeignKey(v => v.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Properties
            builder.Property(v => v.VendorName)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(v => v.Email)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(v => v.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);
            builder.Property(v => v.VendorPictureUrl)
                   .HasMaxLength(255);
        }
    }
}
