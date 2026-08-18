using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Services;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Personas.Tests.Commands;

public class GeneratePersonaDiagnosisCommandHandlerTests
{
    private readonly IPersonaDiagnosisRepository _repository = Substitute.For<IPersonaDiagnosisRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseQueryHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
    private readonly IPersonaDiagnosisGenerator _generator = Substitute.For<IPersonaDiagnosisGenerator>();
    private readonly GeneratePersonaDiagnosisCommandHandler _handler;

    public GeneratePersonaDiagnosisCommandHandlerTests()
    {
        _handler = new GeneratePersonaDiagnosisCommandHandler(
            _repository,
            _tenantContext,
            _getFullAnamneseQueryHandler,
            _generator
        );
    }

    [Fact]
    public async Task Handle_WhenTenantIdIsEmpty_ShouldReturnTenantIdInvalido()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));

        // Act
        var result = await _handler.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.TenantIdInvalido");
    }

    [Fact]
    public async Task Handle_WhenAnamneseNotFound_ShouldReturnAnamneseNaoEncontrada()
    {
        // Arrange
        var tenantId = TenantId.From(Guid.NewGuid());
        _tenantContext.TenantId.Returns(tenantId);

        _getFullAnamneseQueryHandler
            .Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<FullAnamneseDto>(PersonaScript.Modules.Anamnese.Domain.DomainErrors.Anamnese.NaoEncontrada));

        // Act
        var result = await _handler.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.AnamneseNaoEncontrada");
    }

    [Fact]
    public async Task Handle_WhenAnamneseNotCompleted_ShouldReturnAnamneseNaoConcluida()
    {
        // Arrange
        var tenantId = TenantId.From(Guid.NewGuid());
        _tenantContext.TenantId.Returns(tenantId);

        var incompleteAnamnese = TestAnamneseFactory.CreateFullAnamnese(status: AnamneseStatus.Rascunho);

        _getFullAnamneseQueryHandler
            .Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(incompleteAnamnese));

        // Act
        var result = await _handler.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.AnamneseNaoConcluida");
    }

    [Fact]
    public async Task Handle_WhenValidAnamneseCompleted_ShouldCreateAndSaveDiagnosis()
    {
        // Arrange
        var tenantGuid = Guid.NewGuid();
        var anamneseGuid = Guid.NewGuid();
        var tenantId = TenantId.From(tenantGuid);
        _tenantContext.TenantId.Returns(tenantId);

        var completedAnamnese = TestAnamneseFactory.CreateFullAnamnese(anamneseId: anamneseGuid, status: AnamneseStatus.Concluido);

        _getFullAnamneseQueryHandler
            .Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(completedAnamnese));

        var generatedDto = new PersonaDiagnosisLLMResponseDto
        {
            FrasePosicionamento = "A referência em medicina esportiva para atletas de alta performance.",
            SintesePerfil = "Dr. Carlos é ortopedista focado na reabilitação rápida.",
            TomDeVoz = "Científico e Prático",
            EstiloVisualSugerido = "Sóbrio com tons escuros",
            ArquetipoPrincipal = "O Especialista",
            ArquetipoSecundario = "O Herói",
            PilaresConteudo = new List<PilarLLMItemDto>
            {
                new() { Nome = "Educação", Percentual = 40, Descricao = "Dicas de lesões", ExemplosTopicos = new() { "Prevenção" } },
                new() { Nome = "Casos", Percentual = 30, Descricao = "Casos de sucesso", ExemplosTopicos = new() { "Reabilitação" } },
                new() { Nome = "Bastidores", Percentual = 30, Descricao = "Dia a dia", ExemplosTopicos = new() { "Cirurgias" } }
            },
            TemasProibidos = new List<string> { "Sensacionalismo" },
            PalavrasEvitar = new List<string> { "Cura rápida" },
            DiretrizesInegociaveis = new List<string> { "Ética médica" },
            LimitesExposicao = "Não mostrar família"
        };

        _generator.GenerateAsync(completedAnamnese, Arg.Any<CancellationToken>())
            .Returns(Result.Success(generatedDto));

        _repository.GetByTenantIdAsync(Arg.Any<CancellationToken>())
            .Returns((PersonaDiagnosis?)null);

        // Act
        var result = await _handler.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(Arg.Any<PersonaDiagnosis>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
