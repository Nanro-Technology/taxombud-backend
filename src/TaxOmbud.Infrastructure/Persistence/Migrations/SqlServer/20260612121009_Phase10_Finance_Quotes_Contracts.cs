using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Infrastructure.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Phase10_Finance_Quotes_Contracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Quotes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Quotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Quotes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Quotes",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Quotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedDate",
                table: "Quotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Quotes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Quotes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "ParentType",
                table: "Quotes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Quotes",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Quotes",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Quotes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Quotes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedAgentId",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Contracts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Contracts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Contracts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Contracts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "ParentType",
                table: "Contracts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReminderCycleDays",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalDate",
                table: "Contracts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceQuoteId",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Contracts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ContractReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReviewTicketId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReviewDepartmentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_ContractReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractReviews_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuoteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
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
                    table.PrimaryKey("PK_QuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteItems_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ContractReviews_ContractId",
                table: "ContractReviews",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_QuoteId",
                table: "QuoteItems",
                column: "QuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractReviews");

            migrationBuilder.DropTable(
                name: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "IssuedDate",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ParentType",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "AssignedAgentId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ParentType",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ReminderCycleDays",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "RenewalDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SourceQuoteId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Contracts");
        }
    }
}
