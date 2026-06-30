using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaxOmbud.Application.Communications.DTOs;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Services;

public class CommunicationsService : ICommunicationsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CommunicationsService(
        IApplicationDbContext context,
        ICurrentUser currentUser
    )
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Response<object?>> AcknowledgeCommunicationAsync(AcknowledgeCommunicationCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var communication = await _context.CommunicationLogs
                .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

            if (communication == null)
            {
                throw new NotFoundException(nameof(Communication), request.CommunicationId);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Guid> CreateAgentChatAsync(CreateAgentChatCommand request, CancellationToken cancellationToken = default)
{
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();

        var participants = request.ParticipantIds.ToList();
        if (!participants.Contains(_currentUser.UserId.Value))
        {
            participants.Add(_currentUser.UserId.Value);
        }

        // Check if a direct message chat already exists between these 2 users
        if (!request.IsGroupChat && participants.Count == 2)
        {
            var p1 = participants[0].ToString();
            var p2 = participants[1].ToString();

            var existingChat = await _context.AgentChats
                .Where(c => !c.IsGroupChat && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            var chat = existingChat.FirstOrDefault(c => 
                c.ParticipantIds.Contains(p1) && 
                c.ParticipantIds.Contains(p2) &&
                JsonSerializer.Deserialize<List<string>>(c.ParticipantIds)?.Count == 2);

            if (chat != null)
            {
                return chat.Id;
            }
        }

        var newChat = new AgentChat
        {
            Id = Guid.NewGuid(),
            Topic = request.Topic,
            IsGroupChat = request.IsGroupChat,
            ParticipantIds = JsonSerializer.Serialize(participants.Select(p => p.ToString())),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        _context.AgentChats.Add(newChat);
        await _context.SaveChangesAsync(cancellationToken);

        return newChat.Id;
    }

    public async Task<Guid> CreateSmsMessageAsync(CreateSmsMessageCommand request, CancellationToken cancellationToken = default)
{
        var entity = new SmsMessage
        {
            Provider = request.Provider ?? string.Empty,
            SenderId = request.SenderId,
            Body = request.Body,
            ScheduledAt = request.ScheduledAt,
            RecipientType = request.RecipientType ?? string.Empty,
            PhoneNumbers = request.PhoneNumbers,
            Mode = request.Mode ?? string.Empty,
            Direction = "Outbound",
            Status = "Pending"
        };

        _context.SmsMessages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<DeleteSmsMessageCommand> DeleteSmsMessageAsync(DeleteSmsMessageCommand request, CancellationToken cancellationToken = default)
{
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        _context.SmsMessages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<Response<LoggedCommunicationResponse>> LogCommunicationAsync(LogCommunicationCommand request, CancellationToken cancellationToken = default)
    {
        var methodResponse = new Response<LoggedCommunicationResponse>();
        try
        {
            var actorUserId = _currentUser.UserId ?? Guid.Empty;

            var log = new CommunicationLog
            {
                Id = Guid.NewGuid(),
                Channel = request.Channel,
                Subject = request.Subject,
                Body = request.Body,
                Recipient = request.Recipient,
                RecipientName = request.RecipientName,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                Direction = CommunicationDirection.Outbound,
                IsSent = true,
                SentAt = DateTimeOffset.UtcNow,
                SentByUserId = actorUserId
            };

            _context.CommunicationLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            var data = new LoggedCommunicationResponse(log.Id, log.Channel, log.Subject, log.Recipient, log.IsSent, log.SentAt);
            methodResponse.StatusCode = StatusCodes.Status200OK;
            methodResponse.Message = "Success";
            methodResponse.Data = data;
        }
        catch (Exception ex)
        {
            methodResponse.StatusCode = StatusCodes.Status500InternalServerError;
            methodResponse.Message = ex.Message;
        }
        return methodResponse;
    }

    public async Task<Response<RenderedTemplateDto>> RenderCommunicationTemplateAsync(RenderCommunicationTemplateCommand request, CancellationToken cancellationToken = default)
{
        var template = await _context.CommunicationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
            throw new NotFoundException(nameof(Domain.Entities.Communications.CommunicationTemplate), request.TemplateId);

        var subject = template.SubjectTemplate;
        var body = template.BodyTemplate;

        if (request.Payload != null)
        {
            foreach (var kvp in request.Payload)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}"; // e.g. {{Name}}
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
        }

        return new Response<RenderedTemplateDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = new RenderedTemplateDto(subject, body) };
    }

    public async Task<Guid> SendAgentChatMessageAsync(SendAgentChatMessageCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();
 
        var chat = await _context.AgentChats
            .FirstOrDefaultAsync(c => c.Id == request.ChatId && !c.IsDeleted, cancellationToken);
 
        if (chat == null) throw new ArgumentException("Chat not found");
 
        var message = new AgentChatMessage
        {
            Id = Guid.NewGuid(),
            AgentChatId = request.ChatId,
            SenderId = _currentUser.UserId.Value,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = _currentUser.UserId
        };
 
        _context.AgentChatMessages.Add(message);
        
        chat.UpdatedAt = DateTimeOffset.UtcNow;
 
        await _context.SaveChangesAsync(cancellationToken);
 
        return message.Id;
    }

    public async Task<Response<object?>> SendCommunicationAsync(SendCommunicationCommand request, CancellationToken cancellationToken = default)
{
        var response = new Response<object?>();
        var communication = await _context.CommunicationLogs
            .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

        if (communication == null)
            throw new NotFoundException(nameof(Domain.Entities.Communications.Communication), request.CommunicationId);

        if (communication.IsSent)
            return new Response<object?> { StatusCode = StatusCodes.Status400BadRequest, Message = "Communication has already been sent." };
        try
        {

        // In a real application, we would integrate with an email/SMS provider here.
        // For now, we just mark it as sent.
        communication.IsSent = true;
        communication.SentAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new Response<object?> { StatusCode = StatusCodes.Status200OK, Message = "Success" };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<object?> UpdateAgentChatPreferencesAsync(UpdateAgentChatPreferencesCommand request, CancellationToken cancellationToken = default)
{
        if (_currentUser.UserId == null) throw new UnauthorizedAccessException();

        var prefs = await _context.AgentChatPreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value, cancellationToken);

        if (prefs == null)
        {
            prefs = new AgentChatPreference
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId.Value,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = _currentUser.UserId
            };
            _context.AgentChatPreferences.Add(prefs);
        }

        prefs.DoNotDisturb = request.DoNotDisturb;
        prefs.MarkAsAway = request.MarkAsAway;
        prefs.PlayNotificationSound = request.PlayNotificationSound;
        prefs.ShowBrowserNotifications = request.ShowBrowserNotifications;
        prefs.UpdatedAt = DateTimeOffset.UtcNow;
        prefs.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return null;
    }

    public async Task<UpdateSmsMessageCommand> UpdateSmsMessageAsync(UpdateSmsMessageCommand request, CancellationToken cancellationToken = default)
{
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<AgentChatPreferenceDto> GetAgentChatPreferencesAsync(GetAgentChatPreferencesQuery request, CancellationToken cancellationToken = default)
{
        if (_currentUser.UserId == null) return new AgentChatPreferenceDto();

        var prefs = await _context.AgentChatPreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value, cancellationToken);

        if (prefs == null)
        {
            // Return defaults
            return new AgentChatPreferenceDto
            {
                UserId = _currentUser.UserId.Value,
                DoNotDisturb = false,
                MarkAsAway = false,
                PlayNotificationSound = true,
                ShowBrowserNotifications = true
            };
        }

        return new AgentChatPreferenceDto
        {
            UserId = prefs.UserId,
            DoNotDisturb = prefs.DoNotDisturb,
            MarkAsAway = prefs.MarkAsAway,
            PlayNotificationSound = prefs.PlayNotificationSound,
            ShowBrowserNotifications = prefs.ShowBrowserNotifications
        };
    }

    public async Task<List<AgentChatDto>> GetAgentChatsAsync(GetAgentChatsQuery request, CancellationToken cancellationToken = default)
{
        if (_currentUser.UserId == null) return new List<AgentChatDto>();
        var userIdString = _currentUser.UserId.Value.ToString();

        // Find chats where the user is a participant
        var chats = await _context.AgentChats
            .Where(c => c.ParticipantIds.Contains(userIdString) && !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<AgentChatDto>();

        foreach (var chat in chats)
        {
            var pIds = JsonSerializer.Deserialize<List<string>>(chat.ParticipantIds) ?? new List<string>();
            var pGuids = pIds.Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty).Where(g => g != Guid.Empty).ToList();

            var participants = await _context.Users
                .Where(u => pGuids.Contains(u.Id))
                .Select(u => new AgentSummaryDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync(cancellationToken);

            result.Add(new AgentChatDto
            {
                Id = chat.Id,
                Topic = chat.Topic ?? string.Empty,
                IsGroupChat = chat.IsGroupChat,
                Participants = participants,
                CreatedAt = chat.CreatedAt,
                UpdatedAt = chat.UpdatedAt
            });
        }

        return result;
    }

    public async Task<List<AgentChatMessageDto>> GetChatMessagesAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default)
{
        var messages = await _context.AgentChatMessages
            .Where(m => m.AgentChatId == request.ChatId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // Fetch user names for senders
        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, cancellationToken);

        return messages.Select(m => new AgentChatMessageDto
        {
            Id = m.Id,
            AgentChatId = m.AgentChatId,
            SenderId = m.SenderId,
            SenderName = users.GetValueOrDefault(m.SenderId) ?? "Unknown",
            Content = m.Content,
            IsPinned = m.IsPinned,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }

    public async Task<Response<CommunicationDetailDto>> GetCommunicationByIdAsync(GetCommunicationByIdQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<CommunicationDetailDto>();
        var log = await _context.CommunicationLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (log == null)
            return new Response<CommunicationDetailDto> { StatusCode = StatusCodes.Status404NotFound, Message = "Communication log not found." };
        try
        {

        var dto = new CommunicationDetailDto(
            log.Id,
            log.Channel,
            log.Direction.ToString(),
            log.Subject,
            log.Body,
            log.Recipient,
            log.RecipientName,
            log.RelatedEntityId,
            log.RelatedEntityType,
            log.IsSent,
            log.SentAt,
            log.ErrorMessage,
            log.SentByUserId,
            log.CreatedAt
        );

        return new Response<CommunicationDetailDto> { StatusCode = StatusCodes.Status200OK, Message = "Success", Data = dto };
    
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }}

    public async Task<Response<PagedResult<CommunicationListDto>>> GetCommunicationsAsync(GetCommunicationsQuery request, CancellationToken cancellationToken = default)
{
        var response = new Response<PagedResult<CommunicationListDto>>();
        var query = _context.CommunicationLogs.AsNoTracking().AsQueryable();

        if (request.RelatedEntityId.HasValue)
            query = query.Where(c => c.RelatedEntityId == request.RelatedEntityId.Value);

        if (!string.IsNullOrWhiteSpace(request.RelatedEntityType))
            query = query.Where(c => c.RelatedEntityType == request.RelatedEntityType);

        if (!string.IsNullOrWhiteSpace(request.Channel))
            query = query.Where(c => c.Channel.ToLower() == request.Channel.ToLower());

        if (!string.IsNullOrWhiteSpace(request.Direction) && Enum.TryParse<CommunicationDirection>(request.Direction, true, out var dir))
            query = query.Where(c => c.Direction == dir);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CommunicationListDto(
                c.Id,
                c.Channel,
                c.Direction.ToString(),
                c.Subject,
                c.Recipient,
                c.RecipientName,
                c.RelatedEntityId,
                c.RelatedEntityType,
                c.IsSent,
                c.SentAt,
                c.ErrorMessage,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<CommunicationListDto>(items.AsReadOnly(), total, request.Page, request.PageSize);
        response.StatusCode = StatusCodes.Status200OK;
        response.Message = "Success";
        response.Data = pagedResult;
        return response;
    }

    public async Task<Response<List<CommunicationTemplateDto>>> GetCommunicationTemplatesAsync(GetCommunicationTemplatesQuery request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<CommunicationTemplateDto>>();
        try
        {
            var templates = await _context.CommunicationTemplates
                .AsNoTracking()
                .Select(t => new CommunicationTemplateDto(
                    t.Id,
                    t.Name,
                    t.Channel,
                    t.SubjectTemplate,
                    t.BodyTemplate
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Success";
            response.Data = templates;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<SmsMessageDto> GetSmsMessageByIdAsync(GetSmsMessageByIdQuery request, CancellationToken cancellationToken = default)
{
        var entity = await _context.SmsMessages.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SmsMessage), request.Id);
        }

        return new SmsMessageDto
        {
            Id = entity.Id,
            Provider = entity.Provider,
            SenderId = entity.SenderId,
            Body = entity.Body,
            ScheduledAt = entity.ScheduledAt,
            RecipientType = entity.RecipientType,
            PhoneNumbers = entity.PhoneNumbers,
            Mode = entity.Mode,
            Status = entity.Status,
            Direction = entity.Direction,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public async Task<List<SmsMessageDto>> GetSmsMessagesAsync(GetSmsMessagesQuery request, CancellationToken cancellationToken = default)
{
        return await _context.SmsMessages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SmsMessageDto
            {
                Id = x.Id,
                Provider = x.Provider,
                SenderId = x.SenderId,
                Body = x.Body,
                ScheduledAt = x.ScheduledAt,
                RecipientType = x.RecipientType,
                PhoneNumbers = x.PhoneNumbers,
                Mode = x.Mode,
                Status = x.Status,
                Direction = x.Direction,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AgentSummaryDto>> SearchAgentsAsync(SearchAgentsQuery request, CancellationToken cancellationToken = default)
{
        var term = request.SearchTerm?.ToLower() ?? "";

        // We fetch from Users and conditionally join StaffProfiles
        var agents = await _context.Users
            .Where(u => !u.IsDeleted && 
                        (string.IsNullOrEmpty(term) || 
                         u.FirstName.ToLower().Contains(term) || 
                         u.LastName.ToLower().Contains(term) || 
                         u.Email.ToLower().Contains(term)))
            .Select(u => new AgentSummaryDto
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Role = _context.StaffProfiles.Where(sp => sp.UserId == u.Id && !sp.IsDeleted).Select(sp => sp.Title).FirstOrDefault()
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        return agents;
    }

}