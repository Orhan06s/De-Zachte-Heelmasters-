using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByBsnAsync(string bsn);
    Task<Patient?> GetByUserIdAsync(string userId);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
}