namespace PersonaScript.Modules.Anamnese.Application.DTOs;

public record FullAnamneseDto(
    AnamneseStatusDto Status,
    Etapa1Dto? Etapa1,
    Etapa2Dto? Etapa2,
    Etapa3Dto? Etapa3,
    Etapa4Dto? Etapa4,
    Etapa5Dto? Etapa5,
    Etapa6Dto? Etapa6,
    Etapa7Dto? Etapa7,
    Etapa8Dto? Etapa8,
    Etapa9Dto? Etapa9,
    Etapa10Dto? Etapa10
);
