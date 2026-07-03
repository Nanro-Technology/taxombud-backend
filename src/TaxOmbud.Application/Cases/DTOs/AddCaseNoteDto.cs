namespace TaxOmbud.Application.Cases.DTOs;

public record AddCaseNoteCommand(Guid CaseId, string Text, bool IsExternal) ;

public record AddCaseNoteResponse(Guid Id, string NoteText, bool IsExternal, DateTimeOffset CreatedAt);
