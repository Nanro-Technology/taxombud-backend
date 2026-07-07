using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.PayGrades.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class PayGradesService : IPayGradesService
{
    private readonly IGenericRepository<PayGrade> _payGradeRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<SalaryProfile> _salaryProfileRepo;

    public PayGradesService(
        IGenericRepository<PayGrade> payGradeRepo,
        IGenericRepository<User> userRepo,
        IGenericRepository<SalaryProfile> salaryProfileRepo
    )
    {
        _payGradeRepo = payGradeRepo;
        _userRepo = userRepo;
        _salaryProfileRepo = salaryProfileRepo;
    }

    public async Task<Response<CreatedPayGradeResponse>> CreatePayGradeAsync(CreatePayGradeCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreatedPayGradeResponse>();
        var duplicate = await _payGradeRepo.ExistsAsync(g => g.Level == request.Level);
        if (duplicate)
            return new Response<CreatedPayGradeResponse> { StatusCode = StatusCodes.Status400BadRequest, Message = $"A pay grade at level {request.Level} already exists." };
        try
        {
            var grade = new PayGrade
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Level = request.Level,
                BasicSalaryBand = request.BasicSalaryBand
            };

            await _payGradeRepo.AddAsync(grade);
            await _payGradeRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new CreatedPayGradeResponse(grade.Id, grade.Name, grade.Level);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<object?>> DeletePayGradeAsync(DeletePayGradeCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        var grade = await _payGradeRepo.FindAsync(g => g.Id == request.Id);
        if (grade == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Pay grade not found." };
        try
        {
            await _payGradeRepo.RemoveAsync(grade);
            await _payGradeRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<SavedSalaryProfileResponse>> SaveSalaryProfileAsync(SaveSalaryProfileCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SavedSalaryProfileResponse>();
        var userExists = await _userRepo.ExistsAsync(u => u.Id == request.UserId);
        if (!userExists)
            return new Response<SavedSalaryProfileResponse> { StatusCode = StatusCodes.Status400BadRequest, Message = "User not found." };
        try
        {
            // Close any existing active profile
            var existing = await _salaryProfileRepo.Query()
                .Where(s => s.UserId == request.UserId && s.EffectiveTo == null)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null)
            {
                existing.EffectiveTo = request.EffectiveFrom.AddDays(-1);
                await _salaryProfileRepo.UpdateAsync(existing);
            }

            var profile = new SalaryProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Basic = request.Basic,
                Allowances = request.Allowances,
                Deductions = request.Deductions,
                EffectiveFrom = request.EffectiveFrom
            };

            await _salaryProfileRepo.AddAsync(profile);
            await _salaryProfileRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = new SavedSalaryProfileResponse(profile.Id, profile.UserId, profile.Basic, profile.EffectiveFrom);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<object?>> UpdatePayGradeAsync(UpdatePayGradeCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        var grade = await _payGradeRepo.FindAsync(g => g.Id == request.Id);
        if (grade == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Pay grade not found." };
        try
        {
            grade.Name = request.Name;
            grade.Level = request.Level;
            grade.BasicSalaryBand = request.BasicSalaryBand;

            await _payGradeRepo.UpdateAsync(grade);
            await _payGradeRepo.SaveAsync();

            return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<PayGradeDetailDto>> GetPayGradeByIdAsync(GetPayGradeByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<PayGradeDetailDto>();
        var grade = await _payGradeRepo.GetByIdAsync(request.Id);
        if (grade == null)
            return new Response<PayGradeDetailDto> { StatusCode = StatusCodes.Status404NotFound, Message = "Pay grade not found." };
        try
        {
            var dto = new PayGradeDetailDto(
                grade.Id,
                grade.Name,
                grade.Level,
                grade.BasicSalaryBand,
                grade.CreatedAt
            );

            return new Response<PayGradeDetailDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = dto };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<IEnumerable<PayGradeDto>>> GetPayGradesAsync(GetPayGradesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<PayGradeDto>>();
        try
        {
            var grades = await _payGradeRepo.Query()
                .AsNoTracking()
                .OrderBy(g => g.Level)
                .Select(g => new PayGradeDto(
                    g.Id,
                    g.Name,
                    g.Level,
                    g.BasicSalaryBand,
                    g.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = grades;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<IEnumerable<SalaryProfileDto>>> GetSalaryProfilesAsync(GetSalaryProfilesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<SalaryProfileDto>>();
        try
        {
            var query = _salaryProfileRepo.Query()
                .Include(s => s.User)
                .AsNoTracking()
                .AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(s => s.UserId == request.UserId.Value);

            var items = await query
                .OrderByDescending(s => s.EffectiveFrom)
                .Select(s => new SalaryProfileDto(
                    s.Id,
                    s.UserId,
                    s.User.FullName,
                    s.Basic,
                    s.Allowances,
                    s.Deductions,
                    s.EffectiveFrom,
                    s.EffectiveTo,
                    s.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = items;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }
}
