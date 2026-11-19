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

        return errors;
    }
}