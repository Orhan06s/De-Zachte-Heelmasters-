namespace De_zachte_heelmeester.Validators;

public static class PasswordPolicy
{
    
    public static List<string> Validate(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
        {
            errors.Add("Gebruik minimaal 8 tekens.");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Gebruik minimaal één hoofdletter.");
        }

        return errors;
    }
}