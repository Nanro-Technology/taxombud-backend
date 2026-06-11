using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;
using DomainEmail = TaxOmbud.Domain.ValueObjects.Email;
using DomainUser = TaxOmbud.Domain.Entities.Identity.User;

namespace TaxOmbud.Application.Features.Users.Commands.CreateUser;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
) : IRequest<Result<CreateUserResponse>>;

public record CreateUserResponse(Guid Id, string FullName, string Email);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[!@#$%^&*()\-_=+]").WithMessage("Password must contain at least one special character.");
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        DomainEmail emailVo;
        try { emailVo = new DomainEmail(request.Email); }
        catch (ArgumentException ex) { return Result<CreateUserResponse>.Failure(ex.Message); }

        var normalizedEmail = emailVo.Value;
        if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
            return Result<CreateUserResponse>.Failure("Email is already taken.");

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            if (!deptExists)
                return Result<CreateUserResponse>.Failure("Department not found.");
        }

        var user = DomainUser.Create(request.FirstName, request.LastName, emailVo, request.Phone);
        user.SetPasswordHash(_passwordHasher.Hash(request.Password));
        user.SetEmploymentType(request.EmploymentType);
        user.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.JobTitle);

        if (request.DepartmentId.HasValue)
        {
            user.SetDepartment(request.DepartmentId.Value);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CreateUserResponse>.Success(new CreateUserResponse(user.Id, user.FullName, user.Email));
    }
}
