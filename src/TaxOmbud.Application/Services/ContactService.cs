using TaxOmbud.Application.Contact.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Services;

public class ContactService : IContactService
{

    public ContactService(
    )
    {
    }

    public async Task<Response<string>> SubmitContactFormAsync(SubmitContactFormCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<string>
        {
            StatusCode = 200,
            Message = "Message received successfully.",
            Data = "Success"
        };
        return await Task.FromResult(response);
    }

}
