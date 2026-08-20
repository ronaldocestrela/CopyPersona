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

    [Fact]
    public void UpdateLimits_WithValidData_ShouldUpdatePlanSuccessfully()
    {
        // Arrange
        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value;

        // Act
        var result = plan.UpdateLimits("Pro Plus", "Nova Desc", 120m, 1200m, 10, 50, 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Name.Should().Be("Pro Plus");
        plan.MonthlyPrice.Should().Be(120m);
        plan.MaxScriptsPerMonth.Should().Be(50);
        plan.MaxActivePersonas.Should().Be(10);
        plan.MaxAiAnalysesPerMonth.Should().Be(100);
        plan.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateLimits_WithInvalidNegativeValues_ShouldReturnFailure()
    {
        // Arrange
        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value;

        // Act
        var result = plan.UpdateLimits("Pro Plus", "Nova Desc", -10m, 1200m, 10, 50, 100);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Plan.InvalidValues");
    }
}

