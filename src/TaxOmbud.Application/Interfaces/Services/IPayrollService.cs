using TaxOmbud.Application.Payroll.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IPayrollService
{
    Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateSalaryProfileAsync(CreateSalaryProfileCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> RunPayrollAsync(RunPayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default);
}
