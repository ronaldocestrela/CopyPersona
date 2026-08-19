using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Tests;
using PersonaScript.Modules.Scripts.Application.Commands.RegenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using Xunit;
using ScriptDomainErrors = PersonaScript.Modules.Scripts.Domain.DomainErrors;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class RegenerateVideoScriptCommandHandlerTests
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IVideoScriptGenerator _generator;
    private readonly RegenerateVideoScriptCommandHandler _handler;

    public RegenerateVideoScriptCommandHandlerTests()
    {
        _repository = Substitute.For<IVideoScriptRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _getFullAnamneseHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        _personaDiagnosisRepository = Substitute.For<IPersonaDiagnosisRepository>();
        _generator = Substitute.For<IVideoScriptGenerator>();

        _handler = new RegenerateVideoScriptCommandHandler(
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
        var command = new RegenerateVideoScriptCommand(Guid.NewGuid(), "Mais dinâmico");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScriptDomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTargetScriptNotFound()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.NewGuid()));
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((VideoScript?)null);

        var command = new RegenerateVideoScriptCommand(Guid.NewGuid(), "Mais dinâmico");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScriptDomainErrors.Scripts.ScriptNaoEncontrado);
    }

    [Fact]
    public async Task Handle_ShouldRegenerateAndUpdateScript_WhenValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var script = VideoScript.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Oratória",
            "Educação",
            "Engajamento",
            "Gancho antigo",
            "Retenção antiga",
            "CTA antiga",
            "Legenda antiga",
            "Dicas antigas",
            "Tom antigo").Value;

        _repository.GetByIdAsync(script.Id, Arg.Any<CancellationToken>())
            .Returns(script);

        var anamneseDto = TestAnamneseFactory.CreateFullAnamnese();
        _getFullAnamneseHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(anamneseDto));

        var llmResponse = new VideoScriptLLMResponseDto
        {
            Gancho = "Novo gancho de 3s!",
            Retencao = "Nova retenção aprofundada.",
            ChamadaParaAcao = "Nova CTA direta.",
            LegendaSugerida = "Nova legenda.",
            DicasGravacao = "Novas dicas.",
            TomVozAplicado = "Mais dinâmico"
        };

        _generator.GenerateAsync(Arg.Any<FullAnamneseDto>(), Arg.Any<PersonaDiagnosis>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(llmResponse));

        var command = new RegenerateVideoScriptCommand(script.Id, "Quero um tom mais descontraído");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        script.Gancho.Should().Be("Novo gancho de 3s!");
        script.Retencao.Should().Be("Nova retenção aprofundada.");
        script.ChamadaParaAcao.Should().Be("Nova CTA direta.");
        script.FeedbackRating.Should().Be(ScriptFeedbackRating.NeedsAdjustment);
        script.FeedbackNotes.Should().Be("Quero um tom mais descontraído");

        await _repository.Received(1).UpdateAsync(script, Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
