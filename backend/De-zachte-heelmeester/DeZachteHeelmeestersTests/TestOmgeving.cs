using De_zachte_heelmeester;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeZachteHeelmeestersTests;

/// <summary>
/// Start de API een keer voor alle integratietests, met een eigen testdatabase in LocalDB.
/// </summary>
[TestClass]
public class TestOmgeving
{
    private const string TestDatabaseNaam = "De zachte heelmasters - Test";

    private const string TestDatabase =
        "Server=(localdb)\\MSSQLLocalDB;Database=" + TestDatabaseNaam + ";Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    private static readonly string[] Rollen = ["Patient", "Huisarts", "Specialist", "Medewerker", "Beheerder"];

    public static WebApplicationFactory<Program> Api { get; private set; } = null!;

    [AssemblyInitialize]
    public static async Task StartAsync(TestContext _)
    {
        // De API leest de connection string uit omgevingsvariabelen. Zo gebruikt hij de testdatabase.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestDatabase);
        Api = new WebApplicationFactory<Program>();

        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        if (db.Database.GetConnectionString()?.Contains(TestDatabaseNaam) != true)
        {
            throw new InvalidOperationException("De tests gebruiken niet de testdatabase. Gestopt om data te beschermen.");
        }

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        var rolBeheer = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Rollen)
        {
            await rolBeheer.CreateAsync(new IdentityRole(rol));
        }
    }

    [AssemblyCleanup]
    public static void Stop() => Api.Dispose();

    /// <summary>
    /// Een client die zich gedraagt als een browser: hij bewaart cookies, maar volgt geen doorverwijzingen. Zo zie je in de test de echte statuscode van de API.
    /// </summary>
    public static HttpClient NieuweClient() =>
        Api.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>
    /// Maakt een testgebruiker met een uniek e-mailadres en de gegeven rol. Geeft het e-mailadres terug.
    /// </summary>
    public static async Task<string> MaakGebruikerAsync(string rol, string wachtwoord)
    {
        var email = $"test-{Guid.NewGuid():N}@test.nl";

        using var scope = Api.Services.CreateScope();
        var gebruikers = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var gebruiker = new User { UserName = email, Email = email };
        var aangemaakt = await gebruikers.CreateAsync(gebruiker, wachtwoord);
        Assert.IsTrue(aangemaakt.Succeeded, "Testgebruiker kon niet worden aangemaakt.");

        await gebruikers.AddToRoleAsync(gebruiker, rol);
        return email;
    }
}
