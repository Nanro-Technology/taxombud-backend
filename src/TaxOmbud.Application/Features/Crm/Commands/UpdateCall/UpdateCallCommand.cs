using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.UpdateCall;

public record UpdateCallCommand : IRequest
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = null!;
    public string? CallerType { get; init; }
    public string? CallerMethod { get; init; }
    public string? CallerIdentifier { get; init; }
    public string? CalleeMethod { get; init; }
    public string? CalleeIdentifier { get; init; }
    public string Direction { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string? Notes { get; init; }
    public Guid? LinkedToId { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
}

public class UpdateCallCommandValidator : AbstractValidator<UpdateCallCommand>
{
    public UpdateCallCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Direction).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(50);
    }
}

public class UpdateCallCommandHandler : IRequestHandler<UpdateCallCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCallCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calls.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Call), request.Id);
        }

        entity.Subject = request.Subject;
        entity.CallerType = request.CallerType;
        entity.CallerMethod = request.CallerMethod;
        entity.CallerIdentifier = request.CallerIdentifier;
        entity.CalleeMethod = request.CalleeMethod;
        entity.CalleeIdentifier = request.CalleeIdentifier;
        entity.Direction = request.Direction;
        entity.Status = request.Status;
        entity.Phone = request.Phone;
        entity.Notes = request.Notes;
        entity.LinkedToId = request.LinkedToId;
        entity.AgentId = request.AgentId;
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
