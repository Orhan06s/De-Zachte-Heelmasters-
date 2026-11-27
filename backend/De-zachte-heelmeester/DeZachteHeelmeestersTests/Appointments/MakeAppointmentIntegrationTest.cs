using System.Net;
using De_zachte_heelmeester.Models;

namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// AFS-01 Patient makes an appointment at an available time.
/// Integration tests for test cases TC-AFS-01 to TC-AFS-04 from the test plan
/// (TC-AFS-01 = AC2, TC-AFS-02 = AC4, TC-AFS-03 = AC1, TC-AFS-04 = AC3).
/// Z0001 "Consult hart" is Cardiologie and takes 3 quarters of an hour.
/// </summary>
[TestClass]
public class MakeAppointmentIntegrationTest
{
    // Text shown to the patient, so it stays in Dutch.
    private const string ReferralNeededMessage = "Je hebt eerst een doorverwijzing van je huisarts nodig.";

    [TestMethod]
    [TestProperty("AFS-01", "AC2")]
    public async Task TC_AFS_01_ConfirmMoment_AppointmentIsSavedWithReferral()
    {
        // Arrange: the patient has a referral and chooses it
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Cardiologie");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patient = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralId = await AppointmentTestHelpers.CreateReferralAsync(patient, "Z0001");

        var referrals = await AppointmentTestHelpers.GetReferralsAsync(patient);
        Assert.IsTrue(referrals.Any(r => r.ReferralId == referralId && r.CareCode == "Z0001"), "The patient sees his referral.");

        var firstFreeMoment = (await AppointmentTestHelpers.GetStartTimesAsync(patient, referralId, specialistId, date))[0];

        // Act
        var booking = await AppointmentTestHelpers.BookAsync(patient, referralId, specialistId, date, firstFreeMoment);

        // Assert: the appointment is in the planning of the specialist, for this patient and linked to the referral
        Assert.AreEqual(HttpStatusCode.Created, booking.StatusCode);

        var appointments = await AppointmentTestHelpers.GetAppointmentsAsync(specialistId, date);
        Assert.HasCount(1, appointments);
        Assert.AreEqual(patient.Bsn, appointments[0].PatientBSN);
        Assert.AreEqual(referralId, appointments[0].ReferralId);
        Assert.AreEqual(TimeSpan.FromMinutes(45), appointments[0].TimeSlot.EndTime - appointments[0].TimeSlot.StartTime, "The duration comes from the care code.");
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task TC_AFS_02_NoReferral_ShowsMessageAndSavesNothing()
    {
        // Arrange: the patient has no referral
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Cardiologie");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patient = await AppointmentTestHelpers.LoggedInPatientAsync();
        const int unknownReferralId = 999999;

        // Act
        var referrals = await AppointmentTestHelpers.GetReferralsAsync(patient);
        var slotsResponse = await AppointmentTestHelpers.RequestAvailableSlotsAsync(patient, unknownReferralId, date);
        var booking = await AppointmentTestHelpers.BookAsync(patient, unknownReferralId, specialistId, date, "10:00");

        // Assert: no moments, the message about the referral, nothing saved
        Assert.IsEmpty(referrals);
        Assert.AreEqual(HttpStatusCode.NotFound, slotsResponse.StatusCode);
        Assert.AreEqual(ReferralNeededMessage, await AppointmentTestHelpers.ReadErrorMessageAsync(slotsResponse));
        Assert.AreEqual(HttpStatusCode.NotFound, booking.StatusCode);
        Assert.AreEqual(ReferralNeededMessage, await AppointmentTestHelpers.ReadErrorMessageAsync(booking));
        Assert.AreEqual(0, await AppointmentTestHelpers.CountAppointmentsAsync(specialistId, date));
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task TC_AFS_02_ReferralOfOtherPatient_ShowsMessageAndSavesNothing()
    {
        // Arrange: patient B tries to use the referral of patient A
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Cardiologie");
        var date = AppointmentTestHelpers.MondayInFourWeeks();
        using var patientA = await AppointmentTestHelpers.LoggedInPatientAsync();
        using var patientB = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralOfA = await AppointmentTestHelpers.CreateReferralAsync(patientA, "Z0001");

        // Act
        var slotsResponse = await AppointmentTestHelpers.RequestAvailableSlotsAsync(patientB, referralOfA, date);
        var booking = await AppointmentTestHelpers.BookAsync(patientB, referralOfA, specialistId, date, "10:00");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, slotsResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, booking.StatusCode);
        Assert.AreEqual(0, await AppointmentTestHelpers.CountAppointmentsAsync(specialistId, date));
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC1")]
    public async Task TC_AFS_03_FreeMoments_OnlyMatchingSpecialistOnWorkDaysWithDurationFromList()
    {
        // Arrange: a cardiologist who works Monday to Thursday and a dermatologist
        var cardiologistId = await AppointmentTestHelpers.CreateSpecialistAsync(
            "Cardiologie", WorkDays.Monday | WorkDays.Tuesday | WorkDays.Wednesday | WorkDays.Thursday);
        var dermatologistId = await AppointmentTestHelpers.CreateSpecialistAsync("Dermatologie");
        var monday = AppointmentTestHelpers.MondayInFourWeeks();
        using var patient = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralId = await AppointmentTestHelpers.CreateReferralAsync(patient, "Z0001");

        // Act
        var slots = await AppointmentTestHelpers.GetAvailableSlotsAsync(patient, referralId, monday);

        // Assert
        var cardiologistSlots = slots.Where(s => s.SpecialistId == cardiologistId).ToList();
        Assert.IsTrue(cardiologistSlots.Any(s => s.Date == monday), "The cardiologist has free moments on Monday.");
        Assert.IsFalse(cardiologistSlots.Any(s => s.Date.DayOfWeek == DayOfWeek.Friday), "No moments on a day the specialist does not work.");
        Assert.IsFalse(slots.Any(s => s.SpecialistId == dermatologistId), "No moments of a specialist with another specialism.");
        Assert.IsTrue(slots.All(s => s.Date >= monday && s.Date < monday.AddDays(7)), "Only the seven days that were asked for.");
        Assert.IsTrue(slots.All(s => TimeSpan.Parse(s.EndTime) - TimeSpan.Parse(s.StartTime) == TimeSpan.FromMinutes(45)),
            "The duration comes from the list of care treatments.");
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC3")]
    public async Task TC_AFS_04_MomentSixMonthsAhead_CanBeBooked()
    {
        // Arrange: a Monday at least six months from today
        var specialistId = await AppointmentTestHelpers.CreateSpecialistAsync("Cardiologie");
        var date = AppointmentTestHelpers.FirstMondayFrom(DateTime.Today.AddMonths(6));
        using var patient = await AppointmentTestHelpers.LoggedInPatientAsync();
        var referralId = await AppointmentTestHelpers.CreateReferralAsync(patient, "Z0001");

        // Act
        var booking = await AppointmentTestHelpers.BookAsync(patient, referralId, specialistId, date, "14:00");

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, booking.StatusCode, "There is no limit to how far ahead an appointment can be made.");
        Assert.AreEqual(1, await AppointmentTestHelpers.CountAppointmentsAsync(specialistId, date));
    }
}
