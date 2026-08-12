using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.HrRequests.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class HrRequestsService : IHrRequestsService
{
    private readonly IGenericRepository<LeaveRequest> _leaveRepo;
    private readonly IGenericRepository<LoanRequest> _loanRepo;
    private readonly IGenericRepository<EwaRequest> _ewaRepo;
    private readonly ICurrentUser _currentUser;

    public HrRequestsService(
        IGenericRepository<LeaveRequest> leaveRepo,
        IGenericRepository<LoanRequest> loanRepo,
        IGenericRepository<EwaRequest> ewaRepo,
        ICurrentUser currentUser)
    {
        _leaveRepo = leaveRepo;
        _loanRepo = loanRepo;
        _ewaRepo = ewaRepo;
        _currentUser = currentUser;
    }

    public async Task<Response<bool>> ApproveLeaveRequestAsync(ApproveLeaveRequestCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var entity = await _leaveRepo.FindAsync(x => x.Id == request.LeaveId);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = $"Leave Request {request.LeaveId} not found.";
                return response;
            }

            entity.Status = request.Approved ? "Approved" : "Rejected";
            entity.SupervisorNote = request.SupervisorNote;
            entity.ApproverUserId = _currentUser.UserId;
            await _leaveRepo.UpdateAsync(entity);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = true;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<Guid>> SubmitLeaveRequestAsync(SubmitLeaveRequestCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                UserId = request.StaffId,
                LeaveType = request.LeaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Days = (int)(request.EndDate - request.StartDate).TotalDays,
                Reason = request.Reason,
                Status = "Pending"
            };

            await _leaveRepo.AddAsync(entity);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = entity.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<Guid>> SubmitLoanRequestAsync(SubmitLoanRequestCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = new LoanRequest
            {
                Id = Guid.NewGuid(),
                UserId = request.StaffId,
                Amount = request.Amount,
                TermMonths = request.RepaymentMonths,
                Purpose = request.Purpose,
                DisburseTo = request.DisburseTo,
                PayoutReference = request.PayoutReference,
                ActionNote = request.ActionNote,
                Status = "Pending",
                IsSalaryAdvance = request.IsSalaryAdvance,
                CreatedAt = DateTime.UtcNow
            };

            await _loanRepo.AddAsync(entity);
            await _loanRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = entity.Id;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<EwaRequest>>> GetEwaRequestsAsync(GetEwaRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<EwaRequest>>();
        try
        {
            var list = await _ewaRepo.Query()
                .Include(x => x.User)
                .ToListAsync(cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<LeaveRequest>>> GetLeaveRequestsAsync(GetLeaveRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<LeaveRequest>>();
        try
        {
            var list = await _leaveRepo.Query()
                .Include(x => x.User)
                    .ThenInclude(u => u.Department)
                .ToListAsync(cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<LoanRequest>>> GetLoanRequestsAsync(GetLoanRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<LoanRequest>>();
        try
        {
            IQueryable<LoanRequest> query = _loanRepo.Query().Include(x => x.User);
            if (request.IsSalaryAdvance.HasValue)
            {
                query = query.Where(x => x.IsSalaryAdvance == request.IsSalaryAdvance.Value);
            }
            var list = await query.ToListAsync(cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }
}
