using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.Commands.Compliance;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.Compliance;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminEthicsPageTests : BunitContext
{
    private readonly IQueryHandler<GetCouncilRulesQuery, IReadOnlyList<CouncilRuleDto>> _getCouncilRulesHandler = Substitute.For<IQueryHandler<GetCouncilRulesQuery, IReadOnlyList<CouncilRuleDto>>>();
    private readonly IQueryHandler<GetForbiddenTermsQuery, IReadOnlyList<ForbiddenTermDto>> _getForbiddenTermsHandler = Substitute.For<IQueryHandler<GetForbiddenTermsQuery, IReadOnlyList<ForbiddenTermDto>>>();
    private readonly ICommandHandler<CreateCouncilRuleCommand, Guid> _createCouncilRuleHandler = Substitute.For<ICommandHandler<CreateCouncilRuleCommand, Guid>>();
    private readonly ICommandHandler<UpdateCouncilRuleCommand> _updateCouncilRuleHandler = Substitute.For<ICommandHandler<UpdateCouncilRuleCommand>>();
    private readonly ICommandHandler<ToggleCouncilRuleStatusCommand> _toggleCouncilRuleStatusHandler = Substitute.For<ICommandHandler<ToggleCouncilRuleStatusCommand>>();
    private readonly ICommandHandler<CreateForbiddenTermCommand, Guid> _createForbiddenTermHandler = Substitute.For<ICommandHandler<CreateForbiddenTermCommand, Guid>>();
    private readonly ICommandHandler<UpdateForbiddenTermCommand> _updateForbiddenTermHandler = Substitute.For<ICommandHandler<UpdateForbiddenTermCommand>>();
    private readonly ICommandHandler<ToggleForbiddenTermStatusCommand> _toggleForbiddenTermStatusHandler = Substitute.For<ICommandHandler<ToggleForbiddenTermStatusCommand>>();
    private readonly ICommandHandler<DeleteForbiddenTermCommand> _deleteForbiddenTermHandler = Substitute.For<ICommandHandler<DeleteForbiddenTermCommand>>();
    private readonly ICommandHandler<ModerateContentCommand, QualityModerationResultDto> _moderateContentHandler = Substitute.For<ICommandHandler<ModerateContentCommand, QualityModerationResultDto>>();

    public AdminEthicsPageTests()
    {
        Services.AddSingleton(_getCouncilRulesHandler);
        Services.AddSingleton(_getForbiddenTermsHandler);
        Services.AddSingleton(_createCouncilRuleHandler);
        Services.AddSingleton(_updateCouncilRuleHandler);
        Services.AddSingleton(_toggleCouncilRuleStatusHandler);
        Services.AddSingleton(_createForbiddenTermHandler);
        Services.AddSingleton(_updateForbiddenTermHandler);
        Services.AddSingleton(_toggleForbiddenTermStatusHandler);
        Services.AddSingleton(_deleteForbiddenTermHandler);
        Services.AddSingleton(_moderateContentHandler);
    }

    [Fact]
    public void EthicsPage_ShouldRenderCouncilRulesAndTabs()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetPolicies("RequireSystemAdmin");

        var rule = new CouncilRuleDto(
            Id: Guid.NewGuid(),
            CouncilAcronym: "CFM",
            CouncilName: "Conselho Federal de Medicina",
            ResolutionNumber: "2.336/2023",
            GuidelinesText: "Permite divulgação de antes e depois com finalidade educativa.",
            Category: "Publicidade Médica",
            IsActive: true,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);

        _getCouncilRulesHandler.Handle(Arg.Any<GetCouncilRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<CouncilRuleDto>>(new List<CouncilRuleDto> { rule })));

        _getForbiddenTermsHandler.Handle(Arg.Any<GetForbiddenTermsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<ForbiddenTermDto>>(new List<ForbiddenTermDto>())));

        // Act
        var cut = Render<AdminEthicsPage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Central de Governança Ética");
        cut.Find(".space-y-6").TextContent.Should().Contain("CFM");
        cut.Find(".space-y-6").TextContent.Should().Contain("2.336/2023");
    }

    [Fact]
    public void EthicsPage_SwitchingTabToDicionario_ShouldRenderForbiddenTerms()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetPolicies("RequireSystemAdmin");

        var forbiddenTerm = new ForbiddenTermDto(
            Id: Guid.NewGuid(),
            Term: "Garantia de resultado",
            Category: "Promessa Enganosa",
            Severity: ForbiddenTermSeverity.Prohibited,
            ReplacementSuggestion: "Tratamento individualizado com base no prognóstico",
            Reasoning: "A medicina é atividade de meio, vedada a promessa de cura ou resultado.",
            IsActive: true,
            CreatedAt: DateTimeOffset.UtcNow);

        _getCouncilRulesHandler.Handle(Arg.Any<GetCouncilRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<CouncilRuleDto>>(new List<CouncilRuleDto>())));

        _getForbiddenTermsHandler.Handle(Arg.Any<GetForbiddenTermsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<ForbiddenTermDto>>(new List<ForbiddenTermDto> { forbiddenTerm })));

        var cut = Render<AdminEthicsPage>();

        // Act - Click "Dicionário Global" tab
        var tabBtn = cut.Find("button:contains('Dicionário Global')");
        tabBtn.Click();

        // Assert
        cut.Find(".space-y-6").TextContent.Should().Contain("Garantia de resultado");
        cut.Find(".space-y-6").TextContent.Should().Contain("Promessa Enganosa");
    }
}
