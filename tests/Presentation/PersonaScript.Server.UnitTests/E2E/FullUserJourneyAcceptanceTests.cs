using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Billing.Application.Commands.InitializeTenantSubscription;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Application.Queries.GetTenantQuotaUsage;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;
using PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;
using PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;
using PersonaScript.Server.UnitTests.Auth;
using Xunit;

namespace PersonaScript.Server.UnitTests.E2E;

public class FullUserJourneyAcceptanceTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public FullUserJourneyAcceptanceTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static void SetScopeTenant(IServiceScope scope, Guid userId, Guid tenantId, string email)
    {
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, "Subscriber")
        };
        var identity = new ClaimsIdentity(claims, "TestCookieAuth");
        accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }

    [Fact]
    public async Task FullUserJourney_FromRegistrationToScriptAndQuota_ShouldExecuteSuccessfullyWithStrictTenantIsolation()
    {
        using var scope = _factory.Services.CreateScope();

        // -----------------------------------------------------------------------------------------
        // 1. CADASTRO DO USUÁRIO A (Tenant A)
        // -----------------------------------------------------------------------------------------
        var emailA = $"dra.camila.{Guid.NewGuid():N}@clinica.com";
        var registerHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<RegisterUserCommand, LoginResult>>();

        var regResult = await registerHandler.Handle(
            new RegisterUserCommand("Dra. Camila Ramos", emailA, "SenhaForte123!", true),
            CancellationToken.None);

        regResult.IsSuccess.Should().BeTrue();
        var userIdA = regResult.Value.UserId;
        var tenantIdA = userIdA;

        // Estabelece contexto de tenant para o Tenant A
        SetScopeTenant(scope, userIdA, tenantIdA, emailA);
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.TenantId.Value.Should().Be(tenantIdA);

        // -----------------------------------------------------------------------------------------
        // 2. INICIALIZAÇÃO DE ASSINATURA E QUOTAS (Plano Pro)
        // -----------------------------------------------------------------------------------------
        var initSubHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<InitializeTenantSubscriptionCommand, Guid>>();
        var initSubResult = await initSubHandler.Handle(new InitializeTenantSubscriptionCommand(PlanType.Pro), CancellationToken.None);
        initSubResult.IsSuccess.Should().BeTrue();

        var quotaUsageHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetTenantQuotaUsageQuery, TenantQuotaUsageDto>>();
        var initialQuota = await quotaUsageHandler.Handle(new GetTenantQuotaUsageQuery(), CancellationToken.None);
        initialQuota.IsSuccess.Should().BeTrue();
        initialQuota.Value.ScriptsLimit.Should().Be(30);
        initialQuota.Value.ScriptsGeneratedCount.Should().Be(0);

        // -----------------------------------------------------------------------------------------
        // 3. ANAMNESE — JORNADA COMPLETA DE 10 ETAPAS
        // -----------------------------------------------------------------------------------------
        var startAnamneseHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<StartAnamneseCommand, Guid>>();
        var startResult = await startAnamneseHandler.Handle(new StartAnamneseCommand(), CancellationToken.None);
        startResult.IsSuccess.Should().BeTrue();

        var saveStepHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SaveAnamneseStepCommand>>();

        // Salvar Etapas 1 até 10 com dados válidos
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dra. Camila Ramos", "Dermatologia", "CRM/SP 987654", 10, "Rejuvenescimento e Estética Facial", "São Paulo - SP", 20, MomentoAtualEnum.AgendaCheiaCobrarMais)), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(2, Etapa2: new Etapa2Dto("Mulheres 35-60 anos", "Flacidez facial e perda de contorno", "Aparência jovem e natural sem exageros", "Medo de deformidades ou resultados artificiais")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(3, Etapa3: new Etapa3Dto("Protocolo Rejuvenescimento 3D", "Consulta diagnóstica com ultrassom e plano preventivo", "Atendimento humanizado e naturalidade", "Bioestimuladores de colágeno e ultrassom microfocado", "Resultados comprovados e acompanhamento próximo", "Mais de 1.500 pacientes satisfeitas")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(4, Etapa4: new Etapa4Dto("Intermediário", "Tempo restrito e receio de normas do conselho", "Exageros sensacionalistas", "Dermatologistas internacionais de alto padrão", "Não divulgar preços abertamente", CanalOrigemEnum.Instagram)), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(5, Etapa5: new Etapa5Dto(new[] { "Bioestimuladores", "Ultrassom Microfocado", "Botox" }, "Tratamentos de colágeno preventivo", "Bioestimulação com planejamento individual", new[] { "Bioestimulador dói", "Resultado instantâneo" }, "O rejuvenescimento natural é um processo progressivo")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(6, Etapa6: new Etapa6Dto("Elegante, acolhedor e técnico", "Vídeos curtos e explicativos de procedimentos", "Tons neutros, dourado e iluminação suave", "Jaleco impecável e alfaiataria", NivelConfortoCameraEnum.SuperAVontade, "Falo olhando nos olhos com segurança")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(7, Etapa7: new Etapa7Dto("Medo de ficar artificial", "Achar que autocuidado é futilidade", "Não ter tempo para repouso pós-procedimento", "Técnicas modernas sem downtime", "O autocuidado restaura a autoconfiança", "Segurança com profissional médico qualificado")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(8, Etapa8: new Etapa8Dto(new List<ArquetipoComunicacaoEnum> { ArquetipoComunicacaoEnum.Professor, ArquetipoComunicacaoEnum.Autoridade }, "Autoridade médica com acolhimento", "Explicar os fundamentos científicos de cada tratamento", "Aproximação com empatia")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(9, Etapa9: new Etapa9Dto("Fotos não autorizadas", "Valores e promoções de procedimentos", "Comparações depreciativas", "Conformidade irrestrita ao CFM 2.336/2023", "Preservação da dignidade e privacidade")), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await saveStepHandler.Handle(new SaveAnamneseStepCommand(10, Etapa10: new Etapa10Dto("Faturamento de R$ 120k/mês", "Atrair pacientes de alto poder aquisitivo", "Consolidação como autoridade regional em rejuvenescimento", ResultadoPrioritarioEnum.PacientesMelhoresTicketAlto)), CancellationToken.None)).IsSuccess.Should().BeTrue();

        // Concluir Anamnese
        var completeAnamneseHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CompleteAnamneseCommand>>();
        var completeResult = await completeAnamneseHandler.Handle(new CompleteAnamneseCommand(), CancellationToken.None);
        completeResult.IsSuccess.Should().BeTrue();

        // Verificar status da Anamnese
        var anamneseStatusHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        var statusResult = await anamneseStatusHandler.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);
        statusResult.IsSuccess.Should().BeTrue();
        statusResult.Value.Status.Should().Be(AnamneseStatus.Concluido);
        statusResult.Value.PercentualConclusao.Should().Be(100);

        // -----------------------------------------------------------------------------------------
        // 4. GERAÇÃO DE DIAGNÓSTICO DE POSICIONAMENTO
        // -----------------------------------------------------------------------------------------
        var generateDiagnosisHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<GeneratePersonaDiagnosisCommand, Guid>>();
        var diagResult = await generateDiagnosisHandler.Handle(new GeneratePersonaDiagnosisCommand(), CancellationToken.None);
        diagResult.IsSuccess.Should().BeTrue();

        var getDiagnosisHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetPersonaDiagnosisQuery, PersonaDiagnosisDto?>>();
        var diagQueryResult = await getDiagnosisHandler.Handle(new GetPersonaDiagnosisQuery(), CancellationToken.None);
        diagQueryResult.IsSuccess.Should().BeTrue();
        diagQueryResult.Value.Should().NotBeNull();
        var diagnosis = diagQueryResult.Value!;

        diagnosis.TenantId.Should().Be(tenantIdA);
        diagnosis.FrasePosicionamento.Should().NotBeNullOrWhiteSpace();
        diagnosis.PilaresConteudo.Should().NotBeEmpty();
        diagnosis.PilaresConteudo.Sum(p => p.Percentual).Should().Be(100);

        // -----------------------------------------------------------------------------------------
        // 5. GERAÇÃO DE ROTEIRO DE VÍDEO E CONSUMO DE QUOTA
        // -----------------------------------------------------------------------------------------
        var generateScriptHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<GenerateVideoScriptCommand, Guid>>();
        var scriptCmd = new GenerateVideoScriptCommand(
            Tema: "Rejuvenescimento Natural com Bioestimuladores",
            PilarConteudo: diagnosis.PilaresConteudo.First().Nome,
            Objetivo: "Conscientizar sobre prevenção do envelhecimento precoce",
            TomDesejado: diagnosis.IdentidadeMarca.TomDeVoz,
            InstrucoesAdicionais: "Focar em segurança e sem downtime");

        var scriptResult = await generateScriptHandler.Handle(scriptCmd, CancellationToken.None);
        scriptResult.IsSuccess.Should().BeTrue();
        var scriptId = scriptResult.Value;

        // Verificar Roteiro gerado
        var getScriptHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetVideoScriptByIdQuery, VideoScriptDto>>();
        var scriptDtoResult = await getScriptHandler.Handle(new GetVideoScriptByIdQuery(scriptId), CancellationToken.None);
        scriptDtoResult.IsSuccess.Should().BeTrue();
        scriptDtoResult.Value.Should().NotBeNull();
        var scriptDto = scriptDtoResult.Value!;

        scriptDto.TenantId.Should().Be(tenantIdA);
        scriptDto.Gancho.Should().NotBeNullOrWhiteSpace();
        scriptDto.Retencao.Should().NotBeNullOrWhiteSpace();
        scriptDto.ChamadaParaAcao.Should().NotBeNullOrWhiteSpace();

        // Consumir quota de geração de roteiro
        var consumeQuotaHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota.ConsumeQuotaCommand, Guid>>();
        var consumeResult = await consumeQuotaHandler.Handle(
            new PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota.ConsumeQuotaCommand(QuotaResourceType.ScriptGeneration, 1, nameof(GenerateVideoScriptCommand)),
            CancellationToken.None);
        consumeResult.IsSuccess.Should().BeTrue();

        // Verificar consumo da quota
        var updatedQuota = await quotaUsageHandler.Handle(new GetTenantQuotaUsageQuery(), CancellationToken.None);
        updatedQuota.IsSuccess.Should().BeTrue();
        updatedQuota.Value.ScriptsGeneratedCount.Should().Be(1);

        // -----------------------------------------------------------------------------------------
        // 6. GARANTIA DE ISOLAMENTO MULTI-TENANT (ANTI CROSS-TENANT LEAK)
        // -----------------------------------------------------------------------------------------
        var emailB = $"dr.fernando.{Guid.NewGuid():N}@ortopedia.com";
        var regBResult = await registerHandler.Handle(
            new RegisterUserCommand("Dr. Fernando Souza", emailB, "SenhaForte123!", true),
            CancellationToken.None);
        regBResult.IsSuccess.Should().BeTrue();
        var userIdB = regBResult.Value.UserId;
        var tenantIdB = userIdB;

        // Alterna o contexto de execução para o Tenant B
        SetScopeTenant(scope, userIdB, tenantIdB, emailB);
        tenantContext.TenantId.Value.Should().Be(tenantIdB);

        // Tenant B NÃO deve ver a anamnese do Tenant A
        var anamneseStatusTenantB = await anamneseStatusHandler.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);
        anamneseStatusTenantB.IsFailure.Should().BeTrue();

        // Tenant B NÃO deve ver o diagnóstico do Tenant A
        var diagTenantB = await getDiagnosisHandler.Handle(new GetPersonaDiagnosisQuery(), CancellationToken.None);
        diagTenantB.Value.Should().BeNull();

        // Tenant B NÃO deve listar os roteiros do Tenant A
        var listScriptsHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>>>();
        var roteirosTenantB = await listScriptsHandler.Handle(new ListVideoScriptsQuery(), CancellationToken.None);
        roteirosTenantB.IsSuccess.Should().BeTrue();
        roteirosTenantB.Value.Should().BeEmpty();

        // Tenant B NÃO deve conseguir acessar o roteiro específico do Tenant A por ID
        var scriptCrossLeakResult = await getScriptHandler.Handle(new GetVideoScriptByIdQuery(scriptId), CancellationToken.None);
        scriptCrossLeakResult.IsFailure.Should().BeTrue();
    }
}
