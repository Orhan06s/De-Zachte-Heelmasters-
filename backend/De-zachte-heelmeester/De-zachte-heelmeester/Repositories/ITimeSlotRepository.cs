using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(int timeSlotId);
    Task<List<TimeSlot>> GetAvailableSlotsBySpecializationAsync(string specialization);
}