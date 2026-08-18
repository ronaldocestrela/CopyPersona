using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class InstagramLinkInputTests : BunitContext
{
    [Theory]
    [InlineData("@dramariana", "dramariana")]
    [InlineData("https://www.instagram.com/dramariana/", "dramariana")]
    [InlineData("instagram.com/dramariana", "dramariana")]
    [InlineData("dramariana", "dramariana")]
    [InlineData("", "")]
    public void ExtractHandle_ShouldNormalizeInputCorrectly(string input, string expected)
    {
        var result = InstagramLinkInput.ExtractHandle(input);
        result.Should().Be(expected);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Component_ShouldRenderFormattedHandleAndTriggerCallback()
    {
        string? updatedValue = null;
        var cut = Render<InstagramLinkInput>(parameters => parameters
            .Add(p => p.Value, "@dramariana")
            .Add(p => p.ValueChanged, v => updatedValue = v));

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("dramariana");
        cut.Markup.Should().Contain("instagram.com/dramariana");

        input.Input("https://instagram.com/novoperfil/");
        updatedValue.Should().Be("novoperfil");
    }
}
