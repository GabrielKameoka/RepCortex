# RepCortex

RepCortex é uma plataforma multi-tenant para donos de e-commerce receberem,
moderarem e publicarem avaliações de produtos. O desenvolvedor integra a API
ao site da loja: visitantes enviam comentários com uma chave pública e o dono
decide o que aparece na página do produto.

## Fluxo do produto

1. O lojista cria seu espaço e recebe uma `publishableKey` uma única vez.
2. O site envia avaliações para `POST /api/public/avaliacoes` usando `x-api-key`.
   Deve informar `usuarioIdExterno` (ID do autor na loja) e pode informar
   `nomeUsuarioExterno` (nome de exibição, até 100 caracteres).
3. O tenant é identificado pela chave; `tenantId` e IP não vêm do cliente.
4. A política automática aprova somente nota 5 com sentimento positivo. A
   política manual mantém toda avaliação como pendente.
5. O site consulta apenas aprovadas em
   `GET /api/public/avaliacoes?produtoId=...&pagina=1&tamanhoPagina=20`.
6. O lojista usa JWT no dashboard para moderar e acompanhar métricas.
   Em cada avaliação ele vê o nome informado pela loja e o ID do autor;
   avaliações antigas ou sem nome mostram `Nome não informado` junto do ID.

O RepCortex não busca o perfil do comprador na loja: o nome é uma cópia do valor
enviado no momento da avaliação. `ClienteId` permanece apenas como coluna
histórica, sem fazer parte dos novos envios. A listagem pública de avaliações
aprovadas não inclui nome nem ID do autor.

## Arquitetura

- `RepCortex.API`: ASP.NET Core 10, Clean Architecture, JWT, API key pública,
  PostgreSQL, Redis, SignalR e filtros globais de tenant.
- `RepCortex.Dashboard`: Angular 16 com interceptor JWT, guard de rota e painel
  de moderação.
- `RepCortex.Tests`: testes de domínio para as regras de publicação e sentimento.

## Configuração local

```bash
dotnet restore
dotnet test
```

O banco não recebe dados fictícios por padrão. Para uma demonstração local,
habilite explicitamente `Database:ApplyMigrations=true` e `Demo:SeedData=true`,
informando `Demo:TenantId`, `Demo:AdminEmail` e `Demo:AdminPassword` por
variáveis de ambiente ou configuração local não versionada. O seeder é apenas
para desenvolvimento e não deve ser ativado em produção.

O dashboard exige senha com pelo menos oito caracteres. Configure suas origens
web em `Cors:AllowedOrigins`; não use `*` com credenciais.

Em `Production`, a API aplica as migrations do EF Core antes de aceitar requisições.
Isso mantém o esquema do PostgreSQL do Railway alinhado aos modelos publicados;
falhas de migration impedem a inicialização da API e aparecem nos logs do deploy.

## Segurança e limites atuais

- A chave pública pode ser exposta no frontend da loja e permite apenas ingestão
  e leitura pública de avaliações aprovadas.
- A chave secreta não é persistida pelo dashboard; o cadastro a devolve apenas
  para armazenamento seguro do operador.
- O backend captura o IP da conexão e aplica rate limit no endpoint público.
- JWT identifica o tenant do dashboard. Consultas sem tenant no contexto falham
  fechadas.
- Não há widget CDN incluído neste repositório. A integração usa a API REST ou
  um widget próprio da loja.

## Endpoints essenciais

| Objetivo | Método | Endpoint | Credencial |
|---|---:|---|---|
| Cadastro | POST | `/api/auth/registrar` | pública |
| Login | POST | `/api/auth/login` | pública |
| Enviar comentário | POST | `/api/public/avaliacoes` | `x-api-key` pública |
| Listar publicados | GET | `/api/public/avaliacoes` | `x-api-key` pública |
| Métricas | GET | `/api/admin/dashboard/metricas` | JWT |
| Política de moderação | GET/PUT | `/api/admin/configuracoes/moderacao` | JWT |
