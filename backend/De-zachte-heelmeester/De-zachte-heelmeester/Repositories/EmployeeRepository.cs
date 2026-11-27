using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetBySpecializationAsync(string specialization)
    {
        return await _context.Employees
            .Where(e => e.Specialization == specialization)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
}
