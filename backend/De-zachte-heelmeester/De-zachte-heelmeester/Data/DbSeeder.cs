using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Data
{
    public static class DbSeeder
    {
        public static readonly string[] Roles = ["Patient", "Huisarts", "Specialist", "Medewerker", "Beheerder"];

        public static async Task SeedAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
        

            // Roles: only create a role if it does not exist yet, so starting the API twice is safe
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // seeder data comes under this line
        }
    }
}
