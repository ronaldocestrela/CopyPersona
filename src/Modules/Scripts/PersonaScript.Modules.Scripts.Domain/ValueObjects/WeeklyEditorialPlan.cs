namespace PersonaScript.Modules.Scripts.Domain.ValueObjects;

public sealed record WeeklyEditorialPlan
{
    public int NumeroSemana { get; init; }
    public string TemaCentral { get; init; } = string.Empty;
    public string PilarConteudo { get; init; } = string.Empty;
    public string ObjetivoEstrategico { get; init; } = string.Empty;
    public string SugestaoFormato { get; init; } = string.Empty;
    public List<string> IdeiasConteudo { get; init; } = new();

    public WeeklyEditorialPlan() { } // EF Core / JSON Constructor

    public WeeklyEditorialPlan(
        int numeroSemana,
        string temaCentral,
        string pilarConteudo,
        string objetivoEstrategico,
        string sugestaoFormato,
        List<string>? ideiasConteudo = null)
    {
        NumeroSemana = numeroSemana;
        TemaCentral = temaCentral;
        PilarConteudo = pilarConteudo;
        ObjetivoEstrategico = objetivoEstrategico;
        SugestaoFormato = sugestaoFormato;
        IdeiasConteudo = ideiasConteudo ?? new List<string>();
    }
}
