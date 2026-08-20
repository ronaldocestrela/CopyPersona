using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Compliance;

public record CreateForbiddenTermCommand(
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning,
    bool IsActive = true
) : ICommand<Guid>;

public sealed class CreateForbiddenTermCommandHandler : ICommandHandler<CreateForbiddenTermCommand, Guid>
{
    private readonly IForbiddenTermRepository _repository;

    public CreateForbiddenTermCommandHandler(IForbiddenTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateForbiddenTermCommand command, CancellationToken cancellationToken)
    {
        var termResult = ForbiddenTerm.Create(
            command.Term,
            command.Category,
            command.Severity,
            command.ReplacementSuggestion,
            command.Reasoning,
            command.IsActive);

        if (termResult.IsFailure)
            return Result.Failure<Guid>(termResult.Error);

        await _repository.AddAsync(termResult.Value, cancellationToken);
        return Result.Success(termResult.Value.Id);
    }
}

public record UpdateForbiddenTermCommand(
    Guid Id,
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning
) : ICommand;

public sealed class UpdateForbiddenTermCommandHandler : ICommandHandler<UpdateForbiddenTermCommand>
{
    private readonly IForbiddenTermRepository _repository;

    public UpdateForbiddenTermCommandHandler(IForbiddenTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateForbiddenTermCommand command, CancellationToken cancellationToken)
    {
        var term = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (term == null)
            return Result.Failure(Error.NotFound("ForbiddenTerm.NotFound", "Termo proibido não encontrado."));

        term.Update(command.Term, command.Category, command.Severity, command.ReplacementSuggestion, command.Reasoning);
        await _repository.UpdateAsync(term, cancellationToken);

        return Result.Success();
    }
}

public record ToggleForbiddenTermStatusCommand(Guid Id) : ICommand;

public sealed class ToggleForbiddenTermStatusCommandHandler : ICommandHandler<ToggleForbiddenTermStatusCommand>
{
    private readonly IForbiddenTermRepository _repository;

    public ToggleForbiddenTermStatusCommandHandler(IForbiddenTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ToggleForbiddenTermStatusCommand command, CancellationToken cancellationToken)
    {
        var term = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (term == null)
            return Result.Failure(Error.NotFound("ForbiddenTerm.NotFound", "Termo proibido não encontrado."));

        term.ToggleActive();
        await _repository.UpdateAsync(term, cancellationToken);

        return Result.Success();
    }
}

public record DeleteForbiddenTermCommand(Guid Id) : ICommand;

public sealed class DeleteForbiddenTermCommandHandler : ICommandHandler<DeleteForbiddenTermCommand>
{
    private readonly IForbiddenTermRepository _repository;

    public DeleteForbiddenTermCommandHandler(IForbiddenTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteForbiddenTermCommand command, CancellationToken cancellationToken)
    {
        var term = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (term == null)
            return Result.Failure(Error.NotFound("ForbiddenTerm.NotFound", "Termo proibido não encontrado."));

        await _repository.DeleteAsync(term, cancellationToken);
        return Result.Success();
    }
}
