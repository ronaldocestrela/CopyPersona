using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Abstractions;

namespace PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken;

public sealed record GenerateJwtTokenCommand(
    string Email,
    string Password) : ICommand<JwtTokenResult>;
