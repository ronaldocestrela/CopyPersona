using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Identity.Application.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;
