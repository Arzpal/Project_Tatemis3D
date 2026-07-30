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
| [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) | Análisis modular vs bespoke de los 2 kits reales + **18 huecos** del catálogo + orden en **4 olas** *(✏️ 2026-07-29: eran 9 y 3; el rediseño elemental añadió H17 cancelar la recuperación y H18 efectos pegados)* | implementar los kits de Pyrecko/Ottorock o refactorizar el sistema de habilidades |
| [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) | **La base del sistema elemental**, no un subsistema más (✅ **reformulado de raíz 2026-07-29**). **§0 es el punto de entrada y de ahí cuelga todo el resto**: un **verbo por elemento**, un elemento puro **solo hace daño**, las **cinco salidas** (neutro · amplificación · reacción tuya · reacción del terreno · combinación), de quién es cada efecto, el efecto que se pega. Luego: terreno fijo, reglas R1–R6, **las 15 parejas cerradas (6 reacciones + 9 combinaciones)**, marcas de especie, legibilidad | implementar reacciones / terreno / elemento — **empezar siempre por §0** |
| [CAMERA-TARGETING.md](CAMERA-TARGETING.md) | Cámara y lock-on: **spec a replicar de BotW/TOTK** + qué medir/investigar + estado del código (**✅ v1 implementada; faltan los parámetros medidos**) | tocar cámara, apuntado, lock-on u orientación del personaje |
| [VITALITY-DEATH.md](VITALITY-DEATH.md) | Vitalidad, muerte real, noqueo/rescate del caído, derrota total y persecución del Tamer (**🟡 en discusión**) | diseñar/implementar muerte, KO, derrota, swap de emergencia o manadas |
| [TAMER.md](TAMER.md) | **La línea monstruo ↔ Tamer** + el **menú del Tamer** (capturar, objetos, órdenes, swap) + qué queda del botón contextual + el finisher sin botón (**🟡 sin ratificar**) | tocar el esquema de control, la captura, los objetos, las órdenes o el swap |
| [TATEMIS-BASE.md](TATEMIS-BASE.md) · **[tatemis-base.html](tatemis-base.html)** | **Anatomía de un tatemis jugable** (referencia: Ottorock-Libre): cómo encajan movimiento, cámara, habilidades, elemento, feedback visual/físico, combos aéreos y noqueo, con los números del gate. El `.html` es la versión visual con diagramas | entender el conjunto de "cómo se siente y funciona un tatemis" · onboarding de feel |
| **[crear-tatemis.html](crear-tatemis.html)** | **Pipeline de construcción de una especie, de inicio a fin** (9 pasos: hueco que llena → ficha → hexágono → locomoción → kit → componer habilidades → generar assets → afinar → cerrar), con las relaciones entre números que no se pueden romper y la tabla síntoma→palanca. **Solo HTML**: es un documento de proceso visual, sin `.md` fuente | arrancar una especie nueva · onboarding de un colaborador · saber qué palanca tocar en playtest |
| [CREATURES.md](CREATURES.md) | Índice de especies + plantilla de los 18 campos + razonamiento del trío + **mapeo hexágono→HP** | diseñar una criatura nueva / ver qué especie hace qué |
| CREATURE-PYRECKO · CREATURE-OTTOROCK · CREATURE-MANTAGALE (.md) | Ficha completa de cada especie (18 campos + reglas propias + abiertas) | implementar o ajustar el kit de ESA especie |
| [CREATURE-IDEAS.md](CREATURE-IDEAS.md) | **Cantera de ideas de criaturas sin ficha** — la matriz tipo×arquetipo (6×5=30) + conceptos crudos a la espera de madurar | volcar una idea nueva de monstruo · buscar de dónde sacar las especies 4–5 |
| [ARCHETYPES.md](ARCHETYPES.md) | **Brújula de los 5 arquetipos** (cómo gana · cómo se le gana · pregunta generadora) + **pentágono de counters 2-2** + espejo LoL | diseñar o clasificar una especie nueva · dudas de rol, counters o composición de equipos |
| [ARENA.md](ARENA.md) | **Arena y rejilla elemental** (✅ dictada 2026-07-26): mundo continuo + overlay hexagonal leve del elemento predominante + zona de combate enmarcada + tamaño de celda | montar las grids del terreno (Fase 2) · dudas de arena, manadas o encierro |
| [GDD.md](GDD.md) | Visión completa de diseño — ir **por sección**: §3 alcance slice · §6 combate · §7 elementos · §8 ambiente+captura · §9 criaturas · §10 Tamer · §11 mundo · §13 arte/audio | dudas de diseño, visión, lore o alcance |
| gdd.html | Pitch visual autocontenido para humanos — **NO leerlo**, duplica al GDD (⚠️ desfasado, regenerar antes de presentar) | nunca (solo para presentar) |

> **Los `.html` son para humanos, no para contexto:** duplican contenido que ya está en los `.md`, así que **no se leen** para trabajar — se abren en el navegador para presentar o para entender algo de un vistazo. La excepción es `crear-tatemis.html`, que no tiene `.md` detrás.

> **Reparto ADD ↔ ARCHITECTURE** (para no duplicar, regla 1): **add** = cómo encaja y **por qué**, con diagramas · **ARCHITECTURE** = qué clase hace qué, con código. Un diagrama nuevo va **siempre** a `add.html`, no al chat.

## Docs futuros (nombres reservados — crearlos cuando el sistema se diseñe/construya, no antes)

Un sistema = un archivo en `docs/`. Al crear uno: añadir su línea a la tabla de arriba y un enlace de 1 línea en la sección del GDD que corresponda.

- `CAPTURE.md` — captura, Sludge y Tótems a detalle (si supera lo que cubre ARCHITECTURE §4)
- `WORLD.md` — biomas, clima, estructura del mundo, mazmorras
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
