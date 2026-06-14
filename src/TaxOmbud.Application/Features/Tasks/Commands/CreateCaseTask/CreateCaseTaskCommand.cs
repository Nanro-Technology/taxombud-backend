using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Cases;

namespace TaxOmbud.Application.Features.Tasks.Commands.CreateCaseTask;

public record CreateCaseTaskCommand : IRequest<Guid>
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string Status { get; init; } = "Open";
    public string Priority { get; init; } = "Medium";
    public DateTimeOffset? DueAt { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid? LinkedCaseId { get; init; }
}

public class CreateCaseTaskCommandValidator : AbstractValidator<CreateCaseTaskCommand>
{
    public CreateCaseTaskCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            
        RuleFor(v => v.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters.");
            
        RuleFor(v => v.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .MaximumLength(50).WithMessage("Priority must not exceed 50 characters.");
    }
}

public class CreateCaseTaskCommandHandler : IRequestHandler<CreateCaseTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCaseTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCaseTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = new CaseTask
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueAt = request.DueAt,
            AssignedToId = request.AssignedToId,
            LinkedCaseId = request.LinkedCaseId
        };

        _context.CaseTasks.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
