using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;

public record SaveAnamneseStepCommand(
    int Etapa,
    Etapa1Dto? Etapa1 = null,
    Etapa2Dto? Etapa2 = null,
    Etapa3Dto? Etapa3 = null,
    Etapa4Dto? Etapa4 = null,
    Etapa5Dto? Etapa5 = null,
    Etapa6Dto? Etapa6 = null,
    Etapa7Dto? Etapa7 = null,
    Etapa8Dto? Etapa8 = null,
    Etapa9Dto? Etapa9 = null,
    Etapa10Dto? Etapa10 = null
) : ICommand;
