using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;
using System;

namespace TaxOmbud.Application.Features.Cases.Commands.SubmitPublicCase;

public record SubmitPublicCaseCommand(
    string SubmitterType, // Personal or Corporate
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string CountryId,
    string StateId,
    string Description
) : IRequest<Result<SubmitPublicCaseResponse>>;

public record SubmitPublicCaseResponse(Guid CaseId, string TrackingNumber);

public class SubmitPublicCaseCommandHandler : IRequestHandler<SubmitPublicCaseCommand, Result<SubmitPublicCaseResponse>>
{
    public Task<Result<SubmitPublicCaseResponse>> Handle(SubmitPublicCaseCommand request, CancellationToken cancellationToken)
    {
        // Mocking the creation for now
        var response = new SubmitPublicCaseResponse(Guid.NewGuid(), "CAS-" + DateTime.UtcNow.Ticks.ToString().Substring(8));
        return Task.FromResult(Result<SubmitPublicCaseResponse>.Success(response));
    }
}
