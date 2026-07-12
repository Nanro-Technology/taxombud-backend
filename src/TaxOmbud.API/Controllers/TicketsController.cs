using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Domain.Entities.Operations;
using TaxOmbud.Persistence.Data;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/tickets")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public TicketsController(ApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("sent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSentTickets(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var tickets = await _context.Tickets
            .Where(t => t.SenderId == currentUserId.Value && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var users = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
        var depts = await _context.Departments.ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        var result = tickets.Select(t => {
            string type = "Service Request";
            string desc = t.Description ?? "";
            if (desc.StartsWith("Type: "))
            {
                var parts = desc.Split(new[] { "\n\n" }, 2, System.StringSplitOptions.None);
                type = parts[0].Replace("Type: ", "").Trim();
                desc = parts.Length > 1 ? parts[1] : "";
            }

            return new
            {
                Id = t.Id.ToString(),
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Description = desc,
                Type = type,
                Status = t.Status.ToLower(),
                Priority = t.Priority.ToLower(),
                CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd"),
                SenderName = users.TryGetValue(t.SenderId, out var sn) ? sn : "Unknown",
                SenderDept = t.SenderDepartmentId.HasValue && depts.TryGetValue(t.SenderDepartmentId.Value, out var sd) ? sd : "Unknown",
                AssignedName = t.DestinationUserId.HasValue && users.TryGetValue(t.DestinationUserId.Value, out var an) ? an : "Unassigned",
                AssignedDept = t.AssignedDepartmentId.HasValue && depts.TryGetValue(t.AssignedDepartmentId.Value, out var ad) ? ad : "Unassigned",
                DestinationTarget = t.AssignedDepartmentId.HasValue && depts.TryGetValue(t.AssignedDepartmentId.Value, out var dt) ? dt : "Unassigned"
            };
        });

        return Ok(new { StatusCode = 200, Message = "Sent tickets retrieved successfully.", Data = result });
    }

    [HttpGet("received")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceivedTickets(CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var currentUser = await _context.Users.FindAsync(new object[] { currentUserId.Value }, ct);
        var deptId = currentUser?.DepartmentId;

        var ticketsQuery = _context.Tickets.Where(t => !t.IsDeleted);

        if (deptId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.DestinationUserId == currentUserId.Value || t.AssignedDepartmentId == deptId.Value);
        }
        else
        {
            ticketsQuery = ticketsQuery.Where(t => t.DestinationUserId == currentUserId.Value);
        }

        var tickets = await ticketsQuery.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

        var users = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
        var depts = await _context.Departments.ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        var result = tickets.Select(t => {
            string type = "Service Request";
            string desc = t.Description ?? "";
            if (desc.StartsWith("Type: "))
            {
                var parts = desc.Split(new[] { "\n\n" }, 2, System.StringSplitOptions.None);
                type = parts[0].Replace("Type: ", "").Trim();
                desc = parts.Length > 1 ? parts[1] : "";
            }

            return new
            {
                Id = t.Id.ToString(),
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Description = desc,
                Type = type,
                Status = t.Status.ToLower(),
                Priority = t.Priority.ToLower(),
                CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd"),
                SenderName = users.TryGetValue(t.SenderId, out var sn) ? sn : "Unknown",
                SenderDept = t.SenderDepartmentId.HasValue && depts.TryGetValue(t.SenderDepartmentId.Value, out var sd) ? sd : "Unknown",
                AssignedName = t.DestinationUserId.HasValue && users.TryGetValue(t.DestinationUserId.Value, out var an) ? an : "Unassigned",
                AssignedDept = t.AssignedDepartmentId.HasValue && depts.TryGetValue(t.AssignedDepartmentId.Value, out var ad) ? ad : "Unassigned",
                DestinationTarget = t.AssignedDepartmentId.HasValue && depts.TryGetValue(t.AssignedDepartmentId.Value, out var dt) ? dt : "Unassigned"
            };
        });

        return Ok(new { StatusCode = 200, Message = "Received tickets retrieved successfully.", Data = result });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == null) return Unauthorized();

        var currentUser = await _context.Users.FindAsync(new object[] { currentUserId.Value }, ct);
        var senderDeptId = currentUser?.DepartmentId;

        var ticketCount = await _context.Tickets.CountAsync(ct);
        var ticketNumber = $"TCK-{1001 + ticketCount}";

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            Subject = request.Subject,
            Description = $"Type: {request.Type}\n\n{request.Description}",
            SenderId = currentUserId.Value,
            SenderDepartmentId = senderDeptId,
            AssignedDepartmentId = request.AssignedDepartmentId,
            DestinationUserId = request.DestinationUserId,
            Status = request.Status ?? "new",
            Priority = request.Priority ?? "medium",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Tickets.AddAsync(ticket, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(new { StatusCode = 200, Message = "Ticket created successfully.", Data = ticket.Id });
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusRequest request, CancellationToken ct)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { id }, ct);
        if (ticket == null || ticket.IsDeleted) return NotFound();

        ticket.Status = request.Status;
        ticket.LastModifiedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Ticket status updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTicket(Guid id, CancellationToken ct)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { id }, ct);
        if (ticket == null || ticket.IsDeleted) return NotFound();

        ticket.IsDeleted = true;
        ticket.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Ok(new { StatusCode = 200, Message = "Ticket deleted successfully." });
    }
}

public class CreateTicketRequest
{
    public string Subject { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public Guid? AssignedDepartmentId { get; set; }
    public Guid? DestinationUserId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}

public class UpdateTicketStatusRequest
{
    public string Status { get; set; } = null!;
}
