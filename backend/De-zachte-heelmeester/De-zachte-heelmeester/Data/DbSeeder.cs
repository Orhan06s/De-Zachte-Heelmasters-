using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Data
{
    public static class DbSeeder
    {
        public static readonly string[] Roles = ["Patient", "Huisarts", "Specialist", "Medewerker", "Beheerder"];

        // Test accounts for trying the app locally. Only used in Development (see Program.cs).
        public const string TestPassword = "Test123!wachtwoord";
        public const string TestPatientEmail = "patient@test.nl";
        public const string TestSpecialistEmail = "cardioloog@test.nl";

        // A made-up BSN that passes the elfproef
        private const string TestPatientBsn = "123456782";

        // Z0001 "Consult hart", Cardiologie, 3 quarters of an hour
        private const string TestCareCode = "Z0001";

        public static async Task SeedAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

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

        /// <summary>
        /// Adds a test patient with a referral for Z0001 and a cardiologist, so you can book an appointment locally.
        /// Every part is only added if it does not exist yet, so starting the API twice is safe.
        /// </summary>
        public static async Task SeedTestDataAsync(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            // Test patient with an account
            var patientUser = await GetOrCreateUserAsync(userManager, TestPatientEmail, "Patient");

            if (!await context.Patients.AnyAsync(p => p.BSN == TestPatientBsn))
            {
                context.Patients.Add(new Patient
                {
                    BSN = TestPatientBsn,
                    UserId = patientUser.Id,
                    FirstName = "Test",
                    LastName = "Patient",
                    Email = TestPatientEmail
                });
                await context.SaveChangesAsync();
            }

            // Test specialist (Cardiologie, Monday to Friday)
            var specialistUser = await GetOrCreateUserAsync(userManager, TestSpecialistEmail, "Specialist");

            if (!await context.Employees.AnyAsync(e => e.UserId == specialistUser.Id))
            {
                context.Employees.Add(new Employee
                {
                    UserId = specialistUser.Id,
                    Name = "Test Cardioloog",
                    Specialization = "Cardiologie",
                    WorkDays = WorkDays.MondayToFriday
                });
                await context.SaveChangesAsync();
            }

            // Referral for the test patient
            if (!await context.Referrals.AnyAsync(r => r.PatientBSN == TestPatientBsn && r.CareCode == TestCareCode))
            {
                context.Referrals.Add(new Referral { PatientBSN = TestPatientBsn, CareCode = TestCareCode });
                await context.SaveChangesAsync();
            }
        }

        private static async Task<User> GetOrCreateUserAsync(UserManager<User> userManager, string email, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return user;
            }

            user = new User { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, TestPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Test user {email} could not be created.");
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}
