using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Domain.Entities.Documents;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ISignRequestService
{
    Task<List<SignRequest>> GetSignRequestsAsync(CancellationToken ct = default);
    Task<SignRequest> CreateSignRequestAsync(string fileName, Stream content, string contentType, string signatoryEmail, CancellationToken ct = default);
    Task<bool> DeleteSignRequestAsync(Guid id, CancellationToken ct = default);
    Task<SignRequest?> GetSignRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task<SignRequest?> SignRequestAsync(Guid id, Stream signatureImage, CancellationToken ct = default);
}
