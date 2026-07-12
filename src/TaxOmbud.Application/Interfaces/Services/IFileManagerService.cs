using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IFileManagerService
{
    Task<List<UserFile>> GetFilesAsync(string area, string path, CancellationToken ct = default);
    Task<UserFile> CreateFolderAsync(string area, string path, string name, CancellationToken ct = default);
    Task<UserFile> UploadFileAsync(string area, string path, string name, Stream content, string contentType, CancellationToken ct = default);
    Task<bool> DeleteItemsAsync(List<Guid> ids, CancellationToken ct = default);
    Task<UserFile?> GetFileByIdAsync(Guid id, CancellationToken ct = default);
}
