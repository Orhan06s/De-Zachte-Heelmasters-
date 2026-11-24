using System.ComponentModel.DataAnnotations;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Validators;

namespace De_zachte_heelmeester.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationService(IPatientRepository patientRepository, IUserRepository userRepository)
    {
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    public async Task<List<string>> RegisterAsync(RegisterRequest request)
    {
        // 1. BSN
        if (!BsnValidator.IsValid(request.Bsn))
        {
            return new List<string> { "Vul een geldig BSN in." };
        }
        // 2. E-mailadres
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
        {
            return new List<string> { "Vul een geldig e-mailadres in." };
        }

        // 3. Wachtwoord
        var errors = PasswordPolicy.Validate(request.Password);
        if (errors.Count > 0)
        {
            return errors;
        }
        
        // 4. Bestaat er al een account?
        var patient = await _patientRepository.GetByBsnAsync(request.Bsn);
        if (patient != null && patient.UserId != null)
        {
            return new List<string> { "Er bestaat al een account met dit BSN." };
        }

        // 5. Account aanmaken
        var userId = await _userRepository.CreateAsync(request.Email, request.Password);

        // 6. Is het account aangemaakt
        if (userId == null)
        {
            return new List<string> { "Er is al een account met dit e-mailadres." };
        }
        
        if (patient == null)
        {
            // AC1: nieuwe patiënt
            await _patientRepository.AddAsync(new Patient { BSN = request.Bsn, Email = request.Email, UserId = userId });
        }
        else
        {
            // AC2: bestaande patiënt koppelen
            patient.UserId = userId;
            await _patientRepository.UpdateAsync(patient);
        }

        return new List<string>();
    }
}