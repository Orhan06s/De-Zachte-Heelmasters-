using System.Security.Claims;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace De_zachte_heelmeester.Controllers;

/// <summary>
/// Endpoints for a patient to find available slots and book an appointment.
/// </summary>
[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Patient")]
public class AppointmentsController : ControllerBase
{
    // Texts shown to the patient
    private const string SlotNotAvailableMessage = "Dit moment is niet meer vrij.";
    private const string InvalidRequestMessage = "Kies een moment op een werkdag tussen 9:00 en 17:00.";
    private const string SpecialistNotFoundMessage = "Deze specialist bestaat niet.";
    private const string PatientNotFoundMessage = "Alleen een patiënt kan een afspraak maken.";

    private readonly AppointmentService _appointmentService;

    public AppointmentsController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Returns the available slots of a specialist on a day for an appointment of the given length.
    /// </summary>
    /// <response code="200">List of available slots. Empty on a weekend or a day in the past.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user does not have the role Patient.</response>
    /// <response code="404">The specialist does not exist.</response>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<AvailableSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailable([FromQuery] int specialistId, [FromQuery] DateOnly date, [FromQuery] int durationQuarters)
    {
        if (!await _appointmentService.SpecialistExistsAsync(specialistId))
        {
            return NotFound(new { message = SpecialistNotFoundMessage });
        }

        return Ok(await _appointmentService.GetAvailableSlotsAsync(specialistId, date, durationQuarters));
    }

    /// <summary>
    /// Books an appointment for the logged-in patient.
    /// </summary>
    /// <response code="201">The appointment is saved.</response>
    /// <response code="400">The moment is not on a working day between 9:00 and 17:00.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user is not a patient.</response>
    /// <response code="404">The specialist does not exist.</response>
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
            BookingResult.SpecialistNotFound => NotFound(new { message = SpecialistNotFoundMessage }),
            BookingResult.PatientNotFound => StatusCode(StatusCodes.Status403Forbidden, new { message = PatientNotFoundMessage }),
            BookingResult.SlotNotAvailable => Conflict(new { message = SlotNotAvailableMessage }),
            _ => throw new InvalidOperationException($"Unknown booking result: {result}")
        };
    }
}
