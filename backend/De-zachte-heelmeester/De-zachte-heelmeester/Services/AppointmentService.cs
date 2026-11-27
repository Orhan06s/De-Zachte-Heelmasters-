using System.Globalization;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;

namespace De_zachte_heelmeester.Services;

/// <summary>
/// Finds the free moments for a referral and books appointments without double bookings.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IReferralRepository _referralRepository;
    private readonly TimeProvider _clock;

    public AppointmentService(
        IEmployeeRepository employeeRepository,
        IPatientRepository patientRepository,
        ITimeSlotRepository timeSlotRepository,
        IReferralRepository referralRepository,
        TimeProvider clock)
    {
        _employeeRepository = employeeRepository;
        _patientRepository = patientRepository;
        _timeSlotRepository = timeSlotRepository;
        _referralRepository = referralRepository;
        _clock = clock;
    }

    /// <summary>
    /// Returns the referrals of the logged-in patient, newest first. Empty when the patient has no referral.
    /// </summary>
    public async Task<List<ReferralResponse>> GetReferralsAsync(string userId)
    {
        var patient = await _patientRepository.GetByUserIdAsync(userId);
        if (patient == null)
        {
            return new List<ReferralResponse>();
        }

        var referrals = await _referralRepository.GetByPatientBsnAsync(patient.BSN);
        return referrals
            .Select(r => new ReferralResponse(r.ReferralId, r.CareCode, r.CareTreatment.Description))
            .ToList();
    }

    /// <summary>
    /// Returns the free moments for the referral of the logged-in patient, for seven days starting at the given date (today when empty).
    /// Only specialists with the specialism of the care code, only on the days they work,
    /// with the duration from the list of care treatments.
    /// </summary>
    public async Task<ReferralSlotsResult> GetAvailableSlotsForReferralAsync(string userId, int referralId, DateOnly? from)
    {
        var referral = await GetReferralOfPatientAsync(userId, referralId);
        if (referral == null)
        {
            return new ReferralSlotsResult(false, new List<ReferralSlotResponse>());
        }

        var treatment = referral.CareTreatment;
        var specialists = await _employeeRepository.GetBySpecializationAsync(treatment.Specialization);
        var slots = new List<ReferralSlotResponse>();
        var firstDay = from ?? DateOnly.FromDateTime(DutchNow());

        for (var date = firstDay; date < firstDay.AddDays(7); date = date.AddDays(1))
        {
            foreach (var specialist in specialists.Where(s => WorkingHours.IsWorkingDay(s.WorkDays, date)))
            {
                var freeTimes = await GetFreeTimesAsync(specialist.EmployeeId, date, treatment.DurationQuarters);
                slots.AddRange(freeTimes
                    .Where(t => !HasPassed(date, t.Start))
                    .Select(t => new ReferralSlotResponse(specialist.EmployeeId, specialist.Name, date, Format(t.Start), Format(t.End))));
            }
        }

        var sorted = slots.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ThenBy(s => s.SpecialistName).ToList();
        return new ReferralSlotsResult(true, sorted);
    }

    /// <summary>
    /// Books an appointment for the referral of the logged-in patient. The duration comes from the care code of the referral.
    /// Returns ReferralNotFound when the patient has no referral with this id,
    /// and SlotNotAvailable when the moment overlaps an appointment that is already booked.
    /// </summary>
    public async Task<BookingResult> BookAsync(string userId, BookAppointmentRequest request)
    {
        var referral = await GetReferralOfPatientAsync(userId, request.ReferralId);
        if (referral == null)
        {
            return BookingResult.ReferralNotFound;
        }

        var treatment = referral.CareTreatment;
        var specialists = await _employeeRepository.GetBySpecializationAsync(treatment.Specialization);
        var specialist = specialists.FirstOrDefault(s => s.EmployeeId == request.SpecialistId);
        if (specialist == null)
        {
            return BookingResult.SpecialistNotFound;
        }

        if (!TryGetTimes(request.StartTime, treatment.DurationQuarters, out var start, out var end) ||
            !WorkingHours.IsWorkingDay(specialist.WorkDays, request.Date) ||
            HasPassed(request.Date, start))
        {
            return BookingResult.InvalidRequest;
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
            timeSlot.Appointments.Add(new Appointment { PatientBSN = referral.PatientBSN, ReferralId = referral.ReferralId });

            await _timeSlotRepository.AddAsync(timeSlot);
            return BookingResult.Booked;
        });
    }

    /// <summary>
    /// Returns every start time on the day where an appointment of the given length fits
    /// within the working hours without overlapping an appointment that is already booked.
    /// </summary>
    private async Task<List<(TimeSpan Start, TimeSpan End)>> GetFreeTimesAsync(int specialistId, DateOnly date, int durationQuarters)
    {
        var freeTimes = new List<(TimeSpan Start, TimeSpan End)>();
        var duration = WorkingHours.Quarter * durationQuarters;
        var booked = await _timeSlotRepository.GetBookedAsync(specialistId, date);

        for (var start = WorkingHours.Start; start + duration <= WorkingHours.End; start += WorkingHours.Quarter)
        {
            var end = start + duration;
            if (!booked.Any(b => Overlaps(start, end, b.StartTime, b.EndTime)))
            {
                freeTimes.Add((start, end));
            }
        }

        return freeTimes;
    }

    // A referral only counts when it belongs to the logged-in patient. A referral of someone else is treated as no referral.
    private async Task<Referral?> GetReferralOfPatientAsync(string userId, int referralId)
    {
        var patient = await _patientRepository.GetByUserIdAsync(userId);
        if (patient == null)
        {
            return null;
        }

        var referral = await _referralRepository.GetByIdAsync(referralId);
        return referral != null && referral.PatientBSN == patient.BSN ? referral : null;
    }

    // Same overlap rule as TimeSlotRepository.IsTakenAsync(). Touching is not overlapping: 9:15-10:00 and 10:00-10:45 are both allowed.
    private static bool Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB) =>
        startA < endB && startB < endA;

    private static bool TryGetTimes(string startTime, int durationQuarters, out TimeSpan start, out TimeSpan end)
    {
        end = TimeSpan.Zero;

        if (!TimeSpan.TryParseExact(startTime, @"hh\:mm", CultureInfo.InvariantCulture, out start) ||
            !IsValidDuration(durationQuarters))
        {
            return false;
        }

        end = start + WorkingHours.Quarter * durationQuarters;

        var startsOnQuarter = (start - WorkingHours.Start).Ticks % WorkingHours.Quarter.Ticks == 0;
        return startsOnQuarter && start >= WorkingHours.Start && end <= WorkingHours.End;
    }

    // True when the moment lies before the current time. A moment that has started is no longer free.
    private bool HasPassed(DateOnly date, TimeSpan start) =>
        date.ToDateTime(TimeOnly.FromTimeSpan(start)) < DutchNow();

    // The current time in the Netherlands, whatever time zone the server uses.
    private DateTime DutchNow() =>
        TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), WorkingHours.TimeZone).DateTime;

    private static bool IsValidDuration(int durationQuarters) =>
        durationQuarters >= 1 && durationQuarters <= WorkingHours.QuartersPerDay;

    private static string Format(TimeSpan time) => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
}
