using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.IdentityVerification.Commands.VerifyIdentity;

public record VerifyIdentityCommand(
    string IdNumber,
    string IdType // NIN, BVN, Passport, etc.
) : IRequest<Result<IdentityVerificationResponse>>;

public record IdentityVerificationResponse(bool IsVerified, string FullName, string MetaData);

public class VerifyIdentityCommandHandler : IRequestHandler<VerifyIdentityCommand, Result<IdentityVerificationResponse>>
{
    public Task<Result<IdentityVerificationResponse>> Handle(VerifyIdentityCommand request, CancellationToken cancellationToken)
    {
        // Mock identity verification
        var isVerified = !string.IsNullOrWhiteSpace(request.IdNumber);
        var response = new IdentityVerificationResponse(isVerified, "John Doe", "Verified securely.");
        
        return Task.FromResult(Result<IdentityVerificationResponse>.Success(response));
    }
}
