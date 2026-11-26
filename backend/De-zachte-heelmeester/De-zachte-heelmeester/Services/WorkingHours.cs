namespace De_zachte_heelmeester.Services;

/// <summary>
/// The working hours of a specialist. For now the same for everyone: Monday to Friday from 9:00 to 17:00.
/// When the working days per specialist are in the data model, only this class has to change.
/// </summary>
public static class WorkingHours
{
    public static readonly TimeSpan Start = new(9, 0, 0);

    public static readonly TimeSpan End = new(17, 0, 0);

    public static readonly TimeSpan Quarter = TimeSpan.FromMinutes(15);

    public static int QuartersPerDay => (int)((End - Start) / Quarter);

    public static bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
}
