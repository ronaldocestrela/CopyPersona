using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.ExternalLogin;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Application;

public class ExternalLoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();

    [Fact]
    public async Task Handle_ShouldAutoProvisionUserAndSendWelcomeEmail_WhenUserDoesNotExist()
    {
        _userRepository.GetByEmailAsync("carlos@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new ExternalLoginCommandHandler(_userRepository, _emailSender);

        var command = new ExternalLoginCommand("Google", "google-sub-456", "carlos@example.com", "Carlos Alberto");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("carlos@example.com");
        result.Value.FullName.Should().Be("Carlos Alberto");
        result.Value.UserId.Should().NotBe(Guid.Empty);

        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u != null && u.Email == "carlos@example.com" && u.TenantId == u.Id), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendWelcomeEmailAsync("carlos@example.com", "Carlos Alberto", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingUser_WhenUserAlreadyExists()
    {
        var existingUser = User.Register("Carlos Alberto", "carlos@example.com", "hashed-password").Value;

        _userRepository.GetByEmailAsync("carlos@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var handler = new ExternalLoginCommandHandler(_userRepository, _emailSender);

        var command = new ExternalLoginCommand("Google", "google-sub-456", "carlos@example.com", "Carlos Alberto");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(existingUser.Id);

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendWelcomeEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProviderIsEmpty()
    {
        var handler = new ExternalLoginCommandHandler(_userRepository, _emailSender);

        var command = new ExternalLoginCommand("", "sub-123", "carlos@example.com", "Carlos");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.provider_required");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailIsEmpty()
    {
        var handler = new ExternalLoginCommandHandler(_userRepository, _emailSender);

        var command = new ExternalLoginCommand("Google", "sub-123", "", "Carlos");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.email_invalid");
    }
}
