using System.Net;
using System.Net.Http.Json;

namespace DeZachteHeelmeestersTests;

/// <summary>
/// LOG-02 Gebruiker logt veilig in.
/// Integratietests voor de testgevallen TC-LOG-06 tot en met TC-LOG-08 uit het testplan.
/// </summary>
[TestClass]
public class Log02InloggenTests
{
    private const string Wachtwoord = "Test123!wachtwoord";
    private const string LoginAdres = "/api/auth/login";
    private const string IngelogdeGebruikerAdres = "/api/auth/me";
    private const string AlleenHuisartsAdres = "/api/huisarts";

    // AC1: juiste gegevens. De API geeft de rol terug, zodat de frontend de startpagina van die rol kan tonen.
    [TestMethod]
    public async Task TC_LOG_06_JuisteGegevens_GeeftRolTerug()
    {
        var email = await TestOmgeving.MaakGebruikerAsync("Patient", Wachtwoord);
        using var client = TestOmgeving.NieuweClient();

        var antwoord = await client.PostAsJsonAsync(LoginAdres, new { email, password = Wachtwoord });

        Assert.AreEqual(HttpStatusCode.OK, antwoord.StatusCode);
        var inhoud = await antwoord.Content.ReadFromJsonAsync<LoginAntwoord>();
        Assert.IsNotNull(inhoud);
        CollectionAssert.Contains(inhoud.Roles, "Patient");
    }

    // AC2: een ingelogde patient opent een pagina die alleen voor huisartsen is. Hij krijgt geen toegang (403), maar blijft wel ingelogd.
    [TestMethod]
    public async Task TC_LOG_07_PatientOpentHuisartsPagina_GeenToegangMaarBlijftIngelogd()
    {
        var email = await TestOmgeving.MaakGebruikerAsync("Patient", Wachtwoord);
        using var client = TestOmgeving.NieuweClient();
        var login = await client.PostAsJsonAsync(LoginAdres, new { email, password = Wachtwoord });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode, "Inloggen moet eerst lukken.");

        var huisartsPagina = await client.GetAsync(AlleenHuisartsAdres);
        var nogIngelogd = await client.GetAsync(IngelogdeGebruikerAdres);

        Assert.AreEqual(HttpStatusCode.Forbidden, huisartsPagina.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, nogIngelogd.StatusCode);
    }

    // AC3: verkeerd wachtwoord. De gebruiker krijgt de melding uit het acceptatiecriterium en is niet ingelogd.
    [TestMethod]
    public async Task TC_LOG_08_VerkeerdWachtwoord_GeweigerdMetMelding()
    {
        var email = await TestOmgeving.MaakGebruikerAsync("Patient", Wachtwoord);
        using var client = TestOmgeving.NieuweClient();

        var antwoord = await client.PostAsJsonAsync(LoginAdres, new { email, password = "Fout123!wachtwoord" });
        var ingelogd = await client.GetAsync(IngelogdeGebruikerAdres);

        Assert.AreEqual(HttpStatusCode.Unauthorized, antwoord.StatusCode);
        var fout = await antwoord.Content.ReadFromJsonAsync<FoutAntwoord>();
        Assert.IsNotNull(fout);
        Assert.AreEqual("Het e-mailadres of het wachtwoord klopt niet.", fout.Message);
        Assert.AreEqual(HttpStatusCode.Unauthorized, ingelogd.StatusCode);
    }

    private sealed record LoginAntwoord(List<string> Roles);

    private sealed record FoutAntwoord(string Message);
}
