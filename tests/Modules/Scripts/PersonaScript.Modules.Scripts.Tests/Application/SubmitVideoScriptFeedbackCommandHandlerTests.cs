using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.Commands.SubmitVideoScriptFeedback;
using PersonaScript.Modules.Scripts.Domain;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class SubmitVideoScriptFeedbackCommandHandlerTests
{
    private readonly IVideoScriptRepository _repository = Substitute.For<IVideoScriptRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly SubmitVideoScriptFeedbackCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public SubmitVideoScriptFeedbackCommandHandlerTests()
    {
        _tenantContext.TenantId.Returns(TenantId.From(_tenantId));
        _handler = new SubmitVideoScriptFeedbackCommandHandler(_repository, _tenantContext);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantIdIsEmpty()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));
        var command = new SubmitVideoScriptFeedbackCommand(Guid.NewGuid(), ScriptFeedbackRating.Liked);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.TenantIdInvalido);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenScriptNotFound()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((VideoScript?)null);

        var command = new SubmitVideoScriptFeedbackCommand(Guid.NewGuid(), ScriptFeedbackRating.Liked);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.ScriptNaoEncontrado);
    }

    [Fact]
    public async Task Handle_ShouldRegisterFeedbackAndUpdateRepository_WhenValid()
    {
        // Arrange
        var script = VideoScript.Create(
            _tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Tema de Teste",
            "Pilar",
            "Objetivo",
            "Gancho",
            "Retenção",
            "CTA",
            "Legenda",
            "Dicas",
            "Tom").Value;

        _repository.GetByIdAsync(script.Id, Arg.Any<CancellationToken>())
            .Returns(script);

        var command = new SubmitVideoScriptFeedbackCommand(script.Id, ScriptFeedbackRating.Liked, "Ótimo tom!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        script.FeedbackRating.Should().Be(ScriptFeedbackRating.Liked);
        script.FeedbackNotes.Should().Be("Ótimo tom!");
        await _repository.Received(1).UpdateAsync(script, Arg.Any<CancellationToken>());
    }
}
