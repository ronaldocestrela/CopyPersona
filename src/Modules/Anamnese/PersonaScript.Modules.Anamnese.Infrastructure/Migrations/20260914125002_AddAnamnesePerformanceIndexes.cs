using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Anamnese.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnamnesePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_TenantId_Status",
                schema: "anamnese",
                table: "Anamneses",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Anamneses_TenantId_Status",
                schema: "anamnese",
                table: "Anamneses");
        }
    }
}
