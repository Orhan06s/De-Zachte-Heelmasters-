using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface ITimeSlotRepository
{
    Task<List<TimeSlot>> GetBookedAsync(int specialistId, DateOnly date);
    Task<bool> IsTakenAsync(int specialistId, DateOnly date, TimeSpan start, TimeSpan end);
    Task AddAsync(TimeSlot timeSlot);
    Task<T> RunWithPlanningLockAsync<T>(int specialistId, Func<Task<T>> work);
}
