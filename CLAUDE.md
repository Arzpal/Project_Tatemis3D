# Tatemis3D — Contexto del proyecto

> Idioma de trabajo: **español**. Este archivo (y los `docs/`) reemplazan a la memoria de Claude: **todo el contexto persistente vive en el repo**, no en la cuenta de Claude.

## Qué es
Creature-collector 3D en **Unity (URP)** con **combate de acción en tiempo real**: controlas por completo al monstruo y combinas sus **habilidades únicas + elemento + el ambiente**. Tono *Cómo entrenar a tu dragón*. Proyecto **personal de Arzpal** (con 2 colaboradores). Nombre "Tatemis3D" es placeholder.

## Estructura del repo
El **proyecto Unity vive en la subcarpeta `Tatemis3D/`** (se abre ESA carpeta en Unity Hub). La raíz del repo es para lo que **no** va dentro de Unity (docs, y a futuro concept art, builds, etc.). Scripts en `Tatemis3D/Assets/Tatemis/Scripts/` (namespace `Tatemis`; subcarpetas Core, Data, Combat, Environment, Input, Debug, Editor).

## Qué leer según la tarea (economía de contexto: leer SOLO lo necesario)
- **Estado actual / qué toca hacer** → bloque "Estado" al inicio de [docs/TASKS.md](docs/TASKS.md). Fases y gate → [docs/ROADMAP.md](docs/ROADMAP.md).
- **Implementar combate/habilidades** → [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) (fuente de verdad técnica; su "Mapa de sistemas" dice qué script hace qué). Catálogo detallado de efectos/targeting → [docs/ABILITY-SYSTEM.md](docs/ABILITY-SYSTEM.md) §3–5.
- **Reacciones elementales / terreno** → [docs/ELEMENTAL-REACTIONS.md](docs/ELEMENTAL-REACTIONS.md).
- **Diseño, visión, lore, alcance** → [docs/GDD.md](docs/GDD.md) **por sección, no entero** (mapa de secciones en su cabecera).
- **Cualquier otro tema / lista completa de docs** → [docs/INDEX.md](docs/INDEX.md) (índice, hogares reservados para sistemas futuros y reglas para crear docs nuevos).
- **NO leer `docs/gdd.html`** — es el pitch visual; su contenido duplica al GDD.
- Los docs técnicos empiezan con TL;DR + mapa de secciones: leer las primeras ~15 líneas antes de decidir leer más. Para código: preferir Grep / lectura parcial.

## Cómo trabajar en este proyecto (preferencias)
- **Todo local / en el repo.** No publicar Artifacts ni nada en la cuenta de Claude. **No usar la memoria de Claude** — persistir el contexto en archivos `.md` del repo.
- Al terminar avances significativos: **actualizar el bloque "Estado" de docs/TASKS.md** (y el doc afectado) para que la siguiente sesión arranque barato.
- **Este archivo no crece.** Contexto persistente nuevo → doc en `docs/` siguiendo las reglas de [docs/INDEX.md](docs/INDEX.md) (un sistema = un doc, TL;DR, tope ~12 KB, obsoleto → `docs/archive/`).
- **Crédito "Arzpal"**: sin nombre de studio ni fechas en los entregables (salvo que se pida).
- Responder siempre en **español**.

## Equipo y ritmo
**Arzpal en solitario por ahora (2026-07-27)** — los 2 colaboradores quedaron en pausa; part-time (nights & weekends), programación + diseño **muy sólidos** pero **sin artista 3D/animador dedicado** (placeholders). Estrategia acordada: **prototipo de *feel* en caja gris (Nivel A) primero**, antes del vertical slice. Ver [ROADMAP](docs/ROADMAP.md).
