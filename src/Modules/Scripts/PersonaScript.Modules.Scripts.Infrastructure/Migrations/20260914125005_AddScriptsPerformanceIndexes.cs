using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Scripts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScriptsPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VideoScripts_TenantId_GeradoEm",
                schema: "scripts",
                table: "VideoScripts",
                columns: new[] { "TenantId", "GeradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoScripts_TenantId_Status_GeradoEm",
                schema: "scripts",
                table: "VideoScripts",
                columns: new[] { "TenantId", "Status", "GeradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoScripts_TenantId_AnamneseId",
                schema: "scripts",
                table: "VideoScripts",
                columns: new[] { "TenantId", "AnamneseId" });

            migrationBuilder.CreateIndex(
                name: "IX_StoryPlans_TenantId_GeradoEm",
                schema: "scripts",
                table: "StoryPlans",
                columns: new[] { "TenantId", "GeradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_StoryPlans_TenantId_AnamneseId",
                schema: "scripts",
                table: "StoryPlans",
                columns: new[] { "TenantId", "AnamneseId" });

            migrationBuilder.CreateIndex(
                name: "IX_NinetyDayCalendars_TenantId_GeradoEm",
                schema: "scripts",
                table: "NinetyDayCalendars",
                columns: new[] { "TenantId", "GeradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_NinetyDayCalendars_TenantId_AnamneseId",
                schema: "scripts",
                table: "NinetyDayCalendars",
                columns: new[] { "TenantId", "AnamneseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoScripts_TenantId_GeradoEm",
                schema: "scripts",
                table: "VideoScripts");

            migrationBuilder.DropIndex(
                name: "IX_VideoScripts_TenantId_Status_GeradoEm",
                schema: "scripts",
                table: "VideoScripts");

            migrationBuilder.DropIndex(
                name: "IX_VideoScripts_TenantId_AnamneseId",
                schema: "scripts",
                table: "VideoScripts");

            migrationBuilder.DropIndex(
                name: "IX_StoryPlans_TenantId_GeradoEm",
                schema: "scripts",
                table: "StoryPlans");

            migrationBuilder.DropIndex(
                name: "IX_StoryPlans_TenantId_AnamneseId",
                schema: "scripts",
                table: "StoryPlans");

            migrationBuilder.DropIndex(
                name: "IX_NinetyDayCalendars_TenantId_GeradoEm",
                schema: "scripts",
                table: "NinetyDayCalendars");

            migrationBuilder.DropIndex(
                name: "IX_NinetyDayCalendars_TenantId_AnamneseId",
                schema: "scripts",
                table: "NinetyDayCalendars");
        }
    }
}
