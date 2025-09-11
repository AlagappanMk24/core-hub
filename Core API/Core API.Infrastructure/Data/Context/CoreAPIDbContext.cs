using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Context
{
    public class CoreAPIDbContext(DbContextOptions<CoreAPIDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // DbSet properties
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<TaxDetail> TaxDetails { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<TaxType> TaxTypes { get; set; }
        public DbSet<AuthState> AuthStates { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InvoiceSettings> InvoiceSettings { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from separate configuration classes
            // This single line will pick up all IEntityTypeConfiguration implementations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreAPIDbContext).Assembly);

            // Call the seeder
            //DatabaseSeeder.SeedData(modelBuilder);
        }
    }
}