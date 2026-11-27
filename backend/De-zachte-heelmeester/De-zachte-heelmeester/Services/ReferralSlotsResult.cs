using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

/// <summary>
/// The free moments for a referral. ReferralFound is false when the patient has no referral with this id,
/// then Slots is empty and the controller shows that a referral from the general practitioner is needed.
/// </summary>
public record ReferralSlotsResult(bool ReferralFound, List<ReferralSlotResponse> Slots);
