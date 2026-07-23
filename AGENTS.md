# AGENTS.md — Contexto, Arquitetura e Diretrizes para Desenvolvedores LLM

> **AVISO IMPORTANTE PARA A LLM:** 
> Você deve seguir estritamente os padrões, padrões de design, convenções e regras de documentação definidos neste documento durante a criação ou modificação de qualquer trecho de código neste sistema.

---

## 1. Visão Geral do Sistema
O **PersonaScript AI** é uma aplicação **SaaS B2C** multiagente focada em automação de marketing e vendas para profissionais autônomos e criadores de conteúdo.

### 1.1 Modelo de Negócio (B2C)
- **Tipo:** SaaS **B2C** (Business-to-Consumer) — o assinante é o próprio consumidor final, não uma empresa ou equipe corporativa.
- **Aquisição:** **Self-service** — cadastro, onboarding, assinatura e cancelamento sem intervenção comercial ou vendas consultivas.
- **Unidade de assinatura:** 1 conta = 1 tenant lógico = 1 usuário (relação 1:1 entre `UserId` e `TenantId`).
- **Modelo comercial:** planos de assinatura recorrente com limites por plano (ex.: quantidade de personas, roteiros gerados por mês).

- **Agente 1 (Estrategista de Persona):** Recebe dados do usuário (Nome, Profissão, Nicho) e gera o perfil do cliente ideal em formato JSON estruturado.
- **Agente 2 (Copywriter de Vídeo):** Recebe o perfil da persona gerada pelo Agente 1 e produz roteiros otimizados de vídeo (Gancho, Retenção, CTA) para redes sociais.

---

## 2. Stack Tecnológico e Princípios Arquiteturais

### 2.1 Banco de Dados
- **SGBD:** Microsoft SQL Server.
- **Mapeamento:** Entity Framework Core (EF Core) com suporte a Migrations e escopo isolado por módulo.

### 2.2 Backend (.NET 10)
- **Framework:** .NET 10 (C#).
- **Estilo Arquitetural:** **Monolito Modular** (Módulos independentes com limites bem definidos no mesmo processo de execução).
- **Padrão CQRS:** Separação estrita de Commands (escrita/alteração) e Queries (leitura/consultas).
- **Padrão Repository:** Abstração completa do acesso a dados por repositórios tipados dentro do domínio do módulo.
- **Tratamento de Fluxo:** **Padrão Result** (ex: `Result<T>` / `Result`). Proibido utilizar exceções (`throw new Exception()`) para controle de fluxo de negócios.

### 2.3 Frontend (Blazor)
- **Framework:** Blazor (WebAssembly / InteractiveServer segundo o módulo).
- **Padrão de UI:** **Componentização** extrema e reuso de componentes de apresentação/layout.
- **Design & UI Protocol:** **MCP Stitch** (integração e componentes reutilizáveis guiados pelo protocolo MCP).
- **Projeto de referência Stitch:** [PersonaScript AI — Design Reference](https://stitch.withgoogle.com/projects/15459532074568969182) — fonte canônica de telas, componentes, tokens visuais e fluxos de UI; toda implementação Blazor deve se alinhar a este projeto salvo decisão documentada em contrário.

### 2.4 Multi-tenancy e Isolamento Lógico
A aplicação adota **isolamento lógico** (shared database, shared schema) — todos os tenants compartilham o mesmo banco e schema, com separação de dados por coluna discriminadora.

- **Estratégia:** banco compartilhado + schema compartilhado + discriminação por `TenantId` (coluna em todas as entidades de negócio).
- **Granularidade do tenant:** em B2C, `TenantId` equivale ao `UserId` do assinante autenticado (1 usuário = 1 tenant).
- **O que NÃO será usado:** database por tenant, schema por tenant ou instância isolada por cliente (isolamento físico).
- **Contexto de tenant:** resolvido via `ITenantContext`, injetado a partir do token/cookie de autenticação — nunca confiando em `TenantId` enviado pelo cliente na requisição.
- **Filtros globais:** EF Core Global Query Filters aplicados automaticamente em todas as entidades que implementam `IMustHaveTenant`.
- **Identity:** o módulo `Identity` emite claims/JWT contendo `TenantId` (ou `UserId` usado como tenant) para consumo pelos demais módulos.

---

## 3. Metodologia de Desenvolvimento (Obrigatório)

### 3.1 Test-Driven Development (TDD)
Sempre que for solicitar ou gerar novo código, a abordagem TDD deve ser seguida estritamente nos dois ecossistemas (Backend e Frontend):
1. **Red:** Escreva primeiramente o teste de unidade/integração que falhe.
2. **Green:** Implemente o código mínimo necessário para fazer o teste passar.
3. **Refactor:** Melhore o código mantendo todos os testes verdes e sem regressão.

### 3.2 Documentação Viva e Obrigatória
- **Regra de Ouro:** Toda nova funcionalidade, alteração de comportamento, novo Command/Query, novo componente de UI ou refatoração deve ser **obrigatoriamente documentada imediatamente**.
- Atualize os arquivos de especificação de API, diagramas ou o próprio documento de arquitetura viva do projeto sempre que alterar contratos.
- Documentação estrutural da fundação: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

---

## 4. Estrutura do Monolito Modular (.NET 10)

```
src/
├── BuildingBlocks/
│   ├── Domain/                 # BaseEntity, ValueObject, IAggregateRoot, IMustHaveTenant
│   ├── Tenancy/                # ITenantContext, TenantId, filtros globais EF Core
│   ├── Results/                # Implementação do Padrão Result (Error, Result<T>)
│   └── CQRS/                   # Interfaces ICommand, IQuery, ICommandHandler, IQueryHandler
│
├── Modules/
│   ├── Identity/               # Autenticação, usuários e emissão de claims de tenant
│   ├── Billing/                # Planos, assinaturas e limites de uso (B2C self-service)
│   ├── Personas/               # Módulo do Agente 1 (Estrategista)
│   └── Scripts/                # Módulo do Agente 2 (Copywriter)
│       ├── Domain/             # Entidades, Value Objects, Repositories Interfaces
│       ├── Application/        # Commands, Queries, Handlers, DTOs
│       ├── Infrastructure/     # EF Core DbContext, Repositories Implementations
│       └── ModuleSetup.cs      # Injeção de dependência e inicialização do módulo
│
└── Presentation/
    └── Server/                 # App Host / Web API Host .NET 10
```

---

## 5. Diretrizes de Código e Convenções

### 5.1 Implementação do Padrão Result no Backend
Nunca lance exceções para erros de regra de negócio ou dados inválidos. Retorne instâncias de `Result` ou `Result<T>`.

```csharp
// Exemplo de Handler CQRS utilizando Result e isolamento por tenant
public async Task<Result<Guid>> Handle(CreatePersonaCommand command, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(command.Nicho))
    {
        return Result.Failure<Guid>(DomainErrors.Persona.NichoInvalido);
    }

    var persona = Persona.Create(
        _tenantContext.TenantId,
        command.Nome,
        command.Profissao,
        command.Nicho);

    await _personaRepository.AddAsync(persona, cancellationToken);

    return Result.Success(persona.Id);
}
```

### 5.2 Estrutura do Repositório e CQRS
- **Commands:** Alteram estado. Devem retornar `Result` ou `Result<T>`.
- **Queries:** Somente leitura. Devem consultar visões otimizadas ou repositórios de leitura sem rastreamento de mudança (*AsNoTracking*).

### 5.3 Isolamento por Tenant (Obrigatório)
- **Entidades de negócio:** toda entidade persistida deve implementar `IMustHaveTenant` e possuir `TenantId` preenchido na criação.
- **Commands/Queries:** **nunca** aceitam `TenantId` como parâmetro vindo do cliente — obtêm o valor exclusivamente de `ITenantContext`.
- **Repositórios:** aplicam filtro global de tenant via EF Core; proibido consultar ou alterar registros de outro tenant.
- **Cross-tenant leak:** retornar `Result.Failure` com erro de autorização quando um recurso não pertence ao tenant autenticado — nunca expor dados de terceiros.
- **Testes:** todo teste de integração de módulo com dados persistidos deve incluir cenário que comprova que o tenant A não acessa dados do tenant B.

---

## 6. Diretrizes do Frontend (Blazor + MCP Stitch)

- **Projeto Stitch de referência:** https://stitch.withgoogle.com/projects/15459532074568969182 — consulte via MCP Stitch antes de criar ou alterar telas; reproduza layout, hierarquia visual, estados (loading, erro, vazio) e componentes definidos neste projeto.
- **Componentização:** Divida páginas complexas em componentes Blazor isolados e fortemente tipados.
- **TDD em Blazor:** Utilize bUnit para criação de testes automatizados de componentes antes da implementação da lógica de interface.
- **MCP Stitch Integration:** Siga o padrão do protocolo MCP Stitch para vinculação de dados (data binding) e consumo dos endpoints de backend.

---

## 7. Checklist da LLM para Toda Resposta / Alteração de Código

Toda resposta gerada para este projeto DEVE validar os seguintes pontos:

- [ ] Escreveu os testes (TDD) antes ou junto com a implementação da feature?
- [ ] O backend utiliza o padrão `Result<T>` sem disparar `throw` para cenários de negócio?
- [ ] A arquitetura respeita o isolamento de módulos e o padrão CQRS?
- [ ] A entidade/query/command respeita o `TenantId` obtido de `ITenantContext` (nunca do cliente)?
- [ ] O repositório aplica filtro global de tenant e impede vazamento cross-tenant?
- [ ] Existe teste que comprova que o tenant A não acessa dados do tenant B?
- [ ] O repositório abstrai o acesso ao SQL Server via Entity Framework Core?
- [ ] O componente Blazor está modularizado e testado via bUnit?
- [ ] A UI está alinhada ao [projeto Stitch de referência](https://stitch.withgoogle.com/projects/15459532074568969182)?
- [ ] A **Documentação Viva** do repositório/código foi atualizada refletindo a nova adição/modificação?
