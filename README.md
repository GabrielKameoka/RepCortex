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

- `frontend/`: projeto Angular do dashboard, com `package.json` e `angular.json`
  na raiz dessa pasta.
- `backend/`: solução `.NET 10` única que contém API, class libraries e testes.
- `backend/RepCortex.Domain`: agregados `Tenant` e `Avaliacao`, entidade `Usuario`,
  enumerações e regras de negócio. Não referencia os outros projetos.
- `backend/RepCortex.Application`: casos de uso, DTOs e portas para persistência,
  transação, identidade e contexto da requisição. Referencia apenas Domain.
- `backend/RepCortex.Infrastructure`: EF Core/PostgreSQL, migrations, Identity, JWT,
  Redis e análise de sentimento. Implementa as portas de Application.
- `backend/RepCortex.API`: controllers, autenticação HTTP, CORS, middleware, SignalR e
  composição das dependências. Referencia Application e Infrastructure.
- `backend/RepCortex.Tests`: testes de domínio, modelo EF e direção das dependências.

As referências entre projetos impõem a direção `API → Application ← Infrastructure`
e `Application → Domain ← Infrastructure`. Controllers delegam operações aos
serviços e casos de uso da aplicação. `AppDbContext` e `HttpContext` permanecem
fora de Application e Domain.

## Configuração local

```bash
cd backend
dotnet restore RepCortex.sln
dotnet build RepCortex.sln -c Release -warnaserror
dotnet test RepCortex.sln -c Release --no-build
```

As migrations e o snapshot existentes pertencem a `RepCortex.Infrastructure`.
Para listar ou criar migrations, use o projeto da API como startup e o de
Infrastructure como destino:

```bash
dotnet ef migrations list --project RepCortex.Infrastructure --startup-project RepCortex.API
dotnet ef migrations add NomeDaMigration --project RepCortex.Infrastructure --startup-project RepCortex.API
```

O workflow `Backend CI` executa build Release sem avisos, testes de unidade e
arquitetura, um fluxo HTTP com PostgreSQL e Redis temporários e o build da
imagem Docker usada pelo Railway. Para repetir o teste HTTP contra uma API
local com dados descartáveis, execute a partir de `backend/`:

```bash
API_BASE_URL=http://localhost:8080 python3 scripts/smoke-backend.py
```

O Dockerfile da API fica em `backend/Dockerfile`; `railway.json` aponta para
ele com contexto de build na raiz do repositório. O projeto Vercel do dashboard
deve usar `frontend` como Root Directory quando essa mudança chegar a `main`.

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
