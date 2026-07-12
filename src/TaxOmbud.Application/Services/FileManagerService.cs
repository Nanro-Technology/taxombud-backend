using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Services;

public class FileManagerService : IFileManagerService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly ICurrentUser _currentUser;

    public FileManagerService(
        IApplicationDbContext context,
        IFileStorageService storageService,
        ICurrentUser currentUser)
    {
        _context = context;
        _storageService = storageService;
        _currentUser = currentUser;
    }

    private (string Area, string SubPath) ParsePath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return ("MyFiles", "");

        var parts = fullPath.Split('/');
        var areaStr = parts[0].Trim();
        var area = "MyFiles";
        if (areaStr.Equals("Public Files", StringComparison.OrdinalIgnoreCase) || areaStr.Equals("PublicFiles", StringComparison.OrdinalIgnoreCase))
            area = "PublicFiles";
        else if (areaStr.Equals("Temp (Read-only)", StringComparison.OrdinalIgnoreCase) || areaStr.Equals("Temp", StringComparison.OrdinalIgnoreCase))
            area = "Temp";

        var subPath = string.Join("/", parts.Skip(1));
        return (area, subPath);
    }

    public async Task<List<UserFile>> GetFilesAsync(string area, string path, CancellationToken ct = default)
    {
        var (parsedArea, parsedSubPath) = ParsePath(path);

        // Visibility check for "MyFiles" (must be scoped to owner)
        var query = _context.UserFiles.Where(f => f.Area == parsedArea && f.Path == parsedSubPath);

        if (parsedArea == "MyFiles" && _currentUser.UserId.HasValue)
        {
            query = query.Where(f => f.OwnerId == _currentUser.UserId.Value);
        }

        var files = await query.OrderByDescending(f => f.Type).ThenBy(f => f.Name).ToListAsync(ct);

        // If empty root of MyFiles or PublicFiles, seed some initial files
        if (!files.Any() && string.IsNullOrEmpty(parsedSubPath))
        {
            await SeedInitialFileManagerDataAsync(parsedArea, ct);
            files = await query.OrderByDescending(f => f.Type).ThenBy(f => f.Name).ToListAsync(ct);
        }

        return files;
    }

    public async Task<UserFile> CreateFolderAsync(string area, string path, string name, CancellationToken ct = default)
    {
        var (parsedArea, parsedSubPath) = ParsePath(path);

        var folder = new UserFile
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = "folder",
            Area = parsedArea,
            Path = parsedSubPath,
            OwnerId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserFiles.AddAsync(folder, ct);
        await _context.SaveChangesAsync(ct);
        return folder;
    }

    public async Task<UserFile> UploadFileAsync(string area, string path, string name, Stream content, string contentType, CancellationToken ct = default)
    {
        var (parsedArea, parsedSubPath) = ParsePath(path);

        var key = await _storageService.StoreAsync(content, name, contentType, ct);

        // Read content for quick-look preview if it's text/pdf mock preview
        string? textContent = null;
        if (contentType.Contains("text") || name.EndsWith(".txt") || name.EndsWith(".json") || name.EndsWith(".xml"))
        {
            try
            {
                content.Position = 0;
                using var reader = new StreamReader(content);
                textContent = await reader.ReadToEndAsync(ct);
            }
            catch {}
        }
        else
        {
            textContent = $"This is a preview of the secure file '{name}'. Mirroring to S3 is active.";
        }

        var file = new UserFile
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = "file",
            Area = parsedArea,
            Path = parsedSubPath,
            StorageKey = key,
            ContentType = contentType,
            FileSize = content.Length,
            Content = textContent,
            OwnerId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserFiles.AddAsync(file, ct);
        await _context.SaveChangesAsync(ct);
        return file;
    }

    public async Task<bool> DeleteItemsAsync(List<Guid> ids, CancellationToken ct = default)
    {
        var items = await _context.UserFiles.Where(f => ids.Contains(f.Id)).ToListAsync(ct);
        if (!items.Any()) return false;

        foreach (var item in items)
        {
            if (item.Type == "folder")
            {
                // Delete children recursively
                var pathPrefix = string.IsNullOrEmpty(item.Path) ? item.Name : $"{item.Path}/{item.Name}";
                var children = await _context.UserFiles
                    .Where(f => f.Area == item.Area && (f.Path == pathPrefix || f.Path.StartsWith(pathPrefix + "/")))
                    .ToListAsync(ct);

                foreach (var child in children)
                {
                    if (child.Type == "file" && !string.IsNullOrEmpty(child.StorageKey))
                    {
                        await _storageService.DeleteAsync(child.StorageKey, ct);
                    }
                    _context.UserFiles.Remove(child);
                }
            }
            else if (item.Type == "file" && !string.IsNullOrEmpty(item.StorageKey))
            {
                await _storageService.DeleteAsync(item.StorageKey, ct);
            }

            _context.UserFiles.Remove(item);
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserFile?> GetFileByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.UserFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    private async Task SeedInitialFileManagerDataAsync(string area, CancellationToken ct)
    {
        var ownerId = _currentUser.UserId ?? Guid.Empty;

        if (area == "MyFiles")
        {
            var folder1 = new UserFile { Id = Guid.NewGuid(), Name = "Annual Report 2025", Type = "folder", Area = area, Path = "", OwnerId = ownerId };
            var folder2 = new UserFile { Id = Guid.NewGuid(), Name = "Compliance Audits", Type = "folder", Area = area, Path = "", OwnerId = ownerId };
            var file1 = new UserFile 
            { 
                Id = Guid.NewGuid(), 
                Name = "Tax_Policy_Overview.pdf", 
                Type = "file", 
                Area = area, 
                Path = "", 
                StorageKey = "tax_policy_overview.pdf", 
                ContentType = "application/pdf", 
                FileSize = 245000, 
                Content = "This document details the primary tax compliance guidelines for the 2026 fiscal year...",
                OwnerId = ownerId
            };
            var file2 = new UserFile 
            { 
                Id = Guid.NewGuid(), 
                Name = "Staff_Onboarding_Template.docx", 
                Type = "file", 
                Area = area, 
                Path = "", 
                StorageKey = "staff_onboarding_template.docx", 
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 
                FileSize = 124000, 
                Content = "Official employee onboarding workflow checklist...",
                OwnerId = ownerId
            };

            await _context.UserFiles.AddRangeAsync(new[] { folder1, folder2, file1, file2 }, ct);

            // Add files inside folders
            var subFile1 = new UserFile
            {
                Id = Guid.NewGuid(),
                Name = "Draft_Report_Q1.xlsx",
                Type = "file",
                Area = area,
                Path = "Annual Report 2025",
                StorageKey = "draft_report_q1.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileSize = 84000,
                Content = "Quarterly financial overview calculations...",
                OwnerId = ownerId
            };
            var subFile2 = new UserFile
            {
                Id = Guid.NewGuid(),
                Name = "CEO_Final_Feedback.txt",
                Type = "file",
                Area = area,
                Path = "Annual Report 2025",
                StorageKey = "ceo_final_feedback.txt",
                ContentType = "text/plain",
                FileSize = 1200,
                Content = "Draft approved. Please mirror all changes to S3 bucket...",
                OwnerId = ownerId
            };

            await _context.UserFiles.AddRangeAsync(new[] { subFile1, subFile2 }, ct);
        }
        else if (area == "PublicFiles")
        {
            var file1 = new UserFile 
            { 
                Id = Guid.NewGuid(), 
                Name = "Filing_Guidelines.pdf", 
                Type = "file", 
                Area = area, 
                Path = "", 
                StorageKey = "filing_guidelines.pdf", 
                ContentType = "application/pdf", 
                FileSize = 512000, 
                Content = "Impartial mediation public guides...",
                OwnerId = Guid.Empty
            };
            var file2 = new UserFile 
            { 
                Id = Guid.NewGuid(), 
                Name = "FAQ_Taxpayer_Rights.pdf", 
                Type = "file", 
                Area = area, 
                Path = "", 
                StorageKey = "faq_taxpayer_rights.pdf", 
                ContentType = "application/pdf", 
                FileSize = 189000, 
                Content = "Frequently asked questions regarding taxpayer rights...",
                OwnerId = Guid.Empty
            };

            await _context.UserFiles.AddRangeAsync(new[] { file1, file2 }, ct);
        }
        else if (area == "Temp")
        {
            var folder1 = new UserFile { Id = Guid.NewGuid(), Name = "elfinder", Type = "folder", Area = area, Path = "", OwnerId = Guid.Empty };
            var folder2 = new UserFile { Id = Guid.NewGuid(), Name = "elfinder-sync", Type = "folder", Area = area, Path = "", OwnerId = Guid.Empty };
            await _context.UserFiles.AddRangeAsync(new[] { folder1, folder2 }, ct);
        }

        await _context.SaveChangesAsync(ct);
    }
}
