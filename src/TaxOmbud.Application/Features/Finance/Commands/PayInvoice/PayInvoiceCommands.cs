using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.PayInvoice;

public record PayInvoiceCommands : IRequest<Result<PayInvoiceResponse>>
{
}

public class PayInvoiceResponse
{
    public bool Success { get; set; }
}

public class PayInvoiceCommandsHandler : IRequestHandler<PayInvoiceCommands, Result<PayInvoiceResponse>>
{
    public async Task<Result<PayInvoiceResponse>> Handle(PayInvoiceCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<PayInvoiceResponse>.Success(new PayInvoiceResponse { Success = true });
    }
}