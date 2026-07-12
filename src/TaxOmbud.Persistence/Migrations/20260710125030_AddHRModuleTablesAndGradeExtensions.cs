using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHRModuleTablesAndGradeExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "WalletTransactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttemptNumber",
                table: "WalletTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BankDetail",
                table: "WalletTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "WalletTransactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderRef",
                table: "WalletTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WalletTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CaldavPassword",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RateOrAmountStr",
                table: "StatutoryRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployee",
                table: "StatutoryDeductions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployer",
                table: "StatutoryDeductions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Remittances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PayrollRuns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmployeesCount",
                table: "PayrollRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PayrollPeriods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Adapter",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookSecret",
                table: "PayoutProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PayGrades",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PayGrades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxSalary",
                table: "PayGrades",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSalary",
                table: "PayGrades",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSalaryAdvance",
                table: "LoanRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EmployeeWallets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Competencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Competencies");

            migrationBuilder.DropTable(
                name: "ReviewTemplates");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "AttemptNumber",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "BankDetail",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderRef",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CaldavPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RateOrAmountStr",
                table: "StatutoryRules");

            migrationBuilder.DropColumn(
                name: "IsEmployee",
                table: "StatutoryDeductions");

            migrationBuilder.DropColumn(
                name: "IsEmployer",
                table: "StatutoryDeductions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Remittances");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "EmployeesCount",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "Adapter",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "WebhookSecret",
                table: "PayoutProviders");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "MaxSalary",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "MinSalary",
                table: "PayGrades");

            migrationBuilder.DropColumn(
                name: "IsSalaryAdvance",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmployeeWallets");
        }
    }
}
