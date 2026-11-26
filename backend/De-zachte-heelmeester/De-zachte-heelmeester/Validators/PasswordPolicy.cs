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

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errors.Add("Gebruik minimaal één speciaal teken.");
        }
        
        if (!password.Any(char.IsLower))
        {
            errors.Add("Gebruik minimaal één kleine letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Gebruik minimaal één cijfer.");
        }

        for (var i = 2; i < password.Length; i++)
        {
            if (password[i] == password[i - 1] && password[i] == password[i - 2])
            {
                errors.Add("Gebruik niet meer dan twee dezelfde tekens achter elkaar.");
                break;
            }
        }

        return errors;
    }
}