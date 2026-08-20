using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Commands.FreezeAccount;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class FreezeAccountTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();

    [Fact]
    public async Task Handle_ShouldFreezeAccount_WhenUserExists()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hash123").Value;
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var handler = new FreezeTenantAccountCommandHandler(_userRepository, _auditLogRepository);
        var command = new FreezeTenantAccountCommand(Guid.NewGuid(), "admin@personascript.ai", user.TenantId, "Suspeita de uso indevido");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsFrozen.Should().BeTrue();
        user.FreezeReason.Should().Be("Suspeita de uso indevido");

        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "FREEZE_ACCOUNT"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUnfreezeAccount_WhenUserIsFrozen()
    {
        var user = User.Register("Maria Silva", "maria@example.com", "hash123").Value;
        user.Freeze("Motivo teste");
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var handler = new UnfreezeTenantAccountCommandHandler(_userRepository, _auditLogRepository);
        var command = new UnfreezeTenantAccountCommand(Guid.NewGuid(), "admin@personascript.ai", user.TenantId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsFrozen.Should().BeFalse();

        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "UNFREEZE_ACCOUNT"), Arg.Any<CancellationToken>());
    }
}
