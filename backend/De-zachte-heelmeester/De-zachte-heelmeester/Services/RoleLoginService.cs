using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity;

namespace De_zachte_heelmeester.Services
{
    public interface IUserRoleProvider
    {
        Task<IList<string>> GetRolesAsync(User user);
    }

    // Production implementation using ASP.NET Core Identity
    public class IdentityUserRoleProvider : IUserRoleProvider
    {
        private readonly UserManager<User> _userManager;

        public IdentityUserRoleProvider(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            return _userManager.GetRolesAsync(user);
        }
    }

    public class RoleLoginResult
    {
        public bool IsSuccess { get; set; }
        public string? ActiveRole { get; set; }
        public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();
        public bool RequiresRoleSelection { get; set; }
        public string? RedirectPath { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RoleLoginService
    {
        private readonly IUserRoleProvider _roleProvider;

        public RoleLoginService(IUserRoleProvider roleProvider)
        {
            _roleProvider = roleProvider;
        }

        // Determine login routing based on the roles assigned to the user
        public async Task<RoleLoginResult> ResolveLoginRolesAsync(User user)
        {
            var roles = await _roleProvider.GetRolesAsync(user);

            if (roles == null || roles.Count == 0)
            {
                return new RoleLoginResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Dit account heeft nog geen rol toegewezen gekregen."
                };
            }

            if (roles.Count > 1)
            {
                return new RoleLoginResult
                {
                    IsSuccess = true,
                    RequiresRoleSelection = true,
                    AvailableRoles = roles.ToList(),
                    RedirectPath = "/rol-kiezen"
                };
            }

            var singleRole = roles[0];
            return new RoleLoginResult
            {
                IsSuccess = true,
                RequiresRoleSelection = false,
                ActiveRole = singleRole,
                AvailableRoles = roles.ToList(),
                RedirectPath = GetStartPageForRole(singleRole)
            };
        }

        // Validate and apply the chosen role for a multi-role user
        public async Task<RoleLoginResult> SelectRoleAsync(User user, string chosenRole)
        {
            var roles = await _roleProvider.GetRolesAsync(user);

            if (!roles.Contains(chosenRole))
            {
                return new RoleLoginResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Je hebt geen toegang tot de gekozen rol."
                };
            }

            return new RoleLoginResult
            {
                IsSuccess = true,
                RequiresRoleSelection = false,
                ActiveRole = chosenRole,
                AvailableRoles = roles.ToList(),
                RedirectPath = GetStartPageForRole(chosenRole)
            };
        }

        private static string GetStartPageForRole(string role) => role switch
        {
            "Patient" => "/patient",
            "Specialist" => "/specialist",
            "Huisarts" => "/huisarts",
            "Medewerker" => "/medewerker",
            "Beheerder" => "/beheerder",
            _ => "/"
        };
    }
}