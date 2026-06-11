using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.CreateQuote;

public record CreateQuoteCommands : IRequest<Result<CreateQuoteResponse>>
{
}

public class CreateQuoteResponse
{
    public bool Success { get; set; }
}

public class CreateQuoteCommandsHandler : IRequestHandler<CreateQuoteCommands, Result<CreateQuoteResponse>>
{
    public async Task<Result<CreateQuoteResponse>> Handle(CreateQuoteCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<CreateQuoteResponse>.Success(new CreateQuoteResponse { Success = true });
    }
}