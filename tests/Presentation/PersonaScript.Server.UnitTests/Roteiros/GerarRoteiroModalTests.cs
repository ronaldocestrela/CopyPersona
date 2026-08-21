using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class GerarRoteiroModalTests : BunitContext
{
    private readonly ICommandHandler<GenerateVideoScriptCommand, Guid> _generateCommandHandler;

    public GerarRoteiroModalTests()
    {
        _generateCommandHandler = Substitute.For<ICommandHandler<GenerateVideoScriptCommand, Guid>>();
        Services.AddSingleton(_generateCommandHandler);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Modal_WhenIsVisibleIsFalse_ShouldNotRenderModal()
    {
        // Act
        var cut = Render<GerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.FindAll(".modal").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenIsVisibleIsTrue_ShouldRenderFormFields()
    {
        // Act
        var cut = Render<GerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.Find(".modal-title").TextContent.Should().Contain("Gerar Novo Roteiro com IA");
        cut.Find("input[name='tema']").Should().NotBeNull();
        cut.Find("select[name='pilarConteudo']").Should().NotBeNull();
        cut.Find("input[name='objetivo']").Should().NotBeNull();
    }

    [Fact]
    public void Modal_WhenSubmittingEmptyFields_ShouldShowValidationError()
    {
        // Act
        var cut = Render<GerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        var submitButton = cut.Find("button.btn-primary");
        submitButton.Click();

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Preencha todos os campos obrigatórios");
    }

    [Fact]
    public void Modal_WhenSubmittingValidFields_ShouldInvokeCommandHandlerAndNotifyParent()
    {
        // Arrange
        var generatedId = Guid.NewGuid();
        _generateCommandHandler.Handle(Arg.Any<GenerateVideoScriptCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(generatedId)));

        Guid? callbackScriptId = null;

        var cut = Render<GerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnScriptGenerated, (Guid id) => callbackScriptId = id));

        // Act
        cut.Find("input[name='tema']").Change("Como Vender Serviços de Consultoria");
        cut.Find("select[name='pilarConteudo']").Change("Conversão & Vendas");
        cut.Find("input[name='objetivo']").Change("Atrair clientes qualificados");

        var submitButton = cut.Find("button.btn-primary");
        submitButton.Click();

        // Assert
        _generateCommandHandler.Received().Handle(
            Arg.Is<GenerateVideoScriptCommand>(c =>
                c != null &&
                c.Tema == "Como Vender Serviços de Consultoria" &&
                c.PilarConteudo == "Conversão & Vendas" &&
                c.Objetivo == "Atrair clientes qualificados"),
            Arg.Any<CancellationToken>());

        callbackScriptId.Should().Be(generatedId);
    }

    [Fact]
    public void Modal_WhenCommandHandlerFails_ShouldDisplayErrorMessage()
    {
        // Arrange
        _generateCommandHandler.Handle(Arg.Any<GenerateVideoScriptCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<Guid>(new Error("Script.QuotaExceeded", "Limite mensal de roteiros atingido."))));

        var cut = Render<GerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        cut.Find("input[name='tema']").Change("Como Vender Serviços de Consultoria");
        cut.Find("select[name='pilarConteudo']").Change("Conversão & Vendas");
        cut.Find("input[name='objetivo']").Change("Atrair clientes qualificados");

        var submitButton = cut.Find("button.btn-primary");
        submitButton.Click();

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Limite mensal de roteiros atingido.");
    }
}
