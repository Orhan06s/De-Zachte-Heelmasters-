using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

public interface IAppointmentService
{
    Task<(List<TimeSlot> Slots, List<string> Errors)> GetAvailableTimeSlotsAsync(string patientBsn, int referralId);
    Task<List<string>> BookAppointmentAsync(string patientBsn, int referralId, int timeSlotId);
}