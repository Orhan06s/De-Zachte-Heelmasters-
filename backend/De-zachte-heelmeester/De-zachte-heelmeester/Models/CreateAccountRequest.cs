namespace De_zachte_heelmeester.Models;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // bijv. "Huisarts", "Specialist", "AdministratiefMedewerker"
    public List<string> Specializations { get; set; } = new();
}