using Microsoft.AspNetCore.Identity;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Identity.Infrastructure.Security;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password) =>
        _passwordHasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string passwordHash) =>
        _passwordHasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
