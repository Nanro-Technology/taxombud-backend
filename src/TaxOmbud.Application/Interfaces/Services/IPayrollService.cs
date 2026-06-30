using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Payroll.DTOs;
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

public interface IPayrollService
{
    Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateSalaryProfileAsync(CreateSalaryProfileCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> RunPayrollAsync(RunPayrollCommands request, CancellationToken cancellationToken = default);
    Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default);
}
