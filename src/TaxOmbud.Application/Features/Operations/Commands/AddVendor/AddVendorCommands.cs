using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Operations.Commands.AddVendor;

public record AddVendorCommands : IRequest<Result<AddVendorResponse>>
{
}

public class AddVendorResponse
{
    public bool Success { get; set; }
}

public class AddVendorCommandsHandler : IRequestHandler<AddVendorCommands, Result<AddVendorResponse>>
{
    public async Task<Result<AddVendorResponse>> Handle(AddVendorCommands request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<AddVendorResponse>.Success(new AddVendorResponse { Success = true });
    }
}