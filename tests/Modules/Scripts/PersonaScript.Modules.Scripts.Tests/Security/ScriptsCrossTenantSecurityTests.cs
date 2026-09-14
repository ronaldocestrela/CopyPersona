using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.Commands.RegenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.Commands.SubmitVideoScriptFeedback;
using PersonaScript.Modules.Scripts.Application.Commands.UpdateVideoScriptStatus;
using PersonaScript.Modules.Scripts.Application.Queries.GetNinetyDayCalendar;
using PersonaScript.Modules.Scripts.Application.Queries.GetStoryPlan;
using PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;
using PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;
using Xunit;
using ScriptDomainErrors = PersonaScript.Modules.Scripts.Domain.DomainErrors;

namespace PersonaScript.Modules.Scripts.Tests.Security;

public class ScriptsCrossTenantSecurityTests
{
    private static ScriptsDbContext CreateDbContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<ScriptsDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new ScriptsDbContext(options, tenantContext);
    }

    private static VideoScript CreateScript(Guid tenantId, string tema)
    {
        return VideoScript.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            tema,
            "Pilar Educativo",
            "Atrair seguidores",
            "Gancho magnético",
            "Retenção inteligente",
            "Comente EU QUERO",
            "Legenda persuasiva",
            "Olhe para a câmera",
            "Tom Autoridade"
        ).Value!;
    }

    [Fact]
    public async Task VideoScriptRepository_GetByIdAsync_WhenCalledByForeignTenant_ShouldReturnNull()
    {
        // Arrange
        var dbName = "ScriptsSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        Guid scriptIdA;

        // Tenant A salva seu script
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new VideoScriptRepository(contextA);
            var scriptA = CreateScript(tenantA, "Roteiro Tenant A");
            await repoA.AddAsync(scriptA);
            await repoA.SaveChangesAsync();
            scriptIdA = scriptA.Id;
        }

        // Act - Tenant B tenta consultar por Id
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new VideoScriptRepository(contextB);
            var resultB = await repoB.GetByIdAsync(scriptIdA);

            // Assert: Query filter isola e retorna null
            resultB.Should().BeNull();
        }
    }

    [Fact]
    public async Task VideoScriptQueries_TenantBNaoDeveListarNemConsultarRoteirosDoTenantA()
    {
        // Arrange
        var dbName = "ScriptsSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        Guid scriptIdA;

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new VideoScriptRepository(contextA);
            var scriptA = CreateScript(tenantA, "Roteiro Secreto Tenant A");
            await repoA.AddAsync(scriptA);
            await repoA.SaveChangesAsync();
            scriptIdA = scriptA.Id;
        }

        // Act & Assert para Tenant B
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new VideoScriptRepository(contextB);
            var listHandlerB = new ListVideoScriptsQueryHandler(repoB, tenantContextB);
            var getByIdHandlerB = new GetVideoScriptByIdQueryHandler(repoB, tenantContextB);

            var listResult = await listHandlerB.Handle(new ListVideoScriptsQuery(), CancellationToken.None);
            var getByIdResult = await getByIdHandlerB.Handle(new GetVideoScriptByIdQuery(scriptIdA), CancellationToken.None);

            // Lista vazia para Tenant B
            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Should().BeEmpty();

            // Erro de não encontrado para consulta direta por ID do Tenant A
            getByIdResult.IsFailure.Should().BeTrue();
            getByIdResult.Error.Should().Be(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);
        }
    }

    [Fact]
    public async Task VideoScriptCommands_TenantBNaoDeveAlterarStatusNemDarFeedbackNoRoteiroDoTenantA()
    {
        // Arrange
        var dbName = "ScriptsSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        Guid scriptIdA;

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new VideoScriptRepository(contextA);
            var scriptA = CreateScript(tenantA, "Roteiro Intacto Tenant A");
            await repoA.AddAsync(scriptA);
            await repoA.SaveChangesAsync();
            scriptIdA = scriptA.Id;
        }

        // Act - Tenant B tenta alterar status e dar feedback
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new VideoScriptRepository(contextB);
            var updateStatusHandlerB = new UpdateVideoScriptStatusCommandHandler(repoB, tenantContextB);
            var feedbackHandlerB = new SubmitVideoScriptFeedbackCommandHandler(repoB, tenantContextB);

            var statusResult = await updateStatusHandlerB.Handle(
                new UpdateVideoScriptStatusCommand(scriptIdA, VideoScriptStatus.Recorded), CancellationToken.None);

            var feedbackResult = await feedbackHandlerB.Handle(
                new SubmitVideoScriptFeedbackCommand(scriptIdA, ScriptFeedbackRating.Liked, "Feedback não autorizado"), CancellationToken.None);

            statusResult.IsFailure.Should().BeTrue();
            statusResult.Error.Should().Be(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);

            feedbackResult.IsFailure.Should().BeTrue();
            feedbackResult.Error.Should().Be(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);
        }

        // Assert: Script do Tenant A permanece em Draft e FeedbackRating.None
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new VideoScriptRepository(contextA);
            var scriptA = await repoA.GetByIdAsync(scriptIdA);

            scriptA.Should().NotBeNull();
            scriptA!.Status.Should().Be(VideoScriptStatus.Draft);
            scriptA.FeedbackRating.Should().Be(ScriptFeedbackRating.None);
        }
    }

    [Fact]
    public async Task RegenerateVideoScriptCommand_TenantBNaoDeveRegenerarRoteiroDoTenantA()
    {
        // Arrange
        var dbName = "ScriptsSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        Guid scriptIdA;

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new VideoScriptRepository(contextA);
            var scriptA = CreateScript(tenantA, "Roteiro Tenant A");
            await repoA.AddAsync(scriptA);
            await repoA.SaveChangesAsync();
            scriptIdA = scriptA.Id;
        }

        // Tenant B tenta regenerar passando o TargetScriptId do Tenant A
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new VideoScriptRepository(contextB);
            var anamneseHandlerMock = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
            var personaRepoMock = Substitute.For<IPersonaDiagnosisRepository>();
            var generatorMock = Substitute.For<IVideoScriptGenerator>();

            var regenerateHandler = new RegenerateVideoScriptCommandHandler(
                repoB,
                tenantContextB,
                anamneseHandlerMock,
                personaRepoMock,
                generatorMock);

            var result = await regenerateHandler.Handle(
                new RegenerateVideoScriptCommand(scriptIdA, "Mude o tom"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);
        }
    }

    [Fact]
    public async Task StoryPlanAndCalendar_TenantBNaoDeveVerPlanosDoTenantA()
    {
        // Arrange
        var dbName = "ScriptsSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var planA = StoryPlan.Create(
                tenantA, Guid.NewGuid(), null, "3 stories/dia",
                new[] { new StoryBlock("Manhã", "08:00", "Café", "Bastidor", "Foto da xícara", "Conexão") },
                "Humanização Tenant A").Value;

            var calA = NinetyDayCalendar.Create(
                tenantA, Guid.NewGuid(), null, "Autoridade 2026",
                new[] { new WeeklyEditorialPlan(1, "Semana 1", "Pilar", "Obj", "Reels", new List<string> { "Ideia" }) }).Value;

            contextA.StoryPlans.Add(planA);
            contextA.NinetyDayCalendars.Add(calA);
            await contextA.SaveChangesAsync();
        }

        // Act & Assert sob Tenant B
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var storyRepoB = new StoryPlanRepository(contextB);
            var calRepoB = new NinetyDayCalendarRepository(contextB);

            var storyHandlerB = new GetStoryPlanQueryHandler(storyRepoB, tenantContextB);
            var calHandlerB = new GetNinetyDayCalendarQueryHandler(calRepoB, tenantContextB);

            var storyResult = await storyHandlerB.Handle(new GetStoryPlanQuery(), CancellationToken.None);
            var calResult = await calHandlerB.Handle(new GetNinetyDayCalendarQuery(), CancellationToken.None);

            storyResult.IsFailure.Should().BeTrue();
            storyResult.Error.Should().Be(ScriptDomainErrors.Scripts.StoryPlanNaoEncontrado);

            calResult.IsFailure.Should().BeTrue();
            calResult.Error.Should().Be(ScriptDomainErrors.Scripts.NinetyDayCalendarNaoEncontrado);
        }
    }
}
