using TaxOmbud.Application.Crm.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ICrmService
{
    Task<Guid> CreateCallAsync(CreateCallCommand request, CancellationToken cancellationToken = default);
    Task<Guid> CreateInteractionAsync(CreateInteractionCommand request, CancellationToken cancellationToken = default);
    Task<Guid> CreateOrganizationAsync(CreateOrganizationCommand request, CancellationToken cancellationToken = default);
    Task<DeleteCallCommand> DeleteCallAsync(DeleteCallCommand request, CancellationToken cancellationToken = default);
    Task<DeleteInteractionCommand> DeleteInteractionAsync(DeleteInteractionCommand request, CancellationToken cancellationToken = default);
    Task<DeleteOrganizationCommand> DeleteOrganizationAsync(DeleteOrganizationCommand request, CancellationToken cancellationToken = default);
    Task<UpdateCallCommand> UpdateCallAsync(UpdateCallCommand request, CancellationToken cancellationToken = default);
    Task<UpdateInteractionCommand> UpdateInteractionAsync(UpdateInteractionCommand request, CancellationToken cancellationToken = default);
    Task<UpdateOrganizationCommand> UpdateOrganizationAsync(UpdateOrganizationCommand request, CancellationToken cancellationToken = default);
    Task<CallDto> GetCallByIdAsync(GetCallByIdQuery request, CancellationToken cancellationToken = default);
    Task<List<CallDto>> GetCallsAsync(GetCallsQuery request, CancellationToken cancellationToken = default);
    Task<InteractionDto> GetInteractionByIdAsync(GetInteractionByIdQuery request, CancellationToken cancellationToken = default);
    Task<List<InteractionDto>> GetInteractionsAsync(GetInteractionsQuery request, CancellationToken cancellationToken = default);
    Task<OrganizationDto> GetOrganizationByIdAsync(GetOrganizationByIdQuery request, CancellationToken cancellationToken = default);
    Task<List<OrganizationDto>> GetOrganizationsAsync(GetOrganizationsQuery request, CancellationToken cancellationToken = default);
}
