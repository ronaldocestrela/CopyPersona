using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Identity.Application.Commands.RequestPasswordReset;

public record RequestPasswordResetCommand(string Email, string BaseUrl = "http://localhost:5000") : ICommand;
