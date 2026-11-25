using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace De_zachte_heelmeester.Controllers;

/// <summary>
/// Endpoints that only a user with the role Huisarts may use.
/// </summary>
[ApiController]
[Route("api/general-practitioner")]
[Authorize(Roles = "Huisarts")]
public class GeneralPractitionerController : ControllerBase
{
    /// <summary>
    /// Start page of the huisarts. The real content follows with the referral user stories.
    /// </summary>
    /// <response code="200">The user has the role Huisarts.</response>
    /// <response code="401">The user is not logged in.</response>
    /// <response code="403">The user is logged in but does not have the role Huisarts.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get()
    {
        return Ok(new { message = "Startpagina huisarts" });
    }
}
