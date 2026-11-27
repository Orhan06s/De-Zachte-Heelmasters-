using System.Net;

namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// AFS-02 No double bookings.
/// Integration tests for test cases TC-AFS-05 to TC-AFS-07 from the test plan.
/// Assumptions: a specialist works Monday to Friday from 9:00 to 17:00 and available slots start every quarter of an hour.
/// Since AFS-01 a patient books through a referral. The duration comes from the care code:
/// Z0003 "Consult longen" takes 2 quarters, Z0001 "Consult hart" takes 3 quarters.
/// </summary>
[TestClass]
public class NoDoubleBookingsIntegrationTest
{
    // Text shown to the patient, so it stays in Dutch.
    private const string SlotNoLongerAvailableMessage = "Dit moment is niet meer vrij.";

    [TestMethod]
    [TestProperty("AFS-02", "AC1")]
    public async Task TC_AFS_05_BookedSlot_IsNotOffered()
    {
        // Arrange
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Longgeneeskunde");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patientA = await AppointmentTestHelpers.LoggedInPatientAsync();
        using var patientB = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralA = await AppointmentTestHelpers.CreateReferralAsync(patientA, "Z0003");
        var referralB = await AppointmentTestHelpers.CreateReferralAsync(patientB, "Z0003");

        var booking = await AppointmentTestHelpers.BookAsync(patientA, referralA, specialistId, date, "11:00");
        Assert.AreEqual(HttpStatusCode.Created, booking.StatusCode, "The first booking should succeed.");

        // Act
        var slots = await AppointmentTestHelpers.GetStartTimesAsync(patientB, referralB, specialistId, date);

        // Assert
        CollectionAssert.DoesNotContain(slots, "11:00", "A booked slot should not be offered.");
        // Without this check an empty list would also pass the test.
        CollectionAssert.Contains(slots, "13:00", "A slot that is still free should be offered.");
    }

    [TestMethod]
    [TestProperty("AFS-02", "AC2")]
    public async Task TC_AFS_06_OverlappingSlot_IsNotOffered()
    {
        // Arrange
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Cardiologie");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patientA = await AppointmentTestHelpers.LoggedInPatientAsync();
        using var patientB = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralA = await AppointmentTestHelpers.CreateReferralAsync(patientA, "Z0001");
        var referralB = await AppointmentTestHelpers.CreateReferralAsync(patientB, "Z0001");

        var booking = await AppointmentTestHelpers.BookAsync(patientA, referralA, specialistId, date, "10:00");
        Assert.AreEqual(HttpStatusCode.Created, booking.StatusCode, "The appointment from 10:00 to 10:45 should succeed.");

        // Act
        var slots = await AppointmentTestHelpers.GetStartTimesAsync(patientB, referralB, specialistId, date);

        // Assert
        foreach (var time in new[] { "09:30", "09:45", "10:00", "10:15", "10:30" })
        {
            CollectionAssert.DoesNotContain(slots, time, $"An appointment of 3 quarters at {time} overlaps with 10:00-10:45.");
        }

        // Boundary values: these touch the appointment but do not overlap.
        CollectionAssert.Contains(slots, "09:15", "9:15-10:00 does not overlap and should be offered.");
        CollectionAssert.Contains(slots, "10:45", "10:45-11:30 does not overlap and should be offered.");
    }

    [TestMethod]
    [TestProperty("AFS-02", "AC3")]
    public async Task TC_AFS_07_TwoPatientsAtOnce_OneSucceedsOtherGetsMessage()
    {
        // Arrange
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Longgeneeskunde");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patientA = await AppointmentTestHelpers.LoggedInPatientAsync();
        using var patientB = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralA = await AppointmentTestHelpers.CreateReferralAsync(patientA, "Z0003");
        var referralB = await AppointmentTestHelpers.CreateReferralAsync(patientB, "Z0003");

        // Act
        var responses = await Task.WhenAll(
            AppointmentTestHelpers.BookAsync(patientA, referralA, specialistId, date, "14:00"),
            AppointmentTestHelpers.BookAsync(patientB, referralB, specialistId, date, "14:00"));

        // Assert
        Assert.AreEqual(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created), "Exactly one booking should succeed.");
        Assert.AreEqual(1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict), "The other booking should be refused (409).");

        var refused = responses.First(r => r.StatusCode == HttpStatusCode.Conflict);
        Assert.AreEqual(SlotNoLongerAvailableMessage, await AppointmentTestHelpers.ReadErrorMessageAsync(refused));

        Assert.AreEqual(1, await AppointmentTestHelpers.CountAppointmentsAsync(specialistId, date), "Only one appointment should be saved.");

        var slots = await AppointmentTestHelpers.GetStartTimesAsync(patientB, referralB, specialistId, date);
        CollectionAssert.DoesNotContain(slots, "14:00", "The booked slot should no longer be offered.");
    }
}
