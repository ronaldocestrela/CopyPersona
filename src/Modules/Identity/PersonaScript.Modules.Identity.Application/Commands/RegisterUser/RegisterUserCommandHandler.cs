using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterUserCommand, LoginResult>
{
    private const int MinimumPasswordLength = 8;

    public async Task<Result<LoginResult>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (!command.AcceptTerms)
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.TermsNotAccepted);
        }

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < MinimumPasswordLength)
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.PasswordTooShort);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.EmailAlreadyExists);
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var userResult = User.Register(command.FullName, normalizedEmail, passwordHash);

        if (userResult.IsFailure)
        {
            return Result.Failure<LoginResult>(userResult.Error);
        }

        var user = userResult.Value;
        await userRepository.AddAsync(user, cancellationToken);

        return Result.Success(new LoginResult(user.Id, user.Email, user.FullName));
    }
}
