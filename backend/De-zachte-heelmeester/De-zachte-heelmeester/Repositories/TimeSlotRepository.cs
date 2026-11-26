using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly ApplicationDbContext _context;

    public TimeSlotRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeSlot>> GetBookedAsync(int specialistId, DateOnly date)
    {
        return await BookedTimeSlots(specialistId, date).ToListAsync();
    }

    public async Task<bool> IsTakenAsync(int specialistId, DateOnly date, TimeSpan start, TimeSpan end)
    {
        // Same overlap rule as AppointmentService.Overlaps(), written out here so the database can run it.
        return await BookedTimeSlots(specialistId, date)
            .AnyAsync(t => t.StartTime < end && start < t.EndTime);
    }

    public async Task AddAsync(TimeSlot timeSlot)
    {
        _context.TimeSlots.Add(timeSlot);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Runs the work inside a transaction that holds a lock on the planning of one specialist.
    /// Two patients can confirm the same moment at the same time. Without this lock both would
    /// see the moment as free and both would save it. The lock lets only one booking at a time
    /// check and save the planning of this specialist, the other one waits until the transaction ends.
    /// </summary>
    public async Task<T> RunWithPlanningLockAsync<T>(int specialistId, Func<Task<T>> work)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var resource = $"planning-specialist-{specialistId}";
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @result < 0 THROW 50000, 'Could not lock the planning of the specialist.', 1;
            """);

        var result = await work();

        // Ending the transaction also releases the lock.
        await transaction.CommitAsync();
        return result;
    }

    private IQueryable<TimeSlot> BookedTimeSlots(int specialistId, DateOnly date)
    {
        var day = date.ToDateTime(TimeOnly.MinValue);

        return _context.TimeSlots.Where(t =>
            t.EmployeeId == specialistId &&
            t.Date == day &&
            t.Appointments.Any(a => a.Status != AppointmentStatus.Cancelled));
    }
}
