using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;

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

    public Task<List<string>> RegisterAsync(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}