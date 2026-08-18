using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;

namespace PersonaScript.Modules.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    bool AcceptTerms) : ICommand<LoginResult>;
