using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Departments.DTOs;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class DepartmentsService : IDepartmentsService
{
    private readonly IApplicationDbContext _context;

    public DepartmentsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<CreateDepartmentResponse>> CreateDepartmentAsync(CreateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CreateDepartmentResponse>();
        try
        {
            if (await _context.Departments.AnyAsync(d => d.Name == request.Name, cancellationToken))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Department name already exists.";
                return response;
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                RoutingMode = request.RoutingMode.ToLowerInvariant() == "head" ? "head" : "members",
                Description = request.Description
            };

            if (request.HeadUserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.HeadUserId.Value, cancellationToken);
                if (!userExists)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Head user not found.";
                    return response;
                }

                department.HeadUserId = request.HeadUserId.Value;
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Department created successfully.";
            response.Data = new CreateDepartmentResponse(department.Id, department.Name, department.RoutingMode, department.Description);
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateDepartmentAsync(UpdateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (department == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Department not found.";
                return response;
            }

            if (request.HeadUserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.HeadUserId.Value, cancellationToken);
                if (!userExists)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Head user not found.";
                    return response;
                }

                department.HeadUserId = request.HeadUserId.Value;
            }
            else
            {
                department.HeadUserId = null;
            }

            department.Name = request.Name;
            department.RoutingMode = request.RoutingMode.ToLowerInvariant() == "head" ? "head" : "members";
            department.Description = request.Description;

            await _context.SaveChangesAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Department updated successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<DepartmentDto>> GetDepartmentByIdAsync(GetDepartmentByIdQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<DepartmentDto>();
        try
        {
            var department = await _context.Departments
                .AsNoTracking()
                .Include(d => d.HeadUser)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (department == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Department not found.";
                return response;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Department retrieved successfully.";
            response.Data = new DepartmentDto(
                department.Id,
                department.Name,
                department.RoutingMode,
                department.Description,
                department.HeadUser != null ? new HeadUserDto(department.HeadUser.Id, department.HeadUser.FullName, department.HeadUser.Email) : null
            );
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<IEnumerable<DepartmentDto>>> GetDepartmentsAsync(GetDepartmentsQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<IEnumerable<DepartmentDto>>();
        try
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.HeadUser)
                .Select(d => new DepartmentDto(
                    d.Id,
                    d.Name,
                    d.RoutingMode,
                    d.Description,
                    d.HeadUser != null ? new HeadUserDto(d.HeadUser.Id, d.HeadUser.FullName, d.HeadUser.Email) : null
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Departments retrieved successfully.";
            response.Data = departments;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}