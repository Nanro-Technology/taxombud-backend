using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Features.Contact.Commands.SubmitContactForm;

namespace TaxOmbud.Api.Controllers;

[AllowAnonymous]
[Route("api/public")]
public class PublicContactController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitContact([FromForm] SubmitContactFormCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }
}
