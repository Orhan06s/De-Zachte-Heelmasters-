using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IEmployeeRepository
{
    Task AddAsync(Employee employee);
    Task<Employee?> GetByUserIdAsync(string userId);
    Task UpdateAsync(Employee employee);
}