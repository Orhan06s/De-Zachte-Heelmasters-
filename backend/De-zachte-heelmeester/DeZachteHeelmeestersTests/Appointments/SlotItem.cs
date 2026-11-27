namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// A free moment as the API returns it.
/// </summary>
public sealed record SlotItem(int SpecialistId, string SpecialistName, DateOnly Date, string StartTime, string EndTime);
