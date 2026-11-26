namespace De_zachte_heelmeester.Repositories;

public interface IEmployeeRepository
{
    Task<bool> ExistsAsync(int employeeId);
}
