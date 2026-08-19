using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using Xunit;

namespace PersonaScript.Modules.Personas.Tests.Commands;

public class UpdatePersonaDiagnosisCommandHandlerTests
{
    private readonly IPersonaDiagnosisRepository _repository = Substitute.For<IPersonaDiagnosisRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly UpdatePersonaDiagnosisCommandHandler _handler;

    public UpdatePersonaDiagnosisCommandHandlerTests()
    {
        _handler = new UpdatePersonaDiagnosisCommandHandler(_repository, _tenantContext);
    }

    [Fact]
    public async Task Handle_WhenTenantIdIsEmpty_ShouldReturnTenantIdInvalido()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));
        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.TenantIdInvalido");
    }

    [Fact]
    public async Task Handle_WhenDiagnosisNotFound_ShouldReturnDiagnosticoNaoEncontrado()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));
        _repository.GetByTenantIdAsync(Arg.Any<CancellationToken>()).Returns((PersonaDiagnosis?)null);

        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.DiagnosticoNaoEncontrado");
    }

    [Fact]
    public async Task Handle_WhenPilaresSumIsNot100_ShouldReturnPercentualPilaresInvalido()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var existingDiagnosis = CreateExistingDiagnosis(tenantId);
        _repository.GetByTenantIdAsync(Arg.Any<CancellationToken>()).Returns(existingDiagnosis);

        var invalidPilares = new List<PilarConteudoDto>
        {
            new PilarConteudoDto("Pilar 1", 50, "Desc 1", new[] { "T1" }),
            new PilarConteudoDto("Pilar 2", 40, "Desc 2", new[] { "T2" })
        }; // Sum = 90

        var command = CreateValidCommand() with { PilaresConteudo = invalidPilares };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.PercentualPilaresInvalido");
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var existingDiagnosis = CreateExistingDiagnosis(tenantId);
        _repository.GetByTenantIdAsync(Arg.Any<CancellationToken>()).Returns(existingDiagnosis);

        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingDiagnosis.Id);

        _repository.Received(1).Update(existingDiagnosis);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        existingDiagnosis.FrasePosicionamento.Should().Be(command.FrasePosicionamento);
        existingDiagnosis.SintesePerfil.Should().Be(command.SintesePerfil);
    }

    private static UpdatePersonaDiagnosisCommand CreateValidCommand()
    {
        return new UpdatePersonaDiagnosisCommand(
            FrasePosicionamento: "Nova frase de posicionamento",
            SintesePerfil: "Nova síntese de perfil",
            IdentidadeMarca: new IdentidadeMarcaDto("Acolhedor", "Clean e moderno", "O Sábio", "O Cuidador"),
            PilaresConteudo: new List<PilarConteudoDto>
            {
                new PilarConteudoDto("Educação", 40, "Conteúdo educativo", new[] { "Mitos e verdades" }),
                new PilarConteudoDto("Autoridade", 30, "Conteúdo de autoridade", new[] { "Estudo de caso" }),
                new PilarConteudoDto("Conexão", 30, "Bastidores", new[] { "Dia a dia" })
            },
            MatrizRestricoes: new MatrizRestricoesDto(
                TemasProibidos: new[] { "Tema X" },
                PalavrasEvitar: new[] { "Palavra Y" },
                DiretrizesInegociaveis: new[] { "Diretriz Z" },
                LimitesExposicao: "Apenas vida profissional"
            )
        );
    }

    private static PersonaDiagnosis CreateExistingDiagnosis(Guid tenantId)
    {
        var identidade = new IdentidadeMarca("Tom", "Estilo", "Sábio", "Herói");
        var pilares = new List<PilarConteudo>
        {
            new PilarConteudo("Educacional", 50, "Desc 1", new[] { "T1" }),
            new PilarConteudo("Bastidores", 50, "Desc 2", new[] { "T2" })
        };
        var restricoes = new MatrizRestricoes(new[] { "P1" }, new[] { "E1" }, new[] { "D1" }, "Limites");

        return PersonaDiagnosis.Create(
            tenantId,
            Guid.NewGuid(),
            "Frase original",
            "Síntese original",
            identidade,
            pilares,
            restricoes
        ).Value!;
    }
}
