using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Services;
using Microsoft.AspNetCore.Mvc;

namespace De_zachte_heelmeester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    /// <summary>
    /// Registreert een patiënt met BSN, e-mailadres en wachtwoord.
    /// </summary>
    /// <response code="201">Het account is aangemaakt.</response>
    /// <response code="400">Het BSN of wachtwoord is ongeldig, of er bestaat al een account.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var errors = await _registrationService.RegisterAsync(request);

        if (errors.Count > 0)
        {
            return BadRequest(new { message = errors[0], errors });
        }

        return StatusCode(StatusCodes.Status201Created, new { message = "Je account is aangemaakt." });
    }
}