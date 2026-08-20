using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

public static class EthicalGovernanceSeeder
{
    public static async Task SeedAsync(BackofficeDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.CouncilRules.AnyAsync(cancellationToken))
        {
            var defaultCouncilRules = new[]
            {
                CouncilRule.Create(
                    councilAcronym: "CFM",
                    councilName: "Conselho Federal de Medicina",
                    resolutionNumber: "Resolução CFM 2.336/2023",
                    guidelinesText: "É vedado prometer resultados garantidos ou usar expressões apelativas como 'cura milagrosa' ou '100% garantido'. É obrigatório constar o Nome do Médico, CRM e RQE em peças de divulgação. Fotos de 'Antes e Depois' devem ter caráter puramente educativo e sem sensacionalismo.",
                    category: "Publicidade Médica",
                    isActive: true).Value,

                CouncilRule.Create(
                    councilAcronym: "CRO",
                    councilName: "Conselho Regional de Odontologia",
                    resolutionNumber: "Resolução CFO 196/2019",
                    guidelinesText: "Permitida divulgação de autodeclaração de qualificações, antes e depois com autorização formal do paciente e sem edição enganosa. Vedada divulgação de preços promocionais, sorteios ou facilidades de pagamento como atrativo comercial principal.",
                    category: "Publicidade Odontológica",
                    isActive: true).Value,

                CouncilRule.Create(
                    councilAcronym: "CRBM",
                    councilName: "Conselho Federal de Biomedicina",
                    resolutionNumber: "Resolução CFBM 330/2021",
                    guidelinesText: "O Biomédico esteta pode divulgar procedimentos com fins educativos, sendo obrigatório identificar seu número de registro no CRBM. É proibido anunciar produtos ou marcas comerciais como superiores sem comprovação científica.",
                    category: "Estética e Biomedicina",
                    isActive: true).Value
            };

            await dbContext.CouncilRules.AddRangeAsync(defaultCouncilRules, cancellationToken);
        }

        if (!await dbContext.ForbiddenTerms.AnyAsync(cancellationToken))
        {
            var defaultForbiddenTerms = new[]
            {
                ForbiddenTerm.Create(
                    term: "Cura garantida",
                    category: "PromessaExcessiva",
                    severity: ForbiddenTermSeverity.Prohibited,
                    replacementSuggestion: "Tratamento eficaz com acompanhamento personalizado",
                    reasoning: "Promessa irrestrita de cura fere regulamentações dos conselhos de saúde.",
                    isActive: true).Value,

                ForbiddenTerm.Create(
                    term: "100% garantido",
                    category: "PromessaExcessiva",
                    severity: ForbiddenTermSeverity.Prohibited,
                    replacementSuggestion: "Alta taxa de aprovação e satisfação dos pacientes",
                    reasoning: "Expressão apelativa e enganosa em serviços de saúde.",
                    isActive: true).Value,

                ForbiddenTerm.Create(
                    term: "Sem riscos",
                    category: "RegulaçãoSaude",
                    severity: ForbiddenTermSeverity.Prohibited,
                    replacementSuggestion: "Procedimento seguro conduzido por profissional habilitado",
                    reasoning: "Todo procedimento clínico carrega riscos inerentes que devem ser avaliados.",
                    isActive: true).Value,

                ForbiddenTerm.Create(
                    term: "Resultado imediato e definitivo",
                    category: "PromessaExcessiva",
                    severity: ForbiddenTermSeverity.Warning,
                    replacementSuggestion: "Resultados visíveis conforme evolução individual",
                    reasoning: "Respostas biológicas variam entre indivíduos.",
                    isActive: true).Value,

                ForbiddenTerm.Create(
                    term: "Melhor médico da cidade",
                    category: "PublicidadeMédica",
                    severity: ForbiddenTermSeverity.Prohibited,
                    replacementSuggestion: "Especialista dedicado ao seu bem-estar",
                    reasoning: "Autopromoção com superlativo exclusivo é vedada pelo CFM/CRO.",
                    isActive: true).Value
            };

            await dbContext.ForbiddenTerms.AddRangeAsync(defaultForbiddenTerms, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
