namespace De_zachte_heelmeester.Models;

/// <summary>
/// The days of the week a specialist works. Each day is one bit, so several days fit in one number:
/// Monday to Friday is 1 + 2 + 4 + 8 + 16 = 31.
/// </summary>
[Flags]
public enum WorkDays
{
    None = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
    MondayToFriday = Monday | Tuesday | Wednesday | Thursday | Friday
}
