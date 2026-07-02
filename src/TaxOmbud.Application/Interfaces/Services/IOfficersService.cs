using TaxOmbud.Application.Officers.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IOfficersService
{
    Task<Response<CreatedOfficerResponse>> CreateOfficerProfileAsync(CreateOfficerProfileCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateOfficerProfileAsync(UpdateOfficerProfileCommand request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<OfficerListDto>>> GetAvailableOfficersAsync(GetAvailableOfficersQuery request, CancellationToken cancellationToken = default);
    Task<Response<OfficerDetailDto>> GetOfficerByIdAsync(GetOfficerByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<OfficerCaseloadsDto>> GetOfficerCaseloadsAsync(GetOfficerCaseloadsQuery request, CancellationToken cancellationToken = default);
    Task<Response<OfficerPerformanceDto>> GetOfficerPerformanceAsync(GetOfficerPerformanceQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<OfficerListDto>>> GetOfficersAsync(GetOfficersQuery request, CancellationToken cancellationToken = default);
}
