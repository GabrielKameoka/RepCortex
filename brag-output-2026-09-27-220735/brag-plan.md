# Brag Plan: RepCortex

## What is this app?
RepCortex is a multi-tenant review intelligence platform that ingests customer evaluations, classifies sentiment in Brazilian Portuguese, updates live metrics, and gives the business a moderation workspace.

## The angle

Treat every review like a live operational event: it enters through the API, gets a sentiment decision, appears in the dashboard, and becomes an action for the operator. The humor is restrained and specific: “another review just landed” becomes a tiny control room for reputation.

## Hook (first 2-3 seconds)

Large type over the real dark dashboard language: “AVALIAÇÃO NOVA. DECISÃO AGORA.” A violet pulse arrives like a live event, followed by a compact “REALTIME” indicator.

## Key moments (the middle)

- A Portuguese review enters through `POST /api/public/avaliacoes` and is labeled `😊 Positivo` by the VADER Pt-BR analysis.
- The dashboard metrics settle into `Total de Avaliações`, `Média de Notas`, and the positive/neutral/negative IA split, with an ingestion line chart.
- A negative review lands in `Moderação & Gerenciamento de Comentários` with `😡 Negativo`, `Pendente`, and the concrete actions `Aprovar`, `Rejeitar`, and `Responder avaliação...`.
- A final architecture strip compresses the path: API → VADER Pt-BR → SignalR → Dashboard, with tenant isolation called out.

## Outro / punchline

“RepCortex — avaliações que viram ação.” Small footer: “Multi-tenant · realtime · PT-BR”.

## User flow worth showing

Entry → key action → result: submit an evaluation through the public REST endpoint → analyze sentiment and persist it in the tenant context → receive the live dashboard update and moderate/respond to the comment.

## Tone

- Preset: polished
- Creative direction: quiet technical product film for a live reputation control room
- Interpretation: confident pacing, clean hard cuts, restrained motion, and real Portuguese UI copy. No inflated SaaS claims.

## Format: landscape — 1920x1080
## Duration: 21 seconds

## Visual identity (from the project)

- Background: `#090d16`
- Accent: `#7c3aed`
- Text: `#f9fafb`
- Display font: system sans (`-apple-system`, `BlinkMacSystemFont`, `Segoe UI`)
- Body font: same system sans; monospace for endpoints and architecture labels
- Strongest visual element: the dark analytics dashboard with purple cards and sentiment-coded green/yellow/red states

## Share copy (draft)

Construí o RepCortex: avaliações entram pela API, a IA classifica o sentimento em PT-BR e o painel atualiza em tempo real para a equipe moderar e responder.

## Audio direction

- Role: warm, polished rhythmic bed with sparse interface accents
- Music: `happy-beats-business-moves-vol-10-by-ende-dot-app.mp3`
- Music treatment: start immediately at 0.26 volume, light fade-in over 0.25s, duck slightly under the final lockup, fade out in the last 0.45s
- Music cue guidance: bundled preset `assets/music/cues/happy-beats-business-moves-vol-10-by-ende-dot-app.music-cues.md`; tempo about 110 BPM; strong cues in the planned window at 18.55s, 20.19s, and 20.74s. Use the beat grid near 3.01, 6.28, 10.38, 14.20, 17.47, and 20.19s for sequential accents, but preserve text readability.
- Audio-reactive treatment: subtle; let the violet glow and chart presence breathe with the music energy, without waveform graphics or strobing
- SFX posture: sparse and professional; click for the API action, soft drop for metric/card arrivals, one soft impact on the final lockup
- Audio-coupled moments: endpoint submit, metric cards arriving in a readable stagger, moderation badge settling, final logo lockup
- Restraint rule: sound must support the flow, never make the technical product feel like an arcade game

## Storyboard

### Scene 1 — Live review alert — 2.8s

Dark RepCortex dashboard shell with a violet pulse and a small `REALTIME` pill. Headline enters: `AVALIAÇÃO NOVA.` then `DECISÃO AGORA.`

Sequential/interaction: yes — the pulse lands first, then the two headline lines; hold both lines for readability.
Audio intent: quiet start, one soft interface tick on the pulse.
Audio-coupled idea: beat-aligned glow, no rapid text flashing.
Music: upbeat but restrained.
Transition mood: clean hard cut → Scene 2

### Scene 2 — Ingestão via API — 3.5s

Recreate the project's integration panel: green `POST` badge, `/api/public/avaliacoes`, and a fictional safe payload with `nota: 5` and `comentario: “Sensacional! Recomendo demais.”`. A violet analysis chip settles as `😊 Positivo`.

Sequential/interaction: yes — endpoint first, payload second, sentiment result third.
Audio intent: precise click on submit, soft drop on the classification chip.
Audio-coupled idea: endpoint reveal near the 3.01s beat; result near 4.10s.
Transition mood: smooth wipe → Scene 3

### Scene 3 — Métricas Realtime — 4.2s

Actual dashboard copy appears in a faithful metrics grid: `Total de Avaliações`, `Média de Notas`, `IA: Positivas`, `IA: Neutras`, `IA: Negativas`; beneath it, `Volumetria de Ingestão de Dados` with a glowing line chart.

Sequential/interaction: yes — cards enter in a readable left-to-right stagger, then chart draws.
Audio intent: a light rhythmic layer, with one soft drop on the first and last card.
Audio-coupled idea: card arrivals use every other beat; chart draw breathes with RMS.
Transition mood: clean slide → Scene 4

### Scene 4 — Moderação que vira ação — 4.5s

Show the real moderation table language: a fictional `prod_capinha` review, `😡 Negativo`, `Pendente`, plus `Aprovar`, `Rejeitar`, and the reply input `Responder avaliação...`. The reply field receives `Vamos resolver isso.` and a green `Respondido` state appears.

Sequential/interaction: yes — row appears, status badge settles, reply is typed as one short phrase.
Audio intent: restrained keyboard/click suggestion; no comedic error sounds.
Audio-coupled idea: status transition near 10.38s; reply confirmation near 14.20s.
Transition mood: dramatic but clean → Scene 5

### Scene 5 — O caminho inteiro — 3.0s

Four connected nodes appear: `API`, `VADER PT-BR`, `SIGNALR`, `DASHBOARD`. Small caption: `isolamento por tenant`.

Sequential/interaction: yes — nodes connect left to right; each label stays visible after arrival.
Audio intent: a subtle rising bed and one soft impact at the completed path.
Audio-coupled idea: connection ticks near 15.82, 16.38, and 16.93s; keep the labels settled.
Transition mood: confident crossfade → Scene 6

### Scene 6 — Lockup — 3.0s

Centered RepCortex wordmark, violet halo, and the line `avaliações que viram ação.` Footer: `multi-tenant · realtime · PT-BR`.

Sequential/interaction: none — one deliberate logo reveal.
Audio intent: soft impact at the 20.19s strong cue; music fades under the final frame.
Audio-coupled idea: major reveal is beat-locked near 20.19s.
Transition mood: soft fade → end

**Music mood for this video:** upbeat / polished technical.
**Audio summary:** a clean rhythmic bed follows the review from ingestion to decision, with small interface accents and one confident final lockup hit.
