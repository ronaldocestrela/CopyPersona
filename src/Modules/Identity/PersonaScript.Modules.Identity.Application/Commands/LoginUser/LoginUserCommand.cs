using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Identity.Application.Commands.LoginUser;

public sealed record LoginResult(Guid UserId, string Email, string FullName);

public sealed record LoginUserCommand(string Email, string Password) : ICommand<LoginResult>;
