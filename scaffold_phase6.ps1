$featuresDir = "c:\Projects\taxombud\src\TaxOmbud.Application\Features"
$controllersDir = "c:\Projects\taxombud\src\TaxOmbud.API\Controllers"

# Announcements
New-Item -ItemType Directory -Force -Path "$featuresDir\System\Queries\GetAnnouncements"
Set-Content "$featuresDir\System\Queries\GetAnnouncements\GetAnnouncementsQuery.cs" -Value "using MediatR; using TaxOmbud.Application.Common.Models; using TaxOmbud.Domain.Entities.System; namespace TaxOmbud.Application.Features.System.Queries.GetAnnouncements; public record GetAnnouncementsQuery(bool UnreadOnly = false) : IRequest<Result<object>>;""

# Dashboard
New-Item -ItemType Directory -Force -Path "$featuresDir\System\Queries\GetDashboardWidgets"
Set-Content "$featuresDir\System\Queries\GetDashboardWidgets\GetDashboardWidgetsQuery.cs" -Value "using MediatR; using TaxOmbud.Application.Common.Models; namespace TaxOmbud.Application.Features.System.Queries.GetDashboardWidgets; public record GetDashboardWidgetsQuery() : IRequest<Result<object>>;""

New-Item -ItemType Directory -Force -Path "$featuresDir\System\Commands\SaveDashboardWidget"
Set-Content "$featuresDir\System\Commands\SaveDashboardWidget\SaveDashboardWidgetCommand.cs" -Value "using MediatR; using System; using TaxOmbud.Application.Common.Models; namespace TaxOmbud.Application.Features.System.Commands.SaveDashboardWidget; public record SaveDashboardWidgetCommand(Guid? Id, string Name, string Description, string ComponentName, string? RequiredPermission, bool IsActive) : IRequest<Result<Guid>>;""

# Mailbox
New-Item -ItemType Directory -Force -Path "$featuresDir\Communications\Queries\GetMailbox"
Set-Content "$featuresDir\Communications\Queries\GetMailbox\GetMailboxQuery.cs" -Value "using MediatR; using TaxOmbud.Application.Common.Models; namespace TaxOmbud.Application.Features.Communications.Queries.GetMailbox; public record GetMailboxQuery(string Folder) : IRequest<Result<object>>;""

New-Item -ItemType Directory -Force -Path "$featuresDir\Communications\Commands\SendMailboxMessage"
Set-Content "$featuresDir\Communications\Commands\SendMailboxMessage\SendMailboxMessageCommand.cs" -Value "using MediatR; using System; using System.Collections.Generic; using TaxOmbud.Application.Common.Models; namespace TaxOmbud.Application.Features.Communications.Commands.SendMailboxMessage; public record SendMailboxMessageCommand(string Subject, string BodyText, bool IsDraft, List<Guid> ToRecipients) : IRequest<Result<Guid>>;""
