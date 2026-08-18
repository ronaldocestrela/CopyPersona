using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Personas.Tests;

public static class TestAnamneseFactory
{
    public static FullAnamneseDto CreateFullAnamnese(Guid? anamneseId = null, AnamneseStatus status = AnamneseStatus.Concluido)
    {
        var id = anamneseId ?? Guid.NewGuid();
        return new FullAnamneseDto(
            Status: new AnamneseStatusDto(id, status, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow),
            Etapa1: new Etapa1Dto(
                NomeCompleto: "Dra. Ana Paula Silva",
                ComoGostaSerChamado: "Dra. Ana Paula",
                ProfissaoEspecialidade: "Dermatologia Estética",
                TempoAtuacaoAnos: 10,
                FormacoesEspecializacoes: "USP, RQE 12345",
                PremiosTitulos: "Membro da SBD",
                PacientesMes: 50,
                MomentoAtual: MomentoAtualEnum.IniciandoAgenda
            ),
            Etapa2: new Etapa2Dto(
                MotivacaoEscolha: "A paixão por cuidar da saúde da pele",
                CasoMarcante: "Atendimento de paciente com queimadura grave",
                FaseMaisDificil: "Início da carreira solo",
                MotorPessoal: "Autonomia e excelência técnica"
            ),
            Etapa3: new Etapa3Dto(
                ProcedimentoMaster: "Rejuvenescimento Facial",
                ProcedimentoLucrativo: "Bioestimuladores de Colágeno",
                ProcedimentoPreferido: "Preenchimento consciente",
                DiferencialAtendimento: "Avaliação 360 graus",
                PorQueEscolhemVoce: "Naturalidade dos resultados",
                CriticaAosPares: "Excessos e rostos padronizados"
            ),
            Etapa4: new Etapa4Dto(
                PerfilDemograficoPsicografico: "Mulheres de 35 a 55 anos, classe A/B",
                MaioresMedos: "Envelhecer mal e ficar com rosto de filtro de instagram",
                MaioresDesejos: "Aparência descansada e jovial sem perder a identidade",
                PerguntasFrequentes: "Quanto tempo dura? Dói?",
                MitosInformacoesErradas: "Botox congela a expressão",
                CanalOrigem: CanalOrigemEnum.Instagram
            ),
            Etapa5: new Etapa5Dto(
                PerfisArea: new[] { "@perfil1", "@perfil2" },
                OQueAdmiraArea: "Elegância e profundidade científica",
                OQueNaoFariaArea: "Dancinhas ridículas e exposição apelativa",
                PerfisForaArea: new[] { "@arquitetura", "@arte" },
                OQueAtraiForaArea: "Estética minimalista"
            ),
            Etapa6: new Etapa6Dto(
                AssuntosProibidos: "Política partidária, religião",
                VidaPessoalAceita: "Fotos sutis de viagens",
                EstiloVidaAceito: "Rotina saudável",
                TrabalhoAceito: "Procedimentos e clínica",
                NivelConfortoCamera: NivelConfortoCameraEnum.SuperAVontade,
                RegrasConselhoRegional: "Seguir resoluções do CFM/SBD"
            ),
            Etapa7: new Etapa7Dto(
                TemasFavoritos: "Anatomia do envelhecimento, estética natural",
                TemaPalestra: "Como envelhecer com saúde e elegância",
                VerdadeCorajosa: "Menos é mais na estética facial",
                PostsDeuCerto: "Explicando mitos do preenchimento",
                PostsNaoFuncionaram: "Posts muito teóricos",
                ConteudoDosSonhos: "Série educativa em vídeos curtos"
            ),
            Etapa8: new Etapa8Dto(
                ArquetiposComunicacao: new[] { ArquetipoComunicacaoEnum.Professor, ArquetipoComunicacaoEnum.Autoridade },
                AmostraEscritaExplicativa: "Texto calmo, seguro e muito claro.",
                IdentidadeVisualStatus: "Paleta neutra e moderna",
                EsteticaOdiada: "Cores neon e fontes infantis"
            ),
            Etapa9: new Etapa9Dto(
                DiaTipicoRotina: "Atendimentos das 8h às 18h",
                HorasSemanaConteudo: "2 horas por semana",
                ApoioDisponivel: "Secretária para gravar vídeos",
                RankingFacilidadeFormatos: "Vídeos curtos em 1º lugar",
                HistoricoPostagensUltimaSemana: "3 posts semanais"
            ),
            Etapa10: new Etapa10Dto(
                Meta3Meses: "Atrair 15 novos pacientes de alto valor",
                Meta1Ano: "Consolidar autoridade regional",
                ExperienciaPassadaMarketing: "Já contratou agência sem sucesso",
                ResultadoPrioritario: ResultadoPrioritarioEnum.MaisPacientesAgenda
            )
        );
    }
}
