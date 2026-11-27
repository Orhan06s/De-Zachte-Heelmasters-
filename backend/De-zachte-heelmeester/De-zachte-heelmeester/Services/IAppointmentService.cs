using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services;

public interface IAppointmentService
{
    Task<List<ReferralResponse>> GetReferralsAsync(string userId);
    Task<ReferralSlotsResult> GetAvailableSlotsForReferralAsync(string userId, int referralId, DateOnly? from);
    Task<BookingResult> BookAsync(string userId, BookAppointmentRequest request);
}
