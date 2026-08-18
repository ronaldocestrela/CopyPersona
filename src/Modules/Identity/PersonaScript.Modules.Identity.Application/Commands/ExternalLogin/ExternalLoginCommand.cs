using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;

namespace PersonaScript.Modules.Identity.Application.Commands.ExternalLogin;

public sealed record ExternalLoginCommand(
    string Provider,
    string ProviderKey,
    string Email,
    string FullName) : ICommand<LoginResult>;
