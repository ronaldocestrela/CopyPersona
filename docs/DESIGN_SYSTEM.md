# Design System & Identidade Visual — PersonaScript AI

> **Referência Canônica:** Projeto Framer AI Design Agent localizado na pasta [`refrence/`](../refrence/)  
> **Versão do Design System:** 2.0 (Framer True Dark & Glassmorphism)  
> **Framework Base:** Blazor (.NET 10) + Vanilla CSS Modular

---

## 1. Visão Geral e Filosofia do Design

O **PersonaScript AI** adota uma identidade visual moderna, imersiva e de alto contraste, diretamente inspirada no design da campanha de **AI Agents do Framer**.

### 1.1 Pilares Visuais
1. **True Dark (OLED Black):** Uso de preto absoluto (`#000000`) como tela de fundo principal, criando profundidade infinita e destacando as camadas de interface sem saturação desnecessária.
2. **Vidro Fosco & Superfícies Translúcidas (Glassmorphism):** Menus, cabeçalhos e modais utilizam desfoque acrílico (`backdrop-filter: blur(20px)`) com bordas sutis translúcidas (`rgba(255, 255, 255, 0.08)` a `0.18`), transmitindo leveza e sofisticação.
3. **Acentos Neon de Alta Energia:** Destaques cirúrgicos em **Neon Lime** (`#cbff00`) e **Electric Cyan** (`#0099ff`) para indicar status de agentes em tempo real, etapas ativas e métricas de conversão.
4. **Tipografia Editorial Tecnológica:** Mistura equilibrada de fontes sans-serif com tracking apertado (`Geist` e `Inter`) e fonte monospace (`JetBrains Mono`) para rótulos técnicos, prompts de IA e badges de versão.

---

## 2. Tokens de Design (Design Tokens)

Os tokens estão definidos globalmente no arquivo [`src/Presentation/PersonaScript.Server/wwwroot/app.css`](../src/Presentation/PersonaScript.Server/wwwroot/app.css).

### 2.1 Cores e Superfícies (Surfaces)

| Token CSS | Valor Hex / RGBA | Finalidade |
| :--- | :--- | :--- |
| `--ps-bg-canvas` | `#000000` | Fundo principal da aplicação (OLED Black) |
| `--ps-bg-surface` | `#0a0a0c` | Superfície secundária (fundos de seção) |
| `--ps-bg-card` | `#0e0e12` | Cartões padrão, contêineres e blocos |
| `--ps-bg-card-hover` | `#141419` | Estado de hover de cartões e itens interativos |
| `--ps-bg-elevated` | `#202026` | Superfícies sobrepostas (dropdowns, popovers) |
| `--ps-bg-glass` | `rgba(10, 10, 12, 0.72)` | Superfícies com desfoque de fundo |

### 2.2 Bordas e Divisores

| Token CSS | Valor RGBA | Finalidade |
| :--- | :--- | :--- |
| `--ps-border-subtle` | `rgba(255, 255, 255, 0.08)` | Bordas finas de cartões e divisores padrão |
| `--ps-border-default` | `rgba(255, 255, 255, 0.12)` | Contornos de botões secundários e inputs |
| `--ps-border-hover` | `rgba(255, 255, 255, 0.22)` | Realce de borda ao passar o cursor |
| `--ps-border-glow` | `rgba(255, 255, 255, 0.35)` | Efeito de brilho em bordas ativas e selecionadas |

### 2.3 Cores de Acento (Accents)

| Cor / Token | Valor Hex | Amostra | Aplicação |
| :--- | :--- | :--- | :--- |
| **Neon Lime** (`--ps-lime`) | `#cbff00` | `■` | Status de agente ativo, badges de IA, gancho de roteiro |
| **Electric Cyan** (`--ps-cyan`) | `#0099ff` | `■` | Seleção de texto, links, retenção de roteiro |
| **Emerald Green** (`--ps-emerald`) | `#00de68` | `■` | Sucesso, status concluído, validações positivas |
| **Purple / Violet** (`--ps-purple`) | `#8a58ff` | `■` | Iluminação radial superior, CTAs, tags de fechamento |
| **Sunset Orange** (`--ps-orange`) | `#fd7702` | `■` | Alertas moderados, rascunhos pendentes |

### 2.4 Hierarquia de Texto

| Token CSS | Valor | Aplicação |
| :--- | :--- | :--- |
| `--ps-text-primary` | `#ffffff` (100%) | Títulos H1-H6, textos em destaque, labels ativas |
| `--ps-text-secondary` | `rgba(255, 255, 255, 0.72)` | Parágrafos, textos descritivos, corpo de leitura |
| `--ps-text-muted` | `rgba(255, 255, 255, 0.44)` | Metadados, datas, placeholders e legendas |

---

## 3. Tipografia

As famílias tipográficas são importadas centralmente no `<head>` de [`App.razor`](../src/Presentation/PersonaScript.Server/Components/App.razor) via Google Fonts:

```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link href="https://fonts.googleapis.com/css2?family=Geist:wght@300;400;500;600;700;800&family=Inter:wght@300;400;500;600;700;800&family=JetBrains+Mono:wght@400;500;600&family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=swap" rel="stylesheet" />
```

### 3.1 Famílias Tipográficas
1. **`Geist` / `Inter` (`--ps-font-sans`):** Fonte principal para toda a aplicação. Possui legibilidade cristalina em telas escuras e suporte completo a pesos de 300 a 800.
2. **`JetBrains Mono` (`--ps-font-mono`):** Fonte monoespaçada para prompts de IA, tags de roteiro (`[00:00 - Gancho]`), medidores de quota e status de versão (`v2.6 Live`).
3. **`Material Symbols Outlined`:** Biblioteca de ícones com peso ótico ajustado (`24px, 400 weight`).

### 3.2 Escala de Títulos
- **Display Hero (H1):** `clamp(2.4rem, 5.5vw, 4.2rem)`, peso 700, `letter-spacing: -0.04em`, `line-height: 1.08`.
- **Títulos de Seção (H2):** `clamp(1.8rem, 3.5vw, 2.5rem)`, peso 700, `letter-spacing: -0.03em`.
- **Títulos de Cartão (H3):** `1.15rem` a `1.35rem`, peso 600, `letter-spacing: -0.02em`.

---

## 4. Componentes Globais da Interface

### 4.1 Barra de Navegação Superior ([NavMenu.razor](../src/Presentation/PersonaScript.Server/Components/Layout/NavMenu.razor))
- **Posição:** `position: sticky; top: 0; z-index: 1000;`
- **Fundo:** `rgba(0, 0, 0, 0.8)` com `backdrop-filter: blur(20px)` e borda inferior `1px solid rgba(255, 255, 255, 0.08)`.
- **Marca:** Ícone de faísca com borda refinada e brilho neon, acompanhado da tipografia "PersonaScript" e do badge `AI` em neon lime.
- **Links (`NavLink`):** Estilo botão translúcido, estado ativo destacado com `rgba(255, 255, 255, 0.1)` e borda suave.
- **Botão "Minha Conta":** Formato pílula em vidro que transiciona para branco puro no hover.

### 4.2 Botões (Button Variants)

#### Botão Primário Framer (`.btn-framer-primary`)
Branco sólido com texto preto, peso 600, formato pílula (`border-radius: 9999px`) e sombra de elevação sutil:
```css
.btn-framer-primary {
    background: #ffffff;
    color: #000000;
    font-weight: 600;
    padding: 0.65rem 1.35rem;
    border-radius: 9999px;
    border: 1px solid #ffffff;
    box-shadow: 0 4px 20px rgba(255, 255, 255, 0.15);
}
```

#### Botão Secundário Framer (`.btn-framer-secondary`)
Superfície escura translúcida com borda suave:
```css
.btn-framer-secondary {
    background: rgba(255, 255, 255, 0.06);
    color: #ffffff;
    font-weight: 500;
    padding: 0.65rem 1.35rem;
    border-radius: 9999px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    backdrop-filter: blur(10px);
}
```

#### Botão Neon Lime (`.btn-framer-lime`)
Utilizado em ações de alta prioridade ou conversão imediata:
```css
.btn-framer-lime {
    background: #cbff00;
    color: #000000;
    font-weight: 700;
    border-radius: 9999px;
    box-shadow: 0 4px 20px rgba(203, 255, 0, 0.25);
}
```

### 4.3 Badges e Indicadores de Agente
- **`.framer-badge`**: Pílula translúcida com texto semi-opaco.
- **`.framer-badge-lime`**: Destaque em neon lime com fundo translúcido `rgba(203, 255, 0, 0.15)`.
- **`.framer-badge-cyan`**: Destaque em cyan para o Agente 1 (Estrategista).
- **`.agent-status-dot`**: Ponto pulsante verde neon (`@keyframes agentPulse`) que indica execução autônoma de agente em tempo real.

### 4.4 Cartões e Efeitos de Brilho
- **`.framer-card`**: Fundo `#121215`, borda `1px solid rgba(255, 255, 255, 0.08)`, cantos arredondados de `16px`.
- **`.framer-card-glow`**: Adiciona uma linha de luz sutil no topo do cartão via pseudo-elemento `::before` com gradiente linear.

### 4.5 Modais e Diálogos de Sobreposição (Modals)
- **`.modal-backdrop`**: Overlay em tela cheia (`position: fixed; inset: 0; z-index: 1050; background: rgba(0, 0, 0, 0.8); backdrop-filter: blur(10px);`) com animação suave de fade-in (`psModalFadeIn`). Suporta fechamento ao clicar no fundo escurecido.
- **`.modal` & `.modal-dialog`**: Contêiner fixo centralizado na viewport (`z-index: 1055; display: flex; align-items: center; justify-content: center;`) com animação suave de escala e elevação (`psModalScaleUp`).
- **`.modal-content`**: Cartão com fundo `#0e0e13`, gradiente radial sutil no topo, cantos arredondados (`1.25rem`), borda translúcida e sombra projetada profunda (`box-shadow: 0 30px 80px -15px rgba(0, 0, 0, 0.9)`).
- **`.modal-header` & `.modal-title-group`**: Cabeçalho com ícone destacado (ex.: neon lime para IA), título e subtítulo descritivo, além de botão `.btn-close` circular translúcido com transição de hover.
- **`.form-grid` & `.modal-input` / `.modal-select` / `.modal-textarea`**: Campos de entrada estilizados com fundo `#08080b`, borda sutil e anel de foco neon lime/glow, rótulos com tag opcional/obrigatória.
- **`.modal-footer`**: Ações alinhadas à direita com botão secundário de cancelamento e botão primário `.btn-modal-submit` em neon lime de alta energia.

---

## 5. Mapeamento de Telas e Arquivos CSS

| Tela / Módulo | Arquivos de Estilo | Descrição Visual |
| :--- | :--- | :--- |
| **Landing Page** | [`Home.razor.css`](../src/Presentation/PersonaScript.Server/Components/Pages/Home.razor.css) | Hero com iluminação radial, simulação do Canvas com os 2 Agentes, Bento Grid e CTA final. |
| **Autenticação** | [`auth.css`](../src/Presentation/PersonaScript.Server/wwwroot/auth.css) | Telas `/login`, `/cadastro`, `/esqueci-senha`. Cartões centralizados em `#0e0e12`, inputs escuros e botões de login social. |
| **Anamnese** | [`anamnese.css`](../src/Presentation/PersonaScript.Server/wwwroot/anamnese.css) | Wizard da anamnese, stepper em pílulas, cartões de seleção e modal de esclarecimento com brilho da IA. |
| **Posicionamento & Diagnóstico** | [`posicionamento.css`](../src/Presentation/PersonaScript.Server/wwwroot/posicionamento.css) | Hero do diagnóstico de autoridade, pilares com barras de progresso neon/cyan, tags de arquétipos e modal de edição. |
| **Roteiros & Teleprompter** | [`roteiros.css`](../src/Presentation/PersonaScript.Server/wwwroot/roteiros.css) | Grade de roteiros, status tags em JetBrains Mono, abas de roteiro e teleprompter imersivo em OLED black com barra de foco. |
| **Minha Conta & Assinatura** | [`AssinaturaPage.razor.css`](../src/Presentation/PersonaScript.Server/Components/Pages/MinhaConta/AssinaturaPage.razor.css) | Cartões de planos, tag "Plano Atual" em neon lime e barras de progresso de cotas mensais. |
| **Backoffice Operacional** | [`BackofficeDashboard.razor`](../src/Presentation/PersonaScript.Server/Components/Pages/Backoffice/BackofficeDashboard.razor) | Dashboard técnico com métricas operacionais, cards escuros e controle de suporte. |

---

## 6. Diretrizes para Criação de Novos Componentes

Ao criar novos componentes de UI no ecossistema Blazor:

1. **Evitar Fundos Claros Puros:** Não utilize cartões com `background: #ffffff` ou textos pretos em telas escuras. Mantenha a hierarquia de camadas dark (`#000000` &rarr; `#0a0a0c` &rarr; `#0e0e12` &rarr; `#141419`).
2. **Utilizar Tokens Globais:** Sempre consuma variáveis CSS de [`app.css`](../src/Presentation/PersonaScript.Server/wwwroot/app.css) (`var(--ps-bg-card)`, `var(--ps-border-subtle)`, `var(--ps-text-primary)`).
3. **Bordas Sutis:** Prefira sempre bordas de 1px com transparência (`rgba(255, 255, 255, 0.08)`) em vez de linhas opacas sólidas.
4. **Formulários e Inputs:** Os campos de entrada devem ter fundo escuro (`#08080a`), texto branco, placeholder semi-opaco e foco realçado com anel translúcido.
5. **Acessibilidade e Microanimações:** Aplique transições suaves com curva cúbica (`cubic-bezier(0.16, 1, 0.3, 1)`) em estados de `:hover`, `:focus` e `:active`.
6. **TDD Obrigatório:** Escreva testes unitários de renderização com **bUnit** em `tests/Presentation/PersonaScript.Server.UnitTests/` antes ou junto à implementação do componente.
