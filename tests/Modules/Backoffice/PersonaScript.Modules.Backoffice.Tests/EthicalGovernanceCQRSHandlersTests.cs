using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Backoffice.Application.Commands.Compliance;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.Compliance;
using PersonaScript.Modules.Backoffice.Application.Services;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Tests;

public class EthicalGovernanceCQRSHandlersTests
{
    private readonly ICouncilRuleRepository _councilRuleRepository;
    private readonly IForbiddenTermRepository _forbiddenTermRepository;
    private readonly IQualityModeratorService _qualityModeratorService;

    public EthicalGovernanceCQRSHandlersTests()
    {
        _councilRuleRepository = Substitute.For<ICouncilRuleRepository>();
        _forbiddenTermRepository = Substitute.For<IForbiddenTermRepository>();
        _qualityModeratorService = Substitute.For<IQualityModeratorService>();
    }

    [Fact]
    public async Task CreateCouncilRuleCommandHandler_ShouldCreateAndSave()
    {
        // Arrange
        var handler = new CreateCouncilRuleCommandHandler(_councilRuleRepository);
        var command = new CreateCouncilRuleCommand("CFM", "Conselho Federal de Medicina", "2.336/2023", "Regras do CFM", "Medicina", true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _councilRuleRepository.Received(1).AddAsync(Arg.Any<CouncilRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateForbiddenTermCommandHandler_ShouldCreateAndSave()
    {
        // Arrange
        var handler = new CreateForbiddenTermCommandHandler(_forbiddenTermRepository);
        var command = new CreateForbiddenTermCommand("Cura milagrosa", "Promessa", ForbiddenTermSeverity.Prohibited, "Tratamento eficaz", "Motivo", true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _forbiddenTermRepository.Received(1).AddAsync(Arg.Any<ForbiddenTerm>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModerateContentCommandHandler_ShouldInvokeQualityModeratorService()
    {
        // Arrange
        var expectedDto = new QualityModerationResultDto(true, 100, Array.Empty<ModerationViolationDto>(), "Texto", "Texto", null);
        _qualityModeratorService.ModerateContentAsync("Texto de teste", "CFM", Arg.Any<CancellationToken>())
            .Returns(expectedDto);

        var handler = new ModerateContentCommandHandler(_qualityModeratorService);
        var command = new ModerateContentCommand("Texto de teste", "CFM");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Score.Should().Be(100);
        result.Value.IsCompliant.Should().BeTrue();
    }
}
