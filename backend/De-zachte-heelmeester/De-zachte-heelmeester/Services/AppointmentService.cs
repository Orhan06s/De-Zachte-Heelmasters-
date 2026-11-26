using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;

namespace De_zachte_heelmeester.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IReferralRepository _referralRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ITimeSlotRepository timeSlotRepository,
        IReferralRepository referralRepository)
    {
        _appointmentRepository = appointmentRepository;
        _timeSlotRepository = timeSlotRepository;
        _referralRepository = referralRepository;
    }

    public Task<(List<TimeSlot> Slots, List<string> Errors)> GetAvailableTimeSlotsAsync(string patientBsn, int referralId)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> BookAppointmentAsync(string patientBsn, int referralId, int timeSlotId)
    {
        throw new NotImplementedException();
    }
}