using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetBySpecializationAsync(string specialization);
}
