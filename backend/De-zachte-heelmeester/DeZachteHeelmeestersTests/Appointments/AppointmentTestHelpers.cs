using System.Net;
using System.Net.Http.Json;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Validators;
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
    private const string ReferralsUrl = "/api/appointments/referrals";
    private const string AvailableSlotsUrl = "/api/appointments/available";
    private const string AppointmentsUrl = "/api/appointments";

    /// <summary>
    /// Returns the first Monday at least four weeks from today. Always a working day in the future, so the test does not depend on today's date.
    /// </summary>
    public static DateOnly MondayInFourWeeks() => FirstMondayFrom(DateTime.Today.AddDays(28));

    /// <summary>
    /// Returns the first Monday on or after the given day.
    /// </summary>
    public static DateOnly FirstMondayFrom(DateTime from)
    {
        var day = DateOnly.FromDateTime(from);
        while (day.DayOfWeek != DayOfWeek.Monday)
        {
            day = day.AddDays(1);
        }

        return day;
    }

    /// <summary>
    /// Creates a new specialist and returns the EmployeeId. Every test gets its own specialist, so the tests do not see each other's appointments.
    /// </summary>
    public static async Task<int> CreateSpecialistAsync(string specialization, WorkDays workDays = WorkDays.MondayToFriday)
    {
        var email = await TestEnvironment.CreateUserAsync("Specialist", Password);

        using var scope = TestEnvironment.Api.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync(email);
        Assert.IsNotNull(user);

        var specialist = new Employee { UserId = user.Id, Name = "Test specialist", Specialization = specialization, WorkDays = workDays };
        db.Employees.Add(specialist);
        await db.SaveChangesAsync();

        return specialist.EmployeeId;
    }

    /// <summary>
    /// Returns a new patient that is logged in. The client keeps the login cookie.
    /// </summary>
    public static async Task<TestPatient> LoggedInPatientAsync()
    {
        var email = await TestEnvironment.CreateUserAsync("Patient", Password);
        var bsn = CreateValidBsn();

        using (var scope = TestEnvironment.Api.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await userManager.FindByEmailAsync(email);
            Assert.IsNotNull(user);

            db.Patients.Add(new Patient
            {
                BSN = bsn,
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

        return new TestPatient(client, bsn);
    }

    /// <summary>
    /// Creates a referral for the patient with the given care code and returns the ReferralId.
    /// Written straight to the test database, because the screen for the general practitioner comes with VWZ-01.
    /// </summary>
    public static async Task<int> CreateReferralAsync(TestPatient patient, string careCode)
    {
        using var scope = TestEnvironment.Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var referral = new Referral { PatientBSN = patient.Bsn, CareCode = careCode };
        db.Referrals.Add(referral);
        await db.SaveChangesAsync();

        return referral.ReferralId;
    }

    /// <summary>
    /// Gets the referrals of the logged-in patient.
    /// </summary>
    public static async Task<List<ReferralItem>> GetReferralsAsync(TestPatient patient)
    {
        var response = await patient.Client.GetAsync(ReferralsUrl);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Getting the referrals should succeed.");

        var referrals = await response.Content.ReadFromJsonAsync<List<ReferralItem>>();
        Assert.IsNotNull(referrals);

        return referrals;
    }

    /// <summary>
    /// Books an appointment for a referral and returns the response of the API. The duration comes from the care code of the referral.
    /// </summary>
    public static Task<HttpResponseMessage> BookAsync(TestPatient patient, int referralId, int specialistId, DateOnly date, string startTime) =>
        patient.Client.PostAsJsonAsync(AppointmentsUrl, new
        {
            referralId,
            specialistId,
            date = date.ToString("yyyy-MM-dd"),
            startTime
        });

    /// <summary>
    /// Asks the free moments for a referral, for seven days starting at the given date, and returns the response of the API.
    /// </summary>
    public static Task<HttpResponseMessage> RequestAvailableSlotsAsync(TestPatient patient, int referralId, DateOnly from) =>
        patient.Client.GetAsync($"{AvailableSlotsUrl}?referralId={referralId}&from={from:yyyy-MM-dd}");

    /// <summary>
    /// Gets the free moments for a referral, for seven days starting at the given date.
    /// </summary>
    public static async Task<List<SlotItem>> GetAvailableSlotsAsync(TestPatient patient, int referralId, DateOnly from)
    {
        var response = await RequestAvailableSlotsAsync(patient, referralId, from);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Getting the available slots should succeed.");

        var slots = await response.Content.ReadFromJsonAsync<List<SlotItem>>();
        Assert.IsNotNull(slots);

        return slots;
    }

    /// <summary>
    /// Gets the start times of one specialist on one day, for example "09:15".
    /// </summary>
    public static async Task<List<string>> GetStartTimesAsync(TestPatient patient, int referralId, int specialistId, DateOnly date)
    {
        var slots = await GetAvailableSlotsAsync(patient, referralId, date);
        return slots.Where(s => s.SpecialistId == specialistId && s.Date == date).Select(s => s.StartTime).ToList();
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
        var appointments = await GetAppointmentsAsync(specialistId, date);
        return appointments.Count;
    }

    /// <summary>
    /// Returns the appointments of a specialist on a day that are not cancelled, with their time slot.
    /// Reads straight from the test database, because the planning of the specialist comes with AFS-03.
    /// </summary>
    public static async Task<List<Appointment>> GetAppointmentsAsync(int specialistId, DateOnly date)
    {
        using var scope = TestEnvironment.Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var day = date.ToDateTime(TimeOnly.MinValue);

        return await db.Appointments
            .Include(a => a.TimeSlot)
            .Where(a =>
                a.TimeSlot.EmployeeId == specialistId &&
                a.TimeSlot.Date == day &&
                a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();
    }

    // The database only accepts a BSN that passes the elfproef (CK_Patients_BSN)
    private static string CreateValidBsn()
    {
        while (true)
        {
            var bsn = Random.Shared.Next(100_000_000, 1_000_000_000).ToString();
            if (BsnValidator.IsValid(bsn))
            {
                return bsn;
            }
        }
    }

    private sealed record ErrorResponse(string Message);
}
