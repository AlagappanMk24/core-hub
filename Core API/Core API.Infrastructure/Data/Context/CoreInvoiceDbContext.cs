using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Context
{
    public class CoreInvoiceDbContext(DbContextOptions<CoreInvoiceDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        /// <summary>
        /// Identity and Core Business Entities
        /// </summary>
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CompanyRequest> CompanyRequests { get; set; }

        /// <summary>
        /// Standard Invoice System Entities
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<InvoiceTaxDetail> InvoiceTaxDetails { get; set; }
        public DbSet<InvoiceDiscount> InvoiceDiscounts { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<InvoiceAttachment> InvoiceAttachments { get; set; }
        public DbSet<InvoiceAuditLog> InvoiceAuditLogs { get; set; }
        public DbSet<InvoiceSequence> InvoiceSequences { get; set; }

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TaskAuditLog> TaskAuditLogs { get; set; }

        public DbSet<TaskComment> TaskComments { get; set; }
        /// </summary>
        public DbSet<RecurringInvoice> RecurringInvoices { get; set; }
        public DbSet<RecurringInvoiceInstance> RecurringInvoiceInstances { get; set; }
        public DbSet<RecurringInvoiceAuditLog> RecurringInvoiceAuditLogs { get; set; }

        /// <summary>
        /// Configuration and Global Settings
        /// </summary>
        public DbSet<TaxType> TaxTypes { get; set; }
        public DbSet<InvoiceSettings> InvoiceSettings { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }

        /// <summary>
        /// Authentication, Permissions, and Security
        /// </summary>
        public DbSet<AuthState> AuthStates { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /// <summary>
            /// Applies all configurations defined in the Assembly (IEntityTypeConfiguration)
            /// </summary>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreInvoiceDbContext).Assembly);

            // Call the seeder if necessary
            // DatabaseSeeder.SeedData(modelBuilder);
        }
    }
}