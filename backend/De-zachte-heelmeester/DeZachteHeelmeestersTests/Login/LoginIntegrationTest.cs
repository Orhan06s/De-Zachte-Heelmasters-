using System.Net;
using System.Net.Http.Json;

namespace DeZachteHeelmeestersTests.Login;

/// <summary>
/// LOG-02 Gebruiker logt veilig in.
/// Integration tests for test cases TC-LOG-06 to TC-LOG-08 from the test plan.
/// </summary>
[TestClass]
public class LoginIntegrationTest
{
    private const string Password = "Test123!wachtwoord";
    private const string LoginUrl = "/api/auth/login";
    private const string CurrentUserUrl = "/api/auth/me";
    private const string GeneralPractitionerOnlyUrl = "/api/general-practitioner";

    // AC1: correct credentials. The API returns the role, so the frontend can show the start page of that role.
    [TestMethod]
    [TestProperty("LOG-02", "AC1")]
    public async Task TC_LOG_06_CorrectCredentials_ReturnsRoles()
    {
        var email = await TestEnvironment.CreateUserAsync("Patient", Password);
        using var client = TestEnvironment.CreateClient();

        var response = await client.PostAsJsonAsync(LoginUrl, new { email, password = Password });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.IsNotNull(body);
        CollectionAssert.Contains(body.Roles, "Patient");
    }

    // AC2: a logged-in patient opens a page that is only for huisartsen.
    // He gets no access (403), but stays logged in.
    [TestMethod]
    [TestProperty("LOG-02", "AC2")]
    public async Task TC_LOG_07_PatientOpensHuisartsPage_ForbiddenButStaysLoggedIn()
    {
        var email = await TestEnvironment.CreateUserAsync("Patient", Password);
        using var client = TestEnvironment.CreateClient();
        var login = await client.PostAsJsonAsync(LoginUrl, new { email, password = Password });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode, "Logging in must succeed first.");

        var huisartsPage = await client.GetAsync(GeneralPractitionerOnlyUrl);
        var currentUser = await client.GetAsync(CurrentUserUrl);

        Assert.AreEqual(HttpStatusCode.Forbidden, huisartsPage.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, currentUser.StatusCode);
    }

    // AC3: wrong password. The user gets the message from the acceptance criterion and is not logged in.
    [TestMethod]
    [TestProperty("LOG-02", "AC3")]
    public async Task TC_LOG_08_WrongPassword_RejectedWithMessage()
    {
        var email = await TestEnvironment.CreateUserAsync("Patient", Password);
        using var client = TestEnvironment.CreateClient();

        var response = await client.PostAsJsonAsync(LoginUrl, new { email, password = "Fout123!wachtwoord" });
        var currentUser = await client.GetAsync(CurrentUserUrl);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(error);
        Assert.AreEqual("Het e-mailadres of het wachtwoord klopt niet.", error.Message);
        Assert.AreEqual(HttpStatusCode.Unauthorized, currentUser.StatusCode);
    }

    private sealed record LoginResponse(List<string> Roles);

    private sealed record ErrorResponse(string Message);
}