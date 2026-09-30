# Hyperframes Composition Brief: RepCortex

## Objective
Create a 21-second polished launch-style brag video for RepCortex, a multi-tenant review intelligence dashboard.

## Output
- Composition directory: `brag-output-2026-09-27-220735/composition/`
- Rendered video: `brag-output-2026-09-27-220735/brag.mp4`
- Format: landscape — 1920x1080
- Duration: 21 seconds

## Source Material
- Project root: `/Users/mitsuru/Developer/RepCortex`
- Primary files read: `README.md`, `RepCortex.Dashboard/src/app/pages/dashboard/dashboard.component.html`, `dashboard.component.css`, `src/styles.css`, `src/app/pages/login/login.component.html`, backend controllers/services/entities
- Product name: RepCortex
- Tagline / strongest claim: `RepCortex - Moderador & Painel Analítico de Avaliações com IA (Multi-tenant)`
- Key UI or visual moment to recreate: metrics dashboard, moderation table, and integration endpoint panel
- Copy that must appear verbatim: `Painel Analítico em Tempo Real`, `Total de Avaliações`, `Média de Notas`, `IA: Positivas`, `IA: Neutras`, `IA: Negativas`, `Moderação & Gerenciamento de Comentários`, `POST`, `/api/public/avaliacoes`, `Aprovar`, `Rejeitar`, `Responder avaliação...`

## Creative Direction
- Tone preset: polished
- Creative direction: quiet technical product film for a live reputation control room
- Interpretation: dark UI, strong hierarchy, smooth slides, decisive but readable technical copy
- Angle: a review is not just stored; it travels through API ingestion, PT-BR sentiment analysis, realtime delivery, and human moderation
- Hook: `AVALIAÇÃO NOVA. DECISÃO AGORA.` over the live dashboard shell
- Outro / punchline: `RepCortex — avaliações que viram ação.`
- Avoid: generic SaaS language, real keys, real customer data, ungrounded claims, abstract filler, visual redesign unrelated to the source UI

## Visual Identity
- Background: `#090d16`
- Text: `#f9fafb`
- Accent: `#7c3aed`
- Secondary states: `#10b981`, `#f59e0b`, `#ef4444`
- Display font: system sans
- Body font: system sans; monospace for code/flow labels
- Visual references from the project: dark surface cards, violet active navigation, color-coded sentiment badges, realtime line chart, integration API panel

## Storyboard
Use `brag-plan.md` as the creative contract.

1. Live review alert — 2.8s — hook over a dashboard shell
2. Ingestão via API — 3.5s — safe endpoint and Portuguese review classification
3. Métricas Realtime — 4.2s — actual metric labels and chart
4. Moderação que vira ação — 4.5s — moderation row, sentiment, status, reply
5. O caminho inteiro — 3.0s — API to dashboard architecture strip
6. Lockup — 3.0s — RepCortex final message

## Audio
- Audio role: polished rhythmic bed with sparse interface accents
- Audio arc: quiet live pulse → precise ingest action → metric rhythm → restrained moderation confirmation → confident final hit
- Music: `assets/music/happy-beats-business-moves-vol-10-by-ende-dot-app.mp3`
- Music treatment: volume 0.26, immediate start with a short fade-in, slight duck in the lockup, fade at end
- Music cue guidance: `../.agents/skills/brag/assets/music/cues/happy-beats-business-moves-vol-10-by-ende-dot-app.music-cues.md`; strong cue target 20.19s; beat grid supports readable card accents
- Audio-reactive treatment: subtle glow/chart breathing tied to music energy, no waveform/equalizer visual
- Audio-coupled moments: API submit click, sentiment chip drop, metric card stagger, moderation confirmation, final lockup
- SFX selection guidance: choose low/medium high-frequency-risk interface and soft impact cues; exact files are copied into `composition/assets/`
- Audio files: music and three local SFX are copied into the composition

## Hyperframes Instructions
Use a visual identity grounded in the project palette. Build end-state layouts before animation. Use timeline-bound scenes with readable holds, one beat-locked final reveal near 20.19s, and a subtle audio-reactive treatment on the violet glow/chart. Run `npx hyperframes check`, then preview, render, pick a settled poster frame, bake it as frame 0, and write share copy.
