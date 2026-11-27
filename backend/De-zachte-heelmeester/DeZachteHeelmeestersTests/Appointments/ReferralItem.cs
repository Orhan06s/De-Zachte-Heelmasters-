namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// A referral as the API returns it.
/// </summary>
public sealed record ReferralItem(int ReferralId, string CareCode, string Description);
