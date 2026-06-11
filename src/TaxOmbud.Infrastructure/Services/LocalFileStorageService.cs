using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Infrastructure.Services;

/// <summary>
/// Local disk file storage. Drop-in replaced with Azure Blob / AWS S3 in production.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _storagePath = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_storagePath);
        _logger = logger;
    }

    public async Task<string> StoreAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_storagePath, safeFileName);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, cancellationToken);

        _logger.LogInformation("Stored file: {FileName}", safeFileName);
        return safeFileName;
    }

    public Task<string> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        // In local mode, the URL is constructed by the API controller; just return the key.
        return Task.FromResult($"/api/documents/download/{storageKey}");
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
