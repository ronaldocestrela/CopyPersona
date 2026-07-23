namespace PersonaScript.Modules.Identity.Application.Abstractions;

public sealed record AuthUser(Guid UserId, string Email, string FullName);

public interface IAuthSession
{
    Task SignInAsync(AuthUser user, CancellationToken cancellationToken);

    Task SignOutAsync(CancellationToken cancellationToken);
}
