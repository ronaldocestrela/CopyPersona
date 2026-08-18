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

    public string? PasswordResetToken { get; private set; }

    public DateTimeOffset? PasswordResetTokenExpiresAt { get; private set; }

    private User()
    {
    }

    public string GeneratePasswordResetToken(TimeSpan validFor)
    {
        var tokenBytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        PasswordResetToken = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        PasswordResetTokenExpiresAt = DateTimeOffset.UtcNow.Add(validFor);
        return PasswordResetToken;
    }

    public Result ResetPassword(string newPasswordHash, string token, DateTimeOffset currentUtc)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            return Result.Failure(DomainErrors.Identity.PasswordHashRequired);
        }

        if (string.IsNullOrWhiteSpace(PasswordResetToken) || !string.Equals(PasswordResetToken, token, StringComparison.Ordinal))
        {
            return Result.Failure(DomainErrors.Identity.PasswordResetTokenInvalid);
        }

        if (!PasswordResetTokenExpiresAt.HasValue || currentUtc > PasswordResetTokenExpiresAt.Value)
        {
            return Result.Failure(DomainErrors.Identity.PasswordResetTokenExpired);
        }

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;

        return Result.Success();
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

    public static Result<User> RegisterFromExternalProvider(string fullName, string email, string provider, string providerKey)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<User>(DomainErrors.Identity.FullNameRequired);
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email.Trim()))
        {
            return Result.Failure<User>(DomainErrors.Identity.EmailInvalid);
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = $"EXTERNAL_OAUTH_{provider.ToUpperInvariant()}_{providerKey}",
        };

        user.TenantId = user.Id;

        return Result.Success(user);
    }
}
