using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Crm.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Services;

public class CrmService : ICrmService
{
    private readonly IApplicationDbContext _context;

    public CrmService(
        IApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<Guid> CreateCallAsync(CreateCallCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Call
        {
            Subject = request.Subject,
            CallerType = request.CallerType,
            CallerMethod = request.CallerMethod,
            CallerIdentifier = request.CallerIdentifier,
            CalleeMethod = request.CalleeMethod,
            CalleeIdentifier = request.CalleeIdentifier,
            Direction = request.Direction,
            Status = request.Status,
            Phone = request.Phone ?? string.Empty,
            Notes = request.Notes,
            LinkedToId = request.LinkedToId,
            AgentId = request.AgentId,
            StartAt = request.StartAt?.UtcDateTime,
            EndAt = request.EndAt?.UtcDateTime
        };

        _context.Calls.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<Guid> CreateInteractionAsync(CreateInteractionCommand request, CancellationToken cancellationToken = default)
{
        var entity = new Interaction
        {
            Direction = request.Direction,
            Subject = request.Subject,
            Type = request.Type,
            Channel = request.Channel,
            Outcome = request.Outcome,
            Notes = request.Notes,
            RelatedToId = request.RelatedToId,
            LoggedById = request.LoggedById,
            OccurredAt = request.OccurredAt?.UtcDateTime ?? DateTime.UtcNow
        };

        _context.Interactions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<Guid> CreateOrganizationAsync(CreateOrganizationCommand request, CancellationToken cancellationToken = default)
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

    public async Task<DeleteCallCommand> DeleteCallAsync(DeleteCallCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Calls.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Call), request.Id);
        }

        _context.Calls.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<DeleteInteractionCommand> DeleteInteractionAsync(DeleteInteractionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Interactions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        _context.Interactions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<DeleteOrganizationCommand> DeleteOrganizationAsync(DeleteOrganizationCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Organizations.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Organization), request.Id);
        }

        _context.Organizations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<UpdateCallCommand> UpdateCallAsync(UpdateCallCommand request, CancellationToken cancellationToken = default)
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
        entity.Phone = request.Phone ?? string.Empty;
        entity.Notes = request.Notes;
        entity.LinkedToId = request.LinkedToId;
        entity.AgentId = request.AgentId;
        entity.StartAt = request.StartAt?.UtcDateTime;
        entity.EndAt = request.EndAt?.UtcDateTime;

        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<UpdateInteractionCommand> UpdateInteractionAsync(UpdateInteractionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Interactions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        entity.Direction = request.Direction;
        entity.Subject = request.Subject;
        entity.Type = request.Type;
        entity.Channel = request.Channel;
        entity.Outcome = request.Outcome;
        entity.Notes = request.Notes;
        entity.RelatedToId = request.RelatedToId;
        entity.LoggedById = request.LoggedById;
        entity.OccurredAt = request.OccurredAt?.UtcDateTime ?? entity.OccurredAt;

        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<UpdateOrganizationCommand> UpdateOrganizationAsync(UpdateOrganizationCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Organizations.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Organization), request.Id);
        }

        entity.Name = request.Name;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.PrimaryTaxPayerId = request.PrimaryTaxPayerId;

        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<CallDto> GetCallByIdAsync(GetCallByIdQuery request, CancellationToken cancellationToken = default)
{
        var entity = await _context.Calls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Call), request.Id);
        }

        return new CallDto
        {
            Id = entity.Id,
            Subject = entity.Subject,
            CallerType = entity.CallerType,
            CallerMethod = entity.CallerMethod,
            CallerIdentifier = entity.CallerIdentifier,
            CalleeMethod = entity.CalleeMethod,
            CalleeIdentifier = entity.CalleeIdentifier,
            Direction = entity.Direction,
            Status = entity.Status,
            Phone = entity.Phone,
            Notes = entity.Notes,
            LinkedToId = entity.LinkedToId,
            AgentId = entity.AgentId,
            StartAt = entity.StartAt,
            EndAt = entity.EndAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedByUserId,
            UpdatedAt = entity.LastModifiedAt,
            UpdatedBy = entity.LastModifiedByUserId
        };
    }

    public async Task<List<CallDto>> GetCallsAsync(GetCallsQuery request, CancellationToken cancellationToken = default)
{
        return await _context.Calls
            .AsNoTracking()
            .Select(x => new CallDto
            {
                Id = x.Id,
                Subject = x.Subject,
                CallerType = x.CallerType,
                CallerMethod = x.CallerMethod,
                CallerIdentifier = x.CallerIdentifier,
                CalleeMethod = x.CalleeMethod,
                CalleeIdentifier = x.CalleeIdentifier,
                Direction = x.Direction,
                Status = x.Status,
                Phone = x.Phone,
                Notes = x.Notes,
                LinkedToId = x.LinkedToId,
                AgentId = x.AgentId,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedByUserId,
                UpdatedAt = x.LastModifiedAt,
                UpdatedBy = x.LastModifiedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InteractionDto> GetInteractionByIdAsync(GetInteractionByIdQuery request, CancellationToken cancellationToken = default)
{
        var entity = await _context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        return new InteractionDto
        {
            Id = entity.Id,
            Direction = entity.Direction,
            Subject = entity.Subject,
            Type = entity.Type,
            Channel = entity.Channel,
            Outcome = entity.Outcome,
            Notes = entity.Notes,
            RelatedToId = entity.RelatedToId,
            LoggedById = entity.LoggedById,
            OccurredAt = entity.OccurredAt,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedByUserId,
            UpdatedAt = entity.LastModifiedAt,
            UpdatedBy = entity.LastModifiedByUserId
        };
    }

    public async Task<List<InteractionDto>> GetInteractionsAsync(GetInteractionsQuery request, CancellationToken cancellationToken = default)
{
        return await _context.Interactions
            .AsNoTracking()
            .Select(x => new InteractionDto
            {
                Id = x.Id,
                Direction = x.Direction,
                Subject = x.Subject,
                Type = x.Type,
                Channel = x.Channel,
                Outcome = x.Outcome,
                Notes = x.Notes,
                RelatedToId = x.RelatedToId,
                LoggedById = x.LoggedById,
                OccurredAt = x.OccurredAt,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedByUserId,
                UpdatedAt = x.LastModifiedAt,
                UpdatedBy = x.LastModifiedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(GetOrganizationByIdQuery request, CancellationToken cancellationToken = default)
{
        var entity = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Organization), request.Id);
        }

        return new OrganizationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            PrimaryTaxPayerId = entity.PrimaryTaxPayerId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedByUserId,
            UpdatedAt = entity.LastModifiedAt,
            UpdatedBy = entity.LastModifiedByUserId
        };
    }

    public async Task<List<OrganizationDto>> GetOrganizationsAsync(GetOrganizationsQuery request, CancellationToken cancellationToken = default)
{
        return await _context.Organizations
            .AsNoTracking()
            .Select(x => new OrganizationDto
            {
                Id = x.Id,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                PrimaryTaxPayerId = x.PrimaryTaxPayerId,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedByUserId,
                UpdatedAt = x.LastModifiedAt,
                UpdatedBy = x.LastModifiedByUserId
            })
            .ToListAsync(cancellationToken);
    }

}
