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

public class PublicFileRequestService : IPublicFileRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUser _currentUser;

    public PublicFileRequestService(
        IApplicationDbContext context,
        IFileStorageService storage,
        ICurrentUser currentUser)
    {
        _context = context;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<List<PublicFileRequest>> GetPublicFileRequestsAsync(CancellationToken ct = default)
    {
        var requests = await _context.PublicFileRequests
            .Include(r => r.Uploads)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        if (!requests.Any())
        {
            await SeedInitialPublicFileRequestsAsync(ct);
            requests = await _context.PublicFileRequests
                .Include(r => r.Uploads)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        return requests;
    }

    public async Task<PublicFileRequest> CreatePublicFileRequestAsync(string name, DateTime? expiresAt, List<string> allowedExtensions, int maxSizeMb, string notifyEmails, string? notes, CancellationToken ct = default)
    {
        var request = new PublicFileRequest
        {
            Id = Guid.NewGuid(),
            Name = name,
            Token = "req_" + Guid.NewGuid().ToString("N").Substring(0, 8),
            ExpiresAt = expiresAt,
            Status = "Active",
            AllowedExtensions = string.Join(",", allowedExtensions),
            MaxSizeMb = maxSizeMb,
            NotifyEmails = notifyEmails,
            Notes = notes,
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PublicFileRequests.AddAsync(request, ct);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<bool> DeletePublicFileRequestAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _context.PublicFileRequests
            .Include(r => r.Uploads)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (request == null) return false;

        foreach (var upload in request.Uploads)
        {
            await _storage.DeleteAsync(upload.StorageKey, ct);
            _context.PublicFileRequestUploads.Remove(upload);
        }

        _context.PublicFileRequests.Remove(request);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PublicFileRequest?> GetPublicFileRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PublicFileRequests
            .Include(r => r.Uploads)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<PublicFileRequestUpload> UploadFileToRequestAsync(Guid requestId, string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var request = await _context.PublicFileRequests.FindAsync(new object[] { requestId }, ct);
        if (request == null) throw new KeyNotFoundException("Public File Request not found");

        var key = await _storage.StoreAsync(content, fileName, contentType, ct);

        var upload = new PublicFileRequestUpload
        {
            Id = Guid.NewGuid(),
            PublicFileRequestId = requestId,
            FileName = fileName,
            StorageKey = key,
            FileSize = content.Length,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PublicFileRequestUploads.AddAsync(upload, ct);

        // Also mirror upload to FileManager's PublicFiles area so that it is visible to the requester
        // "Uploads land in a designated folder visible to the requester"
        var fileMgrFolder = new UserFile
        {
            Id = Guid.NewGuid(),
            Name = fileName,
            Type = "file",
            Area = "PublicFiles",
            Path = "Public File Requests", // Land in a designated folder
            StorageKey = key,
            ContentType = contentType,
            FileSize = content.Length,
            Content = $"Uploaded file for request dropbox: {request.Name}",
            OwnerId = request.CreatedByUserId ?? Guid.Empty
        };
        await _context.UserFiles.AddAsync(fileMgrFolder, ct);

        await _context.SaveChangesAsync(ct);
        return upload;
    }

    private async Task SeedInitialPublicFileRequestsAsync(CancellationToken ct)
    {
        var req1 = new PublicFileRequest
        {
            Id = Guid.Parse("1a6b7280-7b31-4b10-8b1e-7b728092400a"),
            Name = "Vendor invoices",
            Token = "req_invoices_987",
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            Status = "Active",
            AllowedExtensions = "pdf,png,jpg",
            MaxSizeMb = 10,
            NotifyEmails = "finance@taxombud.gov.ng",
            Notes = "Please upload final invoice matching the approved purchase order.",
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var req2 = new PublicFileRequest
        {
            Id = Guid.Parse("2a6b7280-8b31-4b10-8b1e-7b728092400b"),
            Name = "Contractor Certificates",
            Token = "req_cert_124",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Status = "Expired",
            AllowedExtensions = "pdf,zip",
            MaxSizeMb = 10,
            NotifyEmails = "procurement@taxombud.gov.ng",
            Notes = "Provide both tax clearance certificates and corporate registrations.",
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        await _context.PublicFileRequests.AddRangeAsync(new[] { req1, req2 }, ct);
        await _context.SaveChangesAsync(ct);

        // Seed uploads
        var upload1 = new PublicFileRequestUpload
        {
            Id = Guid.NewGuid(),
            PublicFileRequestId = req1.Id,
            FileName = "invoice_9823.pdf",
            StorageKey = "invoice_9823.pdf",
            FileSize = 256000,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var upload2 = new PublicFileRequestUpload
        {
            Id = Guid.NewGuid(),
            PublicFileRequestId = req1.Id,
            FileName = "invoice_1204.pdf",
            StorageKey = "invoice_1204.pdf",
            FileSize = 1258291,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var upload3 = new PublicFileRequestUpload
        {
            Id = Guid.NewGuid(),
            PublicFileRequestId = req2.Id,
            FileName = "cert_johndoe.pdf",
            StorageKey = "cert_johndoe.pdf",
            FileSize = 819200,
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        };

        await _context.PublicFileRequestUploads.AddRangeAsync(new[] { upload1, upload2, upload3 }, ct);
        await _context.SaveChangesAsync(ct);
    }
}
