using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.PayGrades.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Services;

public class PayGradesService : IPayGradesService
{
    private readonly IApplicationDbContext _context;

    public PayGradesService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Response<CreatedPayGradeResponse>> CreatePayGradeAsync(CreatePayGradeCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<CreatedPayGradeResponse>();
        var duplicate = await _context.PayGrades.AnyAsync(g => g.Level == request.Level, cancellationToken);
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

        _context.PayGrades.Add(grade);
        await _context.SaveChangesAsync(cancellationToken);

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
        }}

    public async Task<Response<object?>> DeletePayGradeAsync(DeletePayGradeCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<object?>();
        var grade = await _context.PayGrades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Pay grade not found." };
        try
        {

        _context.PayGrades.Remove(grade);
        await _context.SaveChangesAsync(cancellationToken);

        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        return response;
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<SavedSalaryProfileResponse>> SaveSalaryProfileAsync(SaveSalaryProfileCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<SavedSalaryProfileResponse>();
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return new Response<SavedSalaryProfileResponse> { StatusCode = StatusCodes.Status400BadRequest, Message = "User not found." };
        try
        {

        // Close any existing active profile
        var existing = await _context.SalaryProfiles
            .Where(s => s.UserId == request.UserId && s.EffectiveTo == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
            existing.EffectiveTo = request.EffectiveFrom.AddDays(-1);

        var profile = new SalaryProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Basic = request.Basic,
            Allowances = request.Allowances,
            Deductions = request.Deductions,
            EffectiveFrom = request.EffectiveFrom
        };

        _context.SalaryProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

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
        }}

    public async Task<Response<object?>> UpdatePayGradeAsync(UpdatePayGradeCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<object?>();
        var grade = await _context.PayGrades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade == null)
            return new Response<object?> { StatusCode = StatusCodes.Status404NotFound, Message = "Pay grade not found." };
        try
        {

        grade.Name = request.Name;
        grade.Level = request.Level;
        grade.BasicSalaryBand = request.BasicSalaryBand;

        await _context.SaveChangesAsync(cancellationToken);

        return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<PayGradeDetailDto>> GetPayGradeByIdAsync(GetPayGradeByIdQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<PayGradeDetailDto>();
        var grade = await _context.PayGrades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
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
        }}

    public async Task<Response<IEnumerable<PayGradeDto>>> GetPayGradesAsync(GetPayGradesQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<IEnumerable<PayGradeDto>>();
        try
        {
        var grades = await _context.PayGrades
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
        }}

    public async Task<Response<IEnumerable<SalaryProfileDto>>> GetSalaryProfilesAsync(GetSalaryProfilesQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<IEnumerable<SalaryProfileDto>>();
        try
        {
        var query = _context.SalaryProfiles
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
        }}

}
