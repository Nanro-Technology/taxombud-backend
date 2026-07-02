namespace TaxOmbud.Application.Cases.DTOs;

public record PostRecommendationCommand(Guid CaseId, string RecommendationText) ;

public record PostRecommendationResponse(Guid Id, string RecommendationText, DateTimeOffset CreatedAt);

public record PostRecommendationRequest(string RecommendationText);