using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Crm.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Services;

public class CrmService : ICrmService
{
    private readonly IGenericRepository<Call> _callRepo;
    private readonly IGenericRepository<Interaction> _interactionRepo;
    private readonly IGenericRepository<Organization> _orgRepo;

    public CrmService(
        IGenericRepository<Call> callRepo,
        IGenericRepository<Interaction> interactionRepo,
        IGenericRepository<Organization> orgRepo)
    {
        _callRepo = callRepo;
        _interactionRepo = interactionRepo;
        _orgRepo = orgRepo;
    }

    public async Task<Guid> CreateCallAsync(CreateCallCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Call
        {
            Subject = request.Subject, CallerType = request.CallerType, CallerMethod = request.CallerMethod,
            CallerIdentifier = request.CallerIdentifier, CalleeMethod = request.CalleeMethod,
            CalleeIdentifier = request.CalleeIdentifier, Direction = request.Direction, Status = request.Status,
            Phone = request.Phone ?? string.Empty, Notes = request.Notes, LinkedToId = request.LinkedToId,
            AgentId = request.AgentId, StartAt = request.StartAt?.UtcDateTime, EndAt = request.EndAt?.UtcDateTime
        };
        await _callRepo.AddAsync(entity);
        await _callRepo.SaveAsync();
        return entity.Id;
    }

    public async Task<Guid> CreateInteractionAsync(CreateInteractionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Interaction
        {
            Direction = request.Direction, Subject = request.Subject, Type = request.Type, Channel = request.Channel,
            Outcome = request.Outcome, Notes = request.Notes, RelatedToId = request.RelatedToId,
            LoggedById = request.LoggedById, OccurredAt = request.OccurredAt?.UtcDateTime ?? DateTime.UtcNow
        };
        await _interactionRepo.AddAsync(entity);
        await _interactionRepo.SaveAsync();
        return entity.Id;
    }

    public async Task<Guid> CreateOrganizationAsync(CreateOrganizationCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Organization
        {
            Name = request.Name, Phone = request.Phone, Email = request.Email,
            PrimaryTaxPayerId = request.PrimaryTaxPayerId
        };
        await _orgRepo.AddAsync(entity);
        await _orgRepo.SaveAsync();
        return entity.Id;
    }

    public async Task<DeleteCallCommand> DeleteCallAsync(DeleteCallCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Call), request.Id);
        await _callRepo.RemoveAsync(entity);
        await _callRepo.SaveAsync();
        return request;
    }

    public async Task<DeleteInteractionCommand> DeleteInteractionAsync(DeleteInteractionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _interactionRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Interaction), request.Id);
        await _interactionRepo.RemoveAsync(entity);
        await _interactionRepo.SaveAsync();
        return request;
    }

    public async Task<DeleteOrganizationCommand> DeleteOrganizationAsync(DeleteOrganizationCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _orgRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Organization), request.Id);
        await _orgRepo.RemoveAsync(entity);
        await _orgRepo.SaveAsync();
        return request;
    }

    public async Task<UpdateCallCommand> UpdateCallAsync(UpdateCallCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Call), request.Id);
        entity.Subject = request.Subject; entity.CallerType = request.CallerType; entity.CallerMethod = request.CallerMethod;
        entity.CallerIdentifier = request.CallerIdentifier; entity.CalleeMethod = request.CalleeMethod;
        entity.CalleeIdentifier = request.CalleeIdentifier; entity.Direction = request.Direction;
        entity.Status = request.Status; entity.Phone = request.Phone ?? string.Empty; entity.Notes = request.Notes;
        entity.LinkedToId = request.LinkedToId; entity.AgentId = request.AgentId;
        entity.StartAt = request.StartAt?.UtcDateTime; entity.EndAt = request.EndAt?.UtcDateTime;
        await _callRepo.UpdateAsync(entity);
        await _callRepo.SaveAsync();
        return request;
    }

    public async Task<UpdateInteractionCommand> UpdateInteractionAsync(UpdateInteractionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _interactionRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Interaction), request.Id);
        entity.Direction = request.Direction; entity.Subject = request.Subject; entity.Type = request.Type;
        entity.Channel = request.Channel; entity.Outcome = request.Outcome; entity.Notes = request.Notes;
        entity.RelatedToId = request.RelatedToId; entity.LoggedById = request.LoggedById;
        entity.OccurredAt = request.OccurredAt?.UtcDateTime ?? entity.OccurredAt;
        await _interactionRepo.UpdateAsync(entity);
        await _interactionRepo.SaveAsync();
        return request;
    }

    public async Task<UpdateOrganizationCommand> UpdateOrganizationAsync(UpdateOrganizationCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _orgRepo.GetByIdAsync(request.Id);
        if (entity == null) throw new NotFoundException(nameof(Organization), request.Id);
        entity.Name = request.Name; entity.Phone = request.Phone;
        entity.Email = request.Email; entity.PrimaryTaxPayerId = request.PrimaryTaxPayerId;
        await _orgRepo.UpdateAsync(entity);
        await _orgRepo.SaveAsync();
        return request;
    }

    public async Task<CallDto> GetCallByIdAsync(GetCallByIdQuery request, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepo.FindAsync(x => x.Id == request.Id);
        if (entity == null) throw new NotFoundException(nameof(Call), request.Id);
        return MapCall(entity);
    }

    public async Task<List<CallDto>> GetCallsAsync(GetCallsQuery request, CancellationToken cancellationToken = default)
    {
        return await _callRepo.Query().AsNoTracking().Select(x => MapCall(x)).ToListAsync(cancellationToken);
    }

    public async Task<InteractionDto> GetInteractionByIdAsync(GetInteractionByIdQuery request, CancellationToken cancellationToken = default)
    {
        var entity = await _interactionRepo.FindAsync(x => x.Id == request.Id);
        if (entity == null) throw new NotFoundException(nameof(Interaction), request.Id);
        return MapInteraction(entity);
    }

    public async Task<List<InteractionDto>> GetInteractionsAsync(GetInteractionsQuery request, CancellationToken cancellationToken = default)
    {
        return await _interactionRepo.Query().AsNoTracking().Select(x => MapInteraction(x)).ToListAsync(cancellationToken);
    }

    public async Task<OrganizationDto> GetOrganizationByIdAsync(GetOrganizationByIdQuery request, CancellationToken cancellationToken = default)
    {
        var entity = await _orgRepo.FindAsync(x => x.Id == request.Id);
        if (entity == null) throw new NotFoundException(nameof(Organization), request.Id);
        return MapOrg(entity);
    }

    public async Task<List<OrganizationDto>> GetOrganizationsAsync(GetOrganizationsQuery request, CancellationToken cancellationToken = default)
    {
        return await _orgRepo.Query().AsNoTracking().Select(x => MapOrg(x)).ToListAsync(cancellationToken);
    }

    // ─── Mappers ───────────────────────────────────────────────────────────────

    private static CallDto MapCall(Call x) => new()
    {
        Id = x.Id, Subject = x.Subject, CallerType = x.CallerType, CallerMethod = x.CallerMethod,
        CallerIdentifier = x.CallerIdentifier, CalleeMethod = x.CalleeMethod, CalleeIdentifier = x.CalleeIdentifier,
        Direction = x.Direction, Status = x.Status, Phone = x.Phone, Notes = x.Notes,
        LinkedToId = x.LinkedToId, AgentId = x.AgentId, StartAt = x.StartAt, EndAt = x.EndAt,
        CreatedAt = x.CreatedAt, CreatedBy = x.CreatedByUserId, UpdatedAt = x.LastModifiedAt, UpdatedBy = x.LastModifiedByUserId
    };

    private static InteractionDto MapInteraction(Interaction x) => new()
    {
        Id = x.Id, Direction = x.Direction, Subject = x.Subject, Type = x.Type, Channel = x.Channel,
        Outcome = x.Outcome, Notes = x.Notes, RelatedToId = x.RelatedToId, LoggedById = x.LoggedById,
        OccurredAt = x.OccurredAt, CreatedAt = x.CreatedAt, CreatedBy = x.CreatedByUserId,
        UpdatedAt = x.LastModifiedAt, UpdatedBy = x.LastModifiedByUserId
    };

    private static OrganizationDto MapOrg(Organization x) => new()
    {
        Id = x.Id, Name = x.Name, Phone = x.Phone, Email = x.Email,
        PrimaryTaxPayerId = x.PrimaryTaxPayerId, CreatedAt = x.CreatedAt,
        CreatedBy = x.CreatedByUserId, UpdatedAt = x.LastModifiedAt, UpdatedBy = x.LastModifiedByUserId
    };
}
