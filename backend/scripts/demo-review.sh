#!/usr/bin/env bash
set -euo pipefail

publishable_key="${1:?Informe a publishableKey mostrada na aba Integração.}"
review_id="$(date +%s)"
api_base_url="${API_BASE_URL:-http://localhost:5154}"
demo_origin="${DEMO_ORIGIN:-http://localhost:4200}"

curl --fail-with-body --silent --show-error \
  --request POST "${api_base_url}/api/public/avaliacoes" \
  --header 'Content-Type: application/json' \
  --header "X-Api-Key: ${publishable_key}" \
  --header "Origin: ${demo_origin}" \
  --data "{\"usuarioIdExterno\":\"recruiter-${review_id}\",\"nomeUsuarioExterno\":\"Visitante da demo\",\"produtoId\":\"produto-demo\",\"nota\":2,\"comentario\":\"Entrega atrasou e o produto veio com defeito.\",\"fingerprint\":\"demo-${review_id}\"}"
printf '\n'
