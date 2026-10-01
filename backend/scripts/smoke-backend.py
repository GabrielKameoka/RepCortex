"""Exercise the published HTTP contract against a disposable PostgreSQL database."""

import json
import os
import uuid
from urllib.error import HTTPError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


BASE = os.environ.get("API_BASE_URL", "http://127.0.0.1:8080").rstrip("/")
DASHBOARD_ORIGIN = "https://repcortex-dashboard.vercel.app"


def call(method, path, expected, payload=None, headers=None):
    body = json.dumps(payload).encode() if payload is not None else None
    request = Request(
        BASE + path,
        data=body,
        method=method,
        headers={"Content-Type": "application/json", **(headers or {})},
    )
    try:
        response = urlopen(request, timeout=20)
    except HTTPError as error:
        response = error
    with response:
        status = response.status
        raw = response.read()
        response_headers = response.headers
    assert status == expected, f"{method} {path}: expected {expected}, got {status}"
    return json.loads(raw) if raw else None, response_headers


def main():
    suffix = uuid.uuid4().hex[:10]
    slug = f"ci-{suffix}"
    email = f"ci-{suffix}@example.test"
    password = "CiPassword123!"

    _, cors = call(
        "OPTIONS", "/api/auth/registrar", 204,
        headers={
            "Origin": DASHBOARD_ORIGIN,
            "Access-Control-Request-Method": "POST",
            "Access-Control-Request-Headers": "content-type",
        },
    )
    assert cors.get("Access-Control-Allow-Origin") == DASHBOARD_ORIGIN

    registration, _ = call(
        "POST", "/api/auth/registrar", 200,
        {
            "tenantIdSlug": slug,
            "nomeComercial": "CI Tenant",
            "nomeCompletoUsuario": "CI Admin",
            "email": email,
            "senha": password,
        },
        {"Origin": DASHBOARD_ORIGIN},
    )
    assert registration["sucesso"] is True
    assert registration["tenantId"] == slug
    assert registration["publishableKey"].startswith("rc_pub_")
    assert registration["tokenJWT"]

    login, _ = call(
        "POST", "/api/auth/login", 200,
        {"tenantId": slug, "email": email, "senha": password},
        {"Origin": DASHBOARD_ORIGIN},
    )
    token = login["token"]
    assert token

    review = {
        "usuarioIdExterno": f"author-{suffix}",
        "nomeUsuarioExterno": "Cliente CI",
        "produtoId": f"product-{suffix}",
        "nota": 5,
        "comentario": "Produto excelente, recomendo demais!",
        "fingerprint": f"fingerprint-{suffix}",
    }
    public_path = "/api/public/avaliacoes"
    call("POST", public_path, 401, review, {"Origin": "http://localhost:4200"})
    created, _ = call(
        "POST", public_path, 201, review,
        {"Origin": "http://localhost:4200", "X-Api-Key": registration["publishableKey"]},
    )
    assert created["produtoId"] == review["produtoId"]

    admin_headers = {"Origin": DASHBOARD_ORIGIN, "Authorization": f"Bearer {token}"}
    test_review, _ = call(
        "POST", "/api/admin/integracao/avaliacoes-teste", 201,
        {**review, "usuarioIdExterno": f"admin-{suffix}"}, admin_headers,
    )
    assert test_review["produtoId"] == review["produtoId"]
    metrics, _ = call("GET", "/api/admin/dashboard/metricas", 200, headers=admin_headers)
    assert metrics["totalAvaliacoes"] == 2

    reviews, _ = call("GET", "/api/admin/avaliacoes", 200, headers=admin_headers)
    assert len(reviews) == 2
    call("POST", f"/api/admin/avaliacoes/{created['id']}/aprovar", 200, headers=admin_headers)
    public_query = urlencode({"produtoId": review["produtoId"]})
    visible, _ = call(
        "GET", f"{public_path}?{public_query}", 200,
        headers={"Origin": "http://localhost:4200", "X-Api-Key": registration["publishableKey"]},
    )
    assert any(item["id"] == created["id"] for item in visible["itens"])
    print("Backend smoke passed: migrations, CORS, registration, login, reviews and moderation.")


if __name__ == "__main__":
    main()
