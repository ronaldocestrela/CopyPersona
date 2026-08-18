using System.Text.RegularExpressions;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Identity.Domain;

public sealed class User : BaseEntity, IMustHaveTenant
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public Guid TenantId { get; private set; }

    public void SetTenantId(Guid tenantId) => TenantId = tenantId;

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    private User()
    {
    }

    public static Result<User> Register(string fullName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<User>(DomainErrors.Identity.FullNameRequired);
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email.Trim()))
        {
            return Result.Failure<User>(DomainErrors.Identity.EmailInvalid);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(DomainErrors.Identity.PasswordHashRequired);
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
        };

        user.TenantId = user.Id;

        return Result.Success(user);
    }
}
