using System.Security.Claims;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace De_zachte_heelmeester.Controllers;

/// <summary>
/// Endpoints for a patient to choose a referral, find free moments for it and book an appointment.
/// </summary>
[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Patient")]
public class AppointmentsController : ControllerBase
{
    // Texts shown to the patient
    private const string SlotNotAvailableMessage = "Dit moment is niet meer vrij.";
    private const string ReferralNeededMessage = "Je hebt eerst een doorverwijzing van je huisarts nodig.";
    private const string InvalidRequestMessage = "Kies een vrij moment op een werkdag van de specialist.";
    private const string SpecialistNotFoundMessage = "Deze specialist voert deze behandeling niet uit.";

    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Returns the referrals of the logged-in patient. An empty list means the patient first needs a referral.
    /// </summary>
    /// <response code="200">List of referrals, newest first. Can be empty.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user does not have the role Patient.</response>
    [HttpGet("referrals")]
    [ProducesResponseType(typeof(List<ReferralResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReferrals()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _appointmentService.GetReferralsAsync(userId));
    }

    /// <summary>
    /// Returns the free moments for a referral of the logged-in patient, for seven days starting at the given date.
    /// </summary>
    /// <response code="200">List of free moments. Empty when no specialist has a free moment in these seven days.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user does not have the role Patient.</response>
    /// <response code="404">The patient has no referral with this id.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ReferralSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailable([FromQuery] int referralId, [FromQuery] DateOnly? from)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.GetAvailableSlotsForReferralAsync(userId, referralId, from);
        if (!result.ReferralFound)
        {
            return NotFound(new { message = ReferralNeededMessage });
        }

        return Ok(result.Slots);
    }

    /// <summary>
    /// Books an appointment for a referral of the logged-in patient. The duration comes from the care code of the referral.
    /// </summary>
    /// <response code="201">The appointment is saved.</response>
    /// <response code="400">The moment is not a free moment on a working day of the specialist.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user is not a patient.</response>
    /// <response code="404">The patient has no referral with this id, or the specialist does not do this treatment.</response>
    /// <response code="409">The moment is already booked. Nothing is saved.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.BookAsync(userId, request);

        return result switch
        {
            BookingResult.Booked => StatusCode(StatusCodes.Status201Created),
            BookingResult.InvalidRequest => BadRequest(new { message = InvalidRequestMessage }),
            BookingResult.ReferralNotFound => NotFound(new { message = ReferralNeededMessage }),
            BookingResult.SpecialistNotFound => NotFound(new { message = SpecialistNotFoundMessage }),
            BookingResult.SlotNotAvailable => Conflict(new { message = SlotNotAvailableMessage }),
            _ => throw new InvalidOperationException($"Unknown booking result: {result}")
        };
    }
}
