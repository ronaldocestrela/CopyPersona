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
using PersonaScript.Modules.Scripts.Application.Commands.GenerateContentPlan;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using Xunit;
using ScriptDomainErrors = PersonaScript.Modules.Scripts.Domain.DomainErrors;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class GenerateContentPlanCommandHandlerTests
{
    private readonly IStoryPlanRepository _storyPlanRepository;
    private readonly INinetyDayCalendarRepository _calendarRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IContentPlanGenerator _generator;
    private readonly GenerateContentPlanCommandHandler _handler;

    public GenerateContentPlanCommandHandlerTests()
    {
        _storyPlanRepository = Substitute.For<IStoryPlanRepository>();
        _calendarRepository = Substitute.For<INinetyDayCalendarRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _getFullAnamneseHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        _personaDiagnosisRepository = Substitute.For<IPersonaDiagnosisRepository>();
        _generator = Substitute.For<IContentPlanGenerator>();

        _handler = new GenerateContentPlanCommandHandler(
            _storyPlanRepository,
            _calendarRepository,
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
        var command = new GenerateContentPlanCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScriptDomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAnamneseNotFound()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.NewGuid()));
        _getFullAnamneseHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<FullAnamneseDto>(Error.NotFound("Anamnese.NotFound", "Não encontrada")));

        var command = new GenerateContentPlanCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScriptDomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndSavePlans_WhenValid()
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
            "Frase Posicionamento",
            "Síntese",
            new IdentidadeMarca("Direto", "Clean", "O Sábio", "O Herói"),
            new List<PilarConteudo> { new PilarConteudo("Educação", 100, "Desc", new List<string>()) },
            new MatrizRestricoes(new List<string>(), new List<string>(), new List<string>(), "Limites")).Value;

        _personaDiagnosisRepository.GetByTenantIdAsync(Arg.Any<CancellationToken>())
            .Returns(diagnosis);

        var llmResponse = new ContentPlanLLMResponseDto(
            new StoryPlanLLMResponseDto(
                "3 stories por dia",
                new List<StoryBlockLLMDto>
                {
                    new("Manhã", "08:00", "Chegada", "Bastidores", "Exemplo", "Conexão")
                },
                "Diretrizes"),
            new NinetyDayCalendarLLMResponseDto(
                "Objetivo Trimestral",
                new List<WeeklyEditorialPlanLLMDto>
                {
                    new(1, "Tema 1", "Educação", "Objetivo", "Vídeo", new List<string> { "Ideia 1" })
                })
        );

        _generator.GeneratePlanAsync(Arg.Any<FullAnamneseDto>(), Arg.Any<PersonaDiagnosis>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(llmResponse));

        var command = new GenerateContentPlanCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlanoStories.Should().NotBeNull();
        result.Value.Calendario90Dias.Should().NotBeNull();
        result.Value.PlanoStories.FrequenciaDiariaRecomendada.Should().Be("3 stories por dia");
        result.Value.Calendario90Dias.ObjetivoTrimestral.Should().Be("Objetivo Trimestral");

        await _storyPlanRepository.Received(1).AddAsync(Arg.Any<StoryPlan>(), Arg.Any<CancellationToken>());
        await _calendarRepository.Received(1).AddAsync(Arg.Any<NinetyDayCalendar>(), Arg.Any<CancellationToken>());
    }
}
