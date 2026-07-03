using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Payroll.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IApplicationDbContext _context;

    public PayrollService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<bool>> ApprovePayrollAsync(ApprovePayrollCommands request, CancellationToken cancellationToken = default)
{
        var response = new Response<bool>();
        var run = await _context.PayrollRuns.FirstOrDefaultAsync(x => x.Id == request.RunId, cancellationToken);
        if (run == null) return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = $"Payroll Run {request.RunId} not found." };
        try
        {
        
        run.Status = "Approved";
        run.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

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
        _context.SalaryProfiles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
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
        _context.PayrollRuns.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<List<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQueries request, CancellationToken cancellationToken = default)
{
        var response = new Response<List<PayrollPeriod>>();
        try
        {
        var list = await _context.PayrollPeriods.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<List<Remittance>>> GetRemittancesAsync(GetRemittancesQueries request, CancellationToken cancellationToken = default)
{
        var response = new Response<List<Remittance>>();
        try
        {
        var list = await _context.Remittances.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<List<SalaryProfile>>> GetSalaryProfilesAsync(GetSalaryProfilesQueries request, CancellationToken cancellationToken = default)
{
        var response = new Response<List<SalaryProfile>>();
        try
        {
        var list = await _context.SalaryProfiles.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

}
