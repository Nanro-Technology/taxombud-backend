using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.GenerateInvoice;

public record GenerateInvoiceCommands : IRequest<Result<GenerateInvoiceResponse>>
{
}

public class GenerateInvoiceResponse
{
    public bool Success { get; set; }
}

public class GenerateInvoiceCommandsHandler : IRequestHandler<GenerateInvoiceCommands, Result<GenerateInvoiceResponse>>
{
    public async Task<Result<GenerateInvoiceResponse>> Handle(GenerateInvoiceCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<GenerateInvoiceResponse>.Success(new GenerateInvoiceResponse { Success = true });
    }
}