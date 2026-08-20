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
        result.Value.Role.Should().Be(UserRole.Subscriber);
    }

    [Fact]
    public void AssignRole_ShouldUpdateUserRole()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hashed-password").Value;

        user.AssignRole(UserRole.SystemAdmin);

        user.Role.Should().Be(UserRole.SystemAdmin);
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

    [Fact]
    public void GeneratePasswordResetToken_ShouldSetTokenAndExpiration()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hashed-password").Value;

        var token = user.GeneratePasswordResetToken(TimeSpan.FromHours(24));

        token.Should().NotBeNullOrWhiteSpace();
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ResetPassword_ShouldFail_WhenTokenIsInvalid()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hashed-password").Value;
        user.GeneratePasswordResetToken(TimeSpan.FromHours(24));

        var result = user.ResetPassword("new-hash", "invalid-token", DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.password_reset_token_invalid");
    }

    [Fact]
    public void ResetPassword_ShouldFail_WhenTokenIsExpired()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hashed-password").Value;
        var token = user.GeneratePasswordResetToken(TimeSpan.FromMinutes(15));
        var future = DateTimeOffset.UtcNow.AddMinutes(30);

        var result = user.ResetPassword("new-hash", token, future);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.password_reset_token_expired");
    }

    [Fact]
    public void ResetPassword_ShouldSucceed_WhenTokenIsValidAndNotExpired()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;
        var token = user.GeneratePasswordResetToken(TimeSpan.FromHours(1));

        var result = user.ResetPassword("new-hash", token, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Freeze_ShouldSetIsFrozenTrue_AndReason()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;

        var result = user.Freeze("Violação de termos");

        result.IsSuccess.Should().BeTrue();
        user.IsFrozen.Should().BeTrue();
        user.FreezeReason.Should().Be("Violação de termos");
        user.FrozenAt.Should().NotBeNull();
    }

    [Fact]
    public void Freeze_ShouldFail_WhenReasonIsEmpty()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;

        var result = user.Freeze("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.freeze_reason_required");
    }

    [Fact]
    public void Unfreeze_ShouldResetIsFrozenFalse()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;
        user.Freeze("Suspeita de fraudes");

        var result = user.Unfreeze();

        result.IsSuccess.Should().BeTrue();
        user.IsFrozen.Should().BeFalse();
        user.FreezeReason.Should().BeNull();
        user.FrozenAt.Should().BeNull();
    }
}
