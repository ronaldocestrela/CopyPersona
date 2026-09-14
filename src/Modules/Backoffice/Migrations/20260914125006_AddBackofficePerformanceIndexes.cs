using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Backoffice.Migrations
{
    /// <inheritdoc />
    public partial class AddBackofficePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AdminImpersonationLogs_TargetTenantId",
                table: "AdminImpersonationLogs",
                column: "TargetTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminImpersonationLogs_TargetUserEmail_StartedAt",
                table: "AdminImpersonationLogs",
                columns: new[] { "TargetUserEmail", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_TargetTenantId",
                table: "AdminAuditLogs",
                column: "TargetTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_ActionType_Timestamp",
                table: "AdminAuditLogs",
                columns: new[] { "ActionType", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminImpersonationLogs_TargetTenantId",
                table: "AdminImpersonationLogs");

            migrationBuilder.DropIndex(
                name: "IX_AdminImpersonationLogs_TargetUserEmail_StartedAt",
                table: "AdminImpersonationLogs");

            migrationBuilder.DropIndex(
                name: "IX_AdminAuditLogs_TargetTenantId",
                table: "AdminAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AdminAuditLogs_ActionType_Timestamp",
                table: "AdminAuditLogs");
        }
    }
}
