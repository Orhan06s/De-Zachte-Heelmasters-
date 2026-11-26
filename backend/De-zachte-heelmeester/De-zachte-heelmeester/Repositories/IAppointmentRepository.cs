using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment);
    Task<List<Appointment>> GetByPatientBsnAsync(string bsn);
    Task<List<Appointment>> GetByEmployeeIdAsync(int employeeId);
}