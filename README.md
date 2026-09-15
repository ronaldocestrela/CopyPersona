# PersonaScript AI

SaaS B2C multiagente para automação de marketing e vendas. Monolito modular .NET 10 + Blazor.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) e Docker Compose

## Início rápido

### 1. Dependências locais (Docker)

```bash
cp .env.example .env
docker compose up -d
docker compose ps
```

Serviços:

| Serviço    | Porta | Uso                          |
|------------|-------|------------------------------|
| SQL Server | 1433  | Banco principal              |
| Mailpit    | 1025  | SMTP local (Identity/B2C)    |
| Mailpit UI | 8025  | http://localhost:8025        |

### 2. Aplicação

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Presentation/PersonaScript.Server
```

Endpoints:

- App: http://localhost:5000 (ou porta do `launchSettings.json` / http://localhost:8080 em Docker)
- Health: http://localhost:5000/health
- Cadastro: http://localhost:5000/cadastro
- Login: http://localhost:5000/login
- Anamnese: http://localhost:5000/anamnese
- Posicionamento: http://localhost:5000/posicionamento/diagnostico
- Backoffice Operacional: http://localhost:5000/admin (Requer papel de administrador)

### 4. Testar cadastro e login

1. Suba SQL Server (`docker compose up -d`) — migrations aplicam automaticamente em Development.
2. Acesse `/cadastro`, crie uma conta (nome, e-mail, senha ≥ 8 caracteres, aceite os termos).
3. Após cadastro, você é autenticado via cookie e redirecionado para `/`.
4. Use `/logout` para sair e `/login` para entrar novamente.

Mailpit (http://localhost:8025) ficará disponível para fluxos de e-mail em entregas futuras (reset de senha).

Ajuste a senha se alterar `MSSQL_SA_PASSWORD` no `.env`.

### 5. Acesso ao Backoffice (Usuário Master)

O sistema conta com um **seeder idempotente automático** que cria/promove o usuário Master Administrador com papel `SystemAdmin` durante a inicialização (quando `APPLY_MIGRATIONS=true` ou em Development).

As credenciais padrão são carregadas a partir do `.env` (ou `.env.production`):

```env
MASTER_ADMIN_EMAIL=admin@personascript.ai
MASTER_ADMIN_PASSWORD=AdminPersonaScript2026!
MASTER_ADMIN_NAME=Master Administrator
```

Para acessar o painel:
1. Navegue para `http://localhost:5000/admin` (ou `http://localhost:8080/admin` se estiver rodando via Docker).
2. O sistema redirecionará para a tela de login.
3. Insira as credenciais do administrador configuradas no `.env`.
4. O redirecionamento concederá acesso direto ao Dashboard Operacional, Gestão de Tenants, Prompts de IA, Telemetria e Governança Ética.

### Troubleshooting: erro 500 em `_framework/blazor.web.js`

Se a tela inicial retornar 500 com `FileNotFoundException` em `wwwroot/_framework/blazor.web.js`:

1. **Use o perfil Development** — `dotnet run --project src/Presentation/PersonaScript.Server` (porta em `launchSettings.json`). Evite `dotnet run --no-launch-profile` com `ASPNETCORE_ENVIRONMENT=Production` em build Debug local.
2. **Não copie `.env.production.example` para `.env`** — o `.env` local deve seguir [`.env.example`](.env.example). Variáveis de produção no `.env` podem sobrescrever o ambiente antes do host subir (o projeto usa `NoClobber`, mas o shell pode já exportar `Production`).
3. **Não commite `blazor.web.js` em `wwwroot/_framework/`** — o arquivo é gerado pelo SDK no build/publish; copiar manualmente quebra o modelo .NET 10.

Detalhes em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#static-web-assets--blazor-script-net-10).

### 6. Execução e Deploy em Produção / Homologação

O **PersonaScript AI** possui esteira completa de deploy automatizado via **GitHub Actions** e scripts operacionais para ambientes containerizados Linux/Docker.

#### Opção A: Deploy Automatizado via CI/CD (GitHub Actions)
A esteira foi configurada em [`.github/workflows/`](.github/workflows/):
- **Staging (Homologação):** Acionado automaticamente a cada `push` na branch `develop`.
- **Produção:** Acionado automaticamente ao publicar uma tag de versão (`git tag v1.0.0 && git push origin v1.0.0`) ou manualmente via `Actions -> PersonaScript AI - CD Pipeline -> Run workflow` selecionando o ambiente `production`.

> [!NOTE]
> Certifique-se de configurar os Secrets do repositório no GitHub (`MSSQL_SA_PASSWORD`, `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, `Stripe__SecretKey`, chaves de LLM, etc.). Consulte a matriz completa em [docs/CI_CD_INFRASTRUCTURE.md](docs/CI_CD_INFRASTRUCTURE.md#6-matriz-de-secrets-necessários-no-repositório).

---

#### Opção B: Deploy via Script Operacional (`scripts/deploy.sh`)
Para servidores dedicados, VPS ou VMs na nuvem com Docker instalado, utilize o script operacional de deploy com **healthcheck ativo** e **rollback automático**:

```bash
# 1. Configurar variáveis de produção
cp .env.production.example .env.production
nano .env.production  # Ajuste senhas, chaves de API e connection strings

# 2. Executar deploy para Produção (com verificação e rollback automático)
./scripts/deploy.sh production latest

# Ou para Staging:
./scripts/deploy.sh staging latest
```

O script executará:
1. Carregamento seguro das variáveis de `.env.production`.
2. Snapshot do container em execução para contingência.
3. Subida/atualização ordenada via `docker-compose.prod.yml`.
4. Loop de checagem ativa no endpoint `/health` (até 20 tentativas).
5. **Rollback automático** imediato para a versão anterior caso a aplicação não responda com HTTP 200.

---

#### Opção C: Deploy Manual via Docker Compose

```bash
# 1. Configurar variáveis de produção
cp .env.production.example .env.production

# 2. Subir os containers em background com build multi-stage .NET 10
docker compose -f docker-compose.prod.yml up --build -d

# 3. Monitorar subida e logs com rotação ativa
docker compose -f docker-compose.prod.yml logs -f web

# 4. Validar status dos containers
docker compose -f docker-compose.prod.yml ps
```

---

#### 7. Migrações do Banco de Dados (EF Core Migrations)

A aplicação conta com 6 DbContexts modulares com isolamento por `TenantId`:
- **Modo Automático (Padrão):** Com `APPLY_MIGRATIONS=true` no container, as migrações de todos os módulos são aplicadas de forma assíncrona na inicialização do servidor assim que o SQL Server estiver saudável.
- **Modo Auditado / Script SQL Idempotente:** Caso a política de governança exija validação prévia de DBA:
  ```bash
  ./scripts/apply-migrations.sh
  ```
  Gera scripts `.sql` idempotentes individuais em `./migrations-sql/` para cada contexto (`Identity`, `Anamnese`, `Billing`, `Personas`, `Scripts`, `Backoffice`).

---

#### 8. Endpoints de Verificação e Monitoramento Pós-Deploy

| Endpoint | Tipo | Finalidade |
| :--- | :--- | :--- |
| `http://localhost:${APP_PORT:-8080}/health` | JSON Diagnóstico | Relatório detalhado de integridade geral do sistema |
| `http://localhost:${APP_PORT:-8080}/health/live` | Liveness Probe | Probe leve para orquestradores (Docker / K8s / Azure) |
| `http://localhost:${APP_PORT:-8080}/health/ready` | Readiness Probe | Validação ativa de conectividade com os 6 DbContexts SQL |

Documentação completa de infraestrutura: [docs/CI_CD_INFRASTRUCTURE.md](docs/CI_CD_INFRASTRUCTURE.md).  
Documentação de observabilidade e alertas: [docs/OBSERVABILITY_AND_ALERTS.md](docs/OBSERVABILITY_AND_ALERTS.md).

## Estrutura da Solução

```
src/
  BuildingBlocks/     # Domain, Results, CQRS, Tenancy, AI (resiliência LLM e sanitização)
  Modules/
    Identity/         # Autenticação B2C, Cookies, RBAC e tenant claim
    Anamnese/         # Formulário digital 10 etapas, JSON columns e IA follow-up
    Personas/         # Agente 1 (Estrategista): Diagnóstico e pilares de marca
    Scripts/          # Agente 2 (Copywriter): Roteiros de vídeo e teleprompter
    Billing/          # Stripe Checkout, quotas de uso e renovação mensal
    Backoffice/       # Painel administrativo, impersonação, editor de prompts e telemetria
  Presentation/       # Host Blazor Interativo + Web API (.NET 10)
tests/
  BuildingBlocks/     # Testes unitários do core e IA
  Modules/            # Testes unitários e de isolamento multi-tenant por módulo
  Presentation/       # Testes bUnit, integração E2E e segurança OWASP/RateLimit
scripts/
  deploy.sh           # Script de deploy automatizado com healthcheck e rollback
  apply-migrations.sh # Gerador de migrações SQL idempotentes
docs/
  ARCHITECTURE.md     # Arquitetura viva do projeto
  CI_CD_INFRASTRUCTURE.md # Esteira de CI/CD e infraestrutura de produção
  OBSERVABILITY_AND_ALERTS.md # Logging Serilog, health checks e alertas
```

## Diretrizes

Consulte [AGENTS.md](AGENTS.md) para padrões de arquitetura, TDD, multi-tenancy e UI (Stitch).

## Design de referência

Telas e componentes: [Stitch — PersonaScript AI](https://stitch.withgoogle.com/projects/15459532074568969182)

