using TaxOmbud.Application.IdentityVerification.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IIdentityVerificationService
{
    Task<Response<IdentityVerificationResponse>> VerifyIdentityAsync(VerifyIdentityCommand request, CancellationToken cancellationToken = default);
}
