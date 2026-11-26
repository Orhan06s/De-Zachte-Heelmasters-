namespace De_zachte_heelmeester.Models;

/// <summary>
/// A free moment that a patient can book. The times are written as "HH:mm".
/// </summary>
public record AvailableSlotResponse(string StartTime, string EndTime);
