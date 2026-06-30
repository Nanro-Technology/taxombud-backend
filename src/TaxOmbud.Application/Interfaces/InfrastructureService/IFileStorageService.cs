using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface IFileStorageService
{
    /// <summary>Stores a file and returns the storage key/path.</summary>
    Task<string> StoreAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Returns a pre-signed URL or a local download path.</summary>
    Task<string> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>Deletes the stored file.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
