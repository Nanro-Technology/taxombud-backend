using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Taxpayers.Queries.VerifyNin;

// ─── Query ───────────────────────────────────────────────────────────────────

public record VerifyNinQuery(string Nin) : IRequest<Result<NinVerificationResponseDto>>;

public record NinVerificationResponseDto(
    bool Verified,
    string Nin,
    string FirstName,
    string LastName,
    string DateOfBirth,
    string Gender,
    string PhotoBase64
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class VerifyNinQueryHandler : IRequestHandler<VerifyNinQuery, Result<NinVerificationResponseDto>>
{
    public Task<Result<NinVerificationResponseDto>> Handle(VerifyNinQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nin) || request.Nin.Length != 11 || !request.Nin.All(char.IsDigit))
        {
            return Task.FromResult(Result<NinVerificationResponseDto>.Failure("Invalid NIN format. NIN must be exactly 11 digits."));
        }

        // Return simulated NIMC verified response payload
        var response = new NinVerificationResponseDto(
            Verified: true,
            Nin: request.Nin,
            FirstName: "Simulated",
            LastName: "NimcUser",
            DateOfBirth: "1990-01-01",
            Gender: "Male",
            PhotoBase64: "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        );

        return Task.FromResult(Result<NinVerificationResponseDto>.Success(response));
    }
}
