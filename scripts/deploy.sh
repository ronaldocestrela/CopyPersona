#!/usr/bin/env bash
# ==============================================================================
# PersonaScript AI - Production / Staging Deployment Script
# ==============================================================================
set -eo pipefail

ENV_TYPE="${1:-production}"
IMAGE_TAG="${2:-latest}"
COMPOSE_FILE="docker-compose.prod.yml"
APP_PORT="${APP_PORT:-8080}"
HEALTH_ENDPOINT="http://localhost:${APP_PORT}/health"
MAX_HEALTH_ATTEMPTS=20
HEALTH_INTERVAL=3

echo "======================================================================"
echo "🚀 Iniciando Deploy do PersonaScript AI"
echo "Ambiente:    ${ENV_TYPE}"
echo "Imagem Tag:  ${IMAGE_TAG}"
echo "Data/Hora:   $(date -u +"%Y-%m-%dT%H:%M:%SZ")"
echo "======================================================================"

# 1. Carregar variáveis de ambiente se existir arquivo .env
if [ -f ".env.${ENV_TYPE}" ]; then
    echo "📄 Carregando variáveis de .env.${ENV_TYPE}..."
    # shellcheck disable=SC2046
    export $(grep -v '^#' ".env.${ENV_TYPE}" | xargs)
elif [ -f ".env" ]; then
    echo "📄 Carregando variáveis de .env..."
    # shellcheck disable=SC2046
    export $(grep -v '^#' ".env" | xargs)
fi

# 2. Guardar ID da imagem/container atual para rollback
PREVIOUS_CONTAINER_ID=$(docker ps -q -f name=personascript-prod-web || true)
echo "🔍 Container atual: ${PREVIOUS_CONTAINER_ID:-Nenhum ativo}"

# 3. Pull / Build e Atualização dos Serviços
echo "📦 Baixando e iniciando os containers via ${COMPOSE_FILE}..."
export DOCKER_IMAGE="ghcr.io/ronaldocestrela/personascript-server:${IMAGE_TAG}"

if ! docker compose -f "${COMPOSE_FILE}" up -d --remove-orphans; then
    echo "❌ Erro ao subir containers com Docker Compose. Abortando."
    exit 1
fi

# 4. Smoke Test e Healthcheck da Aplicação
echo "🩺 Aguardando inicialização e verificando saúde em ${HEALTH_ENDPOINT}..."
ATTEMPT=1
HEALTHY=false

while [ "$ATTEMPT" -le "$MAX_HEALTH_ATTEMPTS" ]; do
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${HEALTH_ENDPOINT}" || true)
    
    if [ "$HTTP_STATUS" -eq 200 ]; then
        echo "✅ Health check bem-sucedido! (HTTP 200 - Tentativa ${ATTEMPT}/${MAX_HEALTH_ATTEMPTS})"
        HEALTHY=true
        break
    fi
    
    echo "⏳ Tentativa ${ATTEMPT}/${MAX_HEALTH_ATTEMPTS}: status HTTP=${HTTP_STATUS:-000}. Aguardando ${HEALTH_INTERVAL}s..."
    sleep "$HEALTH_INTERVAL"
    ATTEMPT=$((ATTEMPT + 1))
done

# 5. Tratamento de Sucesso ou Rollback
if [ "$HEALTHY" = true ]; then
    echo "======================================================================"
    echo "🎉 Deploy concluído com SUCESSO no ambiente ${ENV_TYPE}!"
    echo "Endpoint:    ${HEALTH_ENDPOINT}"
    echo "======================================================================"
    exit 0
else
    echo "======================================================================"
    echo "❌ FALHA NO HEALTH CHECK! Aplicação não respondeu HTTP 200 em tempo hábil."
    echo "⚠️ Iniciando procedimento de Rollback automático..."
    echo "======================================================================"
    
    # Exibir logs do container com falha para depuração imediata
    echo "📋 Logs do container personascript-prod-web:"
    docker logs --tail 40 personascript-prod-web || true
    
    # Se havia versão anterior, reiniciar
    if [ -n "$PREVIOUS_CONTAINER_ID" ]; then
        echo "🔄 Tentando restaurar container anterior (${PREVIOUS_CONTAINER_ID})..."
        docker restart "$PREVIOUS_CONTAINER_ID" || true
    fi
    
    exit 1
fi
