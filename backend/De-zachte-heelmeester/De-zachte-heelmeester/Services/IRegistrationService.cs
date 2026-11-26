using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

public interface IRegistrationService
{
    Task<List<string>> RegisterAsync(RegisterRequest request);
}