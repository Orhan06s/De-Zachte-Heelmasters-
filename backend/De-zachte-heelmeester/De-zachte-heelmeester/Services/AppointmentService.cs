using System.Globalization;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Services;

/// <summary>
/// Finds the available slots of a specialist and books appointments without double bookings.
/// </summary>
public class AppointmentService
{
    private readonly ApplicationDbContext _db;

    public AppointmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> SpecialistExistsAsync(int specialistId) =>
        _db.Employees.AnyAsync(e => e.EmployeeId == specialistId);

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
        var booked = await BookedTimeSlots(specialistId, date).ToListAsync();

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

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
        {
            return BookingResult.PatientNotFound;
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Two patients can confirm the same moment at the same time. Without this lock both would
        // see the moment as free and both would save it. The lock lets only one booking at a time
        // check and save the planning of this specialist, the other one waits until the transaction ends.
        await LockPlanningAsync(request.SpecialistId);

        // Same overlap rule as Overlaps(), written out here so the database can run it.
        var isTaken = await BookedTimeSlots(request.SpecialistId, request.Date)
            .AnyAsync(t => t.StartTime < end && start < t.EndTime);
        if (isTaken)
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

        _db.TimeSlots.Add(timeSlot);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return BookingResult.Booked;
    }

    // Touching is not overlapping: 9:15-10:00 and 10:00-10:45 are both allowed.
    private static bool Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB) =>
        startA < endB && startB < endA;

    private IQueryable<TimeSlot> BookedTimeSlots(int specialistId, DateOnly date)
    {
        var day = date.ToDateTime(TimeOnly.MinValue);

        return _db.TimeSlots.Where(t =>
            t.EmployeeId == specialistId &&
            t.Date == day &&
            t.Appointments.Any(a => a.Status != AppointmentStatus.Cancelled));
    }

    private Task LockPlanningAsync(int specialistId)
    {
        var resource = $"planning-specialist-{specialistId}";

        return _db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @result < 0 THROW 50000, 'Could not lock the planning of the specialist.', 1;
            """);
    }

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
