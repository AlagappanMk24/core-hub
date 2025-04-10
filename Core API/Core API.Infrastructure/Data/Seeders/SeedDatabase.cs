namespace Core_API.Infrastructure.Data.Seeders
{
    public static class SeedDatabase
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            // Seed roles
            await SeedRoles.SeedRolesAsync(serviceProvider);

            // Add other seeding methods here if needed
        }
    }
}
