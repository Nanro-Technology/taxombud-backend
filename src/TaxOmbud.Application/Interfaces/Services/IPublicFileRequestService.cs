using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IPublicFileRequestService
{
    Task<List<PublicFileRequest>> GetPublicFileRequestsAsync(CancellationToken ct = default);
    Task<PublicFileRequest> CreatePublicFileRequestAsync(string name, DateTime? expiresAt, List<string> allowedExtensions, int maxSizeMb, string notifyEmails, string? notes, CancellationToken ct = default);
    Task<bool> DeletePublicFileRequestAsync(Guid id, CancellationToken ct = default);
    Task<PublicFileRequest?> GetPublicFileRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task<PublicFileRequestUpload> UploadFileToRequestAsync(Guid requestId, string fileName, Stream content, string contentType, CancellationToken ct = default);
}
