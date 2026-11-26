using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;

namespace De_zachte_heelmeester.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> CreateAsync(string email, string password)
    {
        var user = new User { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return null;
        }

        return user.Id;
    }
}