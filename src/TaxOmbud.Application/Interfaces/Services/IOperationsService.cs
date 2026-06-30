using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
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
