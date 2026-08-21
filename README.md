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

- App: http://localhost:5000 (ou porta do `launchSettings.json`)
- Health: http://localhost:5000/health
- Cadastro: http://localhost:5000/cadastro
- Login: http://localhost:5000/login
- Anamnese: http://localhost:5000/anamnese
- Posicionamento: http://localhost:5000/posicionamento/diagnostico

### 4. Testar cadastro e login

1. Suba SQL Server (`docker compose up -d`) — migrations aplicam automaticamente em Development.
2. Acesse `/cadastro`, crie uma conta (nome, e-mail, senha ≥ 8 caracteres, aceite os termos).
3. Após cadastro, você é autenticado via cookie e redirecionado para `/`.
4. Use `/logout` para sair e `/login` para entrar novamente.

Mailpit (http://localhost:8025) ficará disponível para fluxos de e-mail em entregas futuras (reset de senha).

Ajuste a senha se alterar `MSSQL_SA_PASSWORD` no `.env`.

### Troubleshooting: erro 500 em `_framework/blazor.web.js`

Se a tela inicial retornar 500 com `FileNotFoundException` em `wwwroot/_framework/blazor.web.js`:

1. **Use o perfil Development** — `dotnet run --project src/Presentation/PersonaScript.Server` (porta em `launchSettings.json`). Evite `dotnet run --no-launch-profile` com `ASPNETCORE_ENVIRONMENT=Production` em build Debug local.
2. **Não copie `.env.production.example` para `.env`** — o `.env` local deve seguir [`.env.example`](.env.example). Variáveis de produção no `.env` podem sobrescrever o ambiente antes do host subir (o projeto usa `NoClobber`, mas o shell pode já exportar `Production`).
3. **Não commite `blazor.web.js` em `wwwroot/_framework/`** — o arquivo é gerado pelo SDK no build/publish; copiar manualmente quebra o modelo .NET 10.

Detalhes em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#static-web-assets--blazor-script-net-10).

### 6. Execução em Produção / Demonstração (Docker Compose Completo)

Para subir todo o ambiente containerizado (Aplicação + SQL Server + Mailpit) para demonstração ou homologação:

```bash
# 1. Copiar variáveis de ambiente de produção
cp .env.production.example .env.production

# 2. Subir o ambiente com Docker Compose de produção
docker compose -f docker-compose.prod.yml up --build -d

# 3. Verificar o status dos containers
docker compose -f docker-compose.prod.yml ps
```

A aplicação estará acessível em:
- **Aplicação Web (.NET 10):** `http://localhost:${APP_PORT:-8080}` (porta configurável via `APP_PORT` no `.env.production`)
- **Healthcheck App:** `http://localhost:${APP_PORT:-8080}/health`
- **Mailpit Web UI:** `http://localhost:8025`

## Estrutura

```
src/
  BuildingBlocks/     # Result, Domain, Tenancy, CQRS
  Modules/            # Identity (auth), Billing, Personas, Scripts
  Presentation/       # Host Blazor (PersonaScript.Server)
tests/
  BuildingBlocks/
  Modules/Identity/
  Presentation/       # bUnit das páginas auth
docs/
  design/stitch/      # Referência visual Cadastro/Login
```

Detalhes em [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Diretrizes

Consulte [AGENTS.md](AGENTS.md) para padrões de arquitetura, TDD, multi-tenancy e UI (Stitch).

## Design de referência

Telas e componentes: [Stitch — PersonaScript AI](https://stitch.withgoogle.com/projects/15459532074568969182)
