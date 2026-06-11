using System;
using System.IO;

class Program
{
    static void Main()
    {
        string controllersDir = @"c:\Projects\taxombud\src\TaxOmbud.API\Controllers";
        
        string projects = $@"using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Operations.Queries.GetProjects;
using TaxOmbud.Application.Features.Operations.Commands.CreateProject;
using TaxOmbud.Application.Features.Operations.Commands.UpdateProjectStatus;

namespace TaxOmbud.Api.Controllers;

public class ProjectsController : ApiControllerBase
{{
    private readonly IMediator _mediator;
    public ProjectsController(IMediator mediator) {{ _mediator = mediator; }}

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] GetProjectsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPatch(""{{id}}/status"")]
    public async Task<IActionResult> UpdateProjectStatus([FromBody] UpdateProjectStatusCommands command) => ToActionResult(await _mediator.Send(command));
}}";

        string inventory = $@"using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Operations.Queries.GetInventoryItems;
using TaxOmbud.Application.Features.Operations.Queries.GetVendors;
using TaxOmbud.Application.Features.Operations.Commands.AddInventoryItem;
using TaxOmbud.Application.Features.Operations.Commands.AddVendor;

namespace TaxOmbud.Api.Controllers;

public class InventoryController : ApiControllerBase
{{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) {{ _mediator = mediator; }}

    [HttpGet(""items"")]
    public async Task<IActionResult> GetInventoryItems([FromQuery] GetInventoryItemsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet(""vendors"")]
    public async Task<IActionResult> GetVendors([FromQuery] GetVendorsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost(""items"")]
    public async Task<IActionResult> AddInventoryItem([FromBody] AddInventoryItemCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost(""vendors"")]
    public async Task<IActionResult> AddVendor([FromBody] AddVendorCommands command) => ToActionResult(await _mediator.Send(command));
}}";

        string finance = $@"using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaxOmbud.Application.Features.Finance.Queries.GetQuotes;
using TaxOmbud.Application.Features.Finance.Queries.GetContracts;
using TaxOmbud.Application.Features.Finance.Queries.GetInvoices;
using TaxOmbud.Application.Features.Finance.Commands.CreateQuote;
using TaxOmbud.Application.Features.Finance.Commands.CreateContract;
using TaxOmbud.Application.Features.Finance.Commands.GenerateInvoice;
using TaxOmbud.Application.Features.Finance.Commands.PayInvoice;

namespace TaxOmbud.Api.Controllers;

public class FinanceController : ApiControllerBase
{{
    private readonly IMediator _mediator;
    public FinanceController(IMediator mediator) {{ _mediator = mediator; }}

    [HttpGet(""quotes"")]
    public async Task<IActionResult> GetQuotes([FromQuery] GetQuotesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet(""contracts"")]
    public async Task<IActionResult> GetContracts([FromQuery] GetContractsQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpGet(""invoices"")]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesQueries query) => ToActionResult(await _mediator.Send(query));

    [HttpPost(""quotes"")]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost(""contracts"")]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost(""invoices"")]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceCommands command) => ToActionResult(await _mediator.Send(command));

    [HttpPost(""invoices/pay"")]
    public async Task<IActionResult> PayInvoice([FromBody] PayInvoiceCommands command) => ToActionResult(await _mediator.Send(command));
}}";
        File.WriteAllText(Path.Combine(controllersDir, "ProjectsController.cs"), projects);
        File.WriteAllText(Path.Combine(controllersDir, "InventoryController.cs"), inventory);
        File.WriteAllText(Path.Combine(controllersDir, "FinanceController.cs"), finance);
    }
}
