using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // seeder data comes under this line
        }
    }
}
