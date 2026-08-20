using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.Commands.Prompts;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.Prompts;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminPromptsPageTests : BunitContext
{
    private readonly IQueryHandler<GetPromptTemplatesQuery, IReadOnlyList<PromptTemplateDto>> _getPromptTemplatesHandler = Substitute.For<IQueryHandler<GetPromptTemplatesQuery, IReadOnlyList<PromptTemplateDto>>>();
    private readonly IQueryHandler<GetPromptHistoryQuery, IReadOnlyList<PromptTemplateDto>> _getPromptHistoryHandler = Substitute.For<IQueryHandler<GetPromptHistoryQuery, IReadOnlyList<PromptTemplateDto>>>();
    private readonly ICommandHandler<CreatePromptVersionCommand, Guid> _createPromptVersionHandler = Substitute.For<ICommandHandler<CreatePromptVersionCommand, Guid>>();
    private readonly ICommandHandler<RollbackPromptVersionCommand> _rollbackPromptVersionHandler = Substitute.For<ICommandHandler<RollbackPromptVersionCommand>>();
    private readonly ICommandHandler<TestPromptPlaygroundCommand, TestPromptResultDto> _testPromptPlaygroundHandler = Substitute.For<ICommandHandler<TestPromptPlaygroundCommand, TestPromptResultDto>>();

    public AdminPromptsPageTests()
    {
        Services.AddSingleton(_getPromptTemplatesHandler);
        Services.AddSingleton(_getPromptHistoryHandler);
        Services.AddSingleton(_createPromptVersionHandler);
        Services.AddSingleton(_rollbackPromptVersionHandler);
        Services.AddSingleton(_testPromptPlaygroundHandler);
    }

    [Fact]
    public void AdminPromptsPage_ShouldRenderHeader_Tabs_AndActiveVersion()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetClaims(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SystemAdmin"));

        var activePrompt = new PromptTemplateDto(
            Guid.NewGuid(),
            "Agent1_Diagnosis",
            1,
            "System prompt v1",
            "User prompt v1 {{AnamneseData}}",
            true,
            "{}",
            "Versão Inicial",
            "admin@personascript.ai",
            DateTimeOffset.UtcNow);

        _getPromptTemplatesHandler.Handle(Arg.Any<GetPromptTemplatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PromptTemplateDto>>(new List<PromptTemplateDto> { activePrompt })));

        _getPromptHistoryHandler.Handle(Arg.Any<GetPromptHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PromptTemplateDto>>(new List<PromptTemplateDto> { activePrompt })));

        // Act
        var cut = Render<AdminPromptsPage>();

        // Assert
        cut.Markup.Should().Contain("Gestão Dinâmica de Prompts de IA");
        cut.Markup.Should().Contain("Agente 1 — Diagnóstico");
        cut.Markup.Should().Contain("System prompt v1");
        cut.Markup.Should().Contain("Playground de Teste em Tempo Real");
        cut.Markup.Should().Contain("Histórico de Versões");
    }

    [Fact]
    public void AdminPromptsPage_TabSwitching_ShouldLoadHistoryForSelectedAgent()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetClaims(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SystemAdmin"));

        _getPromptTemplatesHandler.Handle(Arg.Any<GetPromptTemplatesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PromptTemplateDto>>(new List<PromptTemplateDto>())));

        _getPromptHistoryHandler.Handle(Arg.Any<GetPromptHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PromptTemplateDto>>(new List<PromptTemplateDto>())));

        // Act
        var cut = Render<AdminPromptsPage>();

        // Switch tab to Agent2_VideoScript
        var videoScriptTab = cut.FindAll("button").First(b => b.TextContent.Contains("Roteiro de Vídeo"));
        videoScriptTab.Click();

        // Assert
        cut.Markup.Should().Contain("Agente 2 — Roteiro de Vídeo");
        _getPromptHistoryHandler.Received(1).Handle(Arg.Is<GetPromptHistoryQuery>(q => q != null && q.AgentName == "Agent2_VideoScript"), Arg.Any<CancellationToken>());
    }
}
