using FluentAssertions;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Domain;

public class NinetyDayCalendarTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenTenantIdIsEmpty()
    {
        // Act
        var result = NinetyDayCalendar.Create(
            Guid.Empty,
            Guid.NewGuid(),
            null,
            "Atrair pacientes qualificados",
            new[] { new WeeklyEditorialPlan(1, "Tema", "Educação", "Objetivo", "Vídeo", new List<string> { "Ideia 1" }) });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenSemanasIsEmpty()
    {
        // Act
        var result = NinetyDayCalendar.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Objetivo",
            Array.Empty<WeeklyEditorialPlan>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.NinetyDayCalendarInvalido);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();
        var semanas = new[]
        {
            new WeeklyEditorialPlan(1, "Quebrando Mitos", "Educação", "Conscientização", "Vídeo curto", new List<string> { "Mito 1", "Mito 2" }),
            new WeeklyEditorialPlan(2, "Estudo de Caso", "Prova Social", "Conversão", "Carrossel", new List<string> { "Antes e Depois" })
        };

        // Act
        var result = NinetyDayCalendar.Create(
            tenantId,
            anamneseId,
            null,
            "Lotar agenda com procedimentos premium",
            semanas);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.AnamneseId.Should().Be(anamneseId);
        result.Value.ObjetivoTrimestral.Should().Be("Lotar agenda com procedimentos premium");
        result.Value.Semanas.Should().HaveCount(2);
    }
}
