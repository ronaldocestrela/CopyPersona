# ROADMAP DE IMPLEMENTAÇÃO — PersonaScript AI

Este documento estabelece o roteiro detalhado, estruturado e sequencial para o desenvolvimento e implantação completa do **PersonaScript AI**, englobando a arquitetura SaaS B2C Multi-tenant (.NET 10 + SQL Server + Blazor), o motor de **Anamnese do Posicionamento Digital em 10 etapas**, o **Agente 1 (Estrategista de Persona)**, o **Agente 2 (Copywriter de Vídeo)**, a gestão de **Billing & Assinaturas**, e o **Módulo de Backoffice Administrativo**.

---

## VISÃO GERAL E ALINHAMENTO ARQUITETURAL

O desenvolvimento segue estritamente os princípios definidos em [`AGENTS.md`](file:///home/rony/LPR/IAdeConteudo/AGENTS.md) e [`docs/ARCHITECTURE.md`](file:///home/rony/LPR/IAdeConteudo/docs/ARCHITECTURE.md):
- **Arquitetura:** Monolito Modular em .NET 10 com isolamento de dados lógico por `TenantId` (`TenantId` == `UserId`).
- **Padrões:** Test-Driven Development (TDD mandatory), CQRS, Repository Pattern, Padrão `Result<T>` (sem lançamento de exceções para fluxo de negócio), Global Query Filters no EF Core.
- **Frontend:** Blazor InteractiveServer / WebAssembly guiado pelo design de referência no MCP Stitch.
- **Backoffice:** Painel administrativo isolado para operação, suporte, gestão financeira, versionamento de prompts e observabilidade de LLM.

---

## FASE 1: Fundação Arquitetural, Identidade e Segurança Base

### Subfase 1.1: Consolidação do BuildingBlocks e Tenancy B2C [CONCLUÍDO]
- **Tarefas:**
  - [x] Validar e estender as abstrações em `PersonaScript.BuildingBlocks.Domain` (`BaseEntity`, `ValueObject`, `IMustHaveTenant`, `IAggregateRoot`, `IDomainEvent`).
  - [x] Reforçar em `PersonaScript.BuildingBlocks.Tenancy` o resolvedor `HttpContextTenantContext` garantindo que `TenantId` seja obtido exclusivamente via claims HTTP (`tenant_id`, `NameIdentifier`, `sub`).
  - [x] Implementar suporte completo ao `TenantDbContextInterceptor` para atribuição automática do `TenantId` em inserções EF Core e bloqueio de mutações cross-tenant.
  - [x] Escrever testes unitários e de integração validando a recusa e atribuição correta do `TenantId`.
- **Entregáveis da Subfase 1.1:**
  - [x] `BuildingBlocks.Tenancy` 100% testado com cobertura de 95%+.
  - [x] Interceptor EF Core configurado e injetado no container de DI.

### Subfase 1.2: Expansão do Módulo Identity (Recuperação de Senha & E-mails)
- **Tarefas:**
  - Criar `RequestPasswordResetCommand` e `ResetPasswordCommand` no módulo `Identity`.
  - Integrar com o Mailpit local para envio de e-mails transacionais (boas-vindas e reset de senha) usando `IEmailSender`.
  - Desenvolver as páginas Blazor SSR `/esqueci-senha` e `/redefinir-senha` alinhadas ao design Stitch.
  - Escrever testes bUnit para formulários SSR e testes de integração do envio de e-mail via Mailpit.
- **Entregáveis da Subfase 1.2:**
  - Fluxo completo de recuperação de senha operante em ambiente de dev.
  - Testes de integração de e-mail e comandos CQRS passando sem falhas.

### Subfase 1.3: Autenticação OAuth2 (Google & Apple) e Suporte a JWT
- **Tarefas:**
  - Configurar autenticação OAuth2 social (Google e Apple Identity Providers) nos endpoints `POST /account/external-login`.
  - Implementar emissão de JWT Bearer Token para futuras integrações de API / Mobile no `Identity.Application`.
  - Atualizar telas de `/login` e `/cadastro` ativando visualmente os botões sociais (Stitch UI).
  - Testes unitários para mapeamento de claims externas e provisionamento automático de novo Tenant/User no registro social.
- **Entregáveis da Subfase 1.3:**
  - Login via Google/Apple funcional em dev.
  - Emissor e validador de JWT configurados com testes de contrato.

### Subfase 1.4: Sistema de Roles (RBAC) e Infraestrutura do Backoffice Operacional
- **Tarefas:**
  - Estender o modelo de `User` para suportar papéis (`UserRole`: `Subscriber`, `SupportAgent`, `FinanceAdmin`, `SystemAdmin`).
  - Adicionar claim `role` ao cookie de autenticação e aos tokens JWT.
  - Criar autorização baseada em Roles/Policies no Blazor e na camada de API.
  - Testar isolamento de acesso às rotas administrativas por perfil.
- **Entregáveis da Subfase 1.4:**
  - Atribuição e verificação de roles testada no backend e frontend.
  - Policies `RequireSystemAdmin` e `RequireSupportAgent` registradas no container de DI.

#### Resultado Esperado da FASE 1:
Infraestrutura base de segurança, multi-tenancy B2C e autenticação totalmente funcional, testada via TDD, com e-mails funcionais via Mailpit, login social pronto e RBAC estruturado para suportar assinantes e administradores.

---

## FASE 2: Módulo de Anamnese Digital (Engine de Coleta em 10 Etapas)

### Subfase 2.1: Modelagem de Domínio e Persistência do Formulário
- **Tarefas:**
  - Criar o módulo `PersonaScript.Modules.Anamnese` (ou integrar em `Personas.Domain`).
  - Criar a entidade raiz `Anamnese` (AggregateRoot) implementando `IMustHaveTenant`, contendo Value Objects para cada uma das 10 etapas conforme especificado em [`AnamnesePosicionamento.md`](file:///home/rony/LPR/IAdeConteudo/AnamnesePosicionamento.md):
    - `Etapa1QuemEVoce` (Dados básicos, especialidade, tempo, prêmios, momento atual enum).
    - `Etapa2SuaHistoria` (Motivação, caso marcante, fase difícil, motor pessoal).
    - `Etapa3SeuTrabalho` (Procedimento master, procedimento lucrativo, procedimento preferido, diferencial, por que te escolhem, crítica aos pares).
    - `Etapa4SeuPaciente` (Perfil demográfico/psicográfico, medos, desejos, 5 perguntas frequentes, mitos, canal de origem).
    - `Etapa5SuasReferencias` (3 perfis da área + o que admira/rejeita, perfis fora da área + atrativos).
    - `Etapa6LimitesExposicao` (Assuntos proibidos, vida pessoal aceita, estilo de vida aceito, trabalho aceito, nível de conforto na câmera, regras do conselho regional - CRO/CFM/CRBM).
    - `Etapa7SeuConhecimento` (5 temas favoritos, tema de palestra, verdade corajosa, histórico de posts que deram certo/errado, post dos sonhos).
    - `Etapa8SeuJeito` (Arquétipos de comunicação, amostra de escrita explicativa real 8.2, status da identidade visual, estética odiada).
    - `Etapa9RotinaCapacidade` (Dia típico, horas por semana, apoio disponível, ranking de facilidade de formatos, histórico de postagens).
    - `Etapa10Objetivos` (Meta 3 meses, meta 1 ano, experiência passada com marketing, resultado #1 prioritário).
  - Mapear via EF Core com Value Object Converters/JSON Column Mappings para SQL Server.
  - Criar Migration `AddAnamneseModule` isolada.
- **Entregáveis da Subfase 2.1:**
  - Modelo de domínio `Anamnese` completo com testes unitários de invariants e validações de dados.
  - Mapeamento EF Core e Migrations criadas e validadas em container SQL Server.

### Subfase 2.2: Camada de Aplicação (CQRS) e Auto-Salvamento (Save & Resume)
- **Tarefas:**
  - Criar os Commands: `StartAnamneseCommand`, `SaveAnamneseStepCommand` (para salvar cada etapa individualmente), `CompleteAnamneseCommand`.
  - Criar as Queries: `GetAnamneseStatusQuery`, `GetAnamneseStepQuery`, `GetFullAnamneseQuery`.
  - Implementar lógica de progresso (% de conclusão, etapa atual, status: `Draft`, `Completed`).
  - Escrever testes de integração garantindo que o tenant A não consegue visualizar nem modificar a anamnese do tenant B.
- **Entregáveis da Subfase 2.2:**
  - CQRS Handlers para salvamento incremental e retomada de formulário.
  - Suíte de testes TDD cobrindo cenários de sucesso, erro de validação e isolamento de tenant.

### Subfase 2.3: Interface Blazor Interativa — Wizard em 10 Etapas (Stitch UI)
- **Tarefas:**
  - Desenvolver o componente Blazor `AnamneseWizard.razor` e subcomponentes para cada etapa (`Step1Component.razor` até `Step10Component.razor`).
  - Implementar barra de progresso visual ("Etapa X de 10 — Y% concluído"), botão "Salvar e Continuar Depois", validação em tempo real e tooltips de exemplos didáticos em todas as perguntas.
  - Renderizar tipos de campos dinâmicos:
    - Campos de texto livre com contagem de caracteres e sugestões.
    - Seleção única e múltipla (Checkboxes/Radio buttons estilizados).
    - Componente de reordenação (Drag-and-Drop ou botões cima/baixo para a Etapa 9.4).
    - Validação de links de Instagram (Etapa 5.1 e 5.4).
  - Escrever testes de componente Blazor via bUnit para o wizard e navegação interetapas.
- **Entregáveis da Subfase 2.3:**
  - UI de Anamnese fluida, responsiva, alinhada ao Stitch UI e totalmente testada no frontend.

### Subfase 2.4: Motor de Acompanhamento Automático por IA (AI Clarification Follow-up)
- **Tarefas:**
  - Implementar serviço `IAnamneseClarificationService` para analisar em tempo real ou ao transicionar etapas se respostas a perguntas críticas (ex: 3.5, 4.2, 7.3, 8.2) são muito curtas ou vagas.
  - Exibir popup/card de sugestão de aprofundamento (ex: "Sua resposta 'Sou dedicado' é genérica. E na prática, o que seu paciente percebe na 1ª consulta?").
  - Testar os prompts e cenários de detecção de vagueza com testes unitários demotivos.
- **Entregáveis da Subfase 2.4:**
  - Motor de acompanhamento com IA funcional no frontend, destravando respostas de baixa qualidade antes do envio final.

#### Resultado Esperado da FASE 2:
Módulo de Anamnese 100% funcional, permitindo que o profissional responda pausadamente às 10 etapas, receba ajuda de IA para aprofundar respostas vagas, salve o rascunho em qualquer ponto e conclua o formulário com dados persistidos em SQL Server.

---

## FASE 3: Agente 1 — Estrategista de Persona e Diagnóstico de Posicionamento

### Subfase 3.1: Abstração de Integração com Provedores LLM
- **Tarefas:**
  - Criar o BuildingBlock de IA `PersonaScript.BuildingBlocks.AI` com a interface `ILLMProvider`.
  - Implementar suporte a provedores (OpenAI API / Azure OpenAI / Anthropic / Google Gemini) com fallback automático em caso de indisponibilidade ou rate limit.
  - Implementar mecanismos de Retry com Exponential Backoff via Resilience (Polly) e Structured Output JSON Parsing com Schema Enforcement.
  - Testes de unidade utilizando Mocks do `ILLMProvider`.
- **Entregáveis da Subfase 3.1:**
  - Abstração de LLM robusta com suporte a streaming e retorno em JSON estrito.

### Subfase 3.2: Motor de Prompt do Agente 1 (Estrategista de Persona)
- **Tarefas:**
  - Desenvolver o handler `GeneratePersonaDiagnosisCommandHandler` no módulo `Modules.Personas`.
  - Construir a engenharia de prompt do Agente 1 consumindo o objeto completo `Anamnese`:
    - Síntese de perfil do profissional e dor principal do paciente.
    - Geração da **Frase Única de Posicionamento** (Item 1 do entregável).
    - Mapeamento da **Identidade da Marca** (Tom de voz, estilo visual sugerido, arquétipo) alinhada às referências da Etapa 5 e proibições da Etapa 8.4 (Item 2).
    - Definição dos **Pilares de Conteúdo com Distribuição Percentual** (ex: 30% Educação, 25% Prova/Casos, 25% Autoridade/Opinião, 20% Bastidores) (Item 3).
    - Matriz de **Restrições e Diretrizes Inegociáveis** (derivadas das Etapas 5.3, 6.1 e 8.4).
  - Salvar o resultado na entidade `PersonaDiagnosis` associada ao `TenantId`.
- **Entregáveis da Subfase 3.2:**
  - Agente 1 capaz de ler uma Anamnese concluída e gerar o perfil estruturado em JSON em menos de 15 segundos.
  - Testes unitários do prompt handler e parsing de saída.

### Subfase 3.3: Interface de Exibição e Ajuste do Diagnóstico de Posicionamento
- **Tarefas:**
  - Criar as telas Blazor `/posicionamento` e `/posicionamento/diagnostico`.
  - Exibir visualmente o resumo da marca: Frase de Posicionamento em destaque, cards dos Pilares de Conteúdo com gráficos de pizza/barra (distribuição %), guia de tom de voz e lista de restrições.
  - Permitir pequenas edições manuais pelo usuário ou solicitação de regeneração com feedback direcionado.
  - Escrever testes bUnit da tela de diagnóstico.
- **Entregáveis da Subfase 3.3:**
  - Tela de Diagnóstico de Posicionamento elegante, responsiva e pronta para consumo pelo usuário.

#### Resultado Esperado da FASE 3:
Diagnóstico estratégico do profissional gerado automaticamente por IA a partir das 10 etapas da Anamnese, definindo posicionamento, pilares e limites de marca com visualização em dashboard.

---

## FASE 4: Agente 2 — Copywriter de Vídeo, Conteúdos e Planos Estratégicos

### Subfase 4.1: Engine de Geração de Roteiros de Vídeo (Gancho, Retenção, CTA)
- **Tarefas:**
  - Criar no módulo `Modules.Scripts` o Command `GenerateVideoScriptCommand`.
  - Injetar no prompt do Agente 2 a **amostra de escrita real 8.2** do profissional para clonagem de tom de voz humano e as **restrições inegociáveis** (5.3, 6.1, 8.4, conselho regional 6.6).
  - Estruturar a geração de roteiro de vídeo em 3 blocos obrigatórios:
    1. **Gancho (Hook):** primeiros 3 segundos para parar a rolagem.
    2. **Retenção (Body):** conteúdo prático ou opinião forte mantendo a atenção.
    3. **Chamada para Ação (CTA):** direcionamento ético para comentário, direct ou agendamento.
  - Salvar o roteiro na entidade `VideoScript` com status (`Draft`, `Approved`, `Recorded`, `Published`).
- **Entregáveis da Subfase 4.1:**
  - Gerador de roteiros de alta conversão funcionando com amostragem de escrita real.
  - Testes unitários e de integração do fluxo de copywriting.

### Subfase 4.2: Geração do Plano de Stories e Calendário Editorial de 90 Dias
- **Tarefas:**
  - Criar o Command `GenerateContentPlanCommand` para gerar os entregáveis 5 e 6 da Anamnese:
    - **Plano de Stories (Item 5):** Cronograma de stories diários integrados à rotina real (Etapa 9.1), respeitando os horários em que o profissional trabalha e atende.
    - **Plano de 90 Dias (Item 6):** Calendário editorial trimestral com sugestões de temas por semana, objetivos e formatos adaptados à facilidade de produção do cliente (Etapa 9.4).
  - Persistir as entidades `StoryPlan` e `NinetyDayCalendar`.
- **Entregáveis da Subfase 4.2:**
  - Agente 2 gerando o plano de 90 dias e o plano de stories personalizado conforme a rotina do profissional.

### Subfase 4.3: Hub de Conteúdo Blazor, Modo Teleprompter e Exportação
- **Tarefas:**
  - Criar o painel Blazor `/roteiros` e `/roteiros/{id}`.
  - Desenvolver visualização de Roteiro com abas (Gancho, Retenção, CTA, Legenda do Post, Dicas de Gravação).
  - Desenvolver o modo **Teleprompter Interativo** (scroll automático ajustável para gravação no celular ou desktop).
  - Adicionar exportação de roteiros e plano de 90 dias para PDF, Markdown e cópia rápida para área de transferência.
  - Adicionar botão de feedback ("Gostei", "Ajustar tom", "Regerar") para alimentar o ciclo de melhoria contínua do Agente 2.
  - Testes bUnit para os componentes de visualização e teleprompter.
- **Entregáveis da Subfase 4.3:**
  - Hub de Conteúdo com Teleprompter e exportação em PDF/Markdown funcional.

#### Resultado Esperado da FASE 4:
Sistema completo de geração de cópias e roteiros de vídeo, plano de stories e calendário de 90 dias perfeitamente calibrado com a voz, limites e rotina do profissional, equipado com Teleprompter e exportação.

---

## FASE 5: Módulo de Billing, Planos Recorrentes e Quotas (B2C Self-Service)

### Subfase 5.1: Modelagem do Módulo Billing e Assinaturas
- **Tarefas:**
  - Criar o módulo `PersonaScript.Modules.Billing`.
  - Criar entidades: `Plan` (Básico, Pro, Reference), `Subscription` (Status: `Trialing`, `Active`, `PastDue`, `Canceled`), `UsageQuota` (Limites mensais: roteiros/mês, personas ativas, análises de IA).
  - Criar tabela de auditoria de consumo `QuotaTransaction`.
- **Entregáveis da Subfase 5.1:**
  - Entidades de Billing com isolamento de tenant e testes unitários de lógica de quotas.

### Subfase 5.2: Integração com Gateway de Pagamento (Stripe Checkout & Webhooks)
- **Tarefas:**
  - Implementar `IStripePaymentService` para criação de sessões de Stripe Checkout e Stripe Customer Portal.
  - Criar endpoint HTTP seguro `POST /webhooks/stripe` para processamento idempotente de eventos (`customer.subscription.created`, `customer.subscription.updated`, `customer.subscription.deleted`, `invoice.payment_succeeded`, `invoice.payment_failed`).
  - Testes de integração simulando eventos de Webhook do Stripe via CLI.
- **Entregáveis da Subfase 5.2:**
  - Integração de pagamentos Stripe completa e testada com tratamento idempotente de webhooks.

### Subfase 5.3: Validação de Quotas e Interceptadores de Limite de Uso
- **Tarefas:**
  - Criar Decorator/Pipeline Behavior `QuotaValidationBehavior` para bloquear a execução de Commands de geração de IA se a quota do tenant estiver esgotada.
  - Exibir modal no Blazor orientando o upgrade de plano ao atingir o limite.
  - Implementar reset mensal automático de quotas via background job (Quartz.NET ou Hangfire/HostedService).
- **Entregáveis da Subfase 5.3:**
  - Sistema de controle estrito de quotas de consumo operante com bloqueio gracioso no frontend.

### Subfase 5.4: Portal do Assinante e Gestão de Assinatura
- **Tarefas:**
  - Criar tela Blazor `/minha-conta/assinatura` com informações do plano atual, uso da franquia mensal, histórico de faturas e botão para gerenciar assinatura no Stripe.
  - Fluxo self-service de upgrade, downgrade e cancelamento sem fricção.
- **Entregáveis da Subfase 5.4:**
  - Painel de assinatura self-service funcional no app cliente.

#### Resultado Esperado da FASE 5:
Motor de monetização SaaS B2C totalmente funcional, integrando Stripe Checkout, controle rígido de limites por plano e portal de autoatendimento financeiro.

---

## FASE 6: Módulo de Backoffice — Painel de Gestão e Administração do Sistema

### Subfase 6.1: Arquitetura da Área Administrativa e Layout do Backoffice
- **Tarefas:**
  - Criar o módulo ou área `PersonaScript.Modules.Backoffice` e o conjunto de páginas sob a rota `/admin/...`.
  - Configurar autorização obrigatória baseada no papel `RequireSystemAdmin` ou `RequireSupportAgent`.
  - Desenvolver layout administrativo exclusivo com navegação por: Dashboard Geral, Tenants/Usuários, Gestão de Prompts IA, Monitoria de Execução, Financeiro/Planos, Dicionário de Conselhos Éticos e Logs de Auditoria.
- **Entregáveis da Subfase 6.1:**
  - Layout base do Backoffice responsivo, protegido por autorização RBAC e com menu navegação estruturado.

### Subfase 6.2: Gestão de Tenants, Usuários e Impermersonação de Suporte (Impersonation)
- **Tarefas:**
  - Criar página `/admin/tenants`: tabela com filtros por e-mail, nome, plano, status de assinatura, data de cadastro e consumo de IA.
  - Detalhe do tenant: visualização da Anamnese preenchida, diagnósticos gerados, número de roteiros emitidos e log de erros do tenant.
  - Funcionalidade de **Modo Suporte (Impersonate Tenant)**: permite que um agente de suporte navegue no aplicativo com a visão do usuário para diagnosticar problemas (gerando log de auditoria `AdminImpersonationLog` com motivo obrigatório).
  - Ações administrativas: redefinir senha manualmente, congelar conta, conceder créditos de geração extras.
- **Entregáveis da Subfase 6.2:**
  - Gerenciador de usuários/tenants com ferramenta de suporte auditada funcional.

### Subfase 6.3: Gestão Financeira, Controle de Planos e Sobrescrita de Limites
- **Tarefas:**
  - Criar página `/admin/financeiro`: visão geral de MRR (Receita Recorrente Mensal), Churn Rate, inadimplência e conversão de trial.
  - Painel de gerenciamento de planos: ajuste dinâmico de limites por plano (ex: alterar limite do plano Pro de 30 para 50 roteiros/mês).
  - Sobrescrita de quota por tenant específico (ex: conceder bônus VIP para cliente parceiro).
- **Entregáveis da Subfase 6.3:**
  - Dashboard financeiro e gestão de quotas administrativas ativas.

### Subfase 6.4: Gestão Dinâmica de Prompts de IA, Versionamento e Engenharia de Contexto
- **Tarefas:**
  - Criar página `/admin/prompts`: catálogo centralizado de todos os prompts do sistema (Agente 1 - Diagnóstico, Agente 2 - Roteiro Vídeo, Agente 2 - Stories, Agente 2 - Clarificação Anamnese).
  - Versionamento de Prompts: criar tabela `PromptTemplate` com colunas (`AgentName`, `Version`, `SystemPrompt`, `UserPromptTemplate`, `IsActive`, `ParametersJson`).
  - Permissão para testar alterações de prompt em tempo real no Backoffice (Playground de Teste) antes de publicar a versão para produção.
  - Rollback de versão de prompt com 1 clique.
- **Entregáveis da Subfase 6.4:**
  - Editor dinâmico de prompts de IA com versionamento, ambiente de teste (playground) e rollback instantâneo.

### Subfase 6.5: Telemetria de Tokens, Custos de LLM, Latência e Observabilidade dos Agentes
- **Tarefas:**
  - Criar serviço de rastreamento `ILLMTelemetryService` que grava cada execução dos Agentes na tabela `AgentExecutionLog` (`TenantId`, `AgentName`, `ModelUsed`, `PromptTokens`, `CompletionTokens`, `EstimatedCostUSD`, `LatencyMs`, `Status`, `ErrorMessage`).
  - Criar página `/admin/telemetria`: gráficos de consumo de tokens por dia/mês, custo total de API de LLM versus receita de assinaturas, tempo médio de resposta por agente, taxa de erro por modelo.
  - Alerta de anomalia (ex: notificar se o custo de LLM de um único tenant exceder o limite aceitável no mês).
- **Entregáveis da Subfase 6.5:**
  - Painel de observabilidade de custos, desempenho e erros de LLM em tempo real.

### Subfase 6.6: Moderador de Qualidade, Regras dos Conselhos (CRO/CFM/CRBM) e Dicionário Global
- **Tarefas:**
  - Criar página `/admin/conselhos-eticos`: cadastro e edição das diretrizes regulatórias por conselho profissional (ex: Regras atualizadas de publicidade do CFM 2.336/2023, Resoluções do CRO, CRBM).
  - Injeção automática das regras atualizadas do conselho selecionado na Anamnese do profissional durante a chamada dos Agentes.
  - Banco de termos proibidos (Blacklist de palavras proibidas pela legislação ou diretrizes de anúncio).
- **Entregáveis da Subfase 6.6:**
  - Central de governança ética e compliance regulatório de saúde atualizável no Backoffice.

#### Resultado Esperado da FASE 6:
Módulo de Backoffice completo e seguro, permitindo ao time de operações gerenciar assinantes, prestar suporte via impersonação auditada, gerenciar finanças e quotas, editar e versionar prompts de IA sem re-deploy, monitorar custos de API em tempo real e garantir compliance regulatório com conselhos de saúde.

---

## FASE 7: Qualidade, Cobertura de Testes (TDD), Performance e Segurança

### Subfase 7.1: Suíte de Testes de Isolamento Multi-Tenant (Anti Cross-Tenant Leak)
- **Tarefas:**
  - Escrever suíte de testes de integração automatizados em xUnit que tenta forçar acessos cruzados em todos os Repositórios e CQRS Handlers (tentativa de leitura/escrita do Tenant B usando token do Tenant A).
  - Garantir que todas as consultas retornem nulo/vazio ou `Result.Failure` sem expor dados de terceiros.
- **Entregáveis da Subfase 7.1:**
  - 100% dos repositórios e handlers validados contra vazamento de dados entre tenants.

### Subfase 7.2: Testes de Interface Blazor (bUnit) e Integração E2E
- **Tarefas:**
  - Expandir cobertura de testes de componentes Blazor com bUnit para Wizard de Anamnese, Diagnóstico de Posicionamento, Gerador de Roteiros e Backoffice.
  - Executar testes de aceitação simulando a jornada completa: Cadastro -> Anamnese 10 Etapas -> Geração de Diagnóstico -> Geração de Roteiro -> Assinatura.
- **Entregáveis da Subfase 7.2:**
  - Suíte de testes bUnit cobrindo todos os fluxos críticos de UI.

### Subfase 7.3: Otimização de Consultas SQL Server, Caching e Performance
- **Tarefas:**
  - Adicionar índices otimizados no SQL Server por `TenantId` em todas as tabelas de módulo.
  - Configurar Caching em memória (IMemoryCache / Redis) para leitura de `PromptTemplate` ativos e regras de conselhos éticos.
  - Validar tempo de carregamento de páginas (< 500ms) e tempo de resposta de APIs.
- **Entregáveis da Subfase 7.3:**
  - Banco de dados indexado e caching configurado para baixa latência.

### Subfase 7.4: Hardening de Segurança, Sanitização de Prompts e OWASP Compliance
- **Tarefas:**
  - Implementar sanitização de inputs de usuários para prevenir Prompt Injection nos Agentes de IA.
  - Adicionar Rate Limiting nos endpoints de autenticação, geração de IA e webhooks.
  - Validar cabeçalhos de segurança HTTP (CSP, X-Frame-Options, HSTS, Antiforgery Tokens em todos os forms).
- **Entregáveis da Subfase 7.4:**
  - Aplicação protegida contra OWASP Top 10 e Prompt Injections.

#### Resultado Esperado da FASE 7:
Sistema altamente seguro, imune a vazamentos cross-tenant, com alta cobertura de testes automatizados (backend e frontend), otimizado em performance e protegido contra abusos de IA.

---

## FASE 8: Homologação, Infraestrutura, CI/CD e Lançamento (Go-Live)

### Subfase 8.1: Pipeline de CI/CD e Infraestrutura de Produção
- **Tarefas:**
  - Configurar GitHub Actions / Azure Pipelines para compilação automatizada, execução da suíte completa de testes unitários e de integração, e análise de código estático (SonarQube/dotnet format).
  - Configurar scripts de deployment automatizado para ambiente de Staging e Produção (Docker / Azure App Service / SQL Azure).
  - Executar as EF Core Migrations automatizadas na inicialização do servidor ou via pipeline.
- **Entregáveis da Subfase 8.1:**
  - Esteira de CI/CD funcional implantando automaticamente em ambiente de homologação e produção.

### Subfase 8.2: Logging Estruturado, Observabilidade e Alertas
- **Tarefas:**
  - Configurar logging estruturado com Serilog (enviando para Application Insights / Seq / OpenTelemetry).
  - Configurar painel de saúde em `/health` e monitoramento de disponibilidade da aplicação e do banco de dados SQL Server.
  - Configurar alertas automáticos no Slack/Teams para falhas em Webhooks de pagamento ou taxa de erro elevada em chaves de LLM.
- **Entregáveis da Subfase 8.2:**
  - Sistema 100% observável com alertas proativos de erro em produção.

### Subfase 8.3: Programa Beta Fechado com Profissionais de Saúde
- **Tarefas:**
  - Convidar um grupo de 20 a 50 profissionais de saúde (dentistas, médicos, biomédicos) para preencher a Anamnese e gerar roteiros.
  - Coletar métricas de usabilidade, tempo de preenchimento do formulário e satisfação com a fidelidade do tom de voz.
  - Calibrar os prompts no Backoffice com base nos feedbacks coletados durante o beta.
- **Entregáveis da Subfase 8.3:**
  - Feedback real de clientes validado e ajustes finos de IA publicados via Backoffice.

### Subfase 8.4: Lançamento Oficial (Go-Live SaaS B2C)
- **Tarefas:**
  - Abrir cadastro público self-service na plataforma.
  - Monitorar métricas de aquisição, taxa de conclusão de Anamnese, conversão de checkout Stripe e custos de LLM via Backoffice.
  - Ativação do plano de manutenção e evolução contínua.
- **Entregáveis da Subfase 8.4:**
  - Sistema **PersonaScript AI** operando em produção com receita recorrente B2C ativa.

---

## RESUMO MATRIZ DE ENTREGÁVEIS POR FASE

| Fase | Foco Principal | Qtd Subfases | Principais Entregáveis Esperados |
| :--- | :--- | :---: | :--- |
| **Fase 1** | Fundação, Identity & RBAC | 4 | Multi-tenancy B2C isolado, cookie auth, mailpit e roles admin/user. |
| **Fase 2** | Motor de Anamnese em 10 Etapas | 4 | Wizard 10 etapas Blazor, save&resume, validações e motor IA de follow-up. |
| **Fase 3** | Agente 1 (Estrategista) | 3 | Geração de Posicionamento, Identidade de Marca e Pilares de Conteúdo. |
| **Fase 4** | Agente 2 (Copywriter & Roteiros) | 3 | Roteiros com Gancho/Retenção/CTA, tom de voz 8.2, Teleprompter e 90 dias. |
| **Fase 5** | Billing & Quotas (Stripe) | 4 | Stripe Checkout, webhooks idempotentes, limites mensais e portal de assinatura. |
| **Fase 6** | Módulo de Backoffice Admin | 6 | Gestão de tenants/impersonação, finanças, editor de prompts, telemetria LLM e conselhos. |
| **Fase 7** | Qualidade, TDD, Perf & Sec | 4 | Testes anti-leak tenant, bUnit UI, hardening de segurança e sanitização IA. |
| **Fase 8** | CI/CD, Beta & Lançamento | 4 | Pipeline automatizada, observabilidade, beta fechado com médicos/dentistas e Go-Live. |

---

## CONFORMIDADE COM O CHECKLIST LLM ([`AGENTS.md`](file:///home/rony/LPR/IAdeConteudo/AGENTS.md))

Toda a implementação deste Roadmap respeita integralmente:
- [x] **TDD Mandatório:** Testes escritos antes ou junto com o código em cada subfase.
- [x] **Padrão Result:** Retorno de `Result<T>` no backend sem exceções para controle de fluxo.
- [x] **Isolamento de Tenant:** `TenantId` obtido de `ITenantContext`, filtros globais EF Core e verificação anti-leak.
- [x] **Arquitetura Blazor + Stitch:** UI construída segundo o design canônico e testada via bUnit.
- [x] **Backoffice Dedicado:** Operação completa de IA, finanças, suporte e prompts desacoplada e segura.
- [x] **Documentação Viva:** Atualização constante dos arquivos em `docs/` ao longo de cada entrega.
