using TaxOmbud.Application.Hr.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IHrService
{
    Task<Response<object?>> ApproveLeaveAsync(ApproveLeaveCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> ApproveLoanAsync(ApproveLoanCommand request, CancellationToken cancellationToken = default);
    Task<Response<PayrollRun>> CreatePayrollRunAsync(CreatePayrollRunCommand request, CancellationToken cancellationToken = default);
    Task<Response<LeaveRequest>> RequestLeaveAsync(RequestLeaveCommand request, CancellationToken cancellationToken = default);
    Task<Response<LoanRequest>> RequestLoanAsync(RequestLoanCommand request, CancellationToken cancellationToken = default);
    Task<Response<StaffProfile>> SaveStaffProfileAsync(SaveStaffProfileCommand request, CancellationToken cancellationToken = default);
    Task<Response<EwaWithdrawalResponse>> WithdrawEwaAsync(WithdrawEwaCommand request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<LeaveRequestDto>>> GetLeaveRequestsAsync(GetLeaveRequestsQuery request, CancellationToken cancellationToken = default);
    Task<Response<IEnumerable<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQuery request, CancellationToken cancellationToken = default);
    Task<Response<PagedResult<StaffListDto>>> GetStaffAsync(GetStaffQuery request, CancellationToken cancellationToken = default);
    Task<Response<StaffDetailDto>> GetStaffByIdAsync(GetStaffByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<WalletDto>> GetWalletAsync(GetWalletQuery request, CancellationToken cancellationToken = default);

    // Performance Settings
    Task<Response<IEnumerable<CompetencyDto>>> GetCompetenciesAsync(GetCompetenciesQuery request, CancellationToken cancellationToken = default);
    Task<Response<object?>> CreateCompetencyAsync(CreateCompetencyCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateCompetencyAsync(UpdateCompetencyCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteCompetencyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<IEnumerable<ReviewTemplateDto>>> GetReviewTemplatesAsync(GetReviewTemplatesQuery request, CancellationToken cancellationToken = default);
    Task<Response<object?>> CreateReviewTemplateAsync(CreateReviewTemplateCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> UpdateReviewTemplateAsync(UpdateReviewTemplateCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteReviewTemplateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<IEnumerable<PerformanceCycleDto>>> GetPerformanceCyclesAsync(GetPerformanceCyclesQuery request, CancellationToken cancellationToken = default);
    Task<Response<object?>> CreatePerformanceCycleAsync(CreatePerformanceCycleCommand request, CancellationToken cancellationToken = default);

    // Bulk Onboarding
    Task<Response<List<BulkOnboardResultItem>>> BulkOnboardAsync(BulkOnboardRequest request, CancellationToken cancellationToken = default);
}

