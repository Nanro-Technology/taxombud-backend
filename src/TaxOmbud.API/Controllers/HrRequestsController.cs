using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.HrRequests.Queries.GetLeaveRequests;
using TaxOmbud.Application.Features.HrRequests.Queries.GetLoanRequests;
using TaxOmbud.Application.Features.HrRequests.Queries.GetEwaRequests;
using TaxOmbud.Application.Features.HrRequests.Commands.SubmitLeaveRequest;
using TaxOmbud.Application.Features.HrRequests.Commands.ApproveLeaveRequest;
using TaxOmbud.Application.Features.HrRequests.Commands.SubmitLoanRequest;

namespace TaxOmbud.Api.Controllers;

public class HrRequestsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public HrRequestsController(IMediator mediator) { _mediator = mediator; }
    [HttpGet("leaves")]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] GetLeaveRequestsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("loans")]
    public async Task<IActionResult> GetLoanRequests([FromQuery] GetLoanRequestsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("ewa")]
    public async Task<IActionResult> GetEwaRequests([FromQuery] GetEwaRequestsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost("leaves")]
    public async Task<IActionResult> SubmitLeaveRequest([FromBody] SubmitLeaveRequestCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("leaves/approve")]
    public async Task<IActionResult> ApproveLeaveRequest([FromBody] ApproveLeaveRequestCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("loans")]
    public async Task<IActionResult> SubmitLoanRequest([FromBody] SubmitLoanRequestCommands command) => ToActionResult(await _mediator.Send(command));
}

