using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

/// <summary>
/// The working hours of a specialist: from 9:00 to 17:00 on the days the specialist works.
/// </summary>
public static class WorkingHours
{
    public static readonly TimeSpan Start = new(9, 0, 0);

    public static readonly TimeSpan End = new(17, 0, 0);

    public static readonly TimeSpan Quarter = TimeSpan.FromMinutes(15);

    // The hospital works on Dutch time, also when the server runs on another time zone (for example UTC in Docker).
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

    public static int QuartersPerDay => (int)((End - Start) / Quarter);

    public static bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

    /// <summary>
    /// True when the date falls on one of the work days of the specialist.
    /// </summary>
    public static bool IsWorkingDay(WorkDays workDays, DateOnly date) =>
        workDays.HasFlag(ToWorkDay(date.DayOfWeek));

    private static WorkDays ToWorkDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => WorkDays.Monday,
        DayOfWeek.Tuesday => WorkDays.Tuesday,
        DayOfWeek.Wednesday => WorkDays.Wednesday,
        DayOfWeek.Thursday => WorkDays.Thursday,
        DayOfWeek.Friday => WorkDays.Friday,
        DayOfWeek.Saturday => WorkDays.Saturday,
        _ => WorkDays.Sunday
    };
}
