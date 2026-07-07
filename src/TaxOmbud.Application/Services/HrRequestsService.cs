using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.HrRequests.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class HrRequestsService : IHrRequestsService
{
    private readonly IGenericRepository<LeaveRequest> _leaveRepo;
    private readonly IGenericRepository<LoanRequest> _loanRepo;
    private readonly IGenericRepository<EwaRequest> _ewaRepo;

    public HrRequestsService(
        IGenericRepository<LeaveRequest> leaveRepo,
        IGenericRepository<LoanRequest> loanRepo,
        IGenericRepository<EwaRequest> ewaRepo)
    {
        _leaveRepo = leaveRepo;
        _loanRepo = loanRepo;
        _ewaRepo = ewaRepo;
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
            await _leaveRepo.UpdateAsync(entity);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = true;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
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
                Status = "Pending"
            };

            await _leaveRepo.AddAsync(entity);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = entity.Id;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
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
                CreatedAt = DateTime.UtcNow
            };

            await _loanRepo.AddAsync(entity);
            await _loanRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = entity.Id;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<List<EwaRequest>>> GetEwaRequestsAsync(GetEwaRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<EwaRequest>>();
        try
        {
            var list = await _ewaRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<List<LeaveRequest>>> GetLeaveRequestsAsync(GetLeaveRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<LeaveRequest>>();
        try
        {
            var list = await _leaveRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<List<LoanRequest>>> GetLoanRequestsAsync(GetLoanRequestsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<LoanRequest>>();
        try
        {
            var list = await _loanRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}
