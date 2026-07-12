using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.SecuredFiling.DTOs;
using TaxOmbud.Domain.Entities.SecuredFiling;
using TaxOmbud.Domain.Entities.System;

namespace TaxOmbud.Application.Services;

public class SecuredFilingService : ISecuredFilingService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly ICurrentUser _currentUser;

    public SecuredFilingService(
        IApplicationDbContext context,
        IFileStorageService storageService,
        ICurrentUser currentUser)
    {
        _context = context;
        _storageService = storageService;
        _currentUser = currentUser;
    }

    public async Task<List<FilingFolderDto>> GetFoldersAsync(string? query, CancellationToken ct = default)
    {
        var dbQuery = _context.FilingFolders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lq = query.ToLower();
            dbQuery = dbQuery.Where(f => f.Name.ToLower().Contains(lq) || 
                                         f.FolderCode.ToLower().Contains(lq) || 
                                         f.Description.ToLower().Contains(lq));
        }

        var folders = await dbQuery.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);
        
        // Seed if empty
        if (!folders.Any() && string.IsNullOrWhiteSpace(query))
        {
            await SeedInitialDataAsync(ct);
            folders = await _context.FilingFolders.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);
        }

        return folders.Select(f => MapToFolderDto(f)).ToList();
    }

    public async Task<FilingFolderDto?> GetFolderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var folder = await _context.FilingFolders
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        return folder != null ? MapToFolderDto(folder) : null;
    }

    public async Task<FilingFolderDto> CreateFolderAsync(CreateFolderRequest request, CancellationToken ct = default)
    {
        var count = await _context.FilingFolders.CountAsync(ct);
        var folderCode = $"GC/2026/{(count + 1):D4}";

        var folder = new FilingFolder
        {
            Id = Guid.NewGuid(),
            FolderCode = folderCode,
            Name = request.Name,
            Category = request.Category,
            Priority = request.Priority,
            Confidentiality = request.Confidentiality,
            Dept = request.Dept,
            Description = request.Description,
            IntakeMethod = request.IntakeMethod,
            SenderName = request.SenderName,
            SenderOrg = request.SenderOrg,
            SenderRef = request.SenderRef,
            InternalRef = request.InternalRef,
            Status = "active"
        };

        await _context.FilingFolders.AddAsync(folder, ct);

        // Auto route to logged in user for inbox
        if (_currentUser.UserId.HasValue)
        {
            var routing = new FilingInboxRouting
            {
                Id = Guid.NewGuid(),
                FolderId = folder.Id,
                AssignedToUserId = _currentUser.UserId.Value,
                AssignedToDept = request.Dept,
                Instruction = $"Review legal filings for {request.Name}",
                Status = "to_acknowledge",
                SentBy = "System Administrator"
            };
            await _context.FilingInboxRoutings.AddAsync(routing, ct);
        }

        // Add audit trail log
        await AddAuditLogAsync("folder.created", $"folder #{folderCode}", JsonSerializer.Serialize(request), ct);

        await _context.SaveChangesAsync(ct);
        return MapToFolderDto(folder);
    }

    public async Task<bool> DeleteFoldersAsync(List<Guid> folderIds, CancellationToken ct = default)
    {
        var folders = await _context.FilingFolders
            .Where(f => folderIds.Contains(f.Id))
            .ToListAsync(ct);

        if (!folders.Any()) return false;

        foreach (var folder in folders)
        {
            _context.FilingFolders.Remove(folder);
            await AddAuditLogAsync("folder.deleted", $"folder #{folder.FolderCode}", "{}", ct);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<FilingDocumentDto>> GetDocumentsAsync(Guid? folderId, string? query, CancellationToken ct = default)
    {
        var dbQuery = _context.FilingDocuments.AsQueryable();

        if (folderId.HasValue)
        {
            dbQuery = dbQuery.Where(d => d.FolderId == folderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lq = query.ToLower();
            dbQuery = dbQuery.Where(d => d.Name.ToLower().Contains(lq) || 
                                         d.OcrText.ToLower().Contains(lq) || 
                                         d.Sender.ToLower().Contains(lq));
        }

        var docs = await dbQuery.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);
        return docs.Select(d => MapToDocumentDto(d)).ToList();
    }

    public async Task<FilingDocumentDto> UploadDocumentAsync(Guid folderId, string fileName, Stream fileStream, string contentType, string? sender, string? senderOrg, CancellationToken ct = default)
    {
        var folder = await _context.FilingFolders.FindAsync(new object[] { folderId }, ct);
        if (folder == null) throw new KeyNotFoundException("Folder not found");

        // Save file
        var key = await _storageService.StoreAsync(fileStream, fileName, contentType, ct);
        var sizeBytes = fileStream.Length;
        var sizeStr = sizeBytes > 1024 * 1024 
            ? $"{(sizeBytes / 1024f / 1024f):F1} MB" 
            : $"{(sizeBytes / 1024f):F1} KB";

        // Generate simulated OCR
        var ocrText = $"TAX OMBUDSMAN OFFICE OF NIGERIA\n" +
                      $"Document: {fileName}\n" +
                      $"Sender: {sender ?? "System Upload"}\n" +
                      $"Org: {senderOrg ?? "Tax Ombud Staff"}\n" +
                      $"This document has been secure-filed and OCR-indexed successfully.\n" +
                      $"Content contains general mediation and review requests.";

        var doc = new FilingDocument
        {
            Id = Guid.NewGuid(),
            FolderId = folderId,
            Name = fileName,
            Size = sizeStr,
            Type = "PDF",
            OcrStatus = "done",
            OcrText = ocrText,
            Sender = sender ?? "System Upload",
            SenderOrg = senderOrg ?? "Tax Ombud Staff",
            SenderRef = $"EXT-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
            InternalRef = $"INT-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}"
        };

        await _context.FilingDocuments.AddAsync(doc, ct);

        // Add audit logs
        await AddAuditLogAsync("file.uploaded", $"file #{fileName}", JsonSerializer.Serialize(new { doc.Name, doc.Size }), ct);
        await AddAuditLogAsync("ocr.completed", $"file #{fileName}", JsonSerializer.Serialize(new { ocrTextLength = ocrText.Length, status = "success" }), ct);

        await _context.SaveChangesAsync(ct);
        return MapToDocumentDto(doc);
    }

    public async Task<bool> DeleteDocumentsAsync(List<Guid> documentIds, CancellationToken ct = default)
    {
        var docs = await _context.FilingDocuments
            .Where(d => documentIds.Contains(d.Id))
            .ToListAsync(ct);

        if (!docs.Any()) return false;

        foreach (var dbg in docs)
        {
            _context.FilingDocuments.Remove(dbg);
            await AddAuditLogAsync("file.deleted", $"file #{dbg.Name}", "{}", ct);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<FilingInboxRoutingDto>> GetInboxRoutingsAsync(string? query, CancellationToken ct = default)
    {
        var dbQuery = _context.FilingInboxRoutings.Include(r => r.Folder).AsQueryable();

        if (_currentUser.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.AssignedToUserId == _currentUser.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lq = query.ToLower();
            dbQuery = dbQuery.Where(r => r.Folder.Name.ToLower().Contains(lq) || 
                                         r.Folder.FolderCode.ToLower().Contains(lq) || 
                                         r.SentBy.ToLower().Contains(lq));
        }

        var list = await dbQuery.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return list.Select(r => MapToInboxRoutingDto(r)).ToList();
    }

    public async Task<bool> AcknowledgeRoutingAsync(Guid id, CancellationToken ct = default)
    {
        var routing = await _context.FilingInboxRoutings
            .Include(r => r.Folder)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (routing == null) return false;

        routing.Status = "in_progress";
        if (routing.Folder != null)
        {
            routing.Folder.Status = "in_progress";
        }

        await AddAuditLogAsync("folder.acknowledged", $"folder #{routing.Folder?.FolderCode}", "{}", ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectRoutingAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var routing = await _context.FilingInboxRoutings
            .Include(r => r.Folder)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (routing == null) return false;

        routing.Status = "archive";
        routing.RejectionReason = reason;
        if (routing.Folder != null)
        {
            routing.Folder.Status = "rejected";
        }

        await AddAuditLogAsync("folder.rejected", $"folder #{routing.Folder?.FolderCode}", JsonSerializer.Serialize(new { reason }), ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<FilingCategoryDto>> GetCategoriesAsync(string? query, CancellationToken ct = default)
    {
        var dbQuery = _context.FilingCategories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lq = query.ToLower();
            dbQuery = dbQuery.Where(c => c.Name.ToLower().Contains(lq));
        }

        var list = await dbQuery.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        return list.Select(c => MapToCategoryDto(c)).ToList();
    }

    public async Task<FilingCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = new FilingCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = "active"
        };

        await _context.FilingCategories.AddAsync(category, ct);
        await AddAuditLogAsync("category.created", $"category #{request.Name}", JsonSerializer.Serialize(request), ct);
        await _context.SaveChangesAsync(ct);

        return MapToCategoryDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _context.FilingCategories.FindAsync(new object[] { id }, ct);
        if (category == null) return false;

        _context.FilingCategories.Remove(category);
        await AddAuditLogAsync("category.deleted", $"category #{category.Name}", "{}", ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<AuditLog>> GetSecuredFilingAuditLogsAsync(CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .Where(l => l.EntityType == "SecuredFiling")
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task<bool> ClearSecuredFilingAuditLogsAsync(CancellationToken ct = default)
    {
        var logs = await _context.AuditLogs
            .Where(l => l.EntityType == "SecuredFiling")
            .ToListAsync(ct);

        if (!logs.Any()) return true;

        foreach (var log in logs)
        {
            _context.AuditLogs.Remove(log);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Helper Mapping Methods ──────────────────────────────────────────────
    private static FilingFolderDto MapToFolderDto(FilingFolder f) => new()
    {
        Id = f.Id,
        FolderCode = f.FolderCode,
        Name = f.Name,
        Category = f.Category,
        Priority = f.Priority,
        Confidentiality = f.Confidentiality,
        Dept = f.Dept,
        Description = f.Description,
        IntakeMethod = f.IntakeMethod,
        SenderName = f.SenderName,
        SenderOrg = f.SenderOrg,
        SenderRef = f.SenderRef,
        InternalRef = f.InternalRef,
        Status = f.Status,
        CreatedAt = f.CreatedAt
    };

    private static FilingDocumentDto MapToDocumentDto(FilingDocument d) => new()
    {
        Id = d.Id,
        FolderId = d.FolderId,
        Name = d.Name,
        Size = d.Size,
        Type = d.Type,
        OcrStatus = d.OcrStatus,
        OcrText = d.OcrText,
        Sender = d.Sender,
        SenderOrg = d.SenderOrg,
        SenderRef = d.SenderRef,
        InternalRef = d.InternalRef,
        CreatedAt = d.CreatedAt
    };

    private static FilingCategoryDto MapToCategoryDto(FilingCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Status = c.Status,
        CreatedAt = c.CreatedAt
    };

    private static FilingInboxRoutingDto MapToInboxRoutingDto(FilingInboxRouting r) => new()
    {
        Id = r.Id,
        FolderId = r.FolderId,
        FolderCode = r.Folder?.FolderCode ?? string.Empty,
        FolderName = r.Folder?.Name ?? string.Empty,
        Priority = r.Folder?.Priority ?? "normal",
        SentBy = r.SentBy,
        CreatedAt = r.CreatedAt,
        Instruction = r.Instruction,
        Status = r.Status,
        RejectionReason = r.RejectionReason
    };

    private async Task AddAuditLogAsync(string action, string target, string details, CancellationToken ct)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = "SecuredFiling",
            EntityId = Guid.NewGuid(),
            Action = action,
            OldValues = target,
            NewValues = details,
            UserId = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(log, ct);
    }

    private async Task SeedInitialDataAsync(CancellationToken ct)
    {
        var hasCategories = await _context.FilingCategories.AnyAsync(ct);
        if (!hasCategories)
        {
            await _context.FilingCategories.AddRangeAsync(new[]
            {
                new FilingCategory { Id = Guid.NewGuid(), Name = "Correspondence", Description = "Incoming and outgoing letters, memos, and general notices.", Status = "active" },
                new FilingCategory { Id = Guid.NewGuid(), Name = "Finance", Description = "Budget allocations, payroll audits, and financial requests.", Status = "active" },
                new FilingCategory { Id = Guid.NewGuid(), Name = "Legal Case Files", Description = "Objections, appeals, litigation briefs, and mediation hearings.", Status = "active" }
            }, ct);
        }

        var folder1 = new FilingFolder
        {
            Id = Guid.NewGuid(),
            FolderCode = "GC/2026/0001",
            Name = "Ministry Correspondence 2026",
            Category = "Correspondence",
            Priority = "normal",
            Confidentiality = "normal",
            Dept = "Legal",
            Description = "General correspondence folders.",
            IntakeMethod = "walk_in",
            SenderName = "Olawale Johnson",
            SenderOrg = "Lagos Chambers",
            SenderRef = "EXT-9988",
            InternalRef = "GC/2026/0001",
            Status = "pending_ack"
        };

        var folder2 = new FilingFolder
        {
            Id = Guid.NewGuid(),
            FolderCode = "FA/2026/0002",
            Name = "Financial Audit Filings",
            Category = "Finance",
            Priority = "urgent",
            Confidentiality = "confidential",
            Dept = "Finance",
            Description = "Financial audits folder.",
            IntakeMethod = "courier",
            SenderName = "Audit Committee",
            SenderOrg = "National Registry",
            SenderRef = "EXT-5050",
            InternalRef = "FA/2026/0002",
            Status = "in_progress"
        };

        await _context.FilingFolders.AddRangeAsync(new[] { folder1, folder2 }, ct);

        // Seed some files
        var file1 = new FilingDocument
        {
            Id = Guid.NewGuid(),
            FolderId = folder1.Id,
            Name = "Taxpayer_Appeal_Lagos.pdf",
            Size = "2.4 MB",
            Type = "PDF",
            OcrStatus = "done",
            OcrText = "TAX OMBUDSMAN OFFICE OF NIGERIA\nAPPEAL NO: APP/2026/LA/091\n\nSubject: Objection to unreasonable tax assessment by FIRS on Lagos branch operations.\nTaxpayer details: Olawale Johnson, Lagos Chambers.\nWe submit this appeal requesting a comprehensive mediation session.",
            Sender = "Olawale Johnson",
            SenderOrg = "Lagos Chambers",
            SenderRef = "EXT-9988",
            InternalRef = "GC/2026/0001"
        };

        var file2 = new FilingDocument
        {
            Id = Guid.NewGuid(),
            FolderId = folder2.Id,
            Name = "Internal_Audit_Report.pdf",
            Size = "4.8 MB",
            Type = "PDF",
            OcrStatus = "done",
            OcrText = "CONFIDENTIAL AUDIT REPORT\nTAX OMBUD FINANCE DEPARTMENT\n\nReview of internal budgets, operational disbursements and travel allowances for fiscal year 2026.\nAll records audited match approved treasury guidelines with zero negative highlights.",
            Sender = "Audit Dept",
            SenderOrg = "Tax Ombud Finance",
            SenderRef = "AUD-3322",
            InternalRef = "FA/2026/0002"
        };

        await _context.FilingDocuments.AddRangeAsync(new[] { file1, file2 }, ct);

        // Seed some inbox routings
        if (_currentUser.UserId.HasValue)
        {
            var route1 = new FilingInboxRouting
            {
                Id = Guid.NewGuid(),
                FolderId = folder1.Id,
                AssignedToUserId = _currentUser.UserId.Value,
                AssignedToDept = "Legal",
                Instruction = "Please review legal implications and advise.",
                Status = "to_acknowledge",
                SentBy = "Corporate HQ"
            };

            var route2 = new FilingInboxRouting
            {
                Id = Guid.NewGuid(),
                FolderId = folder2.Id,
                AssignedToUserId = _currentUser.UserId.Value,
                AssignedToDept = "Finance",
                Instruction = "Requires urgent signature sign-off before Friday deadline.",
                Status = "to_acknowledge",
                SentBy = "Finance Dept"
            };

            await _context.FilingInboxRoutings.AddRangeAsync(new[] { route1, route2 }, ct);
        }

        // Seed some default audits
        await AddAuditLogAsync("folder.created", "folder #GC/2026/0001", "{\"name\": \"Ministry Correspondence 2026\"}", ct);
        await AddAuditLogAsync("folder.created", "folder #FA/2026/0002", "{\"name\": \"Financial Audit Filings\"}", ct);
        await AddAuditLogAsync("file.uploaded", "file #Taxpayer_Appeal_Lagos.pdf", "{\"size\": \"2.4 MB\"}", ct);
        await AddAuditLogAsync("ocr.completed", "file #Taxpayer_Appeal_Lagos.pdf", "{\"ocrTextLength\": 245, \"status\": \"success\"}", ct);

        await _context.SaveChangesAsync(ct);
    }
}
