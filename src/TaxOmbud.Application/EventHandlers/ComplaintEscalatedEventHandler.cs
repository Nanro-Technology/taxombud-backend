using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Events.Complaints;

namespace TaxOmbud.Application.EventHandlers;

public class ComplaintEscalatedEventHandler : INotificationHandler<ComplaintEscalatedEvent>
{
    private readonly IGenericRepository<ComplaintStatusHistory> _historyRepo;
    private readonly IGenericRepository<Notification>          _notificationRepo;
    private readonly IGenericRepository<Complaint>             _complaintRepo;
    private readonly IGenericRepository<User>                  _userRepo;

    public ComplaintEscalatedEventHandler(
        IGenericRepository<ComplaintStatusHistory> historyRepo,
        IGenericRepository<Notification>          notificationRepo,
        IGenericRepository<Complaint>             complaintRepo,
        IGenericRepository<User>                  userRepo)
    {
        _historyRepo      = historyRepo;
        _notificationRepo = notificationRepo;
        _complaintRepo    = complaintRepo;
        _userRepo         = userRepo;
    }

    public async Task Handle(ComplaintEscalatedEvent notification, CancellationToken cancellationToken)
    {
        // -- 1. Write accurate status history ----------------------------------
        var history = new ComplaintStatusHistory
        {
            Id              = Guid.NewGuid(),
            ComplaintId     = notification.ComplaintId,
            OldStatus       = notification.PreviousStatus,         // ? fixed — was hardcoded
            NewStatus       = ComplaintStatus.UnderInvestigation,
            ChangedByUserId = notification.EscalatedByUserId,
            TransitionedAt  = notification.OccurredAt,
            Reason          = notification.Reason
        };
        await _historyRepo.AddAsync(history);

        // -- 2. Load complaint with assigned officer ----------------------------
        var complaint = await _complaintRepo.Query()
            .Include(c => c.AssignedOfficer).ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(c => c.Id == notification.ComplaintId, cancellationToken);

        if (complaint is null) return;

        var notifications = new List<Notification>();
        var title   = $"Complaint Escalated: {complaint.ReferenceNumber}";
        var message = $"Complaint {complaint.ReferenceNumber} ('{complaint.Subject}') " +
                      $"has been escalated to Investigation. Reason: {notification.Reason}";

        // -- 3. Notify the assigned officer (if any) ----------------------------
        if (complaint.AssignedOfficer is not null)
            notifications.Add(new Notification
            {
                Id      = Guid.NewGuid(),
                UserId  = complaint.AssignedOfficer.UserId,
                Title   = title,
                Message = message
            });

        // -- 4. Notify all Super Admin + Admin users via custom Role FK ---------
        // User.RoleId ? Role.Name — no ASP.NET Identity UserManager used.
        var adminUserIds = await _userRepo.Query()
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted
                     && u.Role != null
                     && (u.Role.Name == RoleConstants.SuperAdmin
                         || u.Role.Name == RoleConstants.Admin)
                     && u.Id != notification.EscalatedByUserId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var adminId in adminUserIds)
            notifications.Add(new Notification
            {
                Id      = Guid.NewGuid(),
                UserId  = adminId,
                Title   = title,
                Message = message
            });

        if (notifications.Any())
            await _notificationRepo.AddRangeAsync(notifications);
    }
}