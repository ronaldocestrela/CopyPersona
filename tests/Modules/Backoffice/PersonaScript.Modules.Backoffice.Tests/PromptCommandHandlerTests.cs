using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.Commands.Prompts;
using PersonaScript.Modules.Backoffice.Application.Queries.Prompts;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Tests;

public class PromptCommandHandlerTests
{
    private readonly IPromptTemplateRepository _promptRepository = Substitute.For<IPromptTemplateRepository>();
    private readonly IAdminAuditLogRepository _auditLogRepository = Substitute.For<IAdminAuditLogRepository>();
    private readonly ILLMProvider _llmProvider = Substitute.For<ILLMProvider>();

    [Fact]
    public async Task CreatePromptVersion_ShouldIncrementVersion_DeactivatePreviousActive_AndRecordAuditLog()
    {
        var activeV1 = PromptTemplate.Create(
            "Agent1_Diagnosis", 1, "Sys v1", "User v1", "{}", "Initial", "admin@personascript.ai", isActive: true).Value;

        _promptRepository.GetLatestVersionNumberAsync("Agent1_Diagnosis", Arg.Any<CancellationToken>())
            .Returns(1);
        _promptRepository.GetActiveByAgentNameAsync("Agent1_Diagnosis", Arg.Any<CancellationToken>())
            .Returns(activeV1);

        var handler = new CreatePromptVersionCommandHandler(_promptRepository, _auditLogRepository);
        var command = new CreatePromptVersionCommand(
            "Agent1_Diagnosis",
            "Sys v2",
            "User v2",
            "{\"Temperature\": 0.6}",
            "Melhoria na clareza",
            "admin@personascript.ai");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        activeV1!.IsActive.Should().BeFalse();
        await _promptRepository.Received(1).UpdateAsync(activeV1!, Arg.Any<CancellationToken>());
        await _promptRepository.Received(1).AddAsync(Arg.Is<PromptTemplate>(p =>
            p != null &&
            p.AgentName == "Agent1_Diagnosis" &&
            p.Version == 2 &&
            p.SystemPrompt == "Sys v2" &&
            p.UserPromptTemplate == "User v2" &&
            p.IsActive == true), Arg.Any<CancellationToken>());

        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a =>
            a != null && a.ActionType == "CREATE_PROMPT_VERSION"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RollbackPromptVersion_ShouldActivateTargetVersion_DeactivateCurrent_AndRecordAuditLog()
    {
        var v1 = PromptTemplate.Create(
            "Agent1_Diagnosis", 1, "Sys v1", "User v1", "{}", "v1", "admin@personascript.ai", isActive: false).Value;
        var v2 = PromptTemplate.Create(
            "Agent1_Diagnosis", 2, "Sys v2", "User v2", "{}", "v2", "admin@personascript.ai", isActive: true).Value;

        _promptRepository.GetByIdAsync(v1.Id, Arg.Any<CancellationToken>()).Returns(v1);
        _promptRepository.GetActiveByAgentNameAsync("Agent1_Diagnosis", Arg.Any<CancellationToken>()).Returns(v2);

        var handler = new RollbackPromptVersionCommandHandler(_promptRepository, _auditLogRepository);
        var command = new RollbackPromptVersionCommand(v1.Id, "Rollback devido a inconsistência na v2", "admin@personascript.ai");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        v2!.IsActive.Should().BeFalse();
        v1!.IsActive.Should().BeTrue();

        await _promptRepository.Received(1).UpdateAsync(v2, Arg.Any<CancellationToken>());
        await _promptRepository.Received(1).UpdateAsync(v1, Arg.Any<CancellationToken>());
        await _auditLogRepository.Received(1).AddAsync(Arg.Is<AdminAuditLog>(a =>
            a != null && a.ActionType == "ROLLBACK_PROMPT_VERSION"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestPromptPlayground_ShouldExecuteLLM_AndReturnResult()
    {
        var llmResponse = new LLMResponse
        {
            Content = "Resposta gerada de teste",
            ProviderType = LLMProviderType.Mock,
            ModelUsed = "gpt-4o",
            PromptTokens = 50,
            CompletionTokens = 100,
            LatencyMs = 120
        };

        _llmProvider.CompleteAsync(Arg.Any<LLMRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(llmResponse)));

        var handler = new TestPromptPlaygroundCommandHandler(_llmProvider);
        var command = new TestPromptPlaygroundCommand(
            "Agent1_Diagnosis",
            "System prompt de teste",
            "User prompt de teste com {{Nome}}",
            "{\"Temperature\": 0.5}",
            "{\"Nome\": \"Dr. Pedro\"}");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.ResponseContent.Should().Be("Resposta gerada de teste");
        result.Value.PromptTokens.Should().Be(50);
        result.Value.CompletionTokens.Should().Be(100);
    }
}
