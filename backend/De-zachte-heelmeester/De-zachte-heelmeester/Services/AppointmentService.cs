using System.Globalization;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;

namespace De_zachte_heelmeester.Services;

/// <summary>
/// Finds the available slots of a specialist and books appointments without double bookings.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IReferralRepository _referralRepository;

    public AppointmentService(
        IEmployeeRepository employeeRepository,
        IPatientRepository patientRepository,
        ITimeSlotRepository timeSlotRepository,
        IReferralRepository referralRepository)
    {
        _employeeRepository = employeeRepository;
        _patientRepository = patientRepository;
        _timeSlotRepository = timeSlotRepository;
        _referralRepository = referralRepository;
    }

    public Task<bool> SpecialistExistsAsync(int specialistId) =>
        _employeeRepository.ExistsAsync(specialistId);

    /// <summary>
    /// Returns every start time on the day where an appointment of the given length fits
    /// within the working hours without overlapping an appointment that is already booked.
    /// </summary>
    public async Task<List<AvailableSlotResponse>> GetAvailableSlotsAsync(int specialistId, DateOnly date, int durationQuarters)
    {
        var slots = new List<AvailableSlotResponse>();
        if (!IsBookableDay(date) || !IsValidDuration(durationQuarters))
        {
            return slots;
        }

        var duration = WorkingHours.Quarter * durationQuarters;
        var booked = await _timeSlotRepository.GetBookedAsync(specialistId, date);

        for (var start = WorkingHours.Start; start + duration <= WorkingHours.End; start += WorkingHours.Quarter)
        {
            var end = start + duration;
            if (!booked.Any(b => Overlaps(start, end, b.StartTime, b.EndTime)))
            {
                slots.Add(new AvailableSlotResponse(Format(start), Format(end)));
            }
        }

        return slots;
    }

    /// <summary>
    /// Returns the free moments for the referral of the logged-in patient, for seven days starting at the given date.
    /// </summary>
    public Task<ReferralSlotsResult> GetAvailableSlotsForReferralAsync(string userId, int referralId, DateOnly from)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Books an appointment for the patient of the logged-in user.
    /// Returns SlotNotAvailable when the moment overlaps an appointment that is already booked.
    /// </summary>
    public async Task<BookingResult> BookAsync(string userId, BookAppointmentRequest request)
    {
        if (!TryGetTimes(request, out var start, out var end))
        {
            return BookingResult.InvalidRequest;
        }

        if (!await SpecialistExistsAsync(request.SpecialistId))
        {
            return BookingResult.SpecialistNotFound;
        }

        var patient = await _patientRepository.GetByUserIdAsync(userId);
        if (patient == null)
        {
            return BookingResult.PatientNotFound;
        }

        // The check and the save happen inside the lock, so two patients cannot both book the same moment.
        return await _timeSlotRepository.RunWithPlanningLockAsync(request.SpecialistId, async () =>
        {
            if (await _timeSlotRepository.IsTakenAsync(request.SpecialistId, request.Date, start, end))
            {
                return BookingResult.SlotNotAvailable;
            }

            var timeSlot = new TimeSlot
            {
                EmployeeId = request.SpecialistId,
                Date = request.Date.ToDateTime(TimeOnly.MinValue),
                StartTime = start,
                EndTime = end
            };
            timeSlot.Appointments.Add(new Appointment { PatientBSN = patient.BSN });

            await _timeSlotRepository.AddAsync(timeSlot);
            return BookingResult.Booked;
        });
    }

    // Same overlap rule as TimeSlotRepository.IsTakenAsync(). Touching is not overlapping: 9:15-10:00 and 10:00-10:45 are both allowed.
    private static bool Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB) =>
        startA < endB && startB < endA;

    private static bool TryGetTimes(BookAppointmentRequest request, out TimeSpan start, out TimeSpan end)
    {
        end = TimeSpan.Zero;

        if (!TimeSpan.TryParseExact(request.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out start) ||
            !IsBookableDay(request.Date) ||
            !IsValidDuration(request.DurationQuarters))
        {
            return false;
        }

        end = start + WorkingHours.Quarter * request.DurationQuarters;

        var startsOnQuarter = (start - WorkingHours.Start).Ticks % WorkingHours.Quarter.Ticks == 0;
        return startsOnQuarter && start >= WorkingHours.Start && end <= WorkingHours.End;
    }

    private static bool IsBookableDay(DateOnly date) =>
        WorkingHours.IsWorkingDay(date) && date >= DateOnly.FromDateTime(DateTime.Today);

    private static bool IsValidDuration(int durationQuarters) =>
        durationQuarters >= 1 && durationQuarters <= WorkingHours.QuartersPerDay;

    private static string Format(TimeSpan time) => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
}
