using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken;

public sealed class GenerateJwtTokenCommandHandler : ICommandHandler<GenerateJwtTokenCommand, JwtTokenResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public GenerateJwtTokenCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<JwtTokenResult>> Handle(GenerateJwtTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure<JwtTokenResult>(DomainErrors.Identity.InvalidCredentials);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Result.Failure<JwtTokenResult>(DomainErrors.Identity.InvalidCredentials);
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result.Failure<JwtTokenResult>(DomainErrors.Identity.InvalidCredentials);
        }

        var tokenResult = _jwtTokenGenerator.GenerateToken(user);
        return Result.Success(tokenResult);
    }
}
