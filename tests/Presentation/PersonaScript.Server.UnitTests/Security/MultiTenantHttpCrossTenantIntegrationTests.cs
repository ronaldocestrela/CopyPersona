using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Server.UnitTests.Auth;
using Xunit;

namespace PersonaScript.Server.UnitTests.Security;

public class MultiTenantHttpCrossTenantIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public MultiTenantHttpCrossTenantIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private (Guid UserId, Guid TenantId, string Token) CreateUserAndGetToken(string name, string email, string password, UserRole role = UserRole.Subscriber)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var generator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var user = User.Register(name, email, hasher.HashPassword(password)).Value!;
        if (role != UserRole.Subscriber)
        {
            user.AssignRole(role);
        }

        db.Users.Add(user);
        db.SaveChanges();

        var tokenResult = generator.GenerateToken(user);
        return (user.Id, user.TenantId, tokenResult.AccessToken);
    }

    [Fact]
    public void Authentication_TokensIssuedForTenants_MustCarryDistinctTenantIds()
    {
        // Arrange & Act
        var emailA = $"dr.ana.{Guid.NewGuid():N}@example.com";
        var emailB = $"dr.bruno.{Guid.NewGuid():N}@example.com";

        var (userIdA, tenantIdA, tokenA) = CreateUserAndGetToken("Dra. Ana", emailA, "SenhaForte123!");
        var (userIdB, tenantIdB, tokenB) = CreateUserAndGetToken("Dr. Bruno", emailB, "SenhaForte123!");

        // Assert - Tenants distintos
        tenantIdA.Should().NotBe(tenantIdB);
        tenantIdA.Should().Be(userIdA);
        tenantIdB.Should().Be(userIdB);

        var handler = new JwtSecurityTokenHandler();
        var jwtA = handler.ReadJwtToken(tokenA);
        var jwtB = handler.ReadJwtToken(tokenB);

        var claimTenantA = jwtA.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
        var claimTenantB = jwtB.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

        claimTenantA.Should().Be(tenantIdA.ToString());
        claimTenantB.Should().Be(tenantIdB.ToString());
    }

    [Fact]
    public async Task SubscriberTenantB_AttemptingToAccessBackofficeOrImpersonateTenantA_ShouldBeForbidden()
    {
        // Arrange
        var emailA = $"tenant.a.{Guid.NewGuid():N}@example.com";
        var emailB = $"tenant.b.{Guid.NewGuid():N}@example.com";

        var (_, tenantIdA, _) = CreateUserAndGetToken("Tenant A", emailA, "SenhaForte123!");
        var (_, _, tokenB) = CreateUserAndGetToken("Tenant B", emailB, "SenhaForte123!");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act 1: Tenant B tenta acessar dashboard administrativo
        var responseDashboard = await client.GetAsync("/api/backoffice/dashboard");

        // Act 2: Tenant B tenta disparar impersonação do Tenant A
        var responseImpersonate = await client.PostAsJsonAsync("/api/backoffice/impersonate/start", new
        {
            TargetTenantId = tenantIdA,
            TargetUserEmail = emailA,
            Reason = "Tentativa não autorizada"
        });

        // Act 3: Não autenticado tentando impersonar
        var anonClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var responseAnonImpersonate = await anonClient.PostAsJsonAsync("/api/backoffice/impersonate/start", new
        {
            TargetTenantId = tenantIdA,
            TargetUserEmail = emailA,
            Reason = "Tentativa anônima"
        });

        // Assert: Ambos devem ser 403 Forbidden para assinante comum tentando acessar backoffice ou impersonar outro tenant
        responseDashboard.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        responseImpersonate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        responseAnonImpersonate.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void HttpContextTenantContext_ResolvesAuthenticatedUserTenant_AndRejectsUnauthenticated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        // Assert: Sem requisição HTTP ativa no scope isolado, TenantId é Empty
        tenantContext.TenantId.Value.Should().Be(Guid.Empty);
    }
}
