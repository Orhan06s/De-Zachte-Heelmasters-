
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            // ASP.NET Identity
            builder.Services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDataProtection();

            var app = builder.Build();

             // Database: migrations only locally (online the pipeline does this), roles always
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                if (app.Environment.IsDevelopment())
                {
                    await context.Database.MigrateAsync();
                }

                await DbSeeder.SeedAsync(context);
            }
            // Configure HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            // Serve the Vue frontend from wwwroot
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            // Health check for the deploy pipeline
            app.MapGet("/api/health", async (ApplicationDbContext db) =>
                await db.Database.CanConnectAsync()
                    ? Results.Ok(new { status = "ok" })
                    : Results.Problem("database onbereikbaar"));

            // Vue routes (like /login) go to index.html
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
