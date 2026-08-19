using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Personas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "personas");

            migrationBuilder.CreateTable(
                name: "PersonaDiagnoses",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnamneseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrasePosicionamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SintesePerfil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeradoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IdentidadeMarca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatrizRestricoes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PilaresConteudo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonaDiagnoses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonaDiagnoses_TenantId",
                schema: "personas",
                table: "PersonaDiagnoses",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonaDiagnoses",
                schema: "personas");
        }
    }
}
