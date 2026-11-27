namespace De_zachte_heelmeester.Models;

/// <summary>
/// A referral of the patient, so the patient can choose for which referral he makes an appointment.
/// </summary>
public record ReferralResponse(int ReferralId, string CareCode, string Description);
