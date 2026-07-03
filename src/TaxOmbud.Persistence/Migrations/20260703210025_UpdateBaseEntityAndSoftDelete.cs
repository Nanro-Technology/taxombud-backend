using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBaseEntityAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VendorContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VendorContacts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserDashboardLayouts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserDashboardLayouts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Taxpayers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Taxpayers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TaxpayerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TaxpayerProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TaxpayerContactDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TaxpayerContactDetails");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TaxpayerAddresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TaxpayerAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StatutoryRules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StatutoryRules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StatutoryDeductions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StatutoryDeductions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffNote");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StaffNote");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StaffDocument");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StaffDocument");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SmsMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SmsMessages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ScheduledReports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ScheduledReports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalaryProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SalaryProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Remittances");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Remittances");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PerformanceGoals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PerformanceGoals");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PerformanceCycles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PerformanceCycles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OfficerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OfficerProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OfficerPerformanceRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OfficerPerformanceRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OfficerCaseloads");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OfficerCaseloads");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MfaTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MailboxRecipients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MailboxRecipients");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MailboxMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MailboxMessages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MailboxAttachments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MailboxAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveTypeEntities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LeaveTypeEntities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExitRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExitRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EwaRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EwaRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmployeeWallets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeeBenefits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmployeeBenefits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DisciplinaryCases");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DisciplinaryCases");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DepartmentMovement");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DepartmentMovement");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DashboardWidgets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DashboardWidgets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContractReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContractReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ComplaintStatusHistory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ComplaintNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ComplaintNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ComplaintLinks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CommunicationTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CommunicationTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Communications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Communications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseStatusHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseRecommendations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseRecommendations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseMilestones");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseMilestones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseFindings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseFindings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseCommunicationLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CaseCommunicationLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BenefitTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BenefitTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AppealStatusHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AppealGroundPoints");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AppealGroundPoints");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AnnouncementReadReceipts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AnnouncementReadReceipts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AgentChatPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AgentChatPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AgentChatMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AgentChatMessages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "WebhookSubscriptions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "WebhookSubscriptions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Visitors",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Visitors",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "VendorContacts",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "VendorContacts",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Users",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Users",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Users",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "UserDashboardLayouts",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "UserDashboardLayouts",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TimeLogs",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "TimeLogs",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Tickets",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Tickets",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Taxpayers",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Taxpayers",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TaxpayerProfiles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "TaxpayerProfiles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TaxpayerContactDetails",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "TaxpayerContactDetails",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TaxpayerAddresses",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "TaxpayerAddresses",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "SystemSettings",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "SystemSettings",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StatutoryRules",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StatutoryRules",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StatutoryDeductions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StatutoryDeductions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StaffProfiles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StaffProfiles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StaffNote",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StaffNote",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StaffDocument",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "StaffDocument",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "SmsMessages",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "SmsMessages",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ScheduledReports",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ScheduledReports",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "SalaryProfiles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "SalaryProfiles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Roles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Roles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "RolePermissions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "RolePermissions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Remittances",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Remittances",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Quotes",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Quotes",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "QuoteItems",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "QuoteItems",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Projects",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Projects",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ProjectMembers",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ProjectMembers",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Permissions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Permissions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PerformanceReviews",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PerformanceReviews",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PerformanceGoals",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PerformanceGoals",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PerformanceCycles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PerformanceCycles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PayrollRuns",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PayrollRuns",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PayrollPeriods",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PayrollPeriods",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PayoutProviders",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PayoutProviders",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PayGrades",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "PayGrades",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Organizations",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Organizations",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Officers",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Officers",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "OfficerProfiles",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "OfficerProfiles",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "OfficerPerformanceRecords",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "OfficerPerformanceRecords",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "OfficerCaseloads",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "OfficerCaseloads",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Notifications",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Notifications",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "NotificationPreferences",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "NotificationPreferences",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "MailboxRecipients",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "MailboxRecipients",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "MailboxMessages",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "MailboxMessages",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "MailboxAttachments",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "MailboxAttachments",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LoanRequests",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "LoanRequests",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveTypeEntities",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "LeaveTypeEntities",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveRequests",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "LeaveRequests",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveBalances",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "LeaveBalances",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Invoices",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "InvoiceItems",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Interactions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Interactions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Holidays",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Holidays",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "FeatureFlags",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "FeatureFlags",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ExitRecords",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ExitRecords",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "EwaRequests",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "EwaRequests",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "EmployeeBenefits",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "EmployeeBenefits",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "DocumentVersions",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "DocumentVersions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Documents",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Documents",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "DisciplinaryCases",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "DisciplinaryCases",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Departments",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Departments",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "DepartmentMovement",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "DepartmentMovement",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "DashboardWidgets",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "DashboardWidgets",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Contracts",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Contracts",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ContractReviews",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ContractReviews",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Complaints",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Complaints",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ComplaintNotes",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "ComplaintNotes",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CommunicationTemplates",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CommunicationTemplates",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Communications",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Communications",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CommunicationLogs",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CommunicationLogs",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseTasks",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseTasks",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Cases",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Cases",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseRecommendations",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseRecommendations",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseNotes",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseNotes",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseMilestones",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseMilestones",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseFindings",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseFindings",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CaseCommunicationLogs",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CaseCommunicationLogs",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Calls",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Calls",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CalendarEvents",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CalendarEvents",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "BenefitTypes",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "BenefitTypes",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AttendanceLogs",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AttendanceLogs",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Appointments",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Appointments",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Appeals",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Appeals",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AppealGroundPoints",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AppealGroundPoints",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Announcements",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Announcements",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AnnouncementReadReceipts",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AnnouncementReadReceipts",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AgentChats",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AgentChats",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AgentChatPreferences",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AgentChatPreferences",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AgentChatMessages",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "AgentChatMessages",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Account",
                newName: "LastModifiedByUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Account",
                newName: "CreatedByUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WebhookSubscriptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "WebhookSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WebhookDeliveries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "WebhookDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "WebhookDeliveries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "WebhookDeliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WalletTransactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "WalletTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Visitors",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Visitors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorContacts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "VendorContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserDashboardLayouts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "UserDashboardLayouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TimeLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "TimeLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Taxpayers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Taxpayers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TaxpayerProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "TaxpayerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TaxpayerContactDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "TaxpayerContactDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TaxpayerAddresses",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "TaxpayerAddresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemSettings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "SystemSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StatutoryRules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "StatutoryRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StatutoryDeductions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "StatutoryDeductions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "StaffProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffNote",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "StaffNote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffDocument",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "StaffDocument",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SmsMessages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "SmsMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ScheduledReports",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ScheduledReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SalaryProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "SalaryProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RolePermissions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "RolePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Remittances",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Remittances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Quotes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Quotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "QuoteItems",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "QuoteItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProjectMembers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ProjectMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PerformanceReviews",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PerformanceReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PerformanceGoals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PerformanceGoals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PerformanceCycles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PerformanceCycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PayrollRuns",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PayrollRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PayrollPeriods",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PayrollPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PayrollEntries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PayrollEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PayrollEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "PayrollEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PayoutProviders",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PayoutProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PayGrades",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PayGrades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Organizations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Organizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Officers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Officers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OfficerProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "OfficerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OfficerPerformanceRecords",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "OfficerPerformanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OfficerCaseloads",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "OfficerCaseloads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MfaTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "MfaTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "MfaTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "MfaTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MailboxRecipients",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "MailboxRecipients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MailboxMessages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "MailboxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MailboxAttachments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "MailboxAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LoanRequests",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "LoanRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveTypeEntities",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "LeaveTypeEntities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveBalances",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "LeaveBalances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Invoices",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "InvoiceItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "InvoiceItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "InvoiceItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Interactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Interactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Holidays",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Holidays",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "FeatureFlags",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "FeatureFlags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ExitRecords",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ExitRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EwaRequests",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "EwaRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EmployeeWallets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "EmployeeWallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "EmployeeWallets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "EmployeeWallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EmployeeBenefits",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "EmployeeBenefits",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentVersions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "DocumentVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DisciplinaryCases",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "DisciplinaryCases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DepartmentMovement",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "DepartmentMovement",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DashboardWidgets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "DashboardWidgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContractReviews",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ContractReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ComplaintStatusHistory",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ComplaintStatusHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ComplaintStatusHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "ComplaintStatusHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Complaints",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Complaints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ComplaintNotes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ComplaintNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ComplaintLinks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ComplaintLinks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "ComplaintLinks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "ComplaintLinks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CommunicationTemplates",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CommunicationTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Communications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Communications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CommunicationLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CommunicationLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseTasks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseStatusHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "CaseStatusHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseStatusHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "CaseStatusHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Cases",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseRecommendations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseRecommendations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseNotes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseMilestones",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseMilestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseFindings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseFindings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CaseCommunicationLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CaseCommunicationLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Calls",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Calls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CalendarEvents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CalendarEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BenefitTypes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "BenefitTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AttendanceLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AttendanceLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AppealStatusHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "AppealStatusHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AppealStatusHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByUserId",
                table: "AppealStatusHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Appeals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Appeals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AppealGroundPoints",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AppealGroundPoints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AnnouncementReadReceipts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AnnouncementReadReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AgentChats",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AgentChats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AgentChatPreferences",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AgentChatPreferences",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AgentChatMessages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "AgentChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Account",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Account",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "WebhookSubscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "VendorContacts");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "UserDashboardLayouts");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "TimeLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Taxpayers");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "TaxpayerProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "TaxpayerContactDetails");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "TaxpayerAddresses");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "StatutoryRules");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "StatutoryDeductions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "StaffNote");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "StaffDocument");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "SmsMessages");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ScheduledReports");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "SalaryProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Remittances");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PerformanceGoals");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PerformanceCycles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "OfficerProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "OfficerPerformanceRecords");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "OfficerCaseloads");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MfaTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "MfaTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "MfaTokens");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "MailboxRecipients");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "MailboxMessages");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "MailboxAttachments");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "LeaveTypeEntities");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ExitRecords");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "EwaRequests");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "EmployeeWallets");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "EmployeeWallets");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "EmployeeWallets");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "EmployeeBenefits");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "DisciplinaryCases");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "DepartmentMovement");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "DashboardWidgets");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ContractReviews");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ComplaintStatusHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ComplaintStatusHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "ComplaintStatusHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ComplaintNotes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ComplaintLinks");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "ComplaintLinks");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "ComplaintLinks");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CommunicationTemplates");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Communications");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseTasks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CaseStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "CaseStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseRecommendations");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseMilestones");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseFindings");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CaseCommunicationLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "BenefitTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AppealStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AppealStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "AppealStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AppealGroundPoints");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AnnouncementReadReceipts");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AgentChatPreferences");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "AgentChatMessages");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "WebhookSubscriptions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "WebhookSubscriptions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Visitors",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Visitors",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "VendorContacts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "VendorContacts",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Users",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Users",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "UserDashboardLayouts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "UserDashboardLayouts",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "TimeLogs",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TimeLogs",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Tickets",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Tickets",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Taxpayers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Taxpayers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "TaxpayerProfiles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TaxpayerProfiles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "TaxpayerContactDetails",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TaxpayerContactDetails",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "TaxpayerAddresses",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TaxpayerAddresses",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "SystemSettings",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "SystemSettings",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "StatutoryRules",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "StatutoryRules",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "StatutoryDeductions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "StatutoryDeductions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "StaffProfiles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "StaffProfiles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "StaffNote",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "StaffNote",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "StaffDocument",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "StaffDocument",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "SmsMessages",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "SmsMessages",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "ScheduledReports",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "ScheduledReports",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "SalaryProfiles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "SalaryProfiles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Roles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Roles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "RolePermissions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "RolePermissions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Remittances",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Remittances",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Quotes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Quotes",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "QuoteItems",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "QuoteItems",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Projects",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Projects",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "ProjectMembers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "ProjectMembers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Permissions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Permissions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PerformanceReviews",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PerformanceReviews",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PerformanceGoals",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PerformanceGoals",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PerformanceCycles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PerformanceCycles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PayrollRuns",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PayrollRuns",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PayrollPeriods",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PayrollPeriods",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PayoutProviders",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PayoutProviders",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "PayGrades",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "PayGrades",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Organizations",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Organizations",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Officers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Officers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "OfficerProfiles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "OfficerProfiles",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "OfficerPerformanceRecords",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "OfficerPerformanceRecords",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "OfficerCaseloads",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "OfficerCaseloads",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Notifications",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Notifications",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "NotificationPreferences",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "NotificationPreferences",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "MailboxRecipients",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "MailboxRecipients",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "MailboxMessages",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "MailboxMessages",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "MailboxAttachments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "MailboxAttachments",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "LoanRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "LoanRequests",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "LeaveTypeEntities",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "LeaveTypeEntities",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "LeaveRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "LeaveRequests",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "LeaveBalances",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "LeaveBalances",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "Invoices",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "InvoiceItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Interactions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Interactions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Holidays",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Holidays",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "FeatureFlags",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "FeatureFlags",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "ExitRecords",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "ExitRecords",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "EwaRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "EwaRequests",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "EmployeeBenefits",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "EmployeeBenefits",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "DocumentVersions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "DocumentVersions",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Documents",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Documents",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "DisciplinaryCases",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "DisciplinaryCases",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Departments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Departments",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "DepartmentMovement",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "DepartmentMovement",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "DashboardWidgets",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "DashboardWidgets",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Contracts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Contracts",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "ContractReviews",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "ContractReviews",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Complaints",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Complaints",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "ComplaintNotes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "ComplaintNotes",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CommunicationTemplates",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CommunicationTemplates",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Communications",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Communications",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CommunicationLogs",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CommunicationLogs",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseTasks",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseTasks",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Cases",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Cases",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseRecommendations",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseRecommendations",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseNotes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseNotes",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseMilestones",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseMilestones",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseFindings",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseFindings",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CaseCommunicationLogs",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CaseCommunicationLogs",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Calls",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Calls",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "CalendarEvents",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CalendarEvents",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "BenefitTypes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "BenefitTypes",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AttendanceLogs",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AttendanceLogs",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Appointments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Appointments",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Appeals",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Appeals",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AppealGroundPoints",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AppealGroundPoints",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Announcements",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Announcements",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AnnouncementReadReceipts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AnnouncementReadReceipts",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AgentChats",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AgentChats",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AgentChatPreferences",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AgentChatPreferences",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "AgentChatMessages",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "AgentChatMessages",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedByUserId",
                table: "Account",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Account",
                newName: "DeletedBy");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "WebhookSubscriptions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "WebhookSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "WebhookSubscriptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "WebhookDeliveries",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "WebhookDeliveries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "WalletTransactions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "WalletTransactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Visitors",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Visitors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Visitors",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "VendorContacts",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "VendorContacts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "VendorContacts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "UserDashboardLayouts",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserDashboardLayouts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "UserDashboardLayouts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "TimeLogs",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TimeLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "TimeLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tickets",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tickets",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Taxpayers",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Taxpayers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Taxpayers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "TaxpayerProfiles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TaxpayerProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "TaxpayerProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "TaxpayerContactDetails",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TaxpayerContactDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "TaxpayerContactDetails",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "TaxpayerAddresses",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TaxpayerAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "TaxpayerAddresses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SystemSettings",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SystemSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SystemSettings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StatutoryRules",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StatutoryRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StatutoryRules",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StatutoryDeductions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StatutoryDeductions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StatutoryDeductions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StaffProfiles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StaffProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StaffProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StaffNote",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StaffNote",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StaffNote",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StaffDocument",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StaffDocument",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StaffDocument",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SmsMessages",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SmsMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SmsMessages",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ScheduledReports",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ScheduledReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ScheduledReports",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SalaryProfiles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SalaryProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SalaryProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Roles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Roles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RolePermissions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "RolePermissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Remittances",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Remittances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Remittances",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Quotes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Quotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Quotes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "QuoteItems",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "QuoteItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "QuoteItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Projects",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Projects",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ProjectMembers",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ProjectMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProjectMembers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Permissions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Permissions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PerformanceReviews",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PerformanceReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PerformanceReviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PerformanceGoals",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PerformanceGoals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PerformanceGoals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PerformanceCycles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PerformanceCycles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PerformanceCycles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PayrollRuns",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PayrollRuns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PayrollRuns",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PayrollPeriods",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PayrollPeriods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PayrollPeriods",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PayrollEntries",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PayrollEntries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PayoutProviders",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PayoutProviders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PayoutProviders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PayGrades",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PayGrades",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PayGrades",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Organizations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Officers",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Officers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Officers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "OfficerProfiles",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "OfficerProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "OfficerProfiles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "OfficerPerformanceRecords",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "OfficerPerformanceRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "OfficerPerformanceRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "OfficerCaseloads",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "OfficerCaseloads",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "OfficerCaseloads",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Notifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "NotificationPreferences",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "NotificationPreferences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "NotificationPreferences",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MfaTokens",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MfaTokens",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MailboxRecipients",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MailboxRecipients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MailboxRecipients",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MailboxMessages",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MailboxMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MailboxMessages",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MailboxAttachments",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MailboxAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MailboxAttachments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LoanRequests",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LoanRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LoanRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LeaveTypeEntities",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LeaveTypeEntities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LeaveTypeEntities",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LeaveRequests",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LeaveRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LeaveBalances",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LeaveBalances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LeaveBalances",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Interactions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Interactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Interactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Holidays",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Holidays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Holidays",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "FeatureFlags",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "FeatureFlags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "FeatureFlags",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ExitRecords",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ExitRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ExitRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "EwaRequests",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "EwaRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "EwaRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "EmployeeWallets",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "EmployeeWallets",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "EmployeeBenefits",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "EmployeeBenefits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "EmployeeBenefits",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DocumentVersions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "DocumentVersions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DocumentVersions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Documents",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Documents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DisciplinaryCases",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "DisciplinaryCases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DisciplinaryCases",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Departments",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Departments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DepartmentMovement",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "DepartmentMovement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DepartmentMovement",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DashboardWidgets",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "DashboardWidgets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DashboardWidgets",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Contracts",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Contracts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ContractReviews",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ContractReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ContractReviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ComplaintStatusHistory",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ComplaintStatusHistory",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Complaints",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Complaints",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ComplaintNotes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ComplaintNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ComplaintNotes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ComplaintLinks",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ComplaintLinks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CommunicationTemplates",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CommunicationTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CommunicationTemplates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Communications",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Communications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Communications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CommunicationLogs",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CommunicationLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CommunicationLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseTasks",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseTasks",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseStatusHistories",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseStatusHistories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Cases",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Cases",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseRecommendations",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseRecommendations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseRecommendations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseNotes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseNotes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseMilestones",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseMilestones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseMilestones",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseFindings",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseFindings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseFindings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CaseCommunicationLogs",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CaseCommunicationLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CaseCommunicationLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Calls",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Calls",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Calls",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CalendarEvents",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CalendarEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CalendarEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "BenefitTypes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "BenefitTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "BenefitTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AttendanceLogs",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AttendanceLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AttendanceLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Appointments",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Appointments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AppealStatusHistories",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AppealStatusHistories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Appeals",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Appeals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Appeals",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AppealGroundPoints",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AppealGroundPoints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AppealGroundPoints",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Announcements",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Announcements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Announcements",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AnnouncementReadReceipts",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AnnouncementReadReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AnnouncementReadReceipts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AgentChats",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AgentChats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AgentChats",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AgentChatPreferences",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AgentChatPreferences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AgentChatPreferences",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AgentChatMessages",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AgentChatMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AgentChatMessages",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Account",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Account",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Account",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
