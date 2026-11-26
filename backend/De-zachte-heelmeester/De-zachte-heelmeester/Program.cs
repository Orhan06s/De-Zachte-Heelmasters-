
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

             // LOG-04: login links
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddScoped<De_zachte_heelmeester.Services.LoginLinkService>();
            builder.Services.AddScoped<De_zachte_heelmeester.Services.IMailService, De_zachte_heelmeester.Services.DatabaseMailService>();

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                await DbSeeder.SeedAsync(context);
            }

            // Configure HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
