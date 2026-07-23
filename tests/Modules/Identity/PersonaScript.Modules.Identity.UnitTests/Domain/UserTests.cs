using FluentAssertions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Register_ShouldSetTenantIdEqualToUserId()
    {
        var result = User.Register("Maria Silva", "maria@example.com", "hashed-password");

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(result.Value.Id);
        result.Value.Email.Should().Be("maria@example.com");
    }

    [Fact]
    public void Register_ShouldFail_WhenFullNameIsEmpty()
    {
        var result = User.Register("  ", "maria@example.com", "hashed-password");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.full_name_required");
    }

    [Fact]
    public void Register_ShouldFail_WhenEmailIsInvalid()
    {
        var result = User.Register("Maria Silva", "invalid-email", "hashed-password");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.email_invalid");
    }

    [Fact]
    public void Register_ShouldFail_WhenPasswordHashIsEmpty()
    {
        var result = User.Register("Maria Silva", "maria@example.com", string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.password_hash_required");
    }
}
