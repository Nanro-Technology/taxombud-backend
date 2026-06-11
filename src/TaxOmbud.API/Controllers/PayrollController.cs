using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Payroll.Queries.GetPayrollPeriods;
using TaxOmbud.Application.Features.Payroll.Queries.GetSalaryProfiles;
using TaxOmbud.Application.Features.Payroll.Queries.GetRemittances;
using TaxOmbud.Application.Features.Payroll.Commands.RunPayroll;
using TaxOmbud.Application.Features.Payroll.Commands.ApprovePayroll;
using TaxOmbud.Application.Features.Payroll.Commands.CreateSalaryProfile;

namespace TaxOmbud.Api.Controllers;

public class PayrollController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public PayrollController(IMediator mediator) { _mediator = mediator; }
    [HttpGet("periods")]
    public async Task<IActionResult> GetPayrollPeriods([FromQuery] GetPayrollPeriodsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("salary-profiles")]
    public async Task<IActionResult> GetSalaryProfiles([FromQuery] GetSalaryProfilesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet("remittances")]
    public async Task<IActionResult> GetRemittances([FromQuery] GetRemittancesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost("run")]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("approve")]
    public async Task<IActionResult> ApprovePayroll([FromBody] ApprovePayrollCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost("salary-profiles")]
    public async Task<IActionResult> CreateSalaryProfile([FromBody] CreateSalaryProfileCommands command) => ToActionResult(await _mediator.Send(command));
}

