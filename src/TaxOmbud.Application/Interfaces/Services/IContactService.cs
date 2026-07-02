using TaxOmbud.Application.Contact.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IContactService
{
    Task<Response<string>> SubmitContactFormAsync(SubmitContactFormCommand request, CancellationToken cancellationToken = default);
}
