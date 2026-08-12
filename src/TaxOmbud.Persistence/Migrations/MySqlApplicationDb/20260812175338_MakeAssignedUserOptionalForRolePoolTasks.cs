using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxOmbud.Persistence.Migrations.MySqlApplicationDb
{
    /// <inheritdoc />
    public partial class MakeAssignedUserOptionalForRolePoolTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS 
                                  WHERE CONSTRAINT_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'CaseApprovalTasks' 
                                  AND CONSTRAINT_NAME = 'FK_CaseApprovalTasks_Users_AssignedUserId');
                SET @sql = IF(@fk_exists > 0, 'ALTER TABLE `CaseApprovalTasks` DROP FOREIGN KEY `FK_CaseApprovalTasks_Users_AssignedUserId`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE `CaseApprovalTasks` MODIFY COLUMN `AssignedUserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE `CaseApprovalTasks` ADD CONSTRAINT `FK_CaseApprovalTasks_Users_AssignedUserId` 
                FOREIGN KEY (`AssignedUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseApprovalTasks_Users_AssignedUserId",
                table: "CaseApprovalTasks");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseApprovalTasks_Users_AssignedUserId",
                table: "CaseApprovalTasks",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
