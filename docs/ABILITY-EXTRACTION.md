# Tatemis3D — Extracción del sistema: los 3 kits (modular vs específico)

> **TL;DR:** Análisis de los **3 kits reales** ([CREATURES.md](CREATURES.md)) contra el catálogo de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md): qué piezas son **modulares** (librería), cuáles **parametrizables**, cuáles **bespoke**, y los **16 huecos** que el catálogo no cubría. El catálogo salió bien parado (~90% cubierto; los huecos son casi todos infraestructura, no efectos) y su §6 acertó de pleno: Pyrecko y Ottorock son criaturas de stances. **Mantagale (actualizado 2026-07-18 con su kit final) añade los 7 huecos más caros: vuelo, desplazamiento de criaturas y fuerzas que actúan sobre objetos del mundo.** Termina con el orden en 4 olas y las decisiones de locomoción/controles que los 3 kits obligan a tomar **antes de T0.3**.
> **Mapa:** §1 método · §2 desglose por habilidad · §3 veredicto · §4 los 16 huecos · §5 orden en 4 olas · §6 impacto en el scaffold · §7 cobertura de ejes · **§8 locomoción y controles (lo que T0.3–T0.5 deben respetar)**

## 1. Método y leyenda

Cada pieza de los kits se clasifica:
- **[M] Modular** — la usan ≥2 habilidades/criaturas → efecto o sistema de la librería.
- **[P] Parametrizable** — genérica con 1 parámetro nuevo (hoy 1 uso, mañana N).
- **[B] Bespoke** — única de la especie: código directo o `CustomBehaviour`; **NO generalizar aún**.
- **[S] Sistema** — no es un efecto: infraestructura persistente (stances, marcas, básicos).
- **[H#] Hueco** — el catálogo no lo cubría; numerados en §4.

## 2. Desglose por habilidad

### Pyrecko (Zonificador de Fuego)
| Habilidad | Piezas → clasificación |
|---|---|
| **Camuflaje** | `EnterStance(Sigilo)` [S]. Dentro del stance: velocidad ↑ (stats de StanceData) [M] · invisibilidad como pasiva del stance [P] · barra que drena/seg (salida por recurso) [M] · **drenaje extra por contacto enemigo** [H6] · romper al atacar (salida `OnAttack`) [P] · rastro que mancha combustible (OnTick + marca de especie) [H3] |
| **Disparo ígneo** | `SpawnProjectile(homing)` [M] con payload: `DealDamage` [M] + `ApplyStatus(Quemadura)` [M] + reacción de Fuego al impactar [M] + marca combustible r5 [H3]. Targeting `LockedTarget` [M]. **Cero bespoke.** |
| **Explosión venenosa** | Targeting «todas mis marcas vivas» [H4] → por marca: `SpawnHazard` 5 s con DoT + Slow 20 % no acumulable [M + H8]. La orquestación (`IgniteOwnedMarks`) es suya [B leve]. |
| **Básico** | Cadena de 6, entrega proyectil [M], **cada 6º golpe** mancha → sistema de básicos [S] + proc `EveryNthHit` [H5]. |
| **Bloqueo/forma** | `FormaElementalPura` como stance con params por especie (7 s, CD 7/10) [M]. |

### Ottorock (Luchador de Mineral)
| Habilidad | Piezas → clasificación |
|---|---|
| **Cambio de estado** | `ToggleStance(Libre↔Sujeción)` [S]. Al entrar: `GrabChunk` (ya estaba en el catálogo §3.7) [M] → resuelve **una vez** `Mineral + elemento del terreno` y guarda la **reacción sellada** en el blackboard [H2] + la reproduce en cada impacto [H9] + roca = **escudo con evento OnBroken → ExitStance** [H7] · stats y **kit override** por stance (previstos en StanceData) [M] · silueta bípedo/cuadrúpedo = contenido por stance. Coreografía de agarrar/romper [B]. |
| **Embestidas (2 variantes)** | Mismo esqueleto: `DashMotion` + `MeleeHitbox` durante el dash [M]; payload por variante (Sujeción: daño 3 + `Stun` 6 + **reproducir la reacción sellada** / Libre: daño 5 + `Slow` 2) [M]. **CD y energía compartidos entre variantes → coste a nivel de SLOT** [H1]. |
| **Impacto a distancia / Mordisco** | Sujeción: `ThrowHeld` (ya estaba en el catálogo §3.7) [M] con payload daño 5 + `Stun` 2 + **reproducir la reacción sellada** [H9] + side-effect `ExitStance` (al salir se **pierde la combinación**) [M]. Libre: `MeleeHitbox` + daño 7 [M]. |
| **Básicos por stance** | Cadenas distintas (Sujeción: 3, medio, muy lento, reacción por golpe / Libre: 5, corto, medio) → básicos con **override por stance** [S, H5]. |
| **Bloqueo/forma** | Regular; regla de interrupción **dependiente del stance** (Sujeción: solo al romper la roca) [P]. |

### Mantagale (Controlador de Aire, volador) — kit final, [ficha §C](CREATURE-MANTAGALE.md)
| Habilidad | Piezas → clasificación |
|---|---|
| **Altura (pasiva de especie)** | Sistema nuevo [S]: medidor de altura que **sube al formar una reacción** y **decae planeando**, con tope. Necesita que `ReactionSystem` **emita un evento** al resolver una reacción [H11] y **locomoción aérea** con eje vertical [H10]. No es un slot ni gasta energía. |
| **Movilidad — Elevarse + 1 tornado** | Impulso vertical [H10] + hace de dodge [M] + `SpawnHazard` **móvil con deriva aleatoria** [P sobre `SpawnHazard`] que **empuja de forma continua** [H12]. |
| **Flex — Potenciar básicos** | **Buff que modifica el ataque básico** durante N golpes [H13]; cada golpe potenciado: AoE [M] + `Knockback` con daño [H12] + spawn de tornado [M]. Es el primer caso de "habilidad que reescribe el básico". |
| **Signature — Ciclón** | `Pull` [H12] que afecta **a criaturas y a hazards del mundo** [H14] + `DealDamage` [M] + **detonación por colisión de dos fuerzas opuestas** (tornado empuja vs ciclón atrae) [H15] + **arrastre continuo por el suelo** que atraviesa los hazards del trayecto [H16]. |
| **Bloqueo/forma** | `FormaElementalPura` [M] con dos excepciones de especie [P]: **disponible en vuelo** y, al interrumpirse, **cuesta altura** en vez de solo cortar el stance. |
| **Básico** | Cadena normal de proyectiles de aire [M]. Sin proc propio: su función mecánica llega **prestada** por el Flex [H13]. |

## 3. Veredicto

- **Núcleo modular confirmado (lo piden AMBOS):** `SpawnProjectile`+payload · `DealDamage` · `StatusData` (Quemadura/Slow/Stun) · `DashMotion`+`MeleeHitbox` · `TriggerReaction` · `SpawnHazard` · **StanceSystem** — la pieza compartida más valiosa: Sigilo y Sujeción son el mismo esqueleto (stats + pasiva + kit/básicos override + condiciones de salida), y ABILITY-SYSTEM §6 ya tenía "Invisible" y "AgarrandoObjeto" en su lista de ejemplos · sistema de básicos con procs y override por stance · targeting `Self`/`LockedTarget`/`AimDirection`.
- **Parametrizable (1 uso hoy, genérico mañana):** **reacción sellada** en blackboard · escudo con `OnBroken` · drenaje de stance por evento · interrupción del bloqueo según stance · invisibilidad como pasiva de stance.
- **Bespoke (no generalizar):** qué "ve" la IA frente al sigilo (percepción) · `IgniteOwnedMarks` (semántica exacta del Signature de Pyrecko) · coreografía/anims del agarre de roca · condiciones de evolución (sistema futuro, GDD §9.2).
- **Sistemas aparte (no son efectos):** StanceSystem · marcas de especie · ataque básico · reacciones (ya diseñado en [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md)).

## 4. Los 9 huecos descubiertos (lo que el catálogo no cubría)

1. **Coste/CD a nivel de slot, no de `AbilityData`** — las variantes por stance comparten CD y energía. Hoy `AbilityCaster` tiene el cooldown keyed por `AbilityData` (`_readyAt`) → cambiar a por-slot.
2. **Blackboard a nivel de criatura/stance** — `AbilityCtx.bb` ya existe en `AbilityCore.cs`, pero muere con el cast; el trozo agarrado (y su elemento) debe sobrevivir **entre** habilidades → el blackboard vive en el stance.
3. **Marcas de especie** — sistema nuevo: spawn con rango/TTL/dueño, query «mis marcas de los últimos N s», expiración por TTL (10 s) y consumo por detonación. ⚠️ **Las reacciones elementales NO las tocan** — la regla de "limpieza por reacción" quedó **derogada** el 2026-07-18 ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §5).
4. **Targeting `OwnedMarks`** — «todas mis marcas vivas» como modo de targeting.
5. **Sistema de ataque básico** — cadenas (nº golpes, alcance, ritmo), proc `EveryNthHit`, override por stance. El catálogo solo cubría habilidades de slot.
6. **Drenaje/salida de stance por evento externo** (contacto con enemigo reduce la barra de sigilo).
7. **Escudo/segmento de vida extra con evento `OnBroken`** que puede forzar `ExitStance`.
8. **Reglas de stacking en `StatusData`** (el slow 20 % de Pyrecko es no acumulable).
9. **Reproducir una reacción ya resuelta** — la API nueva es `ReactionSystem.Impact(pos, radius, elemento, fuerzaImpacto, instigador)`, que resuelve `(terreno, entrante)` por tabla. Ottorock necesita **además** poder reproducir una reacción **sellada al agarrar**, en cualquier superficie **incluida la neutra** (única excepción a R5, [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §5). Con roca pura: resuelve normal como Mineral contra la superficie de impacto.

### Huecos añadidos por Mantagale (2026-07-18) — los más caros del proyecto
10. **Locomoción aérea con eje vertical** ⚠️ *el más caro, y el único que hay que prever YA*. Vuelo permanente a altura variable, planeo, tope de altura. Toca `CreatureController`, cámara (T0.4) y lock-on (T0.5). Ver §8.
11. **Eventos del `ReactionSystem`** — la pasiva de altura necesita escuchar «se ha formado una reacción». Hoy `Impact()` no notifica nada. Barato si se prevé; caro si se retrofitea.
12. **Desplazamiento de criaturas** (`Knockback` puntual · `Pull` · **empuje continuo** de los tornados · **arrastre por el suelo** del Ciclón). Afecta al diseño físico del receptor de golpes y a cómo interactúa con el `CharacterController`. Era un punto ciego conocido (§7); ahora es requisito.
13. **Buff que modifica el ataque básico** durante N golpes (AoE + knockback + spawn). Extiende el sistema de básicos [H5] con una capa de override **temporal**, distinta del override **por stance** que piden Ottorock y Pyrecko.
14. **Fuerzas que actúan sobre hazards, no solo sobre criaturas** — el Ciclón atrae a los tornados. Implica que los hazards tengan posición mutable y respondan a fuerzas.
15. **Detonación por colisión de dos efectos** — el tornado (empuje) y el Ciclón (atracción) se destruyen mutuamente y disparan la reacción en el punto de encuentro. Es interacción hazard↔hazard, que hoy no existe.
16. **Arrastre continuo que atraviesa hazards del trayecto** — el enemigo arrastrado debe **colisionar con lo que haya en el camino** (no teletransportarse), disparando cada tornado que cruce.

## 5. Orden de construcción (3 olas)

| Ola | Cuándo | Qué se construye | Bespoke permitido |
|---|---|---|---|
| **1 · Gate** | Fase 1 (T1.4) | **Ottorock-Libre:** `DashMotion`, `MeleeHitbox`, `DealDamage`, `Slow`, cadena de 5 básicos, coste/CD por slot [H1] | Todo puede ser código directo — el objetivo es clavar el *feel*, no la arquitectura |
| **2 · Stances** | Fase 2–3 (post-gate) | **StanceSystem + Sujeción completa:** `GrabChunk`/`ThrowHeld`, **combinación sellada** en blackboard [H2], reproducción de reacción sellada [H9], escudo-roca [H7], básicos por stance [H5], `Stun` — se apoya en `ElementalSurface`/`ReactionSystem` de Fase 2 | Coreografía del agarre |
| **3 · Pyrecko** | Fase 4 | **Reusa** el StanceSystem para Sigilo (+ [H6], invisibilidad) · marcas de especie [H3, H4] · `SpawnProjectile(homing)` · DoT/stacking [H8] · Signature | `IgniteOwnedMarks`, percepción de la IA |
| **4 · Mantagale** | Post-prototipo (Nivel B) | Vuelo y altura [H10, H11] · desplazamiento de criaturas [H12] · override temporal del básico [H13] · fuerzas sobre hazards e interacción hazard↔hazard [H14, H15, H16] | Pasiva de altura, coreografía de vuelo |

**El payoff del bespoke-primero, explícito:** el StanceSystem se extrae de Ottorock y Pyrecko lo consume gratis; las marcas se extraen de Pyrecko y quedan para cualquier especie futura. **Test de validez de la abstracción:** si en la ola 3 el Sigilo no encaja limpio en el StanceSystem extraído en la ola 2, la abstracción estaba mal — mejor descubrirlo con 2 criaturas que con 12.

## 6. Impacto en el scaffold actual

- Ya cubierto por **T1.0** (sync pendiente, ver [TASKS.md](TASKS.md)): slots ×3, TypeChart 1.5x, noqueo contextual, despacho del ataque básico.
- **Nuevo para la ola 1:** `AbilityCaster` → coste/CD por **slot** [H1]. Nada más se toca antes del gate.
- `AbilityCore.cs` ya trae fases + `PhaseExit` + blackboard → buena base para las olas 2–3; **no** añadir stances/marcas hasta su ola (regla anti-abstracción-prematura de ABILITY-SYSTEM).

## 7. Cobertura de ejes con los 3 kits

- **Verticalidad/vuelo** — ✅ **cubierto por Mantagale** [H10]. Sigue siendo lo **más caro de retrofitear**: por eso T0.3/T0.4/T0.5 tienen que preverlo aunque Mantagale no se construya hasta el Nivel B (§8).
- **Desplazar criaturas** (`Knockback`/`Pull`/empuje continuo/arrastre) — ✅ **cubierto por Mantagale** [H12], y es la mitad de su kit. Ya no es un "por si acaso": es requisito de una especie diseñada.
- **Cargar/canalizar** — ✅ barato, `PhaseExit` ya lo soporta.
- **Interacción entre efectos del mundo** (hazard↔hazard) — 🆕 **descubierto por Mantagale** [H14, H15]. Ninguna otra especie lo pedía.
- **Soporte/objetivo aliado + taunt/aggro** — ❌ sin cobertura; irrelevante hasta la ola de aliados ([VITALITY-DEATH.md](VITALITY-DEATH.md) §6). No diseñar aún.
- **Summons/trampas** — ❌ sin señal de que el juego los pida.
- **Arquetipos** — los 3 cubren Zonificador, Luchador y Controlador. **Guardián y Ejecutor sin estresar** (especies 4–5, en papel, sin fecha).

**Regla:** NO diseñar criaturas "para darle cobertura al framework" — invierte la estrategia bespoke-primero. Los 3 kits nacieron del diseño y la cobertura salió de rebote; así debe seguir.

---

## 8. Locomoción y controles — lo que T0.3–T0.5 deben respetar

> Esta sección es el motivo de haber diseñado 3 criaturas antes de programar. **Son las decisiones que hay que tomar ahora**, porque los tickets inmediatos las hornean.

### 8.1 · Locomoción: no existe "la locomoción del jugador"
Las tres se mueven de forma **radicalmente distinta**, y dos de ellas cambian de modo **en pleno combate**:

| Especie | Locomoción |
|---|---|
| **Ottorock** | cuadrúpedo medianamente ágil (Libre) ↔ **bípedo pesado y lento** (Sujeción) — cambia con el stance |
| **Pyrecko** | normal ↔ **muy rápido** (Sigilo) — cambia con el stance |
| **Mantagale** | **vuelo permanente**, altura variable como eje propio, planeo — nunca camina |

➡️ **Decisión:** el `CreatureController` **no** puede tener la locomoción hardcodeada. Necesita un **perfil de locomoción intercambiable** (velocidad, aceleración, giro, gravedad, salto, y si el eje Y es libre), seleccionable por **especie** y sobrescribible por **stance**. Es barato ahora (un `ScriptableObject` de parámetros + una interfaz) y carísimo después.

### 8.2 · El eje vertical hay que preverlo, no construirlo
Mantagale es Nivel B, pero **lo que se hornee en Fase 0 decide si es viable**. Sin construir vuelo:
- **T0.3 (locomoción):** el movimiento y la máquina de estados no deben asumir que el personaje está **siempre pegado al suelo** (nada de dar por hecho `isGrounded` para poder actuar).
- **T0.4 (cámara):** el rig no puede asumir objetivos **a ras de suelo**; debe poder inclinar el pitch y alejarse.
- **T0.5 (lock-on):** el `TargetingSystem` debe contemplar objetivos **elevados** y no asumir un plano horizontal.
- **[H11]:** cuando llegue el `ReactionSystem` (Fase 2), que **emita un evento** al resolver una reacción. Es una línea entonces; un retrofit después.

### 8.3 · Controles: el mapeo aguanta, pero el input debe ser consciente del stance
El mapeo de GDD §6.3 **no cambia** con los 3 kits (buena señal: nada pide un botón nuevo). Pero:
- Ottorock y Pyrecko **reescriben su kit al cambiar de stance**: el mismo botón hace cosas distintas → el despacho de input tiene que resolverse contra el **kit activo**, no contra la `AbilityData` fija.
- Coste y cooldown viven en el **slot**, no en la habilidad [H1] — si no, las variantes por stance comparten mal el CD.
- Mantagale **no pide ningún input nuevo**: la altura es pasiva, los tornados no se controlan. Solo el Ciclón se apunta.
- La **interfaz *brain*** (ya sugerida en T0.3) sigue siendo la puerta para IA aliada y órdenes: una orden es otra fuente de input contra el mismo kit activo.

### 8.4 · Orden recomendado (no cambia el ROADMAP, lo aterriza)
1. **T0.1/T0.2** — setup y arena gris. Sin dependencias.
2. **T0.3** con **perfil de locomoción** desde el minuto uno + interfaz *brain*. Solo se implementa el perfil terrestre.
3. **T0.4/T0.5** con las advertencias de 8.2.
4. **T1.0** (sync del scaffold) + `AbilityCaster` con **coste/CD por slot** [H1]. Nada más antes del gate.
5. El gate se juega con **Ottorock-Libre**: `DashMotion`, `MeleeHitbox`, `DealDamage`, `Slow`, cadena de 5 básicos. **Todo bespoke, sin abstraer nada.**
