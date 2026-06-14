using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.CreateOrganization;

public record CreateOrganizationCommand : IRequest<Guid>
{
    public string Name { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public Guid? PrimaryTaxPayerId { get; init; }
}

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
            
        RuleFor(v => v.Phone)
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.");
            
        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("A valid email is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");
    }
}

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var entity = new Organization
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            PrimaryTaxPayerId = request.PrimaryTaxPayerId
        };

        _context.Organizations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
