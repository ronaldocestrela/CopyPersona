using System.Text.Json.Serialization;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa5SuasReferencias(
    [property: JsonConverter(typeof(FlexibleStringCollectionJsonConverter))] IReadOnlyCollection<string> PerfisArea,
    string OQueAdmiraArea,
    string OQueNaoFariaArea,
    [property: JsonConverter(typeof(FlexibleStringCollectionJsonConverter))] IReadOnlyCollection<string> PerfisForaArea,
    string OQueAtraiForaArea
);

