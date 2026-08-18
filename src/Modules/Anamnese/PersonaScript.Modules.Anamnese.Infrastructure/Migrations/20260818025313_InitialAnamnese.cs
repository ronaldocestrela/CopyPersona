using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Anamnese.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnamnese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "anamnese");

            migrationBuilder.CreateTable(
                name: "Anamneses",
                schema: "anamnese",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EtapaAtual = table.Column<int>(type: "int", nullable: false),
                    PercentualConclusao = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConcluidoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Etapa1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Etapa9 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anamneses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_TenantId",
                schema: "anamnese",
                table: "Anamneses",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anamneses",
                schema: "anamnese");
        }
    }
}
