using System.ComponentModel.DataAnnotations;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Validators;

namespace De_zachte_heelmeester.Services;

public class AccountManagementService : IAccountManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public AccountManagementService(IUserRepository userRepository, IEmployeeRepository employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<List<string>> CreateEmployeeAccountAsync(CreateAccountRequest request)
    {
        // Validatie e-mailadres en wachtwoord
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
        {
            return new List<string> { "Vul een geldig e-mailadres in." };
        }

        var passwordErrors = PasswordPolicy.Validate(request.Password);
        if (passwordErrors.Count > 0)
        {
            return passwordErrors;
        }

        // AC4: E-mailadres bestaat al
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new List<string> { "Er is al een account met dit e-mailadres." };
        }

        // Account aanmaken
        var userId = await _userRepository.CreateAsync(request.Email, request.Password);
        if (userId == null)
        {
            return new List<string> { "Account aanmaken is mislukt." };
        }

        // AC1 & AC2: Rol en eventuele specialismen koppelen
        await _userRepository.AddToRoleAsync(userId, request.Role);

        var specializationText = request.Specializations.Count > 0
            ? string.Join(", ", request.Specializations)
            : request.Role;

        await _employeeRepository.AddAsync(new Employee
        {
            UserId = userId,
            Name = request.Name,
            Specialization = specializationText
        });

        return new List<string>();
    }

    public async Task<List<string>> ChangeUserRoleAsync(string userId, string newRole, List<string>? specializations = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new List<string> { "Gebruiker niet gevonden." };
        }

        var currentRoles = await _userRepository.GetRolesAsync(userId);

        // AC5: Bestaande patiënt krijgt ook rol Specialist -> patiëntrol blijft behouden!
        if (currentRoles.Contains("Patient") && newRole == "Specialist")
        {
            await _userRepository.AddToRoleAsync(userId, "Specialist");
        }
        else
        {
            // AC3: Oude rol(len) verwijderen en nieuwe rol instellen
            if (currentRoles.Count > 0)
            {
                await _userRepository.RemoveFromRolesAsync(userId, currentRoles);
            }
            await _userRepository.AddToRoleAsync(userId, newRole);
        }

        // Employee-record bijwerken of aanmaken
        var employee = await _employeeRepository.GetByUserIdAsync(userId);
        var specText = (specializations != null && specializations.Count > 0)
            ? string.Join(", ", specializations)
            : newRole;

        if (employee == null)
        {
            await _employeeRepository.AddAsync(new Employee
            {
                UserId = userId,
                Name = user.Email ?? string.Empty,
                Specialization = specText
            });
        }
        else
        {
            employee.Specialization = specText;
            await _employeeRepository.UpdateAsync(employee);
        }

        return new List<string>();
    }
}