using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.RequestPasswordReset;
using PersonaScript.Modules.Identity.Application.Commands.ResetPassword;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Application;

public class PasswordResetTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnSuccess_AndSendEmail_WhenUserExists()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;
        _userRepository.GetByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new RequestPasswordResetCommandHandler(_userRepository, _emailSender);

        var result = await handler.Handle(
            new RequestPasswordResetCommand("maria@example.com", "https://app.personascript.ai"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendPasswordResetEmailAsync(
            "maria@example.com",
            Arg.Is<string>(link => link != null && link.Contains("/redefinir-senha?email=maria%40example.com&token=")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestPasswordReset_ShouldReturnSuccess_WithoutSendingEmail_WhenUserNotFound()
    {
        _userRepository.GetByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new RequestPasswordResetCommandHandler(_userRepository, _emailSender);

        var result = await handler.Handle(
            new RequestPasswordResetCommand("unknown@example.com"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _emailSender.DidNotReceiveWithAnyArgs().SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenPasswordTooShort()
    {
        var handler = new ResetPasswordCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand("maria@example.com", "token123", "short"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.password_too_short");
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenUserNotFound()
    {
        _userRepository.GetByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.HashPassword("newpassword123").Returns("new-hash");

        var handler = new ResetPasswordCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand("maria@example.com", "token123", "newpassword123"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.user_not_found");
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WhenTokenIsValid()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "old-hash").Value;
        var token = user.GeneratePasswordResetToken(TimeSpan.FromHours(1));

        _userRepository.GetByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword("newpassword123").Returns("new-hash");

        var handler = new ResetPasswordCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand("maria@example.com", token, "newpassword123"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetToken.Should().BeNull();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }
}
