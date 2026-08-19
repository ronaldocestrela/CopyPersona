using FluentAssertions;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Domain;

public class StoryPlanTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenTenantIdIsEmpty()
    {
        // Act
        var result = StoryPlan.Create(
            Guid.Empty,
            Guid.NewGuid(),
            null,
            "3 stories por dia",
            new[] { new StoryBlock("Manhã", "08:00", "Chegada clínica", "Bastidores", "Exemplo", "Conexão") },
            "Diretrizes");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenBlocosHorariosIsEmpty()
    {
        // Act
        var result = StoryPlan.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "3 stories por dia",
            Array.Empty<StoryBlock>(),
            "Diretrizes");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.StoryPlanInvalido);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();
        var blocos = new[]
        {
            new StoryBlock("Manhã", "08:00", "Chegada", "Bastidores", "Bom dia consultório", "Humanização"),
            new StoryBlock("Almoço", "12:30", "Pausa", "Educacional", "Dica rápida", "Autoridade")
        };

        // Act
        var result = StoryPlan.Create(
            tenantId,
            anamneseId,
            null,
            "3 a 5 stories por dia",
            blocos,
            "Humanização e constância");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.AnamneseId.Should().Be(anamneseId);
        result.Value.FrequenciaDiariaRecomendada.Should().Be("3 a 5 stories por dia");
        result.Value.BlocosHorarios.Should().HaveCount(2);
        result.Value.DiretrizesHumanizacao.Should().Be("Humanização e constância");
    }
}
