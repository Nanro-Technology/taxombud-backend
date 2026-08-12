using TaxOmbud.Application.IdentityVerification.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Services;

public class IdentityVerificationService : IIdentityVerificationService
{
    public IdentityVerificationService()
    {
    }

    public async Task<Response<IdentityVerificationResponse>> VerifyIdentityAsync(VerifyIdentityCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IdentityVerificationResponse>();
        try
        {
            var isVerified = !string.IsNullOrWhiteSpace(request.IdNumber);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Identity verification completed.";
            response.Data = new IdentityVerificationResponse(isVerified, request.IdNumber, request.IdType, "John Doe", "1990-01-01", null);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = TaxOmbud.Common.Utilities.Constants.Messages.ServerError;
        }
        return response;
    }
}
