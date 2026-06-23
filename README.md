# RepCortex 🧠🛡️

Plataforma SaaS Multi-Tenant corporativa de coleta de avaliações (reviews) de produtos, equipada com motor de **Análise de Sentimento local (ML.NET)** e **Rate Limiting distribuído (Redis)** baseada em janela deslizante. 

O projeto foi construído utilizando **.NET 10**, aplicando práticas rigorosas de **Clean Architecture** e conceitos de **DDD (Domain-Driven Design)**, projetado para alta escalabilidade e isolamento lógico de inquilinos (Tenants).

---

## 🌐 Live Demo & API Docs

O projeto já está **deployado em produção** e você pode interagir diretamente com ele online!

* **Documentação Interativa (Scalar):** [https://repcortex-production.up.railway.app/scalar/v1](https://repcortex-production.up.railway.app/scalar/v1)
* **Base URL da API:** `https://repcortex-production.up.railway.app`

---

## 🚀 Diferenciais Técnicos e Arquitetura

### 1. Multi-Tenancy Robusto (Shared Database, Tenant Isolation)
* **Isolamento de Inquilinos:** Cada tenant (empresa/e-commerce) possui suas próprias chaves de acesso autogeradas no cadastro (`PublishableKey` para widgets públicos e `SecretKey` para integrações de backend).
* **Filtros Globais de Tenant:** Um middleware customizado (`TenantMiddleware`) atua interceptando requisições, injetando o contexto do Tenant ativo no escopo da requisição e assegurando que nenhum dado vaze entre inquilinos.

### 2. Análise de Sentimento On-the-Fly com ML.NET
* **IA Local:** Em vez de depender de APIs pagas ou lentas de terceiros, o RepCortex treina um modelo de classificação multiclasse local (`SdcaMaximumEntropy`) no startup do contêiner.
* **Auto-Moderação Inteligente:** As avaliações recebidas são classificadas em *Positivo*, *Neutro* ou *Negativo*. Se uma avaliação tiver nota alta (ex: 5 estrelas), mas o algoritmo detectar sentimentos irônicos ou negativos (ex: *"Péssimo serviço, odeio tudo"*), a plataforma retém a avaliação com status `Pendente` para aprovação manual.

### 3. Rate Limiting Avançado com Redis
* **Prevenção contra Bots e Spam:** O endpoint público de ingestão de reviews possui limite de taxa de janela deslizante (`SlidingWindowRateLimiter`) integrado ao ASP.NET Core e distribuído com **Redis Cache**.
* **Particionamento Inteligente:** O limite de requisições é isolado de forma dinâmica usando a chave compostas `rate_limit_tenant:{tenantId}:ip:{ipOrigem}`, garantindo que um bot abusando de uma loja não afete a experiência de outras na mesma API.

### 4. Documentação de API Moderna com Scalar
* Substitui o Swagger tradicional por uma interface de documentação moderna, rica e interativa fornecida pelo **Scalar** com o tema `Purple`, integrada de forma nativa às novas capacidades de OpenAPI do **.NET 10**.

### 5. Arquitetura Limpa (Clean Architecture)
A estrutura segue o fluxo clássico de dependência de dentro para fora:
* **Domain:** Entidades puras de domínio, enums, regras de negócio ricas e contratos de serviços/repositórios.
* **Application:** Casos de uso (Use Cases) e lógica de orquestração de negócios, DTOs e mapeamento de dados.
* **Infrastructure:** Implementação concreta dos repositórios (EF Core com PostgreSQL), Identity de autenticação, integração com Redis Cache, segurança (JWT) e análise de sentimento (ML.NET).
* **API / Presentation:** Controllers REST protegidos, rate limiting, middlewares de isolamento e registro de injeção de dependências.

---

## 🛠️ Stack Tecnológica

* **Backend:** C# .NET 10 (ASP.NET Core Web API)
* **ORM & Banco de Dados:** Entity Framework Core & PostgreSQL (com migração automática na inicialização)
* **Caching & Rate Limiting:** StackExchange.Redis
* **Machine Learning:** ML.NET
* **Documentação:** Scalar API & OpenAPI nativa
* **Testes:** xUnit, FluentAssertions e dotnet CLI
* **Orquestração:** Docker & Docker Compose

---

## 🧪 Como Testar e Interagir com a API (100% Online)

Você não precisa instalar nada ou baixar o projeto localmente para testar os endpoints! Como a aplicação já está **deployada em produção**, você pode testar todo o fluxo de ponta a ponta de duas formas extremamente simples:

### Opção 1: Diretamente pelo Painel Interativo do Scalar (Recomendado)
Acesse a documentação interativa:
👉 **[https://repcortex-production.up.railway.app/scalar/v1](https://repcortex-production.up.railway.app/scalar/v1)**

Pelo próprio painel do Scalar, você consegue disparar requisições em tempo real para a nossa API na nuvem:
1. **Registrar Tenant:** Vá na rota `POST /api/auth/registrar`, digite dados fictícios no JSON e clique em "Send Request".
2. **Copie as chaves geradas:** A resposta trará o seu token JWT e sua `PublishableKey`.
3. **Ingestão de Avaliação:** Vá na rota `POST /api/public/avaliacoes`, clique em Headers, configure a chave `X-Api-Key` com o valor da sua `PublishableKey` gerada e envie uma avaliação com nota e comentário (teste o ML.NET escrevendo comentários negativos com nota alta!).
4. **Consultar como Admin:** Vá em `GET /api/admin/avaliacoes`, configure o header de autorização `Bearer <Seu-JWT-Copiado>` e veja a lista de avaliações processadas com o status atualizado da inteligência artificial local.

---

### Opção 2: Utilizando ferramentas locais (Postman, Insomnia ou VS Code)
Se você utiliza a extensão **REST Client** no VS Code ou o **Rider**, utilize o arquivo `RepCortex.API.http` presente na pasta `RepCortex.API`. Ele já está totalmente pré-configurado para apontar e disparar requisições diretamente contra o servidor de produção na nuvem!

---

## 🤝 Co-authored
Desenvolvido com o auxílio do Copilot CLI como assistente de engenharia de software para garantir práticas de Clean Code e arquitetura corporativa moderna.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
