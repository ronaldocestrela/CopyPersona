using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class UpdatePlanLimitsCommandHandlerTests
{
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();
    private readonly UpdatePlanLimitsCommandHandler _handler;

    public UpdatePlanLimitsCommandHandlerTests()
    {
        _handler = new UpdatePlanLimitsCommandHandler(_planRepository, _auditLogRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePlanAndRecordAuditLog()
    {
        // Arrange
        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value;
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);

        var command = new UpdatePlanLimitsCommand(
            Guid.NewGuid(),
            "admin@example.com",
            plan.Id,
            "Pro Max",
            "Nova Descrição",
            120m,
            1200m,
            10,
            60,
            100);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Name.Should().Be("Pro Max");
        plan.MonthlyPrice.Should().Be(120m);
        plan.MaxScriptsPerMonth.Should().Be(60);

        _planRepository.Received(1).Update(plan);
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a => a != null && a.ActionType == "UPDATE_PLAN_LIMITS"), Arg.Any<CancellationToken>());
    }



    [Fact]
    public async Task Handle_WhenPlanNotFound_ShouldReturnFailure()
    {
        // Arrange
        _planRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Plan?)null);

        var command = new UpdatePlanLimitsCommand(
            Guid.NewGuid(),
            "admin@example.com",
            Guid.NewGuid(),
            "Pro",
            "Desc",
            100m,
            1000m,
            5,
            30,
            50);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UpdatePlanLimits.NotFound");
    }
}
