using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Backoffice.Application.Commands.Impersonation;
using PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;
using Xunit;

namespace PersonaScript.Modules.Backoffice.Tests.Security;

public class BackofficeCrossTenantSecurityTests
{
    [Fact]
    public async Task StartImpersonation_ShouldAuditAndTargetExactTenant()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var adminEmail = "admin@personascript.com";
        var targetTenantId = Guid.NewGuid();

        var userRepoMock = Substitute.For<IUserRepository>();
        var impersonationLogRepoMock = Substitute.For<IAdminImpersonationLogRepository>();
        var auditLogRepoMock = Substitute.For<IAdminAuditLogRepository>();

        var targetUser = User.Register("Médico Alvo", "alvo@example.com", "hash").Value!;
        userRepoMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { targetUser });

        var handler = new StartImpersonationCommandHandler(
            userRepoMock, impersonationLogRepoMock, auditLogRepoMock);

        // Act
        var result = await handler.Handle(
            new StartImpersonationCommand(adminUserId, adminEmail, targetUser.TenantId, "Suporte urgente"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await impersonationLogRepoMock.Received(1).AddAsync(
            Arg.Is<AdminImpersonationLog>(l => l != null && l.TargetTenantId == targetUser.TenantId && l.TargetUserEmail == "alvo@example.com"),
            Arg.Any<CancellationToken>());

        await auditLogRepoMock.Received(1).AddAsync(
            Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "START_IMPERSONATION" && a.TargetTenantId == targetUser.TenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void HttpContextTenantContext_WithImpersonatedTenantId_ShouldResolveImpersonatedTenant()
    {
        // Arrange
        var originalAdminId = Guid.NewGuid();
        var impersonatedTenantId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, originalAdminId.ToString()),
            new("tenant_id", originalAdminId.ToString()),
            new("impersonated_tenant_id", impersonatedTenantId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var tenantContext = new HttpContextTenantContext(httpContextAccessor);

        // Act
        var resolvedTenantId = tenantContext.TenantId;

        // Assert - Contexto DEVE resolver o tenant impersonado, não o admin original
        resolvedTenantId.Value.Should().Be(impersonatedTenantId);
    }

    [Fact]
    public async Task OverrideTenantQuota_ModifiesOnlyTargetTenantQuota_LeavingOtherTenantsUntouched()
    {
        // Arrange
        var userRepoMock = Substitute.For<IUserRepository>();
        var quotaRepoMock = Substitute.For<IUsageQuotaRepository>();
        var auditLogRepoMock = Substitute.For<IAdminAuditLogRepository>();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var userA = User.Register("User A", "usera@example.com", "hash").Value;
        var userB = User.Register("User B", "userb@example.com", "hash").Value;

        userRepoMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { userA, userB });

        var quotaA = UsageQuota.Create(userA.TenantId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 1, 10).Value!;
        var quotaB = UsageQuota.Create(userB.TenantId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 1, 10).Value!;

        quotaRepoMock.GetByTenantIdAsync(userA.TenantId, Arg.Any<CancellationToken>()).Returns(quotaA);
        quotaRepoMock.GetByTenantIdAsync(userB.TenantId, Arg.Any<CancellationToken>()).Returns(quotaB);

        var handler = new OverrideTenantQuotaCommandHandler(userRepoMock, quotaRepoMock, auditLogRepoMock);

        // Act - Admin altera apenas o tenant A
        var command = new OverrideTenantQuotaCommand(
            Guid.NewGuid(), "admin@example.com", userA.TenantId, 100, 5, 50, "Bônus Tenant A");

        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quotaA.ScriptsLimit.Should().Be(100);
        quotaA.ActivePersonasLimit.Should().Be(5);
        quotaA.AiAnalysesLimit.Should().Be(50);

        // Tenant B permanece com seus limites intactos
        quotaB.ScriptsLimit.Should().Be(10);
        quotaB.ActivePersonasLimit.Should().Be(1);
        quotaB.AiAnalysesLimit.Should().Be(10);
    }
}
