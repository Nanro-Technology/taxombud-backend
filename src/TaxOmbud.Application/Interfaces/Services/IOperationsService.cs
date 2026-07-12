using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Operations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IOperationsService
{
    Task<Response<Guid>> AddInventoryItemAsync(AddInventoryItemCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> AddVendorAsync(AddVendorCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateProjectAsync(CreateProjectCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UpdateProjectAsync(UpdateProjectCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteProjectAsync(DeleteProjectCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteVendorAsync(DeleteVendorCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateProjectStatusAsync(UpdateProjectStatusCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UpdateVendorAsync(UpdateVendorCommand request, CancellationToken cancellationToken = default);
    Task<Response<List<InventoryItem>>> GetInventoryItemsAsync(GetInventoryItemsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<ProjectDto>>> GetProjectsAsync(GetProjectsQueries request, CancellationToken cancellationToken = default);
    Task<Response<VendorContact>> GetVendorByIdAsync(GetVendorByIdQuery request, CancellationToken cancellationToken = default);
    Task<Response<List<VendorContact>>> GetVendorsAsync(GetVendorsQueries request, CancellationToken cancellationToken = default);

    // Visitor Operations
    Task<Response<Guid>> CreateVisitorAsync(CreateVisitorCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateVisitorStatusAsync(UpdateVisitorStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteVisitorAsync(DeleteVisitorCommand request, CancellationToken cancellationToken = default);
    Task<Response<List<Visitor>>> GetVisitorsAsync(GetVisitorsQueries request, CancellationToken cancellationToken = default);

    // Inventory Item Operations
    Task<Response<Guid>> UpdateInventoryItemAsync(UpdateInventoryItemCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> UpdateInventoryItemStatusAsync(UpdateInventoryItemStatusCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteInventoryItemAsync(DeleteInventoryItemCommand request, CancellationToken cancellationToken = default);
}
