using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Features.Departments.Commands.CreateDepartment;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateDepartmentCommand(string Name, string RoutingMode, string? Description, Guid? HeadUserId) : IRequest<Result<CreateDepartmentResponse>>;

public record CreateDepartmentResponse(Guid Id, string Name, string RoutingMode, string? Description);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoutingMode).Must(m => m.Equals("head", StringComparison.OrdinalIgnoreCase) || m.Equals("members", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Routing mode must be 'head' or 'members'.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<CreateDepartmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateDepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Departments.AnyAsync(d => d.Name == request.Name, cancellationToken))
            return Result<CreateDepartmentResponse>.Failure("Department name already exists.");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            RoutingMode = request.RoutingMode.ToLowerInvariant() == "head" ? "head" : "members",
            Description = request.Description
        };

        if (request.HeadUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.HeadUserId.Value, cancellationToken);
            if (!userExists)
                return Result<CreateDepartmentResponse>.Failure("Head user not found.");
            
            department.HeadUserId = request.HeadUserId.Value;
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CreateDepartmentResponse>.Success(new CreateDepartmentResponse(department.Id, department.Name, department.RoutingMode, department.Description));
    }
}
