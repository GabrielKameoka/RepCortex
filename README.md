# RepCortex 🧠🛡️

Plataforma SaaS Multi-Tenant corporativa de coleta de avaliações (reviews) de produtos, equipada com motor de **Análise de Sentimento local (ML.NET)** e **Rate Limiting distribuído (Redis)** baseada em janela deslizante. 

O projeto foi construído utilizando **.NET 10**, aplicando práticas rigorosas de **Clean Architecture** e conceitos de **DDD (Domain-Driven Design)**, projetado para alta escalabilidade e isolamento lógico de inquilinos (Tenants).

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

## 🔧 Como Executar Localmente

### Pré-requisitos
* **Docker** instalado.
* **.NET 10 SDK** (opcional, apenas se quiser rodar fora do Docker).

### Passo 1: Subir a Infraestrutura (Banco de Dados e Cache)
Clone o repositório e execute na raiz:
```bash
docker-compose up -d
```
Isso iniciará um contêiner **PostgreSQL 17** e um contêiner **Redis** prontos para uso.

### Passo 2: Configurar o arquivo `.env`
As variáveis de ambiente para inicialização já vêm preenchidas por padrão no arquivo `.env` do ambiente local. Caso precise customizar as chaves de criptografia e bancos de produção, configure as variáveis de ambiente locais do sistema operacional ou injete na API.

### Passo 3: Executar a API
Execute a aplicação via terminal:
```bash
dotnet run --project RepCortex.API/RepCortex.API.csproj
```

A API estará disponível em:
* **HTTP:** `http://localhost:5154`
* **Painel Interativo Scalar:** `http://localhost:5154/scalar/v1`

---

## 🧪 Como Testar a API

Utilize o arquivo pré-configurado `RepCortex.API.http` presente no diretório `RepCortex.API` de forma nativa no VS Code ou Rider para realizar requisições automáticas de ponta a ponta. 

### Fluxo de Teste Sugerido:

1. **Registrar um Novo Tenant:**
   Faça um POST em `/api/auth/registrar` passando o slug desejado. A resposta retornará suas chaves de API exclusivas (`PublishableKey`, `SecretKey`) e um token JWT inicial.
2. **Ingestão de Avaliação (Pública):**
   Envie uma avaliação para `/api/public/avaliacoes` usando o cabeçalho `X-Api-Key` configurado com a sua `PublishableKey`. Experimente colocar textos claramente negativos com nota 5 para ver o ML.NET classificar o sentimento e reter a aprovação automática.
3. **Consulta de Avaliações (Admin):**
   Utilize o token JWT recebido no login para autenticar e consultar as avaliações recebidas em `/api/admin/avaliacoes`.

---

## 🧪 Rodando os Testes Unitários

O projeto possui cobertura de testes focados nas regras de domínio cruciais (como a auto-aprovação de reviews e o tratamento de sentimento). Para rodar os testes, utilize o comando:

```bash
dotnet test
```

---

## 🤝 Co-authored
Desenvolvido com o auxílio do Copilot CLI como assistente de engenharia de software para garantir práticas de Clean Code e arquitetura corporativa moderna.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
