# Tatemis3D — Índice de documentación

> Mapa de toda la documentación del proyecto. Regla general: **leer solo el doc (y la sección) que pida la tarea**. Todo doc técnico empieza con un TL;DR + mapa de secciones — leer las primeras ~15 líneas suele bastar para decidir si hace falta más.

## Docs actuales

| Doc | Qué contiene | Leerlo cuando… |
|---|---|---|
| [TASKS.md](TASKS.md) | **Bloque "Estado"** (fase actual, hecho, siguiente) + backlog de la fase en curso | SIEMPRE al retomar trabajo — normalmente solo el bloque Estado |
| [ROADMAP.md](ROADMAP.md) | Estrategia (feel primero), niveles de scope, fases y el gate | planificación / decidir qué construir |
| [add.md](add.md) · **[add.html](add.html)** | **ADD — Architecture Design Document:** el mapa mental del código (capas, actuador vs payload, las 10 primitivas, locomoción, olas) + **el hogar de todos los diagramas**. `add.html` es la versión visual | entender cómo encaja el código · **añadir un diagrama nuevo** |
| [ARCHITECTURE.md](ARCHITECTURE.md) | La **especificación**: ScriptableObjects, efectos componibles, runtime, captura, reacciones. Su "Mapa de sistemas" dice qué script hace qué | implementar: firmas, structs, código concreto |
| [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md) | Catálogo de efectos/targeting/stances a triagear + estrategia (bespoke primero, extraer después) | diseñar o implementar habilidades concretas |
| [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) | Análisis modular vs bespoke de los 2 kits reales + 9 huecos del catálogo + orden en 3 olas | implementar los kits de Pyrecko/Ottorock o refactorizar el sistema de habilidades |
| [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) | **Fuente de verdad del sistema elemental** (✅ cerrado 2026-07-18): terreno fijo, 3 salidas, reglas R1–R6, la tabla de 15 parejas, marcas de especie, legibilidad | implementar reacciones / terreno / clima |
| [VITALITY-DEATH.md](VITALITY-DEATH.md) | Vitalidad, muerte real, noqueo/rescate del caído, derrota total y persecución del Tamer (**🟡 en discusión**) | diseñar/implementar muerte, KO, derrota, swap de emergencia o manadas |
| [CREATURES.md](CREATURES.md) | Índice de especies + plantilla de los 18 campos + razonamiento del trío | diseñar una criatura nueva / ver qué especie hace qué |
| CREATURE-PYRECKO · CREATURE-OTTOROCK · CREATURE-MANTAGALE (.md) | Ficha completa de cada especie (18 campos + reglas propias + abiertas) | implementar o ajustar el kit de ESA especie |
| [GDD.md](GDD.md) | Visión completa de diseño — ir **por sección**: §3 alcance slice · §6 combate · §7 elementos · §8 ambiente+captura · §9 criaturas · §10 Tamer · §11 mundo · §13 arte/audio | dudas de diseño, visión, lore o alcance |
| gdd.html | Pitch visual autocontenido para humanos — **NO leerlo**, duplica al GDD | nunca (solo para presentar) |

> **Reparto ADD ↔ ARCHITECTURE** (para no duplicar, regla 1): **add** = cómo encaja y **por qué**, con diagramas · **ARCHITECTURE** = qué clase hace qué, con código. Un diagrama nuevo va **siempre** a `add.html`, no al chat.

## Docs futuros (nombres reservados — crearlos cuando el sistema se diseñe/construya, no antes)

Un sistema = un archivo en `docs/`. Al crear uno: añadir su línea a la tabla de arriba y un enlace de 1 línea en la sección del GDD que corresponda.

- `CAPTURE.md` — captura, Sludge y Tótems a detalle (si supera lo que cubre ARCHITECTURE §4)
- `WORLD.md` — biomas, clima, estructura del mundo, mazmorras
- `TAMER.md` — el Tamer, filosofía de equipo, vínculo criatura–Tamer
- `UI-UX.md` — HUD, menús, onboarding, accesibilidad
- `ART.md` — dirección de arte, pipeline de assets, integración del concept art
- `AUDIO.md` — dirección de audio, música, SFX
- `SAVE.md` — guardado y persistencia
- `MULTIPLAYER.md` — 🔮 multijugador (post-slice)
- `BUILD.md` — builds, plataformas, versionado, CI

## Reglas para que la documentación no engorde el contexto

1. **Un sistema = un doc.** No añadir sistemas nuevos dentro del GDD ni de ARCHITECTURE: crear su doc y enlazar.
2. **TL;DR obligatorio:** todo doc nuevo empieza con ≤5 líneas de resumen + un mapa de secciones en una línea.
3. **Tope ~12 KB por doc.** Si crece más, dividirlo por subtema.
4. **Lo obsoleto se archiva, no se acumula:** fases terminadas de TASKS.md y borradores superados van a `docs/archive/` (mismo nombre + sufijo), dejando un puntero de 1 línea en el doc original.
5. **CLAUDE.md no crece:** contexto persistente nuevo → doc en `docs/` + 1 línea en este índice.
6. **El estado vive en un solo sitio:** el bloque "Estado" de TASKS.md. No duplicar estado en otros docs.
