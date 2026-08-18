using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class InstagramLinkListInputTests : BunitContext
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void ShouldRenderInitialEmptyInputWhenValuesIsEmpty()
    {
        var cut = Render<InstagramLinkListInput>(parameters => parameters
            .Add(p => p.Values, Array.Empty<string>())
            .Add(p => p.Placeholder, "ex: dramarianacosta"));

        var inputs = cut.FindAll("input");
        inputs.Should().HaveCount(1);
        inputs[0].GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void ShouldRenderExistingValuesPlusOneEmptyInputWhenLessThanMaxItems()
    {
        var initialValues = new[] { "dramariana" };

        var cut = Render<InstagramLinkListInput>(parameters => parameters
            .Add(p => p.Values, initialValues));

        var inputs = cut.FindAll("input");
        inputs.Should().HaveCount(2);
        inputs[0].GetAttribute("value").Should().Be("dramariana");
        inputs[1].GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void ShouldEnforceMaxItemsLimit()
    {
        var tenValues = Enumerable.Range(1, 10).Select(i => $"perfil{i}").ToArray();

        var cut = Render<InstagramLinkListInput>(parameters => parameters
            .Add(p => p.Values, tenValues)
            .Add(p => p.MaxItems, 10));

        var inputs = cut.FindAll("input");
        inputs.Should().HaveCount(10);
        cut.Markup.Should().Contain("10/10 perfis");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void ShouldTriggerValuesChangedWhenInputIsEntered()
    {
        IReadOnlyCollection<string>? updatedValues = null;

        var cut = Render<InstagramLinkListInput>(parameters => parameters
            .Add(p => p.Values, Array.Empty<string>())
            .Add(p => p.ValuesChanged, v => updatedValues = v));

        var input = cut.Find("input");
        input.Input("https://instagram.com/dramariana/");

        updatedValues.Should().NotBeNull();
        updatedValues.Should().ContainSingle().Which.Should().Be("dramariana");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void ShouldRemoveItemWhenRemoveButtonClicked()
    {
        IReadOnlyCollection<string>? updatedValues = null;

        var cut = Render<InstagramLinkListInput>(parameters => parameters
            .Add(p => p.Values, new[] { "perfil1", "perfil2" })
            .Add(p => p.ValuesChanged, v => updatedValues = v));

        var removeButtons = cut.FindAll("button.btn-remove-profile");
        removeButtons.Should().NotBeEmpty();

        removeButtons[0].Click();

        updatedValues.Should().NotBeNull();
        updatedValues.Should().Equal("perfil2");
    }
}
