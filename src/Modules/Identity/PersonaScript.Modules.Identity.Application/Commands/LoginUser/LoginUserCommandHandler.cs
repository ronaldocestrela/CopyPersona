using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<LoginUserCommand, LoginResult>
{
    public async Task<Result<LoginResult>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.InvalidCredentials);
        }

        return Result.Success(new LoginResult(user.Id, user.Email, user.FullName));
    }
}
