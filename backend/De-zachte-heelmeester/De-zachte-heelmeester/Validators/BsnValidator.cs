namespace De_zachte_heelmeester.Validators;

public static class BsnValidator
{
    public static bool IsValid(string bsn)
    {
        // heeft het 9 tekens
        if (bsn.Length != 9 || !bsn.All(char.IsDigit))
        {
            return false;
        }
        
        // elf proef
        var sum = 0;
        for (var i = 0; i < 8; i++)
        {
            sum += (bsn[i] - '0') * (9 - i);
        }
        sum -= bsn[8] - '0';

        return sum % 11 == 0;
    }
}