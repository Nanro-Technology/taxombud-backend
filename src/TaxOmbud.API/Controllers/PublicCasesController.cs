using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Cases.Commands.SubmitPublicCase;
using TaxOmbud.Application.Features.Cases.Queries.TrackComplaint;

namespace TaxOmbud.Api.Controllers;

[AllowAnonymous]
[Route("api/public")]
public class PublicCasesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicCasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("case")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitCase([FromBody] SubmitPublicCaseCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    [HttpPost("track_complaints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrackComplaint([FromForm] string trackingNumber, CancellationToken ct)
    {
        // Track complaints form likely submits via Form URL Encoded given it's an HTML form action
        var result = await _mediator.Send(new TrackComplaintQuery(trackingNumber), ct);
        return ToActionResult(result);
    }
}
