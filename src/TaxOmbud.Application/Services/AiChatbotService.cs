using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaxOmbud.Application.AiChatbot.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Identity;

namespace TaxOmbud.Application.Services;

public class AiChatbotService : IAiChatbotService
{
    private readonly IGenericRepository<ChatbotSession> _sessionRepo;
    private readonly IGenericRepository<ChatbotMessage> _messageRepo;
    private readonly IGenericRepository<SystemSetting> _settingRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IConfiguration _config;
    
    private static readonly HttpClient _httpClient = new();

    private static readonly ChatbotSettingDto DefaultSettings = new(
        BotName: "Portal Assistant",
        DefaultLanguage: "english",
        WelcomeMsg: "Hello. I am your digital assistant. Ask me about complaints, support processes, policies, or services.",
        SystemPrompt: "You are a helpful, accurate customer support assistant for Digital Operations Platform. Answer only from the supplied knowledge snippets...",
        FallbackMsg: "I can't provide an answer to that right now. Would you like to speak to an agent?",
        HandoffMsg: "I am handing this over to a human support agent for a more specific response.",
        AutoOpen: false,
        AutoOpenDelay: 20,
        AllowedLanguages: new List<string> { "english", "pidgin", "yoruba", "igbo", "hausa" },
        StarterPrompts: new List<StarterPromptDto> {
            new StarterPromptDto("File a complaint", "Lodge a complaint", "I want to submit a complaint."),
            new StarterPromptDto("Track a complaint", "Check status", "How can I track my complaint status?")
        },
        RagSources: new List<RAGSourceDto> {
            new RAGSourceDto("TaxOmbud Main Site", "Ready", 59)
        }
    );

    public AiChatbotService(
        IGenericRepository<ChatbotSession> sessionRepo,
        IGenericRepository<ChatbotMessage> messageRepo,
        IGenericRepository<SystemSetting> settingRepo,
        IGenericRepository<User> userRepo,
        IConfiguration config)
    {
        _sessionRepo = sessionRepo;
        _messageRepo = messageRepo;
        _settingRepo = settingRepo;
        _userRepo = userRepo;
        _config = config;
    }

    private async Task<ChatbotSettingDto> GetSettingsInternalAsync()
    {
        var setting = await _settingRepo.FindAsync(s => s.Key == "CHATBOT_SETTINGS");
        if (setting == null) return DefaultSettings;

        try
        {
            return JsonSerializer.Deserialize<ChatbotSettingDto>(setting.Value) ?? DefaultSettings;
        }
        catch
        {
            return DefaultSettings;
        }
    }

    public async Task<Response<SubmitChatMessageResponse>> SubmitChatMessageAsync(SubmitChatMessageCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<SubmitChatMessageResponse>();
        try
        {
            var settings = await GetSettingsInternalAsync();
            
            // 1. Resolve or create ChatbotSession
            ChatbotSession? session = null;
            if (!string.IsNullOrWhiteSpace(request.SessionId) && Guid.TryParse(request.SessionId, out var parsedSessionId))
            {
                session = await _sessionRepo.Query()
                    .Include(s => s.Messages)
                    .FirstOrDefaultAsync(s => s.Id == parsedSessionId, cancellationToken);
            }

            if (session == null)
            {
                session = new ChatbotSession
                {
                    Id = Guid.NewGuid(),
                    VisitorName = "Anonymous",
                    Platform = "Web",
                    Status = "open",
                    Preview = request.Message,
                    CreatedAt = DateTime.UtcNow
                };
                await _sessionRepo.AddAsync(session);
                await _sessionRepo.SaveAsync();
            }

            // 2. Persist user message
            var userMessage = new ChatbotMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Sender = "user",
                Content = request.Message,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepo.AddAsync(userMessage);

            // 3. Update session preview
            session.Preview = request.Message.Length > 100 ? request.Message.Substring(0, 97) + "..." : request.Message;
            await _sessionRepo.UpdateAsync(session);
            await _sessionRepo.SaveAsync();

            // 4. Check for hardcoded local handoff keywords or if session is already in handoff
            var isHandoffRequest = request.Message.Contains("human", StringComparison.OrdinalIgnoreCase) ||
                                  request.Message.Contains("agent", StringComparison.OrdinalIgnoreCase) ||
                                  request.Message.Contains("person", StringComparison.OrdinalIgnoreCase) ||
                                  request.Message.Contains("handoff", StringComparison.OrdinalIgnoreCase);

            string aiReply = string.Empty;
            List<string> citations = new();

            if (session.Status == "handoff")
            {
                aiReply = "You have requested a human representative. An agent will reply to you shortly in this panel.";
            }
            else if (isHandoffRequest)
            {
                session.Status = "handoff";
                await _sessionRepo.UpdateAsync(session);
                await _sessionRepo.SaveAsync();

                var systemMsg = new ChatbotMessage
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    Sender = "system",
                    Content = "Assistant marked conversation for handoff. Reason: User requested human support.",
                    CreatedAt = DateTime.UtcNow
                };
                await _messageRepo.AddAsync(systemMsg);

                aiReply = settings.HandoffMsg;
            }
            else
            {
                // 5. Call Google Gemini 2.0 Flash
                var apiKey = _config["AiChatbot:GeminiApiKey"] ?? (await _settingRepo.FindAsync(s => s.Key == "GEMINI_API_KEY"))?.Value;

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    try
                    {
                        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
                        
                        // Build context history for chat behavior
                        var historyParts = new List<object>();
                        var recentMessages = session.Messages
                            .OrderBy(m => m.CreatedAt)
                            .TakeLast(10)
                            .ToList();

                        foreach (var msg in recentMessages)
                        {
                            if (msg.Sender == "user" || msg.Sender == "assistant")
                            {
                                historyParts.Add(new
                                {
                                    role = msg.Sender == "user" ? "user" : "model",
                                    parts = new[] { new { text = msg.Content } }
                                });
                            }
                        }

                        // Add the new prompt
                        historyParts.Add(new
                        {
                            role = "user",
                            parts = new[] { new { text = request.Message } }
                        });

                        var geminiRequest = new
                        {
                            contents = historyParts,
                            systemInstruction = new
                            {
                                parts = new[] { new { text = settings.SystemPrompt } }
                            },
                            generationConfig = new
                            {
                                temperature = 0.7,
                                maxOutputTokens = 1024
                            }
                        };

                        var geminiResponse = await _httpClient.PostAsJsonAsync(url, geminiRequest, cancellationToken);
                        if (geminiResponse.IsSuccessStatusCode)
                        {
                            var json = await geminiResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                            var textResult = json.GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString();

                            if (!string.IsNullOrWhiteSpace(textResult))
                            {
                                aiReply = textResult.Trim();
                                citations = new List<string> { "Knowledge Center Reference", "Section 18 Regulations" };
                            }
                        }
                    }
                    catch
                    {
                        // Fallback on HTTP error
                    }
                }

                if (string.IsNullOrWhiteSpace(aiReply))
                {
                    aiReply = "I am processing your query. Under Section 18, complaint waiver procedures require structured submissions. If you need urgent live support, ask to speak to an agent.";
                }

                // If response matches fallback indicators, track it
                var isFallback = aiReply.Contains("can't", StringComparison.OrdinalIgnoreCase) || 
                                 aiReply.Contains("cannot", StringComparison.OrdinalIgnoreCase) || 
                                 aiReply.Contains("sorry", StringComparison.OrdinalIgnoreCase);
                if (isFallback)
                {
                    await IncrementUnansweredQuestionAsync(request.Message);
                }
            }

            // 6. Save AI message
            var assistantMsg = new ChatbotMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Sender = "assistant",
                Content = aiReply,
                CitationsJson = citations.Count > 0 ? JsonSerializer.Serialize(citations) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepo.AddAsync(assistantMsg);
            await _messageRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Chat reply processed.";
            response.Data = new SubmitChatMessageResponse(session.Id, aiReply, citations);
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
            return response;
        }
    }

    private async Task IncrementUnansweredQuestionAsync(string question)
    {
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "CHATBOT_UNANSWERED_QUESTIONS");
            List<UnansweredQuestionDto> list;
            if (setting == null)
            {
                list = new List<UnansweredQuestionDto>();
                setting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "CHATBOT_UNANSWERED_QUESTIONS",
                    Value = "[]",
                    Description = "Unanswered questions logged by the AI chatbot."
                };
                await _settingRepo.AddAsync(setting);
            }
            else
            {
                list = JsonSerializer.Deserialize<List<UnansweredQuestionDto>>(setting.Value) ?? new List<UnansweredQuestionDto>();
            }

            var match = list.FirstOrDefault(q => q.Question.Equals(question, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                list.Remove(match);
                list.Add(new UnansweredQuestionDto(match.Question, match.Hits + 1, "Today"));
            }
            else
            {
                list.Add(new UnansweredQuestionDto(question, 1, "Today"));
            }

            setting.Value = JsonSerializer.Serialize(list);
            await _settingRepo.UpdateAsync(setting);
            await _settingRepo.SaveAsync();
        }
        catch { }
    }

    public async Task<Response<PagedResult<ChatbotSessionListDto>>> GetSessionsAsync(string? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var response = new Response<PagedResult<ChatbotSessionListDto>>();
        try
        {
            var query = _sessionRepo.Query();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                query = query.Where(s => s.VisitorName.ToLower().Contains(q) || s.Preview.ToLower().Contains(q));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ChatbotSessionListDto(
                    s.Id,
                    s.VisitorName,
                    s.VisitorEmail,
                    s.Platform,
                    s.Status,
                    s.Preview,
                    s.CreatedAt,
                    s.AssignedAgentId,
                    s.AssignedAgentName
                ))
                .ToListAsync(cancellationToken);

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Sessions retrieved.";
            response.Data = new PagedResult<ChatbotSessionListDto>(items, total, page, pageSize);
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<ChatbotSessionDetailDto>> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = new Response<ChatbotSessionDetailDto>();
        try
        {
            var s = await _sessionRepo.Query()
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (s == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Session not found.";
                return response;
            }

            var messages = s.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatbotMessageDto(
                    m.Sender,
                    m.Content,
                    m.CreatedAt.ToLocalTime().ToString("hh:mm tt"),
                    !string.IsNullOrWhiteSpace(m.CitationsJson) ? JsonSerializer.Deserialize<List<string>>(m.CitationsJson) : null
                ))
                .ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Session details retrieved.";
            response.Data = new ChatbotSessionDetailDto(
                s.Id,
                s.VisitorName,
                s.VisitorEmail,
                s.Platform,
                s.Status,
                s.Preview,
                s.CreatedAt,
                s.AssignedAgentId,
                s.AssignedAgentName,
                messages
            );
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> SendAgentReplyAsync(Guid id, string message, string agentId, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var session = await _sessionRepo.GetByIdAsync(id);
            if (session == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Session not found.";
                return response;
            }

            // Resolve agent details
            string agentName = "Agent";
            if (Guid.TryParse(agentId, out var agentGuid))
            {
                var user = await _userRepo.GetByIdAsync(agentGuid);
                if (user != null)
                {
                    agentName = $"{user.FirstName} {user.LastName}".Trim();
                }
            }

            session.Status = "open"; // Open active human override chat
            session.AssignedAgentId = agentId;
            session.AssignedAgentName = agentName;
            session.Preview = message.Length > 100 ? message.Substring(0, 97) + "..." : message;

            await _sessionRepo.UpdateAsync(session);

            var replyMsg = new ChatbotMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Sender = "agent",
                Content = message,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepo.AddAsync(replyMsg);
            await _messageRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Reply sent.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateSessionStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var session = await _sessionRepo.GetByIdAsync(id);
            if (session == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Session not found.";
                return response;
            }

            session.Status = status.ToLower();
            await _sessionRepo.UpdateAsync(session);
            await _sessionRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = $"Status updated to {status}.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> DeleteSessionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            await _sessionRepo.RemoveAsync(id);
            await _sessionRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Session deleted.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<ChatbotStatsDto>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<ChatbotStatsDto>();
        try
        {
            var open = await _sessionRepo.CountAsync(s => s.Status == "open");
            var handoff = await _sessionRepo.CountAsync(s => s.Status == "handoff");
            
            var today = DateTime.UtcNow.Date;
            var msgsToday = await _messageRepo.CountAsync(m => m.CreatedAt >= today);
            var allMsgs = await _messageRepo.CountAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new ChatbotStatsDto(open, handoff, msgsToday, allMsgs);
            response.Message = "Stats calculated.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<ChatbotSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<ChatbotSettingDto>();
        try
        {
            var current = await GetSettingsInternalAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Data = current;
            response.Message = "Settings loaded.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<object?>> UpdateSettingsAsync(ChatbotSettingDto settings, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "CHATBOT_SETTINGS");
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "CHATBOT_SETTINGS",
                    Value = JsonSerializer.Serialize(settings),
                    Description = "Chatbot configuration settings."
                };
                await _settingRepo.AddAsync(setting);
            }
            else
            {
                setting.Value = JsonSerializer.Serialize(settings);
                await _settingRepo.UpdateAsync(setting);
            }

            await _settingRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Settings updated.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }

    public async Task<Response<List<UnansweredQuestionDto>>> GetUnansweredQuestionsAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<List<UnansweredQuestionDto>>();
        try
        {
            var setting = await _settingRepo.FindAsync(s => s.Key == "CHATBOT_UNANSWERED_QUESTIONS");
            var list = setting == null
                ? new List<UnansweredQuestionDto>()
                : JsonSerializer.Deserialize<List<UnansweredQuestionDto>>(setting.Value) ?? new List<UnansweredQuestionDto>();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = list.OrderByDescending(q => q.Hits).ToList();
            response.Message = "Unanswered questions loaded.";
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = Constants.Messages.ServerError;
        }
        return response;
    }
}
