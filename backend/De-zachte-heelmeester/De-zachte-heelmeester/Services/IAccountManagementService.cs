using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

public interface IAccountManagementService
{
    Task<List<string>> CreateEmployeeAccountAsync(CreateAccountRequest request);
    Task<List<string>> ChangeUserRoleAsync(string userId, string newRole, List<string>? specializations = null);
}