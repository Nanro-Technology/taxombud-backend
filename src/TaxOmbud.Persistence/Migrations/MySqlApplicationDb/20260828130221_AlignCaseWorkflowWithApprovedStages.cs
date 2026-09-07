using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Persistence.Migrations.MySqlApplicationDb
{
    /// <inheritdoc />
    public partial class AlignCaseWorkflowWithApprovedStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSubStage",
                table: "Cases");

            migrationBuilder.AddColumn<int>(
                name: "IntakeChannel",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "NotAdmissibleReason",
                table: "Complaints",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Cases",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntakeChannel",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Cases",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BoundToStage",
                table: "AgentChats",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "CaseId",
                table: "AgentChats",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "AgentChats",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedAt",
                table: "AgentChats",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "AgentChatMessages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "AgentChatMessages",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "message")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "JurisdictionCheck",
                table: "AdmissibilityAssessments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AgentChatParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgentChatId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantTier = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "role")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsReadOnly = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModifiedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentChatParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentChatParticipants_AgentChats_AgentChatId",
                        column: x => x.AgentChatId,
                        principalTable: "AgentChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CaseArchiveRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CaseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ArchivePurpose = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArchivedDocumentRefs = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArchivedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModifiedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseArchiveRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseArchiveRecords_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotAdmissibleDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CaseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdmissibilityAssessmentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeclaredByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeclaredAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    NotificationSent = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    NotificationSentAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModifiedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotAdmissibleDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotAdmissibleDecisions_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AgentChats_CaseId_Stage",
                table: "AgentChats",
                columns: new[] { "CaseId", "BoundToStage" });

            migrationBuilder.CreateIndex(
                name: "UX_AgentChatParticipants_ChatId_UserId",
                table: "AgentChatParticipants",
                columns: new[] { "AgentChatId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseArchiveRecords_CaseId",
                table: "CaseArchiveRecords",
                column: "CaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotAdmissibleDecisions_CaseId",
                table: "NotAdmissibleDecisions",
                column: "CaseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AgentChats_Cases_CaseId",
                table: "AgentChats",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentChats_Cases_CaseId",
                table: "AgentChats");

            migrationBuilder.DropTable(
                name: "AgentChatParticipants");

            migrationBuilder.DropTable(
                name: "CaseArchiveRecords");

            migrationBuilder.DropTable(
                name: "NotAdmissibleDecisions");

            migrationBuilder.DropIndex(
                name: "IX_AgentChats_CaseId_Stage",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "IntakeChannel",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "NotAdmissibleReason",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IntakeChannel",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "BoundToStage",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "AgentChats");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "AgentChatMessages");

            migrationBuilder.DropColumn(
                name: "JurisdictionCheck",
                table: "AdmissibilityAssessments");

            migrationBuilder.AddColumn<string>(
                name: "CurrentSubStage",
                table: "Cases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "AgentChatMessages",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);
        }
    }
}
