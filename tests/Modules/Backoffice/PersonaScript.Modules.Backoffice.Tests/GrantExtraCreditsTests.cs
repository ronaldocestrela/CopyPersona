using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Commands.GrantExtraCredits;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class GrantExtraCreditsTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUsageQuotaRepository _usageQuotaRepository = Substitute.For<IUsageQuotaRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();

    [Fact]
    public async Task Handle_ShouldGrantExtraCredits_WhenQuotaExists()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hash123").Value;
        var quota = UsageQuota.Create(user.TenantId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10, 2, 20).Value;

        _userRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });
        _usageQuotaRepository.GetByTenantIdAsync(user.TenantId, Arg.Any<CancellationToken>())
            .Returns(quota);

        var handler = new GrantTenantExtraCreditsCommandHandler(_userRepository, _usageQuotaRepository, _auditLogRepository);
        var command = new GrantTenantExtraCreditsCommand(Guid.NewGuid(), "admin@personascript.ai", user.TenantId, 5, 10, "Bônus Suporte #456");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        quota.ScriptsLimit.Should().Be(15);
        quota.AiAnalysesLimit.Should().Be(30);

        await _usageQuotaRepository.Received(1).UpdateAsync(quota, Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "GRANT_EXTRA_CREDITS"), Arg.Any<CancellationToken>());
    }
}
