using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Application;

public class AuthCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task Register_ShouldFail_WhenTermsNotAccepted()
    {
        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new RegisterUserCommand("Maria", "maria@example.com", "password123", false),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.terms_not_accepted");
    }

    [Fact]
    public async Task Register_ShouldFail_WhenEmailAlreadyExists()
    {
        _userRepository.ExistsByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns(true);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hash");

        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new RegisterUserCommand("Maria Silva", "maria@example.com", "password123", true),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.email_already_exists");
    }

    [Fact]
    public async Task Register_ShouldReturnLoginResult_WhenSuccessful()
    {
        _userRepository.ExistsByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hash");

        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new RegisterUserCommand("Maria Silva", "maria@example.com", "password123", true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("maria@example.com");
        result.Value.FullName.Should().Be("Maria Silva");
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Login_ShouldFail_WithGenericError_WhenUserNotFound()
    {
        _userRepository.GetByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher);

        var result = await handler.Handle(
            new LoginUserCommand("maria@example.com", "password123"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.invalid_credentials");
    }
}
