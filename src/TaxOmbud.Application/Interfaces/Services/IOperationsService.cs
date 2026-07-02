using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Operations;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IOperationsService
{
    Task<Response<Guid>> AddInventoryItemAsync(AddInventoryItemCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> AddVendorAsync(AddVendorCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateProjectAsync(CreateProjectCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteVendorAsync(DeleteVendorCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateProjectStatusAsync(UpdateProjectStatusCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UpdateVendorAsync(UpdateVendorCommand request, CancellationToken cancellationToken = default);
    Task<Response<List<InventoryItem>>> GetInventoryItemsAsync(GetInventoryItemsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Project>>> GetProjectsAsync(GetProjectsQueries request, CancellationToken cancellationToken = default);
    Task<Response<VendorContact>> GetVendorByIdAsync(GetVendorByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<VendorContact>>> GetVendorsAsync(GetVendorsQueries request, CancellationToken cancellationToken = default);
}
