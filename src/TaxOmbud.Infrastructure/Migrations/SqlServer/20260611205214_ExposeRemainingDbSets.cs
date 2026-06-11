using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ExposeRemainingDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Users_ApproverUserId",
                table: "LeaveRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Users_ApproverUserId",
                table: "LeaveRequests",
                column: "ApproverUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Users_ApproverUserId",
                table: "LeaveRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Users_ApproverUserId",
                table: "LeaveRequests",
                column: "ApproverUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
