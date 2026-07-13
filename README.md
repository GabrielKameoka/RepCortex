# RepCortex - Moderador & Painel Analítico de Avaliações com IA (Multi-tenant)

RepCortex é uma plataforma moderna e escalável de monitoramento de avaliações em tempo real para múltiplos clientes (*Multi-tenancy*). Através de uma API de alto desempenho acoplada a um modelo de Inteligência Artificial baseado no algoritmo de ponta **VADER** adaptado para Português, o sistema classifica automaticamente sentimentos, gerencia fluxos de moderação e propaga eventos em tempo real via websockets para um dashboard analítico.

---

## 🚀 Funcionalidades Principais

*   **Isolamento Multi-tenant Robusto:** Isolamento completo de dados por cliente (*tenant*) via filtros dinâmicos globais no Entity Framework Core.
*   **Análise de Sentimento com IA (VADER Pt-BR):** Implementação nativa ultra-rápida do algoritmo de valência léxica VADER, calibrado com mais de 100 termos em português, tratando negações com limites de barreira de cláusula (ex. vírgulas e pontos), boosters de intensidade (ex: *muito*, *super*), ALL CAPS e conjunções contrastivas (ex: *mas*, *porém*).
*   **WebSocket Realtime (SignalR):** Barramento de eventos em tempo real que atualiza gráficos analíticos e painéis de forma instantânea.
*   **CORS & Autenticação Segura:** Autenticação via JWT Bearer para administradores e API Keys (Publishable/Secret) para ingestão externa de avaliações, com suporte a preflights de CORS.
*   **Moderação Integrada & Auto-Aprovação:** Painel completo de moderação para o lojista aprovar ou rejeitar avaliações. Se o lojista responder a um comentário pendente, o sistema **auto-aprova** o comentário no ato da resposta!
*   **Sandbox de Testes Integrado:** Simulador de envio de avaliações em lote diretamente pelo painel para testes locais rápidos.

---

## 🧪 Executando os Testes Unitários

O projeto conta com uma suíte de testes de domínio que cobrem desde regras de negócio de auto-aprovação de avaliações até o funcionamento detalhado do algoritmo de IA VADER. Para rodar os testes:
```bash
dotnet test
```

---

## 🌟 Detalhes da Arquitetura do VADER IA

Nossa inteligência artificial não necessita de APIs externas caras ou lentas. Ela usa regras de valência léxica precisas:
*   *Exemplo:* `"Não gostei, produto horrível"`
    *   `Não gostei` é classificado como negativo pelo inversor de negação (`-1.48`).
    *   `produto horrível` é isolado da negação pela vírgula (barreira de cláusula) e avaliado corretamente como negativo (`-3.0`), evitando falsos positivos e falsos negativos comuns em modelos menos sofisticados!
