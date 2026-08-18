using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
        {
            return Result.Failure(DomainErrors.Identity.PasswordTooShort);
        }

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Token))
        {
            return Result.Failure(DomainErrors.Identity.PasswordResetTokenInvalid);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Failure(DomainErrors.Identity.UserNotFound);
        }

        var newPasswordHash = passwordHasher.HashPassword(command.NewPassword);
        var resetResult = user.ResetPassword(newPasswordHash, command.Token, DateTimeOffset.UtcNow);

        if (resetResult.IsFailure)
        {
            return resetResult;
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
