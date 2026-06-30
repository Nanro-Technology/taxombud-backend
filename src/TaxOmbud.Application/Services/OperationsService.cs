using TaxOmbud.Common.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Enums;
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
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Services;

public class OperationsService : IOperationsService
{
    private readonly IApplicationDbContext _context;

    public OperationsService(
        IApplicationDbContext context
    )
    {
        _context = context;
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
        _context.InventoryItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
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
        _context.VendorContacts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
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
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<bool>> DeleteVendorAsync(DeleteVendorCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<bool>();
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(VendorContact), request.Id);
        }

        _context.VendorContacts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<bool>> UpdateProjectStatusAsync(UpdateProjectStatusCommands request, CancellationToken cancellationToken = default)
{
        var response = new Response<bool>();
        var entity = await _context.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if(entity == null) return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = $"Project {request.Id} not found." };
        try
        {
        
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<Guid>> UpdateVendorAsync(UpdateVendorCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<Guid>();
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

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

        await _context.SaveChangesAsync(cancellationToken);

        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<List<InventoryItem>>> GetInventoryItemsAsync(GetInventoryItemsQueries request, CancellationToken cancellationToken = default)
{
        var response = new Response<List<InventoryItem>>();
        try
        {
        var list = await _context.InventoryItems.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<List<Project>>> GetProjectsAsync(GetProjectsQueries request, CancellationToken cancellationToken = default)
{
        var response = new Response<List<Project>>();
        try
        {
        var list = await _context.Projects.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<VendorContact>> GetVendorByIdAsync(GetVendorByIdQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<VendorContact>();
        var entity = await _context.VendorContacts.FindAsync(new object[] { request.Id }, cancellationToken);

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
        var list = await _context.VendorContacts.ToListAsync(cancellationToken);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = list;
        return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

}