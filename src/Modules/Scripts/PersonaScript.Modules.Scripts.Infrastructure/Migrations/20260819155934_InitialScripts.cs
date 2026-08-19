using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Scripts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialScripts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scripts");

            migrationBuilder.CreateTable(
                name: "NinetyDayCalendars",
                schema: "scripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnamneseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaDiagnosisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ObjetivoTrimestral = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GeradoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Semanas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NinetyDayCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoryPlans",
                schema: "scripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnamneseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaDiagnosisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FrequenciaDiariaRecomendada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiretrizesHumanizacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeradoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BlocosHorarios = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoScripts",
                schema: "scripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnamneseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaDiagnosisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PilarConteudo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Gancho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Retencao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChamadaParaAcao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegendaSugerida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DicasGravacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TomVozAplicado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FeedbackRating = table.Column<int>(type: "int", nullable: false),
                    FeedbackNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GeradoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoScripts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NinetyDayCalendars_TenantId",
                schema: "scripts",
                table: "NinetyDayCalendars",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryPlans_TenantId",
                schema: "scripts",
                table: "StoryPlans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoScripts_TenantId",
                schema: "scripts",
                table: "VideoScripts",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NinetyDayCalendars",
                schema: "scripts");

            migrationBuilder.DropTable(
                name: "StoryPlans",
                schema: "scripts");

            migrationBuilder.DropTable(
                name: "VideoScripts",
                schema: "scripts");
        }
    }
}
