using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.UnitTests.Application;

public class GenerateJwtTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();

    [Fact]
    public async Task Handle_ShouldReturnJwtTokenResult_WhenCredentialsAreValid()
    {
        var user = User.Register("Ana Souza", "ana@example.com", "hashed-password").Value;
        _userRepository.GetByEmailAsync("ana@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("secret123", "hashed-password").Returns(true);

        var expectedJwtResult = new JwtTokenResult("fake-jwt-token", "Bearer", 7200, user.Id, user.TenantId);
        _jwtTokenGenerator.GenerateToken(user).Returns(expectedJwtResult);

        var handler = new GenerateJwtTokenCommandHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
        var command = new GenerateJwtTokenCommand("ana@example.com", "secret123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("fake-jwt-token");
        result.Value.TenantId.Should().Be(user.TenantId);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFoundOrPasswordInvalid()
    {
        _userRepository.GetByEmailAsync("ana@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new GenerateJwtTokenCommandHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
        var command = new GenerateJwtTokenCommand("ana@example.com", "wrong-pass");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("identity.invalid_credentials");
    }
}
