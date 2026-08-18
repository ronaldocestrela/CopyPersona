using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IEmailSender emailSender)
    : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            // Retorna sucesso para não expor estado
            return Result.Success();
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        var token = user.GeneratePasswordResetToken(TimeSpan.FromHours(24));
        await userRepository.UpdateAsync(user, cancellationToken);

        var baseUrl = string.IsNullOrWhiteSpace(command.BaseUrl) ? "http://localhost:5000" : command.BaseUrl.TrimEnd('/');
        var resetLink = $"{baseUrl}/redefinir-senha?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendPasswordResetEmailAsync(user.Email, resetLink, cancellationToken);

        return Result.Success();
    }
}
