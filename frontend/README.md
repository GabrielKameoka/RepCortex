# Dashboard RepCortex

Interface Angular para cadastro de lojas, moderação de avaliações, análise de
sentimento e integração com a API pública. A autenticação administrativa usa
JWT; a fila e os indicadores são atualizados por SignalR.

## Executar

Use o [ambiente de demonstração](../README.md#demonstração-em-2-minutos) para
subir frontend, API, PostgreSQL e Redis com um comando. Para desenvolver apenas
o dashboard, execute `npm ci` e `npm start` nesta pasta; a API deve responder em
`http://localhost:5154`.

## Validar

```bash
npm run build
npm test -- --watch=false --browsers=ChromeHeadless
```

O workflow `Frontend CI` repete esses comandos em PRs. A integração pública
usa `X-Api-Key` da loja; a chave secreta exibida no cadastro não fica salva no
armazenamento do navegador.
