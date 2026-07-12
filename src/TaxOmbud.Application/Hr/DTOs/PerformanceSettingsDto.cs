namespace TaxOmbud.Application.Hr.DTOs;

// ── Competencies ──────────────────────────────────────────────────────────────

public record GetCompetenciesQuery();

public record CompetencyDto(
    Guid Id,
    string Name,
    string Description,
    int SortOrder,
    string Status,
    DateTimeOffset CreatedAt
);

public record CreateCompetencyCommand(string Name, string Description, int SortOrder);
public record CreateCompetencyRequest(string Name, string Description, int SortOrder);

public record UpdateCompetencyCommand(Guid Id, string Name, string Description, int SortOrder, string Status);
public record UpdateCompetencyRequest(string Name, string Description, int SortOrder, string Status);

// ── Review Templates ──────────────────────────────────────────────────────────

public record GetReviewTemplatesQuery();

public record ReviewTemplateDto(
    Guid Id,
    string Name,
    string Description,
    int QuestionCount,
    string Status,
    DateTimeOffset CreatedAt
);

public record CreateReviewTemplateCommand(string Name, string Description, int QuestionCount);
public record CreateReviewTemplateRequest(string Name, string Description, int QuestionCount);

public record UpdateReviewTemplateCommand(Guid Id, string Name, string Description, int QuestionCount, string Status);
public record UpdateReviewTemplateRequest(string Name, string Description, int QuestionCount, string Status);

// ── Performance Cycles ────────────────────────────────────────────────────────

public record GetPerformanceCyclesQuery();

public record PerformanceCycleDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    DateTimeOffset CreatedAt
);

public record CreatePerformanceCycleCommand(string Name, DateTime StartDate, DateTime EndDate);
public record CreatePerformanceCycleRequest(string Name, DateTime StartDate, DateTime EndDate);

// ── Bulk Onboarding ───────────────────────────────────────────────────────────

public record BulkOnboardItem(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string JobTitle,
    string EmploymentType,
    string? DepartmentId,
    string HireDate
);

public record BulkOnboardRequest(List<BulkOnboardItem> Employees);

public record BulkOnboardResultItem(
    string Email,
    bool Success,
    string? Message
);
