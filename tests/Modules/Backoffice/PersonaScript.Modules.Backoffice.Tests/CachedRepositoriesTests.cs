using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

namespace PersonaScript.Modules.Backoffice.Tests;

public class CachedRepositoriesTests
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task CachedPromptTemplateRepository_GetActiveByAgentNameAsync_ShouldHitCacheOnSecondCall()
    {
        var innerRepo = Substitute.For<IPromptTemplateRepository>();
        var prompt = PromptTemplate.Create("Strategist", 1, "System", "User", "{}", "Desc", "admin@test.com", true).Value;
        innerRepo.GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>())
            .Returns(prompt);

        var cachedRepo = new CachedPromptTemplateRepository(innerRepo, _memoryCache);

        // First call - cache miss -> calls inner repo
        var firstResult = await cachedRepo.GetActiveByAgentNameAsync("Strategist");
        firstResult.Should().Be(prompt);
        await innerRepo.Received(1).GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>());

        // Second call - cache hit -> inner repo is NOT called again
        var secondResult = await cachedRepo.GetActiveByAgentNameAsync("Strategist");
        secondResult.Should().Be(prompt);
        await innerRepo.Received(1).GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedPromptTemplateRepository_UpdateAsync_ShouldEvictCacheForAgent()
    {
        var innerRepo = Substitute.For<IPromptTemplateRepository>();
        var prompt = PromptTemplate.Create("Strategist", 1, "System", "User", "{}", "Desc", "admin@test.com", true).Value;
        innerRepo.GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>())
            .Returns(prompt);

        var cachedRepo = new CachedPromptTemplateRepository(innerRepo, _memoryCache);

        // Populate cache
        await cachedRepo.GetActiveByAgentNameAsync("Strategist");
        await innerRepo.Received(1).GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>());

        // Update evicts
        await cachedRepo.UpdateAsync(prompt);

        // Subsequent call queries inner repo again
        await cachedRepo.GetActiveByAgentNameAsync("Strategist");
        await innerRepo.Received(2).GetActiveByAgentNameAsync("Strategist", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedCouncilRuleRepository_GetByAcronymAsync_ShouldHitCacheOnSecondCall()
    {
        var innerRepo = Substitute.For<ICouncilRuleRepository>();
        var rule = CouncilRule.Create("CFM", "Conselho Federal de Medicina", "Res 123", "Guidelines", "Medicina").Value;
        innerRepo.GetByAcronymAsync("CFM", Arg.Any<CancellationToken>())
            .Returns(rule);

        var cachedRepo = new CachedCouncilRuleRepository(innerRepo, _memoryCache);

        // First call
        var first = await cachedRepo.GetByAcronymAsync("CFM");
        first.Should().Be(rule);
        await innerRepo.Received(1).GetByAcronymAsync("CFM", Arg.Any<CancellationToken>());

        // Second call from cache
        var second = await cachedRepo.GetByAcronymAsync("CFM");
        second.Should().Be(rule);
        await innerRepo.Received(1).GetByAcronymAsync("CFM", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedCouncilRuleRepository_UpdateAsync_ShouldEvictCache()
    {
        var innerRepo = Substitute.For<ICouncilRuleRepository>();
        var rule = CouncilRule.Create("CFM", "Conselho Federal de Medicina", "Res 123", "Guidelines", "Medicina").Value;
        innerRepo.GetByAcronymAsync("CFM", Arg.Any<CancellationToken>())
            .Returns(rule);

        var cachedRepo = new CachedCouncilRuleRepository(innerRepo, _memoryCache);

        await cachedRepo.GetByAcronymAsync("CFM");
        await innerRepo.Received(1).GetByAcronymAsync("CFM", Arg.Any<CancellationToken>());

        await cachedRepo.UpdateAsync(rule);

        await cachedRepo.GetByAcronymAsync("CFM");
        await innerRepo.Received(2).GetByAcronymAsync("CFM", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedForbiddenTermRepository_GetAllActiveAsync_ShouldHitCacheAndEvictOnDelete()
    {
        var innerRepo = Substitute.For<IForbiddenTermRepository>();
        var term = ForbiddenTerm.Create("Garantia de cura", "Medicina", ForbiddenTermSeverity.Prohibited, "Resultados comprovados", "Vedado pelo CFM", true).Value;
        var list = new List<ForbiddenTerm> { term };
        innerRepo.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns(list);

        var cachedRepo = new CachedForbiddenTermRepository(innerRepo, _memoryCache);

        var first = await cachedRepo.GetAllActiveAsync();
        first.Should().HaveCount(1);
        await innerRepo.Received(1).GetAllActiveAsync(Arg.Any<CancellationToken>());

        var second = await cachedRepo.GetAllActiveAsync();
        second.Should().HaveCount(1);
        await innerRepo.Received(1).GetAllActiveAsync(Arg.Any<CancellationToken>());

        await cachedRepo.DeleteAsync(term);

        await cachedRepo.GetAllActiveAsync();
        await innerRepo.Received(2).GetAllActiveAsync(Arg.Any<CancellationToken>());
    }
}
