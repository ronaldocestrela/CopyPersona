# Diretrizes de CI/CD e Infraestrutura de Produção — PersonaScript AI

> **Status:** Ativo e Validado  
> **Última Atualização:** 2026-09-14  
> **Versão do .NET:** 10.0  
> **Módulos Integrados:** Identity, Anamnese, Billing, Personas, Scripts, Backoffice  

---

## 1. Visão Geral da Arquitetura de Entrega

O ciclo de vida de integração e entrega contínua do **PersonaScript AI** foi projetado para assegurar alta confiabilidade, estabilidade e conformidade estrita com as diretrizes do [AGENTS.md](../AGENTS.md).

```
                         [ Git Push / Pull Request ]
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │    Workflow de CI (.NET)  │
                        │ 1. Linter (dotnet format) │
                        │ 2. Build Release          │
                        │ 3. 431 Testes Automatiz.  │
                        │ 4. Auditoria NuGet        │
                        └─────────────┬─────────────┘
                                      │ (Merge / Tag)
                                      ▼
                        ┌───────────────────────────┐
                        │    Workflow de CD         │
                        │ 1. Docker Buildx (GHCR)   │
                        │ 2. Tag Semântica          │
                        │ 3. Deploy Staging/Prod    │
                        │ 4. EF Core Migrations     │
                        │ 5. Smoke Test /health     │
                        └───────────────────────────┘
```

---

## 2. Workflows do GitHub Actions

### 2.1 Integração Contínua (`ci.yml`)
- **Gatilhos:** Pushes e Pull Requests nas branches `main` e `develop`.
- **Jobs Executados:**
  1. **`lint-and-format`**: Executa `dotnet format --verify-no-changes` validando formatação de código, ordenação de imports e ausência de divergências de estilo em todos os projetos C#.
  2. **`build-and-test`**: Restaura pacotes com cache NuGet (`actions/cache@v4`), compila a solução completa (`PersonaScript.slnx`) em modo Release e roda a suíte com 431 testes (testes unitários, de integração, bUnit e de isolamento multi-tenant). Coleta cobertura em formato XPlat.
  3. **`security-audit`**: Executa `dotnet list package --vulnerable --include-transitive` para alertar antecipadamente sobre falhas conhecidas de segurança em dependências NuGet.

### 2.2 Entrega Contínua (`cd.yml`)
- **Gatilhos:**
  - Push na branch `develop` ➔ Implantação no ambiente de **Staging**.
  - Push de tags de release `v*.*.*` ou despacho manual (`workflow_dispatch`) ➔ Implantação no ambiente de **Produção**.
- **Containerização Multi-Stage:**
  - Constrói o container com base em `mcr.microsoft.com/dotnet/aspnet:10.0` e SDK `.NET 10`.
  - Execução restrita sob o usuário de menor privilégio `USER $APP_UID` (não-root).
  - Publicação segura no GitHub Container Registry (`ghcr.io/ronaldocestrela/personascript-server`).
- **Verificação Pós-Deploy:**
  - Realização de smoke test no endpoint `/health` com política de retry e rollback automático.

---

## 3. Containerização e Docker Compose de Produção

O arquivo [`docker-compose.prod.yml`](../docker-compose.prod.yml) orquestra a stack de produção com isolamento de rede e limites de recursos:

| Serviço | Imagem | Finalidade | Limites de Recursos |
| :--- | :--- | :--- | :--- |
| **`sqlserver`** | `mcr.microsoft.com/mssql/server:2022-latest` | Microsoft SQL Server (Shared DB, Shared Schema) | 2.0 CPUs, 2048 MB RAM |
| **`web`** | `ghcr.io/.../personascript-server` | Monolito Modular .NET 10 (Blazor Server + Web API) | 2.0 CPUs, 1024 MB RAM |
| **`mailpit`** | `axllent/mailpit:latest` | Captura/Inspecção de e-mails em Staging | 0.5 CPUs, 256 MB RAM |

### Política de Logs e Rotação
Todos os serviços configuram driver `json-file` com tamanho máximo de 50MB e retenção de até 5 arquivos, prevenindo o esgotamento do disco do host.

---

## 4. Estratégia de Migrações do EF Core

A aplicação possui 6 DbContexts independentes que mapeiam as tabelas no mesmo banco SQL Server com discriminação por `TenantId`:
1. `IdentityDbContext`
2. `AnamneseDbContext`
3. `BillingDbContext`
4. `PersonasDbContext`
5. `ScriptsDbContext`
6. `BackofficeDbContext`

### Modalidades de Aplicação:
- **Automatizada no Startup (`APPLY_MIGRATIONS=true`):** O container executa `Apply...MigrationsAsync()` no bootstrap do aplicativo antes de abrir o listener HTTP. O `docker-compose.prod.yml` assegura que o container web só inicializa após o SQL Server responder com sucesso ao healthcheck (`SELECT 1`).
- **Scripts Idempotentes Pré-Deploy (`scripts/apply-migrations.sh`):** Em ambientes com governança restrita de banco, o script gera arquivos SQL idempotentes para cada módulo, permitindo revisão ou execução por pipeline de DBA antes da atualização do container web.

---

## 5. Scripts de Automação Operacional

- **[`scripts/deploy.sh`](../scripts/deploy.sh):**
  - Script bash robusto com validação de pré-requisitos, leitura de `.env.production`, acionamento do Docker Compose, loop de health check em `/health` e rollback automático para a versão anterior em caso de falha.
- **[`scripts/apply-migrations.sh`](../scripts/apply-migrations.sh):**
  - Gerador de scripts SQL idempotentes dos 6 DbContexts para auditoria.

---

## 6. Matriz de Secrets Necessários no Repositório

Para a operação plena dos pipelines no GitHub Actions / Produção, devem ser cadastrados os seguintes secrets:

| Secret | Finalidade | Exemplo / Formato |
| :--- | :--- | :--- |
| `MSSQL_SA_PASSWORD` | Senha do SA do SQL Server | Senha forte alfanumérica com caracteres especiais |
| `ConnectionStrings__DefaultConnection` | String de conexão com SQL Server de produção | `Server=...;Database=PersonaScript;User Id=...;Password=...;Encrypt=True;` |
| `Jwt__Secret` | Chave simétrica JWT (mínimo 32 caracteres) | `Chave_Segura_Producao_Minimo_32_Caracteres` |
| `Stripe__SecretKey` | Chave privada Stripe | `sk_live_...` |
| `Stripe__WebhookSecret` | Segredo de assinatura de webhook Stripe | `whsec_...` |
| `Stripe__PublishableKey` | Chave pública Stripe | `pk_live_...` |
| `LLM__PrimaryProvider` | Provedor de IA ativo | `OpenAI`, `Gemini`, `Anthropic` ou `Mock` |
| `LLM__OpenAiApiKey` | Chave API OpenAI | `sk-...` |
| `LLM__GeminiApiKey` | Chave API Google Gemini | `AIzaSy...` |
| `LLM__AnthropicApiKey` | Chave API Anthropic Claude | `sk-ant-...` |
| `Resend__ApiKey` | Chave do provedor de e-mails transacionais | `re_...` |
| `MASTER_ADMIN_EMAIL` | E-mail do usuário Master Administrador do Backoffice | `admin@personascript.ai` |
| `MASTER_ADMIN_PASSWORD` | Senha inicial do usuário Master Administrador | Senha alfanumérica forte (mínimo 8 caracteres) |
| `MASTER_ADMIN_NAME` | Nome de exibição do administrador | `Master Administrator` |
