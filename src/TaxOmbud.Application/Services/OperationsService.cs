using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.Operations.DTOs;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Operations;
using Microsoft.AspNetCore.Http;

using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class OperationsService : IOperationsService
{
    private readonly IGenericRepository<InventoryItem> _itemRepo;
    private readonly IGenericRepository<VendorContact> _vendorRepo;
    private readonly IGenericRepository<Project> _projectRepo;
    private readonly IGenericRepository<Visitor> _visitorRepo;
    private readonly IGenericRepository<ProjectMember> _memberRepo;
    private readonly IGenericRepository<ProjectTask> _taskRepo;
    private readonly IGenericRepository<User> _userRepo;

    public OperationsService(
        IGenericRepository<InventoryItem> itemRepo,
        IGenericRepository<VendorContact> vendorRepo,
        IGenericRepository<Project> projectRepo,
        IGenericRepository<Visitor> visitorRepo,
        IGenericRepository<ProjectMember> memberRepo,
        IGenericRepository<ProjectTask> taskRepo,
        IGenericRepository<User> userRepo
    )
    {
        _itemRepo = itemRepo;
        _vendorRepo = vendorRepo;
        _projectRepo = projectRepo;
        _visitorRepo = visitorRepo;
        _memberRepo = memberRepo;
        _taskRepo = taskRepo;
        _userRepo = userRepo;
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
        var entity = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = request.Status ?? "planning",
            StartDate = request.StartDate,
            Deadline = request.DueDate,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        };
        await _projectRepo.AddAsync(entity);
        await _projectRepo.SaveAsync();

        if (request.MemberIds != null && request.MemberIds.Any())
        {
            var members = request.MemberIds.Select(userId => new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = entity.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _memberRepo.AddRangeAsync(members);
            await _memberRepo.SaveAsync();
        }

        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<Guid>> UpdateProjectAsync(UpdateProjectCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _projectRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.StartDate = request.StartDate;
        entity.Deadline = request.DueDate;
        entity.OwnerId = request.OwnerId;
        entity.LastModifiedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(entity);
        await _projectRepo.SaveAsync();

        // Sync members
        var existingMembers = await _memberRepo.FindAllAsync(m => m.ProjectId == entity.Id);
        if (existingMembers.Any())
        {
            await _memberRepo.RemoveRangeAsync(existingMembers);
            await _memberRepo.SaveAsync();
        }

        if (request.MemberIds != null && request.MemberIds.Any())
        {
            var newMembers = request.MemberIds.Select(userId => new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = entity.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _memberRepo.AddRangeAsync(newMembers);
            await _memberRepo.SaveAsync();
        }

        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<bool>> DeleteProjectAsync(DeleteProjectCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _projectRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
        }

        var members = await _memberRepo.FindAllAsync(m => m.ProjectId == entity.Id);
        if (members.Any())
        {
            await _memberRepo.RemoveRangeAsync(members);
            await _memberRepo.SaveAsync();
        }

        await _projectRepo.RemoveAsync(entity);
        await _projectRepo.SaveAsync();

        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<bool>> DeleteVendorAsync(DeleteVendorCommand request, CancellationToken cancellationToken = default)
    {
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

    public async Task<Response<List<ProjectDto>>> GetProjectsAsync(GetProjectsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<ProjectDto>>();
        try
        {
            var projects = await _projectRepo.GetAllAsync();
            var allMembers = await _memberRepo.GetAllAsync();
            var allTasks = await _taskRepo.GetAllAsync();
            var allUsers = await _userRepo.GetAllAsync();

            var dtoList = new List<ProjectDto>();

            foreach (var p in projects)
            {
                var owner = allUsers.FirstOrDefault(u => u.Id == p.OwnerId);
                var ownerName = owner != null ? owner.FullName : "Unknown";

                var projMembers = allMembers.Where(m => m.ProjectId == p.Id).ToList();
                var memberDtos = new List<ProjectMemberDto>();

                foreach (var m in projMembers)
                {
                    var u = allUsers.FirstOrDefault(usr => usr.Id == m.UserId);
                    if (u != null)
                    {
                        memberDtos.Add(new ProjectMemberDto(
                            u.Id,
                            u.FullName,
                            GetInitials(u.FullName),
                            GetColor(u.FullName)
                        ));
                    }
                }

                var projTasks = allTasks.Where(t => t.ProjectId == p.Id).ToList();
                var tasksTotal = projTasks.Count;
                var tasksDone = projTasks.Count(t => t.Status != null && 
                    (t.Status.ToLower() == "completed" || t.Status.ToLower() == "done" || t.Status.ToLower() == "resolved"));

                var progress = tasksTotal > 0 ? (tasksDone * 100) / tasksTotal : (p.Status?.ToLower() == "completed" ? 100 : 0);
                
                var priority = (Math.Abs(p.Id.GetHashCode()) % 3) switch
                {
                    0 => "low",
                    1 => "medium",
                    _ => "high"
                };

                dtoList.Add(new ProjectDto(
                    p.Id,
                    p.Name ?? string.Empty,
                    p.Description ?? string.Empty,
                    p.Status ?? "planning",
                    p.OwnerId,
                    ownerName,
                    p.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    p.Deadline?.ToString("yyyy-MM-dd") ?? string.Empty,
                    progress,
                    priority,
                    memberDtos,
                    tasksTotal,
                    tasksDone
                ));
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = dtoList;
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

    // ─── Visitors ─────────────────────────────────────────────────────────────

    public async Task<Response<Guid>> CreateVisitorAsync(CreateVisitorCommands request, CancellationToken cancellationToken = default)
    {
        var entity = new Visitor
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            VisitorCode = request.VisitorCode,
            HostId = request.HostId,
            ExpectedArrival = request.ExpectedArrival,
            Status = request.Status ?? "pending",
            RequestedById = request.RequestedById,
            CreatedAt = DateTime.UtcNow
        };
        await _visitorRepo.AddAsync(entity);
        await _visitorRepo.SaveAsync();
        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<bool>> UpdateVisitorStatusAsync(UpdateVisitorStatusCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _visitorRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Visitor not found.", Data = false };
        }
        entity.Status = request.Status;
        entity.LastModifiedAt = DateTime.UtcNow;
        await _visitorRepo.UpdateAsync(entity);
        await _visitorRepo.SaveAsync();
        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<bool>> DeleteVisitorAsync(DeleteVisitorCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _visitorRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Visitor not found.", Data = false };
        }
        await _visitorRepo.RemoveAsync(entity);
        await _visitorRepo.SaveAsync();
        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<List<Visitor>>> GetVisitorsAsync(GetVisitorsQueries request, CancellationToken cancellationToken = default)
    {
        var query = _visitorRepo.Query();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(v => v.Status == request.Status);
        }
        if (request.HostId.HasValue)
        {
            query = query.Where(v => v.HostId == request.HostId.Value);
        }
        if (request.ExpectedDateFrom.HasValue)
        {
            query = query.Where(v => v.ExpectedArrival >= request.ExpectedDateFrom.Value);
        }
        if (request.ExpectedDateTo.HasValue)
        {
            query = query.Where(v => v.ExpectedArrival <= request.ExpectedDateTo.Value);
        }

        var list = await query.ToListAsync(cancellationToken);
        return new Response<List<Visitor>> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = list };
    }

    // ─── Inventory Items ──────────────────────────────────────────────────────

    public async Task<Response<Guid>> UpdateInventoryItemAsync(UpdateInventoryItemCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _itemRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return new Response<Guid> { StatusCode = StatusCodes.Status404NotFound, Message = "Inventory item not found." };
        }

        entity.Name = request.Name;
        entity.Category = request.Category;
        entity.Description = request.Description;
        entity.SKU = request.SKU;
        entity.DepartmentId = request.DepartmentId;
        entity.AssignedUserId = request.AssignedUserId;
        entity.Location = request.Location;
        entity.Mode = request.Mode;
        entity.Quantity = request.Quantity;
        entity.SerialNumber = request.SerialNumber;
        entity.ImageUrl = request.ImageUrl;
        entity.Status = request.Status;
        entity.Note = request.Note;
        entity.UpdatedAt = DateTime.UtcNow;

        await _itemRepo.UpdateAsync(entity);
        await _itemRepo.SaveAsync();

        return new Response<Guid> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = entity.Id };
    }

    public async Task<Response<bool>> UpdateInventoryItemStatusAsync(UpdateInventoryItemStatusCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _itemRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Inventory item not found.", Data = false };
        }

        entity.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Note))
        {
            entity.Note = request.Note;
        }
        entity.UpdatedAt = DateTime.UtcNow;

        await _itemRepo.UpdateAsync(entity);
        await _itemRepo.SaveAsync();

        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    public async Task<Response<bool>> DeleteInventoryItemAsync(DeleteInventoryItemCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _itemRepo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return new Response<bool> { StatusCode = StatusCodes.Status404NotFound, Message = "Inventory item not found.", Data = false };
        }

        await _itemRepo.RemoveAsync(entity);
        await _itemRepo.SaveAsync();

        return new Response<bool> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = true };
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Length > 0 ? parts[0][0].ToString().ToUpper() : string.Empty;
        return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
    }

    private static readonly string[] MemberColors = { "#1a56db", "#184e35", "#be123c", "#b66a12", "#7c3aed", "#0891b2", "#059669", "#dc2626" };

    private static string GetColor(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return MemberColors[0];
        int hash = Math.Abs(name.GetHashCode());
        return MemberColors[hash % MemberColors.Length];
    }
}
