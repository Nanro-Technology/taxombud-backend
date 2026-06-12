using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Infrastructure.Migrations.MySql
{
    /// <inheritdoc />
    public partial class Phase5_HR_Expansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKin",
                table: "StaffProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EducationDetails",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "StaffProfiles",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "StaffProfiles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinAddress",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinName",
                table: "StaffProfiles",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinPhone",
                table: "StaffProfiles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKinRelationship",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "SupervisorId",
                table: "StaffProfiles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "StaffProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DepartmentMovement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StaffProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromDepartmentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ToDepartmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MovementDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentMovement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentMovement_StaffProfiles_StaffProfileId",
                        column: x => x.StaffProfileId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StaffDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StaffProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffDocument_StaffProfiles_StaffProfileId",
                        column: x => x.StaffProfileId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StaffNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StaffProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffNote_StaffProfiles_StaffProfileId",
                        column: x => x.StaffProfileId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMovement_StaffProfileId",
                table: "DepartmentMovement",
                column: "StaffProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDocument_StaffProfileId",
                table: "StaffDocument",
                column: "StaffProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffNote_StaffProfileId",
                table: "StaffNote",
                column: "StaffProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentMovement");

            migrationBuilder.DropTable(
                name: "StaffDocument");

            migrationBuilder.DropTable(
                name: "StaffNote");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "City",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "EducationDetails",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinAddress",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinName",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinPhone",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "NextOfKinRelationship",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "State",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "StaffProfiles");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "StaffProfiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NextOfKin",
                table: "StaffProfiles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
