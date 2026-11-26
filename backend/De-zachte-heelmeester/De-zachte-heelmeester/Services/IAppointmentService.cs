using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

public interface IAppointmentService
{
    Task<bool> SpecialistExistsAsync(int specialistId);
    Task<List<AvailableSlotResponse>> GetAvailableSlotsAsync(int specialistId, DateOnly date, int durationQuarters);
    Task<BookingResult> BookAsync(string userId, BookAppointmentRequest request);
}
