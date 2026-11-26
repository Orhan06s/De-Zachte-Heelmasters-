using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Services;
using Moq;

namespace DeZachteHeelmeestersTests.Appointments;

[TestClass]
public class AppointmentServiceTest
{
    private Mock<IAppointmentRepository> _mockAppointmentRepository = null!;
    private Mock<ITimeSlotRepository> _mockTimeSlotRepository = null!;
    private Mock<IReferralRepository> _mockReferralRepository = null!;
    private AppointmentService _appointmentService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockTimeSlotRepository = new Mock<ITimeSlotRepository>();
        _mockReferralRepository = new Mock<IReferralRepository>();

        _appointmentService = new AppointmentService(
            _mockAppointmentRepository.Object,
            _mockTimeSlotRepository.Object,
            _mockReferralRepository.Object);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC1")]
    public async Task GetAvailableTimeSlotsAsync_ValidReferral_ReturnsMatchingAvailableSlots()
    {
        // Arrange
        var bsn = "111222333";
        var referral = new Referral
        {
            ReferralId = 1,
            PatientBSN = bsn,
            CareCode = "CARD-01",
            Specialization = "Cardiologie",
            DurationMinutes = 30
        };

        var specialist = new Employee { EmployeeId = 10, Name = "Dr. Jansen", Specialization = "Cardiologie" };
        var slots = new List<TimeSlot>
        {
            new TimeSlot
            {
                TimeSlotId = 100,
                EmployeeId = 10,
                Employee = specialist,
                Date = DateTime.Today.AddDays(3),
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(9, 30, 0),
                Appointments = new List<Appointment>()
            }
        };

        _mockReferralRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(referral);
        _mockTimeSlotRepository.Setup(r => r.GetAvailableSlotsBySpecializationAsync("Cardiologie")).ReturnsAsync(slots);

        // Act
        var (resultSlots, errors) = await _appointmentService.GetAvailableTimeSlotsAsync(bsn, 1);

        // Assert
        Assert.AreEqual(0, errors.Count);
        Assert.AreEqual(1, resultSlots.Count);
        Assert.AreEqual(100, resultSlots[0].TimeSlotId);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC2")]
    public async Task BookAppointmentAsync_ValidSlotAndReferral_SavesAppointment()
    {
        // Arrange
        var bsn = "111222333";
        var referral = new Referral
        {
            ReferralId = 1,
            PatientBSN = bsn,
            CareCode = "CARD-01",
            Specialization = "Cardiologie",
            DurationMinutes = 30
        };

        var timeSlot = new TimeSlot
        {
            TimeSlotId = 100,
            EmployeeId = 10,
            Date = DateTime.Today.AddDays(5),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(10, 30, 0),
            Appointments = new List<Appointment>()
        };

        _mockReferralRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(referral);
        _mockTimeSlotRepository.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(timeSlot);

        // Act
        var errors = await _appointmentService.BookAppointmentAsync(bsn, 1, 100);

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockAppointmentRepository.Verify(r => r.AddAsync(It.Is<Appointment>(a =>
            a.PatientBSN == bsn &&
            a.TimeSlotId == 100 &&
            a.Status == AppointmentStatus.Scheduled)), Times.Once);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC3")]
    public async Task BookAppointmentAsync_SixMonthsInAdvance_SavesAppointment()
    {
        // Arrange
        var bsn = "111222333";
        var referral = new Referral
        {
            ReferralId = 1,
            PatientBSN = bsn,
            CareCode = "CARD-01",
            Specialization = "Cardiologie",
            DurationMinutes = 30
        };

        var futureDate = DateTime.Today.AddMonths(6);
        var timeSlot = new TimeSlot
        {
            TimeSlotId = 200,
            EmployeeId = 10,
            Date = futureDate,
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(14, 30, 0),
            Appointments = new List<Appointment>()
        };

        _mockReferralRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(referral);
        _mockTimeSlotRepository.Setup(r => r.GetByIdAsync(200)).ReturnsAsync(timeSlot);

        // Act
        var errors = await _appointmentService.BookAppointmentAsync(bsn, 1, 200);

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockAppointmentRepository.Verify(r => r.AddAsync(It.Is<Appointment>(a =>
            a.PatientBSN == bsn &&
            a.TimeSlotId == 200)), Times.Once);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task GetAvailableTimeSlotsAsync_NoReferral_ReturnsErrorAndNoSlots()
    {
        // Arrange
        var bsn = "111222333";
        _mockReferralRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Referral?)null);

        // Act
        var (resultSlots, errors) = await _appointmentService.GetAvailableTimeSlotsAsync(bsn, 99);

        // Assert
        Assert.IsTrue(errors.Count > 0);
        Assert.AreEqual(0, resultSlots.Count);
    }

    [TestMethod]
    [TestProperty("AFS-01", "AC4")]
    public async Task BookAppointmentAsync_NoReferral_DoesNotSaveAppointment()
    {
        // Arrange
        var bsn = "111222333";
        _mockReferralRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Referral?)null);

        // Act
        var errors = await _appointmentService.BookAppointmentAsync(bsn, 99, 100);

        // Assert
        Assert.IsTrue(errors.Count > 0);
        _mockAppointmentRepository.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }
}