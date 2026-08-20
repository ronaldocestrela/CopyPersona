using FluentAssertions;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Tests.Domain;

public class PlanTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreatePlanSuccessfully()
    {
        // Act
        var planResult = Plan.Create(
            PlanType.Pro,
            "Plano Pro",
            "Plano completo para criadores de conteúdo",
            97.00m,
            970.00m,
            maxActivePersonas: 5,
            maxScriptsPerMonth: 30,
            maxAiAnalysesPerMonth: 50);

        // Assert
        planResult.IsSuccess.Should().BeTrue();
        var plan = planResult.Value;
        plan.PlanType.Should().Be(PlanType.Pro);
        plan.Name.Should().Be("Plano Pro");
        plan.MonthlyPrice.Should().Be(97.00m);
        plan.YearlyPrice.Should().Be(970.00m);
        plan.MaxActivePersonas.Should().Be(5);
        plan.MaxScriptsPerMonth.Should().Be(30);
        plan.MaxAiAnalysesPerMonth.Should().Be(50);
        plan.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldReturnFailure(string invalidName)
    {
        // Act
        var planResult = Plan.Create(
            PlanType.Basic,
            invalidName,
            "Descrição",
            47.00m,
            470.00m,
            1, 10, 10);

        // Assert
        planResult.IsFailure.Should().BeTrue();
        planResult.Error.Should().Be(DomainErrors.Plan.InvalidName);
    }
}
