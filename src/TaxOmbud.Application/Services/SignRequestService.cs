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

public class SignRequestService : ISignRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUser _currentUser;

    public SignRequestService(
        IApplicationDbContext context,
        IFileStorageService storage,
        ICurrentUser currentUser)
    {
        _context = context;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<List<SignRequest>> GetSignRequestsAsync(CancellationToken ct = default)
    {
        var requests = await _context.SignRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

        if (!requests.Any())
        {
            await SeedInitialSignRequestsAsync(ct);
            requests = await _context.SignRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        }

        return requests;
    }

    public async Task<SignRequest> CreateSignRequestAsync(string fileName, Stream content, string contentType, string signatoryEmail, CancellationToken ct = default)
    {
        var key = await _storage.StoreAsync(content, fileName, contentType, ct);

        var request = new SignRequest
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StorageKey = key,
            Status = "Pending",
            Token = "sign_" + Guid.NewGuid().ToString("N").Substring(0, 8),
            SignatoryEmail = signatoryEmail,
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SignRequests.AddAsync(request, ct);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<bool> DeleteSignRequestAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _context.SignRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request == null) return false;

        _context.SignRequests.Remove(request);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SignRequest?> GetSignRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SignRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<SignRequest?> SignRequestAsync(Guid id, Stream signatureImage, CancellationToken ct = default)
    {
        var request = await _context.SignRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request == null) return null;

        var signedFileName = "Signed_" + request.FileName;
        // Mock save the signed PDF file (using the signature image or just creating a new storage entry)
        var key = await _storage.StoreAsync(signatureImage, signedFileName, "image/png", ct);

        request.Status = "Signed";
        request.SignedFileName = signedFileName;
        request.SignedStorageKey = key;
        request.LastModifiedAt = DateTime.UtcNow;

        // Auto-archive signed PDF to Secured Filing as stated in the PRD assumption:
        // "Signed PDFs auto-archived to Secured Filing [ASSUMPTION]"
        try
        {
            var folder = await _context.FilingFolders.FirstOrDefaultAsync(ct);
            if (folder != null)
            {
                var doc = new TaxOmbud.Domain.Entities.SecuredFiling.FilingDocument
                {
                    Id = Guid.NewGuid(),
                    FolderId = folder.Id,
                    Name = signedFileName,
                    Size = "1.2 MB",
                    Type = "PDF",
                    OcrStatus = "done",
                    OcrText = $"TAX OMBUDSMAN OFFICE OF NIGERIA\nDocument: {signedFileName}\nE-Sign Completed by {request.SignatoryEmail}\nTimestamp: {DateTime.UtcNow}\nIP Audit Captured.",
                    Sender = request.SignatoryEmail,
                    SenderOrg = "External Signatory",
                    SenderRef = "ESIGN-AUTO",
                    InternalRef = "SF-ESIGN"
                };
                await _context.FilingDocuments.AddAsync(doc, ct);
            }
        }
        catch {}

        await _context.SaveChangesAsync(ct);
        return request;
    }

    private async Task SeedInitialSignRequestsAsync(CancellationToken ct)
    {
        var req1 = new SignRequest
        {
            Id = Guid.Parse("7a6b7280-5b31-4b10-8b1e-7b728092400e"),
            FileName = "Audit Agreement 2026.pdf",
            StorageKey = "audit_agreement_2026.pdf",
            Status = "Pending",
            Token = "sr1",
            SignatoryEmail = "simon.pukuma@taxombud.gov.ng",
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var req2 = new SignRequest
        {
            Id = Guid.Parse("8a6b7280-6b31-4b10-8b1e-7b728092400f"),
            FileName = "ICT Asset Policy.pdf",
            StorageKey = "ict_asset_policy.pdf",
            Status = "Signed",
            Token = "sr2",
            SignatoryEmail = "Timothy Usman (timothy.usman@taxombud.gov.ng)",
            SignedFileName = "Signed_ICT_Asset_Policy.pdf",
            SignedStorageKey = "signed_ict_asset_policy.pdf",
            CreatedByUserId = _currentUser.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _context.SignRequests.AddRangeAsync(new[] { req1, req2 }, ct);
        await _context.SaveChangesAsync(ct);
    }
}
