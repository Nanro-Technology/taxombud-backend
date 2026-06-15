using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Contact.Commands.SubmitContactForm;

public record SubmitContactFormCommand(
    string Name,
    string Email,
    string Subject,
    string Message
) : IRequest<Result<string>>;

public class SubmitContactFormCommandHandler : IRequestHandler<SubmitContactFormCommand, Result<string>>
{
    public Task<Result<string>> Handle(SubmitContactFormCommand request, CancellationToken cancellationToken)
    {
        // For now, mock successful contact form submission
        return Task.FromResult(Result<string>.Success("Message received successfully."));
    }
}
