using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user);
}
