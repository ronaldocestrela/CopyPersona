using FluentAssertions;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Tests;

public class EthicalGovernanceDomainTests
{
    [Fact]
    public void CouncilRule_Create_ShouldReturnSuccess_WhenValid()
    {
        var result = CouncilRule.Create(
            councilAcronym: "CFM",
            councilName: "Conselho Federal de Medicina",
            resolutionNumber: "Resolução CFM 2.336/2023",
            guidelinesText: "É vedada a promessa de resultados garantidos em procedimentos médicos.",
            category: "Publicidade Médica",
            isActive: true);

        result.IsSuccess.Should().BeTrue();
        var rule = result.Value;
        rule.CouncilAcronym.Should().Be("CFM");
        rule.CouncilName.Should().Be("Conselho Federal de Medicina");
        rule.ResolutionNumber.Should().Be("Resolução CFM 2.336/2023");
        rule.GuidelinesText.Should().Be("É vedada a promessa de resultados garantidos em procedimentos médicos.");
        rule.Category.Should().Be("Publicidade Médica");
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CouncilRule_Create_ShouldFail_WhenCouncilAcronymIsEmpty()
    {
        var result = CouncilRule.Create(
            councilAcronym: "",
            councilName: "Conselho Teste",
            resolutionNumber: "123",
            guidelinesText: "Diretrizes",
            category: "Geral");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CouncilRule.AcronymRequired");
    }

    [Fact]
    public void ForbiddenTerm_Create_ShouldReturnSuccess_WhenValid()
    {
        var result = ForbiddenTerm.Create(
            term: "Cura Garantida",
            category: "PromessaExcessiva",
            severity: ForbiddenTermSeverity.Prohibited,
            replacementSuggestion: "Tratamento Eficaz",
            reasoning: "Promessa irrestrita de cura fere resoluções de órgãos de saúde.",
            isActive: true);

        result.IsSuccess.Should().BeTrue();
        var term = result.Value;
        term.Term.Should().Be("Cura Garantida");
        term.Category.Should().Be("PromessaExcessiva");
        term.Severity.Should().Be(ForbiddenTermSeverity.Prohibited);
        term.ReplacementSuggestion.Should().Be("Tratamento Eficaz");
        term.Reasoning.Should().Contain("Promessa irrestrita");
        term.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ForbiddenTerm_Create_ShouldFail_WhenTermIsEmpty()
    {
        var result = ForbiddenTerm.Create(
            term: "   ",
            category: "Geral",
            severity: ForbiddenTermSeverity.Warning,
            replacementSuggestion: "",
            reasoning: "");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ForbiddenTerm.TermRequired");
    }
}
