using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.ExternalLogin;

public sealed class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public ExternalLoginCommandHandler(
        IUserRepository userRepository,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<Result<LoginResult>> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Provider))
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.ProviderRequired);
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<LoginResult>(DomainErrors.Identity.EmailInvalid);
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            var fullName = string.IsNullOrWhiteSpace(command.FullName)
                ? normalizedEmail.Split('@')[0]
                : command.FullName.Trim();

            var userResult = User.RegisterFromExternalProvider(fullName, normalizedEmail, command.Provider, command.ProviderKey);
            if (userResult.IsFailure)
            {
                return Result.Failure<LoginResult>(userResult.Error);
            }

            user = userResult.Value;
            await _userRepository.AddAsync(user, cancellationToken);

            await _emailSender.SendWelcomeEmailAsync(user.Email, user.FullName, cancellationToken);
        }

        return Result.Success(new LoginResult(user.Id, user.Email, user.FullName));
    }
}
