using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IUserRepository
{
    Task<string?> CreateAsync(string email, string password);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string userId);
    Task<IList<string>> GetRolesAsync(string userId);
    Task<bool> AddToRoleAsync(string userId, string role);
    Task<bool> RemoveFromRolesAsync(string userId, IEnumerable<string> roles);
}