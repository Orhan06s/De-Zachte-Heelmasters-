using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Services;
using Moq;

namespace DeZachteHeelmeestersTests.Appointments;

[TestClass]
public class AppointmentServiceTest
{
    private const string UserId = "user-1";
    private const string Bsn = "111222333";

    private Mock<IEmployeeRepository> _mockEmployeeRepository = null!;
    private Mock<IPatientRepository> _mockPatientRepository = null!;
    private Mock<ITimeSlotRepository> _mockTimeSlotRepository = null!;
    private Mock<IReferralRepository> _mockReferralRepository = null!;
    private AppointmentService _appointmentService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockTimeSlotRepository = new Mock<ITimeSlotRepository>();
        _mockReferralRepository = new Mock<IReferralRepository>();

        // The logged-in user is the patient with this BSN
        _mockPatientRepository.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new Patient { BSN = Bsn, UserId = UserId });

        // The lock just runs the work, there is no database in a unit test
        _mockTimeSlotRepository
            .Setup(r => r.RunWithPlanningLockAsync(It.IsAny<int>(), It.IsAny<Func<Task<BookingResult>>>()))
            .Returns((int _, Func<Task<BookingResult>> work) => work());

        _appointmentService = new AppointmentService(
            _mockEmployeeRepository.Object,
            _mockPatientRepository.Object,
            _mockTimeSlotRepository.Object,
            _mockReferralRepository.Object);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC1")]
    public async Task GetAvailableTimeSlotsAsync_ValidReferral_ReturnsMatchingAvailableSlots()
    {
        // Arrange: Z0001 "Consult hart" is Cardiologie and takes 3 quarters
        var monday = NextMonday(DateTime.Today.AddDays(7));
        SetupReferral(CardiologyReferral());
        SetupSpecialists("Cardiologie", new Employee { EmployeeId = 10, Name = "Dr. Jansen", Specialization = "Cardiologie" });
        SetupSpecialists("Dermatologie", new Employee { EmployeeId = 20, Name = "Dr. de Vries", Specialization = "Dermatologie" });

        // Act
        var result = await _appointmentService.GetAvailableSlotsForReferralAsync(UserId, 1, monday);

        // Assert
        Assert.IsTrue(result.ReferralFound);
        Assert.IsNotEmpty(result.Slots);
        Assert.IsTrue(result.Slots.All(s => s.SpecialistId == 10), "Only specialists with the specialism of the care code.");
        Assert.IsTrue(result.Slots.All(s => s.Date >= monday && s.Date < monday.AddDays(7)), "Only the seven days that were asked for.");

        var first = result.Slots[0];
        Assert.AreEqual(monday, first.Date);
        Assert.AreEqual("09:00", first.StartTime);
        Assert.AreEqual("09:45", first.EndTime, "The duration comes from the list of care treatments.");
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC1")]
    public async Task GetAvailableTimeSlotsAsync_SpecialistDoesNotWorkOnFriday_ReturnsNoSlotsOnFriday()
    {
        // Arrange: the specialist works Monday to Thursday
        var monday = NextMonday(DateTime.Today.AddDays(7));
        SetupReferral(CardiologyReferral());
        SetupSpecialists("Cardiologie", new Employee
        {
            EmployeeId = 10,
            Name = "Dr. Jansen",
            Specialization = "Cardiologie",
            WorkDays = WorkDays.Monday | WorkDays.Tuesday | WorkDays.Wednesday | WorkDays.Thursday
        });

        // Act
        var result = await _appointmentService.GetAvailableSlotsForReferralAsync(UserId, 1, monday);

        // Assert
        Assert.IsTrue(result.Slots.Any(s => s.Date == monday), "There are slots on Monday.");
        Assert.IsFalse(result.Slots.Any(s => s.Date.DayOfWeek == DayOfWeek.Friday), "No slots on a day the specialist does not work.");
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC2")]
    public async Task BookAppointmentAsync_ValidSlotAndReferral_SavesAppointment()
    {
        // Arrange
        var date = NextMonday(DateTime.Today.AddDays(7));
        SetupReferral(CardiologyReferral());
        SetupSpecialists("Cardiologie", new Employee { EmployeeId = 10, Name = "Dr. Jansen", Specialization = "Cardiologie" });

        // Act
        var result = await _appointmentService.BookAsync(UserId, BookingRequest(date, "10:00"));

        // Assert: saved in the planning of the specialist, for this patient and linked to the referral
        Assert.AreEqual(BookingResult.Booked, result);
        _mockTimeSlotRepository.Verify(r => r.AddAsync(It.Is<TimeSlot>(t =>
            t.EmployeeId == 10 &&
            t.Date == date.ToDateTime(TimeOnly.MinValue) &&
            t.StartTime == new TimeSpan(10, 0, 0) &&
            t.EndTime == new TimeSpan(10, 45, 0) &&
            t.Appointments.Count == 1 &&
            t.Appointments.All(a =>
                a.PatientBSN == Bsn &&
                a.ReferralId == 1 &&
                a.Status == AppointmentStatus.Scheduled))), Times.Once);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC3")]
    public async Task BookAppointmentAsync_SixMonthsInAdvance_SavesAppointment()
    {
        // Arrange: a Monday, so the test does not fail when six months from today is a weekend
        var futureDate = NextMonday(DateTime.Today.AddMonths(6));
        SetupReferral(CardiologyReferral());
        SetupSpecialists("Cardiologie", new Employee { EmployeeId = 10, Name = "Dr. Jansen", Specialization = "Cardiologie" });

        // Act
        var result = await _appointmentService.BookAsync(UserId, BookingRequest(futureDate, "14:00"));

        // Assert
        Assert.AreEqual(BookingResult.Booked, result);
        _mockTimeSlotRepository.Verify(r => r.AddAsync(It.Is<TimeSlot>(t =>
            t.Date == futureDate.ToDateTime(TimeOnly.MinValue) &&
            t.Appointments.All(a => a.PatientBSN == Bsn))), Times.Once);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task GetAvailableTimeSlotsAsync_NoReferral_ReturnsErrorAndNoSlots()
    {
        // Arrange
        _mockReferralRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Referral?)null);

        // Act
        var result = await _appointmentService.GetAvailableSlotsForReferralAsync(UserId, 99, NextMonday(DateTime.Today.AddDays(7)));

        // Assert
        Assert.IsFalse(result.ReferralFound);
        Assert.IsEmpty(result.Slots);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task BookAppointmentAsync_NoReferral_DoesNotSaveAppointment()
    {
        // Arrange
        _mockReferralRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Referral?)null);
        var request = BookingRequest(NextMonday(DateTime.Today.AddDays(7)), "10:00");
        request.ReferralId = 99;

        // Act
        var result = await _appointmentService.BookAsync(UserId, request);

        // Assert
        Assert.AreEqual(BookingResult.ReferralNotFound, result);
        _mockTimeSlotRepository.Verify(r => r.AddAsync(It.IsAny<TimeSlot>()), Times.Never);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task BookAppointmentAsync_ReferralOfOtherPatient_DoesNotSaveAppointment()
    {
        // Arrange: the referral exists, but belongs to another patient
        var referral = CardiologyReferral();
        referral.PatientBSN = "999888777";
        SetupReferral(referral);
        SetupSpecialists("Cardiologie", new Employee { EmployeeId = 10, Name = "Dr. Jansen", Specialization = "Cardiologie" });

        // Act
        var result = await _appointmentService.BookAsync(UserId, BookingRequest(NextMonday(DateTime.Today.AddDays(7)), "10:00"));

        // Assert
        Assert.AreEqual(BookingResult.ReferralNotFound, result);
        _mockTimeSlotRepository.Verify(r => r.AddAsync(It.IsAny<TimeSlot>()), Times.Never);
    }

    private static Referral CardiologyReferral() => new()
    {
        ReferralId = 1,
        PatientBSN = Bsn,
        CareCode = "Z0001",
        CareTreatment = new CareTreatment
        {
            CareCode = "Z0001",
            Description = "Consult hart",
            Specialization = "Cardiologie",
            DurationQuarters = 3
        }
    };

    private static BookAppointmentRequest BookingRequest(DateOnly date, string startTime) => new()
    {
        ReferralId = 1,
        SpecialistId = 10,
        Date = date,
        StartTime = startTime
    };

    private void SetupReferral(Referral referral) =>
        _mockReferralRepository.Setup(r => r.GetByIdAsync(referral.ReferralId)).ReturnsAsync(referral);

    private void SetupSpecialists(string specialization, params Employee[] specialists) =>
        _mockEmployeeRepository.Setup(r => r.GetBySpecializationAsync(specialization)).ReturnsAsync(specialists.ToList());

    // The first Monday on or after the given day. Always a working day, so the tests do not depend on today's date.
    private static DateOnly NextMonday(DateTime from)
    {
        var day = DateOnly.FromDateTime(from);
        while (day.DayOfWeek != DayOfWeek.Monday)
        {
            day = day.AddDays(1);
        }

        return day;
    }
}
