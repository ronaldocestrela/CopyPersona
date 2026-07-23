using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    bool AcceptTerms) : ICommand<Guid>;
