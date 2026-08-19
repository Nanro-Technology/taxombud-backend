using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Persistence.Migrations.MySqlApplicationDb
{
    /// <inheritdoc />
    public partial class AddCaseCurrentSubStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentSubStage",
                table: "Cases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedUserId",
                table: "CaseApprovalTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSubStage",
                table: "Cases");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedUserId",
                table: "CaseApprovalTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
