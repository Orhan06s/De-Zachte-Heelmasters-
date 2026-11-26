using De_zachte_heelmeester.Data;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int employeeId)
    {
        return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
    }
}
