using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Decorators;

namespace PersonaScript.Modules.Billing.Tests.Application;

public record TestQuotaCommand(QuotaResourceType ResourceType, int Quantity = 1) : ICommand<Guid>, IQuotaProtectedCommand
{
    public QuotaResourceType QuotaResource => ResourceType;
    public int QuotaQuantity => Quantity;
}

public class QuotaValidationCommandHandlerDecoratorTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IUsageQuotaRepository _quotaRepository = Substitute.For<IUsageQuotaRepository>();
    private readonly IQuotaTransactionRepository _transactionRepository = Substitute.For<IQuotaTransactionRepository>();
    private readonly ICommandHandler<TestQuotaCommand, Guid> _innerHandler = Substitute.For<ICommandHandler<TestQuotaCommand, Guid>>();

    private QuotaValidationCommandHandlerDecorator<TestQuotaCommand, Guid> CreateDecorator()
    {
        return new QuotaValidationCommandHandlerDecorator<TestQuotaCommand, Guid>(
            _innerHandler,
            _tenantContext,
            _quotaRepository,
            _transactionRepository);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));
        var decorator = CreateDecorator();
        var command = new TestQuotaCommand(QuotaResourceType.ScriptGeneration);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Billing.TenantIdInvalid");
        await _innerHandler.DidNotReceive().Handle(Arg.Any<TestQuotaCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotaNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((UsageQuota?)null);

        var decorator = CreateDecorator();
        var command = new TestQuotaCommand(QuotaResourceType.ScriptGeneration);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.UsageQuota.NotFound.Code);
        await _innerHandler.DidNotReceive().Handle(Arg.Any<TestQuotaCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotaExceeded_ShouldReturnQuotaExceededErrorAndNotCallInnerHandler()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        // Create quota with scripts limit = 1 and generated count = 1
        var quota = UsageQuota.Create(tenantId, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 2, 5).Value;
        quota.ConsumeScript(); // Limit reached

        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(quota);

        var decorator = CreateDecorator();
        var command = new TestQuotaCommand(QuotaResourceType.ScriptGeneration);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.UsageQuota.ScriptLimitExceeded.Code);
        await _innerHandler.DidNotReceive().Handle(Arg.Any<TestQuotaCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotaAvailable_ShouldCallInnerHandlerAndConsumeQuota()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var expectedId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var quota = UsageQuota.Create(tenantId, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 2, 5).Value;
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(quota);

        var command = new TestQuotaCommand(QuotaResourceType.ScriptGeneration);
        _innerHandler.Handle(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedId));

        var decorator = CreateDecorator();

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        quota.ScriptsGeneratedCount.Should().Be(1);
        _quotaRepository.Received(1).Update(quota);
        await _transactionRepository.Received(1).AddAsync(
            Arg.Is<QuotaTransaction>(t => t != null && t.TenantId == tenantId && t.ResourceType == QuotaResourceType.ScriptGeneration),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantIsolation_TenantAExceededDoesNotBlockTenantB()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var expectedId = Guid.NewGuid();

        var quotaA = UsageQuota.Create(tenantA, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 2, 5).Value;
        quotaA.ConsumeScript(); // Tenant A exceeded

        var quotaB = UsageQuota.Create(tenantB, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 2, 5).Value; // Tenant B has quota

        _quotaRepository.GetByTenantIdAsync(tenantA, Arg.Any<CancellationToken>()).Returns(quotaA);
        _quotaRepository.GetByTenantIdAsync(tenantB, Arg.Any<CancellationToken>()).Returns(quotaB);

        var command = new TestQuotaCommand(QuotaResourceType.ScriptGeneration);
        _innerHandler.Handle(command, Arg.Any<CancellationToken>()).Returns(Result.Success(expectedId));

        var decorator = CreateDecorator();

        // Act - Tenant A
        _tenantContext.TenantId.Returns(TenantId.From(tenantA));
        var resultA = await decorator.Handle(command, CancellationToken.None);

        // Act - Tenant B
        _tenantContext.TenantId.Returns(TenantId.From(tenantB));
        var resultB = await decorator.Handle(command, CancellationToken.None);

        // Assert
        resultA.IsFailure.Should().BeTrue();
        resultB.IsSuccess.Should().BeTrue();
        quotaB.ScriptsGeneratedCount.Should().Be(1);
    }
}
