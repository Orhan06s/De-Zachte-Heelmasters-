namespace De_zachte_heelmeester.Models;

/// <summary>
/// A free moment for a referral: which specialist, on which day and at what time. The times are written as "HH:mm".
/// </summary>
public record ReferralSlotResponse(int SpecialistId, string SpecialistName, DateOnly Date, string StartTime, string EndTime);
