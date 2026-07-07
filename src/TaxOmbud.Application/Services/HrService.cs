using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Hr.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class HrService : IHrService
{
    private readonly IGenericRepository<LeaveRequest> _leaveRepo;
    private readonly IGenericRepository<LoanRequest> _loanRepo;
    private readonly IGenericRepository<PayrollRun> _payrollRunRepo;
    private readonly IGenericRepository<PayrollPeriod> _payrollPeriodRepo;
    private readonly IGenericRepository<StaffProfile> _staffRepo;
    private readonly IGenericRepository<EmployeeWallet> _walletRepo;
    private readonly IGenericRepository<EwaRequest> _ewaRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;

    public HrService(
        IGenericRepository<LeaveRequest> leaveRepo,
        IGenericRepository<LoanRequest> loanRepo,
        IGenericRepository<PayrollRun> payrollRunRepo,
        IGenericRepository<PayrollPeriod> payrollPeriodRepo,
        IGenericRepository<StaffProfile> staffRepo,
        IGenericRepository<EmployeeWallet> walletRepo,
        IGenericRepository<EwaRequest> ewaRepo,
        IGenericRepository<User> userRepo,
        ICurrentUser currentUser
    )
    {
        _leaveRepo = leaveRepo;
        _loanRepo = loanRepo;
        _payrollRunRepo = payrollRunRepo;
        _payrollPeriodRepo = payrollPeriodRepo;
        _staffRepo = staffRepo;
        _walletRepo = walletRepo;
        _ewaRepo = ewaRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
    }

    public async Task<Response<object?>> ApproveLeaveAsync(ApproveLeaveCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var leave = await _leaveRepo.FindAsync(l => l.Id == request.Id);
            if (leave == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Leave request not found.";
                return response;
            }

            leave.Status = request.Approved ? "approved" : "rejected";
            leave.ApproverUserId = _currentUser.UserId ?? Guid.Empty;
            leave.SupervisorNote = request.SupervisorNote;

            await _leaveRepo.UpdateAsync(leave);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Leave request processed successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while processing the leave request.";
            return response;
        }
    }

    public async Task<Response<object?>> ApproveLoanAsync(ApproveLoanCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var loan = await _loanRepo.FindAsync(l => l.Id == request.Id);
            if (loan == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Loan request not found.";
                return response;
            }

            loan.Status = request.Approved ? "approved" : "rejected";
            await _loanRepo.UpdateAsync(loan);
            await _loanRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Loan request processed successfully.";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while processing the loan request.";
            return response;
        }
    }

    public async Task<Response<PayrollRun>> CreatePayrollRunAsync(CreatePayrollRunCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PayrollRun>();
        try
        {
            var period = await _payrollPeriodRepo.FindAsync(p => p.Id == request.PeriodId);
            if (period == null)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Payroll period not found.";
                return response;
            }

            var payrollRun = new PayrollRun
            {
                Id = Guid.NewGuid(),
                PeriodId = request.PeriodId,
                Status = "draft"
            };

            await _payrollRunRepo.AddAsync(payrollRun);
            await _payrollRunRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Payroll run created successfully.";
            response.Data = payrollRun;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the payroll run.";
            return response;
        }
    }

    public async Task<Response<LeaveRequest>> RequestLeaveAsync(RequestLeaveCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<LeaveRequest>();
        try
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var leave = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                LeaveType = request.LeaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Days = (request.EndDate - request.StartDate).Days + 1,
                Status = "pending"
            };

            await _leaveRepo.AddAsync(leave);
            await _leaveRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Leave request submitted successfully.";
            response.Data = leave;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while submitting the leave request.";
            return response;
        }
    }

    public async Task<Response<LoanRequest>> RequestLoanAsync(RequestLoanCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<LoanRequest>();
        try
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var loan = new LoanRequest
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Amount = request.Amount,
                TermMonths = request.TermMonths,
                Purpose = request.Purpose,
                Status = "pending"
            };

            await _loanRepo.AddAsync(loan);
            await _loanRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Loan request submitted successfully.";
            response.Data = loan;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while submitting the loan request.";
            return response;
        }
    }

    public async Task<Response<StaffProfile>> SaveStaffProfileAsync(SaveStaffProfileCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<StaffProfile>();
        try
        {
            var userExists = await _userRepo.ExistsAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Associated User account not found.";
                return response;
            }

            var staff = await _staffRepo.FindAsync(s => s.UserId == request.UserId);
            bool isNew = false;
            if (staff == null)
            {
                staff = new StaffProfile { Id = Guid.NewGuid(), UserId = request.UserId };
                isNew = true;
            }

            staff.EmployeeCode = request.EmployeeCode;
            staff.Title = request.Title;
            staff.SupervisorId = request.SupervisorId;
            staff.HireDate = request.HireDate;
            staff.EmploymentStatus = request.EmploymentStatus;
            staff.DateOfBirth = request.DateOfBirth;
            staff.Nationality = request.Nationality;
            staff.MaritalStatus = request.MaritalStatus;
            staff.EducationLevel = request.EducationLevel;
            staff.EducationDetails = request.EducationDetails;
            staff.AddressLine1 = request.AddressLine1;
            staff.AddressLine2 = request.AddressLine2;
            staff.City = request.City;
            staff.State = request.State;
            staff.Country = request.Country;
            staff.EmergencyContactName = request.EmergencyContactName;
            staff.EmergencyContactPhone = request.EmergencyContactPhone;
            staff.BankAccountNo = request.BankAccountNo;
            staff.BankId = request.BankId;
            staff.NextOfKinName = request.NextOfKinName;
            staff.NextOfKinRelationship = request.NextOfKinRelationship;
            staff.NextOfKinPhone = request.NextOfKinPhone;
            staff.NextOfKinAddress = request.NextOfKinAddress;

            if (isNew)
                await _staffRepo.AddAsync(staff);
            else
                await _staffRepo.UpdateAsync(staff);

            await _staffRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Staff profile saved successfully.";
            response.Data = staff;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while saving the staff profile.";
            return response;
        }
    }

    public async Task<Response<EwaWithdrawalResponse>> WithdrawEwaAsync(WithdrawEwaCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<EwaWithdrawalResponse>();
        try
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var wallet = await _walletRepo.Query()
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == currentUserId, cancellationToken);

            if (wallet == null || wallet.BalanceNgn < request.Amount)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Insufficient wallet balance.";
                return response;
            }

            var req = new EwaRequest
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Amount = request.Amount,
                Status = "approved",
                DisbursedAt = DateTimeOffset.UtcNow
            };

            wallet.BalanceNgn -= request.Amount;
            wallet.Transactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.UserId,
                Type = "debit",
                Amount = request.Amount,
                Reference = "EWA-" + Guid.NewGuid().ToString("N").Substring(0, 8)
            });

            await _walletRepo.UpdateAsync(wallet);
            await _ewaRepo.AddAsync(req);
            await _walletRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Earned Wage Access payout initiated successfully.";
            response.Data = new EwaWithdrawalResponse("Earned Wage Access payout initiated successfully.", request.Amount);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while processing the EWA withdrawal.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<LeaveRequestDto>>> GetLeaveRequestsAsync(GetLeaveRequestsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<LeaveRequestDto>>();
        try
        {
            var query = _leaveRepo.Query()
                .Include(l => l.User)
                .AsNoTracking()
                .AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(l => l.UserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var statusLower = request.Status.ToLower();
                query = query.Where(l => l.Status == statusLower);
            }

            var items = await query
                .OrderByDescending(l => l.StartDate)
                .Select(l => new LeaveRequestDto(
                    l.Id,
                    l.UserId,
                    l.User.FullName,
                    l.LeaveType,
                    l.StartDate,
                    l.EndDate,
                    l.Days,
                    l.Status,
                    l.SupervisorNote
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Leave requests retrieved successfully.";
            response.Data = items;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving leave requests.";
            return response;
        }
    }

    public async Task<Response<IEnumerable<PayrollPeriod>>> GetPayrollPeriodsAsync(GetPayrollPeriodsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<PayrollPeriod>>();
        try
        {
            var periods = await _payrollPeriodRepo.Query().AsNoTracking().ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Payroll periods retrieved successfully.";
            response.Data = periods;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving payroll periods.";
            return response;
        }
    }

    public async Task<Response<PagedResult<StaffListDto>>> GetStaffAsync(GetStaffQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<StaffListDto>>();
        try
        {
            var query = _staffRepo.Query()
                .Include(s => s.User)
                    .ThenInclude(u => u.Department)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(s =>
                    s.User!.FirstName!.ToLower().Contains(searchLower) ||
                    s.User!.LastName!.ToLower().Contains(searchLower) ||
                    s.User!.Email!.Contains(searchLower));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.HireDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new StaffListDto(
                    s.Id,
                    s.UserId,
                    s.User.FullName,
                    s.User!.Email ?? string.Empty,
                    s.User.Phone,
                    s.User.JobTitle,
                    s.User.Department != null ? s.User.Department.Name : "Unassigned",
                    s.HireDate,
                    s.EmploymentStatus,
                    s.MaritalStatus
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Staff retrieved successfully.";
            response.Data = new PagedResult<StaffListDto>(items, total, request.Page, request.PageSize);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving staff.";
            return response;
        }
    }

    public async Task<Response<StaffDetailDto>> GetStaffByIdAsync(GetStaffByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<StaffDetailDto>();
        try
        {
            var staff = await _staffRepo.Query()
                .Include(s => s.User)
                    .ThenInclude(u => u.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (staff == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Staff profile not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Staff profile retrieved successfully.";
            response.Data = new StaffDetailDto(
                staff.Id,
                staff.UserId,
                staff.User.FirstName,
                staff.User.LastName,
                staff.User.FullName,
                staff.User.Email ?? string.Empty,
                staff.User.Phone,
                staff.User.JobTitle,
                staff.User.Department != null ? new StaffDepartmentDto(staff.User.Department.Id, staff.User.Department.Name) : null,
                staff.HireDate,
                staff.EmploymentStatus,
                staff.DateOfBirth,
                staff.Nationality,
                staff.EmergencyContactName,
                staff.EmergencyContactPhone,
                staff.BankAccountNo,
                staff.BankId,
                staff.NextOfKinName,
                staff.NextOfKinPhone
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the staff profile.";
            return response;
        }
    }

    public async Task<Response<WalletDto>> GetWalletAsync(GetWalletQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<WalletDto>();
        try
        {
            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var wallet = await _walletRepo.Query()
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == currentUserId, cancellationToken);

            if (wallet == null)
            {
                wallet = new EmployeeWallet { UserId = currentUserId, BalanceNgn = 0, LedgerVersion = 1 };
                await _walletRepo.AddAsync(wallet);
                await _walletRepo.SaveAsync();
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Wallet retrieved successfully.";
            response.Data = new WalletDto(
                wallet.UserId,
                wallet.BalanceNgn,
                wallet.LedgerVersion,
                wallet.Transactions.Select(t => new WalletTransactionDto(
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.Reference,
                    t.CreatedAt
                ))
            );
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving the wallet.";
            return response;
        }
    }
}
