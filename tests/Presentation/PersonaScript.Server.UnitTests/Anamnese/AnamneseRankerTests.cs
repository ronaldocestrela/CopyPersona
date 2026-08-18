using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class AnamneseRankerTests : BunitContext
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Component_ShouldRenderDefaultItemsAndMoveUpCorrectly()
    {
        string? updatedValue = null;
        var cut = Render<AnamneseRanker>(parameters => parameters
            .Add(p => p.Value, "Item A; Item B; Item C")
            .Add(p => p.ValueChanged, v => updatedValue = v));

        cut.FindAll(".anamnese-ranker-item").Count.Should().Be(3);

        // Click move up on second item (Item B)
        var buttons = cut.FindAll(".anamnese-ranker-btn");
        // Index 2 is MoveUp for Item B
        buttons[2].Click();

        updatedValue.Should().Be("Item B; Item A; Item C");
    }
}
