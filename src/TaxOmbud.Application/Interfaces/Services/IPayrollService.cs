using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Payroll.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IPayrollService
{
    // Salary Profiles
    Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> SaveSalaryProfileAsync(SaveSalaryProfileCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteSalaryProfileAsync(Guid id, CancellationToken cancellationToken = default);

    // Statutory Deductions & Rules
    Task<Response<List<StatutoryDeduction>>> GetStatutoryDeductionsAsync(CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateStatutoryDeductionAsync(CreateStatutoryDeductionCommand request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateStatutoryRuleAsync(Guid deductionId, CreateStatutoryRuleCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteStatutoryRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<Response<bool>> ToggleStatutoryDeductionStatusAsync(Guid id, CancellationToken cancellationToken = default);

    // Payout Providers
    Task<Response<List<PayoutProvider>>> GetPayoutProvidersAsync(CancellationToken cancellationToken = default);
    Task<Response<Guid>> SavePayoutProviderAsync(SavePayoutProviderCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> TogglePayoutProviderStatusAsync(Guid id, CancellationToken cancellationToken = default);

    // Payroll Periods
    Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreatePayrollPeriodAsync(CreatePayrollPeriodCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> TogglePayrollPeriodStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Response<ValidationResultDto>> ValidatePayrollPeriodAsync(Guid id, CancellationToken cancellationToken = default);

    // Payroll Runs
    Task<Response<List<PayrollRun>>> GetPayrollRunsAsync(CancellationToken cancellationToken = default);
    Task<Response<Guid>> RunPayrollAsync(RunPayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> PostPayrollAsync(Guid runId, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeletePayrollRunAsync(Guid runId, CancellationToken cancellationToken = default);

    // Scheduler
    Task<Response<SchedulerConfigDto>> GetSchedulerConfigAsync(CancellationToken cancellationToken = default);
    Task<Response<bool>> SaveSchedulerConfigAsync(SchedulerConfigDto request, CancellationToken cancellationToken = default);
    Task<Response<string>> TriggerSchedulerRunAsync(CancellationToken cancellationToken = default);

    // Remittance
    Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default);
    Task<Response<bool>> GenerateRemittancesAsync(Guid periodId, string type, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateRemittanceStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
