#!/usr/bin/env bash
# ==============================================================================
# PersonaScript AI - Script de Geração e Aplicação de EF Core Migrations
# ==============================================================================
set -eo pipefail

OUTPUT_DIR="migrations-sql"
mkdir -p "$OUTPUT_DIR"

echo "======================================================================"
echo "🗄️  PersonaScript AI - EF Core Modular Migrations Script Generator"
echo "Data/Hora: $(date -u +"%Y-%m-%dT%H:%M:%SZ")"
echo "======================================================================"

STARTUP_PROJECT="src/Presentation/PersonaScript.Server/PersonaScript.Server.csproj"

MODULES=(
    "Identity:src/Modules/Identity/PersonaScript.Modules.Identity.Infrastructure/PersonaScript.Modules.Identity.Infrastructure.csproj:IdentityDbContext"
    "Anamnese:src/Modules/Anamnese/PersonaScript.Modules.Anamnese.Infrastructure/PersonaScript.Modules.Anamnese.Infrastructure.csproj:AnamneseDbContext"
    "Billing:src/Modules/Billing/PersonaScript.Modules.Billing.Infrastructure/PersonaScript.Modules.Billing.Infrastructure.csproj:BillingDbContext"
    "Personas:src/Modules/Personas/PersonaScript.Modules.Personas.Infrastructure/PersonaScript.Modules.Personas.Infrastructure.csproj:PersonasDbContext"
    "Scripts:src/Modules/Scripts/PersonaScript.Modules.Scripts.Infrastructure/PersonaScript.Modules.Scripts.Infrastructure.csproj:ScriptsDbContext"
    "Backoffice:src/Modules/Backoffice/PersonaScript.Modules.Backoffice.csproj:BackofficeDbContext"
)

for ITEM in "${MODULES[@]}"; do
    IFS=":" read -r NAME PROJ CONTEXT <<< "$ITEM"
    SQL_FILE="${OUTPUT_DIR}/${NAME}_idempotent.sql"
    echo "⚙️  Gerando SQL idempotente para Módulo: [${NAME}] (Context: ${CONTEXT})..."
    
    dotnet ef migrations script \
        --project "$PROJ" \
        --startup-project "$STARTUP_PROJECT" \
        --context "$CONTEXT" \
        --idempotent \
        --output "$SQL_FILE" || {
            echo "⚠️ Aviso: Não foi possível gerar script para ${NAME}. Verifique se há migrations pendentes ou dotnet-ef instalado."
        }
        
    if [ -f "$SQL_FILE" ]; then
        echo "✅ Gerado com sucesso: ${SQL_FILE}"
    fi
done

echo "======================================================================"
echo "🎯 Scripts SQL de Migrações salvos no diretório: ./${OUTPUT_DIR}/"
echo "Observação: Em produção com APPLY_MIGRATIONS=true, o monolito executa"
echo "as migrações automaticamente no startup via métodos assíncronos."
echo "======================================================================"
