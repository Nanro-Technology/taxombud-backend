namespace TaxOmbud.Common.Utilities;

public static class Constants
{
    public static class Messages
    {
        public const string Success = "Operation completed successfully.";
        public const string Created = "Resource created successfully.";
        public const string Updated = "Resource updated successfully.";
        public const string Deleted = "Resource deleted successfully.";
        public const string NotFound = "Resource not found.";
        public const string Unauthorized = "Authentication required.";
        public const string Forbidden = "Access denied.";
        public const string BadRequest = "Invalid request data.";
        public const string ServerError = "An unexpected error occurred. Please try again.";
        public const string DuplicateEntry = "A record with the provided details already exists.";
        public const string InvalidCredentials = "Invalid email or password.";
        public const string AccountLocked = "Your account has been locked. Please contact support.";
        public const string EmailNotVerified = "Please verify your email address before logging in.";
        public const string OtpSent = "OTP sent to your registered email/phone.";
        public const string OtpInvalid = "The OTP provided is invalid or has expired.";
        public const string OtpVerified = "OTP verified successfully.";
        public const string PasswordReset = "Password reset successfully.";
        public const string TokenRefreshed = "Token refreshed successfully.";
        public const string LogoutSuccess = "Logged out successfully.";

        // Complaints
        public const string ComplaintSubmitted = "Complaint submitted successfully.";
        public const string ComplaintNotFound = "Complaint not found.";
        public const string ComplaintAssigned = "Complaint assigned successfully.";
        public const string ComplaintEscalated = "Complaint escalated successfully.";
        public const string ComplaintResolved = "Complaint resolved successfully.";
        public const string ComplaintClosed = "Complaint closed successfully.";
        public const string ComplaintReopened = "Complaint reopened successfully.";
        public const string ComplaintUpdated = "Complaint updated successfully.";
        public const string ComplaintStatusUpdated = "Complaint status updated.";
        public const string ComplaintDeleted = "Complaint deleted successfully.";
        public const string ComplaintsLinked = "Complaints linked successfully.";
        public const string DocumentUploaded = "Document uploaded successfully.";
        public const string ComplaintsRetrieved = "Complaints retrieved successfully.";
        public const string ComplaintRetrieved = "Complaint retrieved successfully.";
        public const string NotesRetrieved = "Notes retrieved successfully.";
        public const string DocumentsRetrieved = "Documents retrieved successfully.";
        public const string TimelineRetrieved = "Timeline retrieved successfully.";
        public const string RelatedComplaintsRetrieved = "Related complaints retrieved successfully.";
        public const string NoteAdded = "Note added successfully.";
        public const string ComplaintRetrieveError = "An error occurred while retrieving complaints.";
        public const string ComplaintGetError = "An error occurred while retrieving the complaint.";
        public const string ComplaintNotesError = "An error occurred while retrieving notes.";
        public const string ComplaintDocsError = "An error occurred while retrieving documents.";
        public const string ComplaintTimelineError = "An error occurred while retrieving the timeline.";
        public const string ComplaintRelatedError = "An error occurred while retrieving related complaints.";
        public const string ComplaintSubmitError = "An error occurred while submitting the complaint.";
        public const string ComplaintNoteAddError = "An error occurred while adding the note.";
        public const string ComplaintUpdateError = "An error occurred while updating the complaint.";
        public const string ComplaintStatusUpdateError = "An error occurred while updating the status.";
        public const string ComplaintDeleteError = "An error occurred while deleting the complaint.";
        public const string ComplaintLinkError = "An error occurred while linking complaints.";
        public const string ComplaintDocUploadError = "An error occurred while uploading the document.";

        // Cases
        public const string CaseCreated = "Case created successfully.";
        public const string CaseNotFound = "Case not found.";
        public const string CaseUpdated = "Case updated successfully.";
        public const string CaseAssigned = "Case assigned successfully.";
        public const string CaseClosed = "Case closed successfully.";
        public const string CaseTaskAdded = "Case task added successfully.";
        public const string CaseTaskCompleted = "Case task completed successfully.";
        public const string TimeLogged = "Time logged successfully.";
        public const string CaseTimelineRetrieved = "Case timeline retrieved successfully.";
        public const string CasesRetrieved = "Cases retrieved successfully.";
        public const string CaseRetrieved = "Case retrieved successfully.";
        public const string CaseTasksRetrieved = "Case tasks retrieved successfully.";
        public const string TimeLogsRetrieved = "Time logs retrieved successfully.";
        public const string CaseRetrieveError = "An error occurred while retrieving cases.";
        public const string CaseQueueRetrieved = "Queue retrieved successfully.";
        public const string CaseQueueError = "An error occurred while retrieving the queue.";
        public const string CaseGetError = "An error occurred while retrieving the case.";
        public const string CaseFindingsRetrieved = "Findings retrieved successfully.";
        public const string CaseFindingsError = "An error occurred while retrieving findings.";
        public const string CaseMilestonesRetrieved = "Milestones retrieved successfully.";
        public const string CaseMilestonesError = "An error occurred while retrieving milestones.";
        public const string CaseCommsRetrieved = "Communications retrieved successfully.";
        public const string CaseCommsError = "An error occurred while retrieving communications.";
        public const string CaseDocsError = "An error occurred while retrieving documents.";
        public const string ComplaintTrackNotFound = "No complaint found with the given tracking number.";
        public const string ComplaintTracked = "Complaint tracked successfully.";
        public const string ComplaintTrackError = "An error occurred while tracking the complaint.";
        public const string CaseSubmitError = "An error occurred while submitting the case.";
        public const string CaseNoteAddError = "An error occurred while adding the note.";
        public const string CaseFindingAdded = "Finding added successfully.";
        public const string CaseFindingAddError = "An error occurred while adding the finding.";
        public const string CaseFindingNotFound = "Finding not found.";
        public const string CaseFindingUpdated = "Finding updated successfully.";
        public const string CaseFindingUpdateError = "An error occurred while updating the finding.";
        public const string CaseAssignError = "An error occurred while assigning the case.";
        public const string CaseTransitionError = "An error occurred while transitioning the case.";
        public const string CaseRecommendationPosted = "Recommendation posted successfully.";
        public const string CaseRecommendationError = "An error occurred while posting the recommendation.";
        public const string CaseCeApprovalRationaleShort = "CE approval rationale must be at least 100 characters.";
        public const string CaseClosureApprovalError = "An error occurred while processing closure approval.";
        public const string CaseMilestoneNotFound = "Milestone not found.";
        public const string CaseMilestoneCompleted = "Milestone completed.";
        public const string CaseMilestoneCompleteError = "An error occurred while completing the milestone.";
        public const string CaseDocUploadError = "An error occurred while uploading the document.";

        // Appeals
        public const string AppealSubmitted = "Appeal submitted successfully.";
        public const string AppealNotFound = "Appeal not found.";
        public const string AppealStatusUpdated = "Appeal status updated successfully.";
        public const string AppealRetrieved = "Appeal retrieved successfully.";
        public const string AppealsRetrieved = "Appeals retrieved successfully.";
        public const string AppealClosedCaseOnly = "Appeals can only be filed against closed cases.";
        public const string AppealFileError = "An error occurred while filing the appeal.";
        public const string AppealReviewError = "An error occurred while reviewing the appeal.";
        public const string AppealGetError = "An error occurred while retrieving the appeal.";
        public const string AppealRetrieveError = "An error occurred while retrieving the appeals.";

        // Auth
        public const string InvalidRequest = "Invalid request.";
        public const string MfaRequired = "MFA is required. An OTP has been sent.";
        public const string AuthUserNotFound = "User not found.";
        public const string AuthPasswordIncorrect = "Current password is incorrect.";
        public const string AuthPasswordConfirmationFailed = "Password confirmation failed.";
        public const string AuthAccountDisabled = "This account has been disabled.";
        public const string AuthInvalidRefreshToken = "Invalid refresh token.";
        public const string AuthRefreshTokenRevoked = "Refresh token has been revoked.";
        public const string AuthRefreshTokenExpired = "Refresh token has expired. Please log in again.";
        public const string AuthInvalidResetToken = "Invalid or expired reset token.";
        public const string AuthInvalidVerificationToken = "Invalid or expired verification token.";
        public const string AuthVerificationTokenExpired = "Verification token has expired. Please request a new one.";
        public const string AuthMfaNotSetUp = "MFA has not been set up. Please call /mfa/setup first.";
        public const string AuthInvalidTotp = "Invalid TOTP code.";
    }

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Officer = "Officer";
        public const string Taxpayer = "Taxpayer";
        public const string Investigator = "Investigator";
        public const string Auditor = "Auditor";
        public const string CaseManager = "CaseManager";
    }

    public static class ClaimTypes
    {
        public const string UserId = "uid";
        public const string Email = "email";
        public const string Role = "role";
        public const string FullName = "name";
        public const string TaxpayerId = "taxpayer_id";
    }

    public static class CacheKeys
    {
        public const string AllUsers = "all_users";
        public const string AllRoles = "all_roles";
        public const string SystemSettings = "system_settings";
        public static string UserById(string id) => $"user_{id}";
        public static string OtpByEmail(string email) => $"otp_{email}";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireOfficerRole = "RequireOfficerRole";
        public const string RequireTaxpayerRole = "RequireTaxpayerRole";
    }
}
