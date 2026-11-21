using System.Net;
using System.Net.Http.Json;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// Shared setup for the appointment tests.
/// </summary>
public static class AppointmentTestHelpers
{
    private const string Password = "Test123!wachtwoord";
    private const string LoginUrl = "/api/auth/login";
    private const string AvailableSlotsUrl = "/api/appointments/available";
    private const string AppointmentsUrl = "/api/appointments";

    /// <summary>
    /// Returns the first Monday at least four weeks from today. Always a working day in the future, so the test does not depend on today's date.
    /// </summary>
    public static DateOnly MondayInFourWeeks()
    {
        var day = DateOnly.FromDateTime(DateTime.Today.AddDays(28));
        while (day.DayOfWeek != DayOfWeek.Monday)
        {
            day = day.AddDays(1);
        }

        return day;
    }

    /// <summary>
    /// Creates a new specialist and returns the EmployeeId. Every test gets its own specialist, so the tests do not see each other's appointments.
    /// </summary>
    public static async Task<int> CreateSpecialistAsync()
    {
        var email = await TestEnvironment.CreateUserAsync("Specialist", Password);

        using var scope = TestEnvironment.Api.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync(email);
        Assert.IsNotNull(user);

        var specialist = new Employee { UserId = user.Id, Name = "Test specialist", Specialization = "Cardiologie" };
        db.Employees.Add(specialist);
        await db.SaveChangesAsync();

        return specialist.EmployeeId;
    }

    /// <summary>
    /// Returns a client that is logged in as a new patient. The client keeps the login cookie.
    /// </summary>
    public static async Task<HttpClient> LoggedInPatientAsync()
    {
        var email = await TestEnvironment.CreateUserAsync("Patient", Password);

        using (var scope = TestEnvironment.Api.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await userManager.FindByEmailAsync(email);
            Assert.IsNotNull(user);

            db.Patients.Add(new Patient
            {
                BSN = Random.Shared.Next(100_000_000, 1_000_000_000).ToString(),
                UserId = user.Id,
                FirstName = "Test",
                LastName = "Patient",
                Email = email
            });
            await db.SaveChangesAsync();
        }

        var client = TestEnvironment.CreateClient();
        var login = await client.PostAsJsonAsync(LoginUrl, new { email, password = Password });
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode, "Logging in should succeed first.");

        return client;
    }

    /// <summary>
    /// Books an appointment and returns the response of the API. The duration is the number of quarters of an hour.
    /// </summary>
    public static Task<HttpResponseMessage> BookAsync(HttpClient client, int specialistId, DateOnly date, string startTime, int quarters) =>
        client.PostAsJsonAsync(AppointmentsUrl, new
        {
            specialistId,
            date = date.ToString("yyyy-MM-dd"),
            startTime,
            durationQuarters = quarters
        });

    /// <summary>
    /// Gets the available slots of a specialist on a day and returns only the start times, for example "09:15".
    /// </summary>
    public static async Task<List<string>> GetAvailableSlotsAsync(HttpClient client, int specialistId, DateOnly date, int quarters)
    {
        var url = $"{AvailableSlotsUrl}?specialistId={specialistId}&date={date:yyyy-MM-dd}&durationQuarters={quarters}";
        var response = await client.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Getting the available slots should succeed.");

        var slots = await response.Content.ReadFromJsonAsync<List<AvailableSlot>>();
        Assert.IsNotNull(slots);

        return slots.Select(s => s.StartTime).ToList();
    }

    /// <summary>
    /// Returns the message from an error response of the API.
    /// </summary>
    public static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.IsNotNull(error);

        return error.Message;
    }

    /// <summary>
    /// Counts the appointments of a specialist on a day that are not cancelled. Reads straight from the test database, because the API has no endpoint for this yet.
    /// </summary>
    public static async Task<int> CountAppointmentsAsync(int specialistId, DateOnly date)
    {
        using var scope = TestEnvironment.Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var day = date.ToDateTime(TimeOnly.MinValue);

        return await db.Appointments.CountAsync(a =>
            a.TimeSlot.EmployeeId == specialistId &&
            a.TimeSlot.Date == day &&
            a.Status != AppointmentStatus.Cancelled);
    }

    private sealed record AvailableSlot(string StartTime, string EndTime);

    private sealed record ErrorResponse(string Message);
}
