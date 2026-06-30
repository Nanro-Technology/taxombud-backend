# GenerateStubs.ps1
# Adds stub record/class definitions for every missing DTO/Command/Query/Response type.
# Types are grouped by module and written into a _Stubs.cs file in each module folder.

$appDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application"

# Map: namespace => list of type stubs to generate
# Format: "TypeName" => "record" or "class"
$moduleStubs = @{

"TaxOmbud.Application.AiChatbot.DTOs" = @"
public record SubmitChatMessageCommand(string Message, string? ConversationId);
public record SubmitChatMessageResponse(string Reply, string ConversationId);
"@

"TaxOmbud.Application.Appeals.DTOs" = @"
public record GetAppealDocumentsQuery(Guid AppealId);
public record UploadAppealDocumentCommand(Guid AppealId, Microsoft.AspNetCore.Http.IFormFile File);
public record AppealDocumentDto(Guid Id, string FileName, string ContentType, long FileSize, DateTimeOffset UploadedAt);
"@

"TaxOmbud.Application.Cases.DTOs" = @"
public record AddCaseFindingCommand(Guid CaseId, string Description);
public record AddCaseNoteCommand(Guid CaseId, string Text, bool IsExternal);
public record AddCaseNoteResponse(Guid NoteId);
public record ApproveClosureCommand(Guid CaseId, bool Approve, string Rationale);
public record AssignCaseCommand(Guid CaseId, Guid OfficerId);
public record CompleteMilestoneCommand(Guid CaseId, Guid MilestoneId);
public record PostRecommendationCommand(Guid CaseId, string RecommendationText);
public record PostRecommendationResponse(Guid RecommendationId);
public record SubmitPublicCaseCommand(string SubmitterType, string FirstName, string LastName, string Email, string Phone, string CountryId, string StateId, string Description);
public record SubmitPublicCaseResponse(Guid CaseId, string TrackingNumber);
public record TransitionCaseCommand(Guid CaseId, string TargetStage, string? Reason);
public record UpdateCaseFindingCommand(Guid CaseId, Guid FindingId, string Description);
public record UploadCaseDocumentCommand(Guid CaseId, Microsoft.AspNetCore.Http.IFormFile File);
public record GetCaseByIdQuery(Guid CaseId);
public record GetCaseCommunicationsQuery(Guid CaseId);
public record GetCaseDocumentsQuery(Guid CaseId);
public record GetCaseFindingsQuery(Guid CaseId);
public record GetCaseMilestonesQuery(Guid CaseId);
public record GetCasesQuery(string? Search, string? Stage, string? Status, int Page, int PageSize);
public record GetMyCasesQuery(string? Search, string? Stage, string? Status, int Page, int PageSize);
public record GetOverdueCasesQuery(int Page, int PageSize);
public record GetQueueQuery(string QueueName, int Page, int PageSize);
public record TrackComplaintQuery(string TrackingNumber);
public record TrackComplaintResponse(string TrackingNumber, string Status, string Description, DateTime SubmittedAt);
public record CaseCommunicationDto(Guid Id, string Message, DateTimeOffset SentAt);
public record CaseDocumentDto(Guid Id, string FileName, string ContentType, DateTimeOffset UploadedAt);
public record CaseFindingDto(Guid Id, string Description, DateTimeOffset CreatedAt);
public record CaseMilestoneDto(Guid Id, string Title, bool IsCompleted, DateTimeOffset? CompletedAt);
public record QueueResultDto(string QueueName, int TotalCount, IReadOnlyList<CaseListDto> Items);
"@

"TaxOmbud.Application.Chats.DTOs" = @"
public record CreateChatCommand(string Name, IEnumerable<Guid> ParticipantIds);
public record CreateChatResponse(Guid ChatId);
public record MarkMessageAsReadCommand(Guid ChatId, Guid MessageId);
public record PinMessageCommand(Guid ChatId, Guid MessageId, bool IsPinned);
public record SendMessageCommand(Guid ChatId, string Content);
public record GetChatsQuery(int Page = 1, int PageSize = 20);
public record GetChatMessagesQuery(Guid ChatId, int Page = 1, int PageSize = 50);
public record ChatDto(Guid Id, string Name, DateTimeOffset LastMessageAt);
public record ChatMessageDto(Guid Id, string Content, Guid SenderId, DateTimeOffset SentAt, bool IsRead);
"@

"TaxOmbud.Application.Communications.DTOs" = @"
public record AcknowledgeCommunicationCommand(Guid CommunicationId);
public record CreateAgentChatCommand(string Subject, IEnumerable<Guid> ParticipantIds);
public record CreateSmsMessageCommand(string To, string Body);
public record DeleteSmsMessageCommand(Guid SmsId);
public record GetAgentChatPreferencesQuery(Guid AgentId);
public record GetAgentChatsQuery(int Page = 1, int PageSize = 20);
public record GetCommunicationTemplatesQuery(string? Category);
public record GetSmsMessageByIdQuery(Guid SmsId);
public record GetSmsMessagesQuery(int Page = 1, int PageSize = 20);
public record RenderCommunicationTemplateCommand(Guid TemplateId, object Model);
public record SearchAgentsQuery(string? Search, int Page = 1, int PageSize = 20);
public record SendAgentChatMessageCommand(Guid ChatId, string Content);
public record SendCommunicationCommand(string To, string Subject, string Body, string Channel);
public record UpdateAgentChatPreferencesCommand(Guid AgentId, bool EmailNotifications, bool SmsNotifications);
public record UpdateSmsMessageCommand(Guid SmsId, string Body);
public record AgentChatDto(Guid Id, string Subject, DateTimeOffset CreatedAt);
public record AgentChatMessageDto(Guid Id, string Content, Guid SenderId, DateTimeOffset SentAt);
public record AgentChatPreferenceDto(bool EmailNotifications, bool SmsNotifications);
public record AgentSummaryDto(Guid Id, string Name, string Email);
public record CommunicationTemplateDto(Guid Id, string Name, string Category, string Body);
public record RenderedTemplateDto(string Subject, string Body);
public record SmsMessageDto(Guid Id, string To, string Body, string Status, DateTimeOffset SentAt);
"@

"TaxOmbud.Application.Contact.DTOs" = @"
public record SubmitContactFormCommand(string Name, string Email, string Subject, string Message);
"@

"TaxOmbud.Application.Crm.DTOs" = @"
public record CreateCallCommand(string Subject, string Direction, string Status, string Phone, string? Notes, Guid? AgentId);
public record CreateInteractionCommand(string Type, string Summary, Guid? ContactId);
public record CreateOrganizationCommand(string Name, string? Industry, string? Website);
public record DeleteCallCommand(Guid CallId);
public record DeleteInteractionCommand(Guid InteractionId);
public record DeleteOrganizationCommand(Guid OrganizationId);
public record GetCallByIdQuery(Guid CallId);
public record GetCallsQuery(int Page = 1, int PageSize = 20);
public record GetInteractionByIdQuery(Guid InteractionId);
public record GetInteractionsQuery(int Page = 1, int PageSize = 20);
public record GetOrganizationByIdQuery(Guid OrganizationId);
public record GetOrganizationsQuery(int Page = 1, int PageSize = 20);
public record UpdateCallCommand(Guid CallId, string? Notes, string? Status);
public record UpdateInteractionCommand(Guid InteractionId, string? Summary);
public record UpdateOrganizationCommand(Guid OrganizationId, string? Name, string? Industry);
public record CallDto(Guid Id, string Subject, string Direction, string Status, string Phone, DateTimeOffset? StartAt);
public record InteractionDto(Guid Id, string Type, string Summary, DateTimeOffset CreatedAt);
public record OrganizationDto(Guid Id, string Name, string? Industry, string? Website);
"@

"TaxOmbud.Application.Documents.DTOs" = @"
public record ClassifyDocumentCommand(Guid DocumentId, string Classification);
public record GetDocumentVersionsQuery(Guid DocumentId);
"@

"TaxOmbud.Application.Finance.DTOs" = @"
public record CreateContractCommand(string Title, Guid VendorId, decimal Value, DateTime StartDate, DateTime EndDate);
public record CreateQuoteCommand(string Title, Guid ClientId, decimal TotalAmount);
public record GenerateInvoiceCommand(Guid QuoteId);
public record GetContractsQuery(int Page = 1, int PageSize = 20);
public record GetInvoicesQuery(int Page = 1, int PageSize = 20);
public record GetQuotesQuery(int Page = 1, int PageSize = 20);
public record PayInvoiceCommand(Guid InvoiceId, decimal Amount, string PaymentMethod);
"@

"TaxOmbud.Application.Geo.DTOs" = @"
public record GetCountriesQuery();
public record GetStatesQuery(string? CountryId);
public record CountryDto(string Id, string Name, string Code);
public record StateDto(string Id, string Name, string CountryId);
"@

"TaxOmbud.Application.HrRequests.DTOs" = @"
public record ApproveLeaveRequestCommand(Guid RequestId, bool Approved, string? Reason);
public record GetEwaRequestsQuery(int Page = 1, int PageSize = 20);
public record GetLeaveRequestsQuery(int Page = 1, int PageSize = 20);
public record GetLoanRequestsQuery(int Page = 1, int PageSize = 20);
public record SubmitLeaveRequestCommand(DateTime StartDate, DateTime EndDate, string LeaveType, string Reason);
public record SubmitLoanRequestCommand(decimal Amount, string Purpose);
"@

"TaxOmbud.Application.IdentityVerification.DTOs" = @"
public record VerifyIdentityCommand(Guid UserId, string DocumentType, string DocumentNumber);
public record IdentityVerificationResponse(bool Verified, string? FailureReason);
"@

"TaxOmbud.Application.Lookups.DTOs" = @"
public record GetLookupsQuery(string Category);
public record LookupDto(string Key, string Value, string? Description);
"@

"TaxOmbud.Application.Notifications.DTOs" = @"
public record GetUnreadNotificationCountQuery(Guid UserId);
public record GetNotificationPreferencesQuery(Guid UserId);
public record UpdateNotificationPreferencesCommand(bool EmailEnabled, bool SmsEnabled, bool PushEnabled);
public record NotificationPreferenceDto(bool EmailEnabled, bool SmsEnabled, bool PushEnabled);
"@

"TaxOmbud.Application.Officers.DTOs" = @"
public record GetOfficerPerformanceQuery(Guid OfficerId, DateTime? From, DateTime? To);
public record OfficerPerformanceDto(Guid OfficerId, string Name, int CasesHandled, double AverageResolutionDays);
"@

"TaxOmbud.Application.Operations.DTOs" = @"
public record AddInventoryItemCommand(string Name, string Category, int Quantity, decimal UnitCost);
public record AddVendorCommand(string Name, string Email, string Phone, string? Address);
public record CreateProjectCommand(string Title, string Description, DateTime StartDate, DateTime EndDate);
public record DeleteVendorCommand(Guid VendorId);
public record GetInventoryItemsQuery(int Page = 1, int PageSize = 20);
public record GetProjectsQuery(int Page = 1, int PageSize = 20);
public record GetVendorByIdQuery(Guid VendorId);
public record GetVendorsQuery(int Page = 1, int PageSize = 20);
public record UpdateProjectStatusCommand(Guid ProjectId, string Status);
public record UpdateVendorCommand(Guid VendorId, string? Name, string? Email, string? Phone);
"@

"TaxOmbud.Application.Payroll.DTOs" = @"
public record ApprovePayrollCommand(Guid PayrollRunId, bool Approved);
public record CreateSalaryProfileCommand(Guid EmployeeId, decimal BasicSalary, decimal Allowances);
public record GetPayrollPeriodsQuery(int Page = 1, int PageSize = 20);
public record GetRemittancesQuery(int Page = 1, int PageSize = 20);
public record GetSalaryProfilesQuery(int Page = 1, int PageSize = 20);
public record RunPayrollCommand(DateTime PeriodStart, DateTime PeriodEnd);
"@

"TaxOmbud.Application.Reports.DTOs" = @"
public record ExportReportCommand(string ReportType, DateTime From, DateTime To, string Format);
public record ExportReportDto(byte[] Data, string FileName, string ContentType);
public record GetAgentReportsQuery(Guid? AgentId, DateTime? From, DateTime? To);
public record GetAnnualReportQuery(int Year);
public record GetCaseReportsQuery(DateTime? From, DateTime? To, string? Stage);
public record GetComplaintsByRegionQuery(DateTime? From, DateTime? To);
public record GetErpReportsQuery(DateTime? From, DateTime? To);
public record GetHrReportsQuery(DateTime? From, DateTime? To);
public record GetInteractionReportsQuery(DateTime? From, DateTime? To);
public record GetResolutionTimeReportQuery(DateTime? From, DateTime? To);
public record GetSlaReportsQuery(DateTime? From, DateTime? To);
public record GetTaskReportsQuery(DateTime? From, DateTime? To);
public record GetTimeTrackingReportsQuery(DateTime? From, DateTime? To);
public record AgentReportDto(Guid AgentId, string AgentName, int TotalCases, double AvgResolutionDays);
public record AnnualReportDto(int Year, int TotalComplaints, int Resolved, int Pending);
public record CaseReportDto(Guid CaseId, string Stage, string Status, DateTimeOffset CreatedAt);
public record RegionReportDto(string Region, int TotalComplaints, int Resolved);
public record ErpReportDto(string Category, int Count, decimal TotalValue);
public record HrReportDto(string Department, int HeadCount, decimal TotalPayroll);
public record InteractionReportDto(string Channel, int TotalInteractions, double AvgDurationMinutes);
public record ResolutionTimeDto(string Stage, double AverageDays, double MedianDays);
public record SlaReportDto(string Priority, int Total, int MetSla, double ComplianceRate);
public record TaskReportDto(string Status, int Count);
public record TimeTrackingReportDto(Guid EmployeeId, string Name, double TotalHours);
"@

"TaxOmbud.Application.Search.DTOs" = @"
public record GlobalSearchQuery(string Term, int Page = 1, int PageSize = 20);
public record SearchCasesQuery(string Term, int Page = 1, int PageSize = 20);
public record SearchComplaintsQuery(string Term, int Page = 1, int PageSize = 20);
public record SearchDocumentsQuery(string Term, int Page = 1, int PageSize = 20);
public record SearchTaxpayersQuery(string Term, int Page = 1, int PageSize = 20);
public record GlobalSearchResultDto(IReadOnlyList<CaseSearchResultDto> Cases, IReadOnlyList<ComplaintSearchResultDto> Complaints, IReadOnlyList<DocumentSearchResultDto> Documents, IReadOnlyList<TaxpayerSearchResultDto> Taxpayers);
public record CaseSearchResultDto(Guid Id, string CaseNumber, string Stage, string Status);
public record ComplaintSearchResultDto(Guid Id, string TrackingNumber, string Status);
public record DocumentSearchResultDto(Guid Id, string FileName, string ContentType);
public record TaxpayerSearchResultDto(Guid Id, string FirstName, string LastName, string Tin);
"@

"TaxOmbud.Application.System.DTOs" = @"
public record CreateAnnouncementCommand(string Title, string Body, DateTime? ExpiresAt);
"@

"TaxOmbud.Application.SystemSettings.DTOs" = @"
public record GetE2eeStatusQuery();
public record ToggleE2eeCommand(bool Enabled);
public record E2eeStatusDto(bool Enabled, DateTimeOffset? LastToggled);
"@

"TaxOmbud.Application.Tasks.DTOs" = @"
public record CreateCaseTaskCommand(Guid CaseId, string Title, string? Description, DateTime? DueDate, Guid? AssigneeId);
public record DeleteCaseTaskCommand(Guid TaskId);
public record UpdateCaseTaskCommand(Guid TaskId, string? Title, string? Description, DateTime? DueDate, string? Status);
public record GetCaseTaskByIdQuery(Guid TaskId);
public record GetCaseTasksQuery(Guid CaseId);
public record CaseTaskDto(Guid Id, string Title, string? Description, string Status, DateTime? DueDate);
"@

"TaxOmbud.Application.Wallet.DTOs" = @"
public record GetWalletBalanceQuery(Guid UserId);
public record GetWalletTransactionsQuery(Guid UserId, int Page = 1, int PageSize = 20);
public record ProcessWithdrawalCommand(Guid WithdrawalRequestId, bool Approved, string? Reason);
public record RequestWithdrawalCommand(decimal Amount, string BankName, string AccountNumber);
"@
}

Write-Host "=== Generating module stubs ==="

foreach ($ns in $moduleStubs.Keys) {
    # Derive folder path from namespace
    # e.g., TaxOmbud.Application.Cases.DTOs → src/TaxOmbud.Application/Cases/DTOs
    $parts     = $ns -replace '^TaxOmbud\.Application\.', '' -split '\.'
    $folderRel = $parts -join '\'
    $folder    = Join-Path $appDir $folderRel

    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }

    $stubFile = Join-Path $folder "_Stubs.cs"
    $content  = @"
// Auto-generated stubs — replace with real implementations as needed.
namespace $ns;

$($moduleStubs[$ns].Trim())
"@

    Set-Content -Path $stubFile -Value $content -Encoding UTF8
    Write-Host "  CREATED: $($stubFile.Replace($appDir, '').TrimStart('\'))"
}

Write-Host ""
Write-Host "Stubs generated."
