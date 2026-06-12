using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.System;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.System.Commands.CreateAnnouncement;

public record CreateAnnouncementCommand(string Title, string Message, string Scope) : IRequest<Result<Guid>>;

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateAnnouncementCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var entity = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Message = request.Message,
            Scope = request.Scope,
            CreatedAt = DateTime.UtcNow
        };
        _context.Announcements.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
