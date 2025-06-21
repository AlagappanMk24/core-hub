using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core_API.Infrastructure.Data.Seeders;

namespace Core_API.Infrastructure.Data.Context
{
    public class CoreAPIDbContext(DbContextOptions<CoreAPIDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // DbSet properties
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<InvoiceAttachments> InvoiceAttachments { get; set; }
        public DbSet<TaxDetail> TaxDetails { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Timezone> Timezones { get; set; }
        public DbSet<ContactUs> ContactUsSubmissions { get; set; }
        public DbSet<AuthState> AuthStates { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<OrderActivityLog> OrderActivityLogs { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }
        public DbSet<ImpersonationLog> ImpersonationLogs { get; set; }
        public DbSet<ExportLog> ExportLogs { get; set; }
        public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from separate configuration classes
            // This single line will pick up all IEntityTypeConfiguration implementations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreAPIDbContext).Assembly);

            // Call the seeder
            DatabaseSeeder.SeedData(modelBuilder);
        }
    }
}