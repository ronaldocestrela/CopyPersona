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

### 4. Testar cadastro e login

1. Suba SQL Server (`docker compose up -d`) — migrations aplicam automaticamente em Development.
2. Acesse `/cadastro`, crie uma conta (nome, e-mail, senha ≥ 8 caracteres, aceite os termos).
3. Após cadastro, você é autenticado via cookie e redirecionado para `/`.
4. Use `/logout` para sair e `/login` para entrar novamente.

Mailpit (http://localhost:8025) ficará disponível para fluxos de e-mail em entregas futuras (reset de senha).

### 5. Connection string (Development)

Configurada em `src/Presentation/PersonaScript.Server/appsettings.Development.json`:

```
Server=localhost,1433;Database=PersonaScript;User Id=sa;Password=PersonaScript_Dev123!;TrustServerCertificate=True
```

Ajuste a senha se alterar `MSSQL_SA_PASSWORD` no `.env`.

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
