using FluentAssertions;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.UnitTests.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldBeSuccessfulWithoutError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldBeFailedWithError()
    {
        var error = Error.Validation("persona.nicho_invalido", "Nicho is required.");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ShouldExposeValue()
    {
        var id = Guid.NewGuid();
        var result = Result.Success(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
    }

    [Fact]
    public void GenericFailure_ShouldThrowWhenAccessingValue()
    {
        var error = Error.NotFound("persona.not_found", "Persona not found.");
        var result = Result.Failure<Guid>(error);

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}
