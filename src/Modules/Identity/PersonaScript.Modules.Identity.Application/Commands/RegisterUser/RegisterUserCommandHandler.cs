using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAuthSession authSession) : ICommandHandler<RegisterUserCommand, Guid>
{
    private const int MinimumPasswordLength = 8;

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (!command.AcceptTerms)
        {
            return Result.Failure<Guid>(DomainErrors.Identity.TermsNotAccepted);
        }

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < MinimumPasswordLength)
        {
            return Result.Failure<Guid>(DomainErrors.Identity.PasswordTooShort);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<Guid>(DomainErrors.Identity.EmailAlreadyExists);
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var userResult = User.Register(command.FullName, normalizedEmail, passwordHash);

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        var user = userResult.Value;
        await userRepository.AddAsync(user, cancellationToken);

        await authSession.SignInAsync(new AuthUser(user.Id, user.Email, user.FullName), cancellationToken);

        return Result.Success(user.Id);
    }
}
