# Observabilidade, Logging Estruturado e Alertas Operacionais

> **PersonaScript AI — Subfase 8.2**  
> Diretrizes e especificações técnicas de observabilidade, telemetria, monitoramento de integridade e canais de alerta proativos.

---

## 1. Visão Geral da Arquitetura

O sistema de observabilidade do **PersonaScript AI** foi projetado para ambientes de produção B2C de alta disponibilidade e multi-tenancy, combinando:
1. **Logging Estruturado (Serilog):** Contexto enriquecido por requisição com `TenantId`, `UserId`, `TraceId`, além de mascaramento e sinks para Console JSON e Seq/OTLP.
2. **Health Checks Granulares (ASP.NET Core):** Endpoints `/health/live`, `/health/ready` e `/health` inspecionando todos os 6 `DbContexts` da arquitetura modular.
3. **Alertas Operacionais Proativos (Slack & Microsoft Teams):** Notificação imediata e resiliente para eventos críticos (falhas em Webhooks do Stripe e exaustão/falha de chaves de provedores de LLM).
4. **Telemetria e Métricas (.NET 10 Meters):** Contadores e histogramas para requisições, latências de IA e anomalias de billing.

```
                  ┌────────────────────────────────────────┐
                  │          Requisição HTTP / Webhook     │
                  └───────────────────┬────────────────────┘
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │ TenantLogContextMiddleware│ ──► Injeta TenantId, UserId e TraceId
                        └─────────────┬─────────────┘
                                      │
             ┌────────────────────────┼─────────────────────────┐
             ▼                        ▼                         ▼
   ┌───────────────────┐    ┌───────────────────┐     ┌───────────────────┐
   │  Endpoints Saúde  │    │  Stripe Webhooks  │     │ FallbackLLM (IA)  │
   │ /health{/live,/ready}   │  (Falha Assinatura│     │ (RateLimit / 401) │
   └─────────┬─────────┘    └─────────┬─────────┘     └─────────┬─────────┘
             │                        │                         │
             ▼                        ▼                         ▼
    [HealthReport JSON]      [IOperationalAlertService]  [ILLMFailureNotifier]
                                      │                         │
                                      └────────────┬────────────┘
                                                   │
                                                   ▼
                                     ┌───────────────────────────┐
                                     │SlackTeamsWebhookAlertServ.│
                                     └─────────────┬─────────────┘
                                                   │
                                      ┌────────────┴────────────┐
                                      ▼                         ▼
                                [Slack Webhook]          [Teams Webhook]
```

---

## 2. Logging Estruturado com Serilog

### 2.1 Enriquecimento Multi-Tenant (`TenantLogContextMiddleware`)
O middleware extrai de forma segura o `TenantId` do `ITenantContext` e o identificador do usuário (`ClaimTypes.NameIdentifier`), injetando-os no `Serilog.Context.LogContext`:

```csharp
using (LogContext.PushProperty("TenantId", tenantId))
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("TraceId", traceId))
{
    await _next(context);
}
```

### 2.2 Sinks e Formatos de Log
- **Desenvolvimento (`Development`):** Formato textual colorido com template legível:  
  `[{Timestamp:HH:mm:ss} {Level:u3}] [{TenantId}] {Message:lj}{NewLine}{Exception}`
- **Staging / Produção:** Formato JSON estruturado padrão (`JsonFormatter`), ideal para ingestão por Fluentd, Logstash, Datadog ou CloudWatch.
- **Seq / OpenTelemetry:** Ingestão habilitada via variável de ambiente `SERILOG__WRITETO__SEQ__SERVERURL` ou configuração `Seq:ServerUrl`.

---

## 3. Endpoints de Monitoramento e Health Checks

Os endpoints de integridade são disponibilizados de forma modular:

| Endpoint | Propósito | Comportamento |
| :--- | :--- | :--- |
| `GET /health/live` | **Liveness Probe** | Verifica se o processo web está ativo e respondendo (sem avaliar banco de dados). Retorna HTTP 200 `{ "status": "Healthy" }`. Utilizado por orquestradores para reinicialização de contêineres. |
| `GET /health/ready` | **Readiness Probe** | Avalia se a aplicação está pronta para tráfego checando conectividade com os 6 `DbContexts` (`Identity`, `Billing`, `Anamnese`, `Personas`, `Scripts`, `Backoffice`). |
| `GET /health` | **Full Diagnostics JSON** | Retorna relatório completo com status geral (`Healthy`, `Degraded`, `Unhealthy`), tempo de execução global e detalhado por subsistema. |

### Exemplo de Payload do `/health`:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0125430",
  "timestamp": "2026-09-14T14:48:32.195Z",
  "entries": {
    "IdentityDbContext": { "status": "Healthy", "duration": "00:00:00.002", "description": null },
    "BillingDbContext": { "status": "Healthy", "duration": "00:00:00.001", "description": null },
    "AnamneseDbContext": { "status": "Healthy", "duration": "00:00:00.002", "description": null },
    "PersonasDbContext": { "status": "Healthy", "duration": "00:00:00.001", "description": null },
    "ScriptsDbContext": { "status": "Healthy", "duration": "00:00:00.002", "description": null },
    "BackofficeDbContext": { "status": "Healthy", "duration": "00:00:00.001", "description": null }
  }
}
```

---

## 4. Sistema de Alertas Proativos (Slack & Microsoft Teams)

### 4.1 Contratos (`IOperationalAlertService`)
O serviço oferece três canais de alerta proativo:
- `NotifyPaymentWebhookFailureAsync`: Acionado automaticamente em caso de assinatura Stripe inválida ou falha no processamento de faturas/assinaturas.
- `NotifyLLMQuotaOrFailureAsync`: Acionado quando um provedor de LLM atinge quota (`429`), erro de chave (`401`) ou falha em todos os provedores em cadeia de fallback.
- `NotifySystemCriticalErrorAsync`: Acionado em caso de anomalias inesperadas no núcleo do sistema.

### 4.2 Formatação Slack vs Microsoft Teams
- **Slack:** Mensagens com layout Block Kit, campo de anexo com cor de severidade (`#E01E5A` para crítico, `#ECB22E` para aviso), detalhamento de campos e carimbo de tempo Unix.
- **Microsoft Teams:** MessageCard no padrão Office 365 / Power Automate Webhook com `themeColor`, sumário e tabela de fatos formatada.

### 4.3 Resiliência e Tolerância a Falhas
O envio de alertas opera com timeout de 5 segundos via `IHttpClientFactory` e captura de exceções. Caso o webhook do Slack ou Teams esteja indisponível, o erro é registrado em log sem abortar o fluxo da requisição do cliente nem lançar exceções não tratadas.

---

## 5. Métricas Operacionais (.NET 10 Metrics)

Centralizadas em `PersonaScriptMetrics` (`PersonaScript.Observability`):
- `personscript.llm.requests`: Contador de requisições de IA (tags: `provider`, `model`).
- `personscript.llm.failures`: Contador de falhas/rate limits em LLM (tags: `provider`, `error_code`).
- `personscript.billing.webhook_failures`: Contador de falhas de webhook do Stripe (tags: `event_type`, `error_code`).
- `personscript.llm.duration.ms`: Histograma de tempo de resposta dos provedores de IA.
