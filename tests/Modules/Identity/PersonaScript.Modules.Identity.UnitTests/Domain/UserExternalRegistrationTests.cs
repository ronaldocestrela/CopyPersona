using FluentAssertions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Domain;

public class UserExternalRegistrationTests
{
    [Fact]
    public void RegisterFromExternalProvider_ShouldSucceed_AndSetTenantIdEqualToUserId()
    {
        var result = User.RegisterFromExternalProvider("João Souza", "joao@example.com", "Google", "google-sub-123");

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("João Souza");
        result.Value.Email.Should().Be("joao@example.com");
        result.Value.TenantId.Should().Be(result.Value.Id);
    }

    [Fact]
    public void RegisterFromExternalProvider_ShouldFail_WhenEmailIsInvalid()
    {
        var result = User.RegisterFromExternalProvider("João Souza", "invalid-email", "Google", "google-sub-123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.email_invalid");
    }

    [Fact]
    public void RegisterFromExternalProvider_ShouldFail_WhenFullNameIsEmpty()
    {
        var result = User.RegisterFromExternalProvider("  ", "joao@example.com", "Google", "google-sub-123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.full_name_required");
    }
}
