using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Services;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Tests;

public class QualityModeratorServiceTests
{
    private readonly IForbiddenTermRepository _forbiddenTermRepository;
    private readonly ICouncilRuleRepository _councilRuleRepository;
    private readonly QualityModeratorService _moderatorService;

    public QualityModeratorServiceTests()
    {
        _forbiddenTermRepository = Substitute.For<IForbiddenTermRepository>();
        _councilRuleRepository = Substitute.For<ICouncilRuleRepository>();
        _moderatorService = new QualityModeratorService(_forbiddenTermRepository, _councilRuleRepository);
    }

    [Fact]
    public async Task ModerateContentAsync_ShouldReturnScore100_WhenNoForbiddenTermsFound()
    {
        // Arrange
        _forbiddenTermRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ForbiddenTerm>
            {
                ForbiddenTerm.Create("Cura garantida", "Promessa", ForbiddenTermSeverity.Prohibited, "Tratamento eficaz", "Fere CFM").Value
            });

        var content = "Este é um tratamento seguro e eficaz focado no bem-estar do paciente.";

        // Act
        var result = await _moderatorService.ModerateContentAsync(content, "CFM", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeTrue();
        result.Score.Should().Be(100);
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task ModerateContentAsync_ShouldDetectProhibitedTerms_AndCalculateScore()
    {
        // Arrange
        var prohibitedTerm = ForbiddenTerm.Create("Cura garantida", "Promessa", ForbiddenTermSeverity.Prohibited, "Tratamento eficaz", "Fere CFM").Value;
        var warningTerm = ForbiddenTerm.Create("Sem riscos", "Regulação", ForbiddenTermSeverity.Warning, "Procedimento seguro", "Risco inerente").Value;

        _forbiddenTermRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ForbiddenTerm> { prohibitedTerm, warningTerm });

        var content = "Oferecemos cura garantida e um método sem riscos para sua saúde!";

        // Act
        var result = await _moderatorService.ModerateContentAsync(content, "CFM", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeFalse();
        result.Score.Should().BeLessThan(100);
        result.Violations.Should().HaveCount(2);

        result.Violations.Should().Contain(v => v.Term == "Cura garantida" && v.Severity == ForbiddenTermSeverity.Prohibited);
        result.Violations.Should().Contain(v => v.Term == "Sem riscos" && v.Severity == ForbiddenTermSeverity.Warning);
        result.SanitizedContent.Should().Contain("Tratamento eficaz");
        result.SanitizedContent.Should().Contain("Procedimento seguro");
    }

    [Fact]
    public async Task ModerateContentAsync_ShouldIncludeCouncilGuidelines_WhenCouncilIsProvided()
    {
        // Arrange
        var councilRule = CouncilRule.Create("CFM", "Conselho Federal de Medicina", "2.336/2023", "Vedado promessa de cura", "Medicina").Value;
        _councilRuleRepository.GetByAcronymAsync("CFM", Arg.Any<CancellationToken>())
            .Returns(councilRule);

        _forbiddenTermRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ForbiddenTerm>());

        // Act
        var result = await _moderatorService.ModerateContentAsync("Texto limpo", "CFM", CancellationToken.None);

        // Assert
        result.CouncilRuleApplied.Should().NotBeNull();
        result.CouncilRuleApplied!.CouncilAcronym.Should().Be("CFM");
        result.CouncilRuleApplied.GuidelinesText.Should().Be("Vedado promessa de cura");
    }
}
