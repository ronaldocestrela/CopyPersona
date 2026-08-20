using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;

public sealed record GenerateVideoScriptCommand(
    string Tema,
    string PilarConteudo,
    string Objetivo,
    string? TomDesejado = null,
    string? InstrucoesAdicionais = null
) : ICommand<Guid>, IQuotaProtectedCommand
{
    public QuotaResourceType QuotaResource => QuotaResourceType.ScriptGeneration;
}

