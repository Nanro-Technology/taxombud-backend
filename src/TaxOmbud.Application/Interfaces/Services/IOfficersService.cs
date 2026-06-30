using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Officers.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;

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
