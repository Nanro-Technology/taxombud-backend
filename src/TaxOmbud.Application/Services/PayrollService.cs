using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Payroll.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IGenericRepository<PayrollRun> _payrollRunRepo;
    private readonly IGenericRepository<PayrollPeriod> _payrollPeriodRepo;
    private readonly IGenericRepository<SalaryProfile> _salaryProfileRepo;
    private readonly IGenericRepository<Remittance> _remittanceRepo;

    public PayrollService(
        IGenericRepository<PayrollRun> payrollRunRepo,
        IGenericRepository<PayrollPeriod> payrollPeriodRepo,
        IGenericRepository<SalaryProfile> salaryProfileRepo,
        IGenericRepository<Remittance> remittanceRepo)
    {
        _payrollRunRepo = payrollRunRepo;
        _payrollPeriodRepo = payrollPeriodRepo;
        _salaryProfileRepo = salaryProfileRepo;
        _remittanceRepo = remittanceRepo;
    }

    public async Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        var run = await _payrollRunRepo.FindAsync(x => x.Id == request.RunId);
        if (run == null)
            return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = $"Payroll Run {request.RunId} not found." };

        try
        {
            run.Status = "Approved";
            run.ApprovedAt = DateTime.UtcNow;
            await _payrollRunRepo.UpdateAsync(run);
            await _payrollRunRepo.SaveAsync();
            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<Guid>> CreateSalaryProfileAsync(CreateSalaryProfileCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        var entity = new SalaryProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.StaffId,
            Basic = request.BaseSalary,
            EffectiveFrom = DateTime.UtcNow
        };
        await _salaryProfileRepo.AddAsync(entity);
        await _salaryProfileRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<Guid>> RunPayrollAsync(RunPayrollCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        var entity = new PayrollRun
        {
            Id = Guid.NewGuid(),
            PeriodId = request.PeriodId,
            Status = "Pending",
            PostedAt = DateTime.UtcNow
        };
        await _payrollRunRepo.AddAsync(entity);
        await _payrollRunRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<PayrollPeriod>>();
        try
        {
            var list = await _payrollPeriodRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Remittance>>();
        try
        {
            var list = await _remittanceRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<SalaryProfile>>();
        try
        {
            var list = await _salaryProfileRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }
}
