using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Operations;

namespace TaxOmbud.Application.Services;

public class OperationsService : IOperationsService
{
    private readonly IGenericRepository<InventoryItem> _itemRepo;
    private readonly IGenericRepository<VendorContact> _vendorRepo;
    private readonly IGenericRepository<Project> _projectRepo;

    public OperationsService(
        IGenericRepository<InventoryItem> itemRepo,
        IGenericRepository<VendorContact> vendorRepo,
        IGenericRepository<Project> projectRepo
    )
    {
        _itemRepo = itemRepo;
        _vendorRepo = vendorRepo;
        _projectRepo = projectRepo;
    }

    public async Task<Response<Guid>> AddInventoryItemAsync(AddInventoryItemCommands request, CancellationToken cancellationToken = default)
    {
        var entity = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            SKU = request.SKU,
            DepartmentId = request.DepartmentId,
            AssignedUserId = request.AssignedUserId,
            Location = request.Location,
            Mode = request.Mode,
            Quantity = request.Quantity,
            SerialNumber = request.SerialNumber,
            ImageUrl = request.ImageUrl,
            Status = request.Status,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };
        await _itemRepo.AddAsync(entity);
        await _itemRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<Guid>> AddVendorAsync(AddVendorCommands request, CancellationToken cancellationToken = default)
    {
        var entity = new VendorContact
        {
            Name = request.Name,
            Company = request.Company,
            Email = request.Email,
            Phone = request.Phone,
            Designation = request.Designation,
            Scope = request.Scope,
            ScopeTarget = request.ScopeTarget,
            Notes = request.Notes
        };
        await _vendorRepo.AddAsync(entity);
        await _vendorRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<Guid>> CreateProjectAsync(CreateProjectCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        var entity = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        await _projectRepo.AddAsync(entity);
        await _projectRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<bool>> DeleteVendorAsync(DeleteVendorCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        var entity = await _vendorRepo.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        await _vendorRepo.RemoveAsync(entity);
        await _vendorRepo.SaveAsync();

        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<bool>> UpdateProjectStatusAsync(UpdateProjectStatusCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        var entity = await _projectRepo.FindAsync(x => x.Id == request.Id);
        if (entity == null) return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = $"Project {request.Id} not found." };
        try
        {
            entity.Status = request.Status;
            entity.LastModifiedAt = DateTime.UtcNow;
            await _projectRepo.UpdateAsync(entity);
            await _projectRepo.SaveAsync();
            return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<Guid>> UpdateVendorAsync(UpdateVendorCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        var entity = await _vendorRepo.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        entity.Name = request.Name;
        entity.Company = request.Company;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Designation = request.Designation;
        entity.Scope = request.Scope;
        entity.ScopeTarget = request.ScopeTarget;
        entity.Notes = request.Notes;

        await _vendorRepo.UpdateAsync(entity);
        await _vendorRepo.SaveAsync();

        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<List<InventoryItem>>> GetInventoryItemsAsync(GetInventoryItemsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<InventoryItem>>();
        try
        {
            var list = await _itemRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<List<Project>>> GetProjectsAsync(GetProjectsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Project>>();
        try
        {
            var list = await _projectRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    public async Task<Response<VendorContact>> GetVendorByIdAsync(GetVendorByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<VendorContact>();
        var entity = await _vendorRepo.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        return new Response<VendorContact> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity };
    }

    public async Task<Response<List<VendorContact>>> GetVendorsAsync(GetVendorsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<VendorContact>>();
        try
        {
            var list = await _vendorRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }
}
