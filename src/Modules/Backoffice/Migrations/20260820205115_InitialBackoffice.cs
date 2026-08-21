using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Backoffice.Migrations
{
    /// <inheritdoc />
    public partial class InitialBackoffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminImpersonationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminImpersonationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: false),
                    CompletionTokens = table.Column<int>(type: "int", nullable: false),
                    EstimatedCostUSD = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExecutedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CouncilRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CouncilAcronym = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CouncilName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResolutionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuidelinesText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouncilRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForbiddenTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    ReplacementSuggestion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForbiddenTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedByAdminEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_AgentName_Status",
                table: "AgentExecutionLogs",
                columns: new[] { "AgentName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_ExecutedAtUtc",
                table: "AgentExecutionLogs",
                column: "ExecutedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutionLogs_TenantId",
                table: "AgentExecutionLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilRules_CouncilAcronym",
                table: "CouncilRules",
                column: "CouncilAcronym");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilRules_CouncilAcronym_IsActive",
                table: "CouncilRules",
                columns: new[] { "CouncilAcronym", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ForbiddenTerms_IsActive",
                table: "ForbiddenTerms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ForbiddenTerms_Term",
                table: "ForbiddenTerms",
                column: "Term");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_AgentName_IsActive",
                table: "PromptTemplates",
                columns: new[] { "AgentName", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_AgentName_Version",
                table: "PromptTemplates",
                columns: new[] { "AgentName", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogs");

            migrationBuilder.DropTable(
                name: "AdminImpersonationLogs");

            migrationBuilder.DropTable(
                name: "AgentExecutionLogs");

            migrationBuilder.DropTable(
                name: "CouncilRules");

            migrationBuilder.DropTable(
                name: "ForbiddenTerms");

            migrationBuilder.DropTable(
                name: "PromptTemplates");
        }
    }
}
