using FluentAssertions;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.UnitTests.CQRS;

public class CqrsTests
{
    private sealed record PingCommand : ICommand;

    private sealed record GetPingQuery : IQuery<string>;

    private sealed class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public Task<Result> Handle(PingCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success());
    }

    private sealed class GetPingQueryHandler : IQueryHandler<GetPingQuery, string>
    {
        public Task<Result<string>> Handle(GetPingQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success("pong"));
    }

    [Fact]
    public async Task CommandHandler_ShouldReturnResult()
    {
        var handler = new PingCommandHandler();
        var result = await handler.Handle(new PingCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task QueryHandler_ShouldReturnResultWithValue()
    {
        var handler = new GetPingQueryHandler();
        var result = await handler.Handle(new GetPingQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("pong");
    }
}
