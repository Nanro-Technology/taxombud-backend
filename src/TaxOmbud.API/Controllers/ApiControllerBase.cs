using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Converts a Result&lt;T&gt; to an appropriate HTTP response.
    /// </summary>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.IsNotFound)
            return NotFound(new { errors = result.Errors });

        if (result.IsForbidden)
            return StatusCode(403, new { errors = result.Errors });

        if (result.IsConflict)
            return Conflict(new { errors = result.Errors });

        if (result.IsValidationFailure)
            return UnprocessableEntity(new { errors = result.Errors });

        return BadRequest(new { errors = result.Errors });
    }

    /// <summary>
    /// Returns 201 Created with a Location header pointing to the new resource.
    /// </summary>
    protected IActionResult Created<T>(Result<T> result, string routeName, object routeValues)
    {
        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtRoute(routeName, routeValues, result.Value);
    }
}
