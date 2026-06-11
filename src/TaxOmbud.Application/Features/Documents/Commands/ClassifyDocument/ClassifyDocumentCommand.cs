using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Documents.Commands.ClassifyDocument;

public record ClassifyDocumentCommand(Guid DocumentId, string Classification) : IRequest<Result<Unit>>;

public class ClassifyDocumentCommandValidator : AbstractValidator<ClassifyDocumentCommand>
{
    public ClassifyDocumentCommandValidator()
    {
        RuleFor(v => v.DocumentId).NotEmpty();
        RuleFor(v => v.Classification).NotEmpty().MaximumLength(100);
    }
}

public class ClassifyDocumentCommandHandler : IRequestHandler<ClassifyDocumentCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ClassifyDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ClassifyDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document == null)
            throw new NotFoundException(nameof(Domain.Entities.Documents.Document), request.DocumentId);

        document.Classification = request.Classification;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
