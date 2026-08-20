using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Commands.Impersonation;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class ImpersonationTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IAdminImpersonationLogRepository _impersonationLogRepository = Substitute.For<IAdminImpersonationLogRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();

    [Fact]
    public async Task Start_ShouldCreateImpersonationLog_AndAuditLog()
    {
        var targetUser = User.Register("Cliente Alvo", "cliente@example.com", "hash123").Value;
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { targetUser });

        var handler = new StartImpersonationCommandHandler(_userRepository, _impersonationLogRepository, _auditLogRepository);
        var command = new StartImpersonationCommand(
            Guid.NewGuid(),
            "suporte@personascript.ai",
            targetUser.TenantId,
            "Atendimento ao chamado #999",
            "127.0.0.1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _impersonationLogRepository.Received(1).AddAsync(Arg.Is<AdminImpersonationLog>(l => l != null && l.TargetTenantId == targetUser.TenantId), Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "START_IMPERSONATION"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Start_ShouldFail_WhenReasonIsTooShort()
    {
        var targetUser = User.Register("Cliente Alvo", "cliente@example.com", "hash123").Value;
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { targetUser });

        var handler = new StartImpersonationCommandHandler(_userRepository, _impersonationLogRepository, _auditLogRepository);
        var command = new StartImpersonationCommand(
            Guid.NewGuid(),
            "suporte@personascript.ai",
            targetUser.TenantId,
            "curto");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AdminImpersonationLog.ReasonRequired");
    }
}
