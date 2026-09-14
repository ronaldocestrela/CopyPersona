using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Personas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonasPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PersonaDiagnoses_TenantId_GeradoEm",
                schema: "personas",
                table: "PersonaDiagnoses",
                columns: new[] { "TenantId", "GeradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonaDiagnoses_TenantId_AnamneseId",
                schema: "personas",
                table: "PersonaDiagnoses",
                columns: new[] { "TenantId", "AnamneseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonaDiagnoses_TenantId_GeradoEm",
                schema: "personas",
                table: "PersonaDiagnoses");

            migrationBuilder.DropIndex(
                name: "IX_PersonaDiagnoses_TenantId_AnamneseId",
                schema: "personas",
                table: "PersonaDiagnoses");
        }
    }
}
