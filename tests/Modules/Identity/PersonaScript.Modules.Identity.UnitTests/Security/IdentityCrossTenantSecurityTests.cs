using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.ResetPassword;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Security;
using Xunit;
using IdentityUser = PersonaScript.Modules.Identity.Domain.User;

namespace PersonaScript.Modules.Identity.UnitTests.Security;

public class IdentityCrossTenantSecurityTests
{
    private static IdentityDbContext CreateDbContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new IdentityDbContext(options, tenantContext);
    }

    [Fact]
    public void JwtToken_GeneratedForUserA_MustContainStrictTenantIdMatchingUserId()
    {
        // Arrange
        var userA = IdentityUser.Register("Dr. Tenant A", "tenanta@example.com", "hash_secret").Value;
        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "Super_Secret_Test_Key_That_Is_Long_Enough_256_Bits!",
            Issuer = "PersonaScriptTest",
            Audience = "PersonaScriptTestAudience",
            ExpirationMinutes = 60
        });

        var tokenGenerator = new JwtTokenGenerator(jwtOptions);

        // Act
        var tokenResult = tokenGenerator.GenerateToken(userA);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenResult.AccessToken);

        var tenantClaim = token.Claims.FirstOrDefault(c => c.Type == "tenant_id");
        tenantClaim.Should().NotBeNull();
        tenantClaim!.Value.Should().Be(userA.TenantId.ToString());
        tenantClaim.Value.Should().Be(userA.Id.ToString());

        var subClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(userA.Id.ToString());
    }

    [Fact]
    public async Task Interceptor_AttemptingToModifyTenantId_MustThrowException()
    {
        // Arrange
        var dbName = "IdentitySecurity_" + Guid.NewGuid();
        var userA = IdentityUser.Register("Dr. Tenant A", "tenanta@example.com", "hash_secret").Value;
        var tenantContextA = new FixedTenantContext(TenantId.From(userA.TenantId));

        using (var context = CreateDbContext(dbName, tenantContextA))
        {
            context.Users.Add(userA);
            await context.SaveChangesAsync();

            // Act: Tentativa de alterar o TenantId para o de outro usuário/tenant
            userA.SetTenantId(Guid.NewGuid());
            context.Entry(userA).Property(u => u.TenantId).IsModified = true;

            var act = async () => await context.SaveChangesAsync();

            // Assert: Interceptor bloqueia mutação cross-tenant
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Cannot modify TenantId*");
        }
    }

    [Fact]
    public async Task PasswordReset_UserBCannotUseUserAResetToken()
    {
        // Arrange
        var userA = IdentityUser.Register("User A", "usera@example.com", "hashA").Value;
        var userB = IdentityUser.Register("User B", "userb@example.com", "hashB").Value;

        var tokenA = userA.GeneratePasswordResetToken(TimeSpan.FromHours(1));

        var userRepoMock = Substitute.For<IUserRepository>();
        var passwordHasherMock = Substitute.For<IPasswordHasher>();
        passwordHasherMock.HashPassword(Arg.Any<string>()).Returns("hashed_secure_password");

        userRepoMock.GetByEmailAsync("userb@example.com", Arg.Any<CancellationToken>())
            .Returns(userB);

        var handler = new ResetPasswordCommandHandler(userRepoMock, passwordHasherMock);

        // Act - User B tenta usar o token gerado para o User A
        var result = await handler.Handle(
            new ResetPasswordCommand("userb@example.com", tokenA, "NewSecurePassword123!"),
            CancellationToken.None);

        // Assert - Falha na validação do token
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Identity.PasswordResetTokenInvalid);
    }
}
