using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class OverrideTenantQuotaCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUsageQuotaRepository _quotaRepository = Substitute.For<IUsageQuotaRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();
    private readonly OverrideTenantQuotaCommandHandler _handler;

    public OverrideTenantQuotaCommandHandlerTests()
    {
        _handler = new OverrideTenantQuotaCommandHandler(_userRepository, _quotaRepository, _auditLogRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldOverrideQuotaAndRecordAuditLog()
    {
        // Arrange
        var user = User.Register("Médico", "medico@example.com", "hash").Value;
        var tenantId = user.TenantId;
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User> { user });

        var quota = UsageQuota.Create(tenantId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 2, 20).Value;
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(quota);

        var command = new OverrideTenantQuotaCommand(
            Guid.NewGuid(),
            "admin@example.com",
            tenantId,
            100,
            10,
            200,
            "Plano Parceiro VIP");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quota.ScriptsLimit.Should().Be(100);
        quota.ActivePersonasLimit.Should().Be(10);
        quota.AiAnalysesLimit.Should().Be(200);

        await _quotaRepository.Received(1).UpdateAsync(quota, Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "OVERRIDE_TENANT_QUOTA"), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>());

        var command = new OverrideTenantQuotaCommand(
            Guid.NewGuid(),
            "admin@example.com",
            Guid.NewGuid(),
            100, 10, 200, "Motivo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OverrideTenantQuota.UserNotFound");
    }

    [Fact]
    public async Task Handle_WhenQuotaNotFound_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Register("Médico", "medico@example.com", "hash").Value;
        var tenantId = user.TenantId;
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User> { user });

        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((UsageQuota?)null);

        var command = new OverrideTenantQuotaCommand(
            Guid.NewGuid(),
            "admin@example.com",
            tenantId,
            100, 10, 200, "Motivo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OverrideTenantQuota.QuotaNotFound");
    }
}

