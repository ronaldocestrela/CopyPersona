using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using PersonaScript.Modules.Personas.Tests;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class GenerateVideoScriptCommandHandlerTests
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IVideoScriptGenerator _generator;
    private readonly GenerateVideoScriptCommandHandler _handler;

    public GenerateVideoScriptCommandHandlerTests()
    {
        _repository = Substitute.For<IVideoScriptRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _getFullAnamneseHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        _personaDiagnosisRepository = Substitute.For<IPersonaDiagnosisRepository>();
        _generator = Substitute.For<IVideoScriptGenerator>();

        _handler = new GenerateVideoScriptCommandHandler(
            _repository,
            _tenantContext,
            _getFullAnamneseHandler,
            _personaDiagnosisRepository,
            _generator);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantIdIsEmpty()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));
        var command = new GenerateVideoScriptCommand("Tema", "Pilar", "Objetivo", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAnamneseNotFound()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.NewGuid()));
        _getFullAnamneseHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<FullAnamneseDto>(Error.NotFound("Anamnese.NotFound", "Não encontrada")));

        var command = new GenerateVideoScriptCommand("Tema", "Pilar", "Objetivo", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PersonaScript.Modules.Scripts.Domain.DomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndSaveVideoScriptWithDraftStatus_WhenValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var anamneseDto = TestAnamneseFactory.CreateFullAnamnese();

        _getFullAnamneseHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(anamneseDto));

        var diagnosis = PersonaDiagnosis.Create(
            tenantId,
            anamneseDto.Status.Id,
            "Frase de posicionamento",
            "Síntese",
            new IdentidadeMarca("Direto", "Clean", "O Sábio", "O Herói"),
            new List<PilarConteudo> { new PilarConteudo("Educação", 100, "Desc", new List<string>()) },
            new MatrizRestricoes(new List<string>(), new List<string>(), new List<string>(), "Limites")).Value;

        _personaDiagnosisRepository.GetByTenantIdAsync(Arg.Any<CancellationToken>())
            .Returns(diagnosis);

        var llmResponse = new VideoScriptLLMResponseDto
        {
            Gancho = "Primeiros 3 segundos matadores!",
            Retencao = "Desenvolvimento com alto valor e retenção.",
            ChamadaParaAcao = "Comente 'Roteiro' para saber mais.",
            LegendaSugerida = "Legenda pronta para Instagram.",
            DicasGravacao = "Enquadramento em plano médio com boa iluminação.",
            TomVozAplicado = "Direto e Acolhedor"
        };

        _generator.GenerateAsync(Arg.Any<FullAnamneseDto>(), Arg.Any<PersonaDiagnosis>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(llmResponse));

        var command = new GenerateVideoScriptCommand("3 Segredos de Oratória", "Educação", "Engajamento", "Empático", "Instruções extras");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(Arg.Is<VideoScript>(s =>
            s != null &&
            s.TenantId == tenantId &&
            s.Tema == "3 Segredos de Oratória" &&
            s.Gancho == "Primeiros 3 segundos matadores!" &&
            s.Retencao == "Desenvolvimento com alto valor e retenção." &&
            s.ChamadaParaAcao == "Comente 'Roteiro' para saber mais." &&
            s.Status == VideoScriptStatus.Draft
        ), Arg.Any<CancellationToken>());

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
