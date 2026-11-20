namespace De_zachte_heelmeester.Repositories;

public interface IUserRepository
{
    Task<string?> CreateAsync(string email, string password);
}