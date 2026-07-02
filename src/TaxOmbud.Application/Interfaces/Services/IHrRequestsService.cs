using TaxOmbud.Application.HrRequests.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IHrRequestsService
{
    Task<Response<bool>> ApproveLeaveRequestAsync(ApproveLeaveRequestCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> SubmitLeaveRequestAsync(SubmitLeaveRequestCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> SubmitLoanRequestAsync(SubmitLoanRequestCommands request, CancellationToken cancellationToken = default);
    Task<Response<List<EwaRequest>>> GetEwaRequestsAsync(GetEwaRequestsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<LeaveRequest>>> GetLeaveRequestsAsync(GetLeaveRequestsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<LoanRequest>>> GetLoanRequestsAsync(GetLoanRequestsQueries request, CancellationToken cancellationToken = default);
}
