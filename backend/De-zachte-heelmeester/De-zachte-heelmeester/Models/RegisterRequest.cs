namespace De_zachte_heelmeester.Models;

public class RegisterRequest
{
    public string Bsn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}