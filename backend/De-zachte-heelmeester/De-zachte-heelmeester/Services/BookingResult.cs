namespace De_zachte_heelmeester.Services;

/// <summary>
/// The outcome of a booking. The controller turns it into a status code and a message.
/// </summary>
public enum BookingResult
{
    Booked,
    InvalidRequest,
    SpecialistNotFound,
    SlotNotAvailable,
    ReferralNotFound
}
