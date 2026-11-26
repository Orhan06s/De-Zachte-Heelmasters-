using De_zachte_heelmeester;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeZachteHeelmeestersTests;

/// <summary>
/// Starts the API once for all integration tests, with its own test database in LocalDB.
/// </summary>
[TestClass]
public class TestEnvironment
{
    private const string TestDatabaseName = "De zachte heelmasters - Test";

    private const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=" + TestDatabaseName + ";Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public static WebApplicationFactory<Program> Api { get; private set; } = null!;

    [AssemblyInitialize]
    public static async Task StartAsync(TestContext _)
    {
        // The API also reads the connection string from environment variables, so it uses the test database.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestConnectionString);
        Api = new WebApplicationFactory<Program>();

        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Safety check: only the test database may be deleted.
        if (db.Database.GetConnectionString()?.Contains(TestDatabaseName) != true)
        {
            throw new InvalidOperationException("The tests are not using the test database. Stopped to protect data.");
        }

        await db.Database.EnsureDeletedAsync();

        // The same seeder as the API: migrations and roles
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await DbSeeder.SeedAsync(db, roleManager);
    }

    [AssemblyCleanup]
    public static void Stop() => Api.Dispose();

    /// <summary>
    /// A client that behaves like a browser: it keeps cookies but does not follow redirects, so the test sees the real status code of the API.
    /// </summary>
    public static HttpClient CreateClient() =>
        Api.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>
    /// Creates a test user with a unique email address and the given role. Returns the email address.
    /// </summary>
    public static async Task<string> CreateUserAsync(string role, string password)
    {
        var email = $"test-{Guid.NewGuid():N}@test.nl";

        using var scope = Api.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User { UserName = email, Email = email };
        var created = await userManager.CreateAsync(user, password);
        Assert.IsTrue(created.Succeeded, "The test user could not be created.");

        await userManager.AddToRoleAsync(user, role);
        return email;
    }
}