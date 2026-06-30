using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Hr.DTOs;
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
}
