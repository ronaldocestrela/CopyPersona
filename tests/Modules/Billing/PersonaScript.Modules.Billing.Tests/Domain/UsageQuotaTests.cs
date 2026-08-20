using FluentAssertions;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Tests.Domain;


public class UsageQuotaTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _subscriptionId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeUsageQuotaWithZeroCounts()
    {
        // Arrange
        var periodStart = DateTime.UtcNow;
        var periodEnd = periodStart.AddMonths(1);

        // Act
        var quotaResult = UsageQuota.Create(_tenantId, _subscriptionId, periodStart, periodEnd, scriptsLimit: 10, personasLimit: 2, aiAnalysesLimit: 15);

        // Assert
        quotaResult.IsSuccess.Should().BeTrue();
        var quota = quotaResult.Value;
        quota.TenantId.Should().Be(_tenantId);
        quota.SubscriptionId.Should().Be(_subscriptionId);
        quota.ScriptsGeneratedCount.Should().Be(0);
        quota.ActivePersonasCount.Should().Be(0);
        quota.AiAnalysesCount.Should().Be(0);
        quota.ScriptsLimit.Should().Be(10);
        quota.ActivePersonasLimit.Should().Be(2);
        quota.AiAnalysesLimit.Should().Be(15);
    }

    [Fact]
    public void ConsumeScript_WhenQuotaAvailable_ShouldIncrementCountAndReturnTransaction()
    {
        // Arrange
        var quota = UsageQuota.Create(_tenantId, _subscriptionId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), scriptsLimit: 5, personasLimit: 2, aiAnalysesLimit: 10).Value;

        // Act
        var consumeResult = quota.ConsumeScript("CreateScriptCommand");

        // Assert
        consumeResult.IsSuccess.Should().BeTrue();
        quota.ScriptsGeneratedCount.Should().Be(1);
        var transaction = consumeResult.Value;
        transaction.TenantId.Should().Be(_tenantId);
        transaction.ResourceType.Should().Be(QuotaResourceType.ScriptGeneration);
        transaction.Quantity.Should().Be(1);
        transaction.SourceCommand.Should().Be("CreateScriptCommand");
    }

    [Fact]
    public void ConsumeScript_WhenQuotaExceeded_ShouldReturnFailureWithoutIncrementing()
    {
        // Arrange
        var quota = UsageQuota.Create(_tenantId, _subscriptionId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), scriptsLimit: 1, personasLimit: 2, aiAnalysesLimit: 10).Value;
        quota.ConsumeScript("Cmd1");

        // Act
        var result = quota.ConsumeScript("Cmd2");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UsageQuota.ScriptLimitExceeded);
        quota.ScriptsGeneratedCount.Should().Be(1);
    }

    [Fact]
    public void CanCreatePersona_WhenLimitReached_ShouldReturnFailure()
    {
        // Arrange
        var quota = UsageQuota.Create(_tenantId, _subscriptionId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), scriptsLimit: 5, personasLimit: 2, aiAnalysesLimit: 10).Value;

        // Act
        var canCreateResult = quota.CanCreatePersona(currentActivePersonasCount: 2);

        // Assert
        canCreateResult.IsFailure.Should().BeTrue();
        canCreateResult.Error.Should().Be(DomainErrors.UsageQuota.PersonaLimitExceeded);
    }

    [Fact]
    public void ResetMonthlyQuota_ShouldResetCountsAndUpdateLimits()
    {
        // Arrange
        var quota = UsageQuota.Create(_tenantId, _subscriptionId, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(15), scriptsLimit: 5, personasLimit: 2, aiAnalysesLimit: 10).Value;
        quota.ConsumeScript("Cmd1");
        quota.ConsumeAiAnalysis("Cmd2");

        var newStart = DateTime.UtcNow;
        var newEnd = newStart.AddMonths(1);

        // Act
        var resetResult = quota.ResetMonthlyQuota(newStart, newEnd, scriptsLimit: 20, personasLimit: 5, aiAnalysesLimit: 50);

        // Assert
        resetResult.IsSuccess.Should().BeTrue();
        quota.ScriptsGeneratedCount.Should().Be(0);
        quota.AiAnalysesCount.Should().Be(0);
        quota.ScriptsLimit.Should().Be(20);
        quota.ActivePersonasLimit.Should().Be(5);
        quota.AiAnalysesLimit.Should().Be(50);
        quota.PeriodStart.Should().Be(newStart);
        quota.PeriodEnd.Should().Be(newEnd);
    }
}
