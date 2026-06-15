using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaxOmbud.Application.Common.Models;
using System;

namespace TaxOmbud.Application.Features.Cases.Queries.TrackComplaint;

public record TrackComplaintQuery(string TrackingNumber) : IRequest<Result<TrackComplaintResponse>>;

public record TrackComplaintResponse(string TrackingNumber, string Status, string Description, DateTime SubmittedAt);

public class TrackComplaintQueryHandler : IRequestHandler<TrackComplaintQuery, Result<TrackComplaintResponse>>
{
    public Task<Result<TrackComplaintResponse>> Handle(TrackComplaintQuery request, CancellationToken cancellationToken)
    {
        // Mocking response
        var response = new TrackComplaintResponse(
            request.TrackingNumber, 
            "Pending Review", 
            "Your complaint has been received and is currently under review.", 
            DateTime.UtcNow.AddDays(-1));

        return Task.FromResult(Result<TrackComplaintResponse>.Success(response));
    }
}
