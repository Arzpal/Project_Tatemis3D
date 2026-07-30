# Tatemis3D — Extracción del sistema: los 3 kits (modular vs específico)

> **TL;DR:** Análisis de los **3 kits reales** ([CREATURES.md](CREATURES.md)) contra el catálogo de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md): qué piezas son **modulares** (librería), cuáles **parametrizables**, cuáles **bespoke**, y los **18 huecos** que el catálogo no cubría. El catálogo salió bien parado (~90% cubierto; los huecos son casi todos infraestructura, no efectos) y su §6 acertó de pleno: Pyrecko y Ottorock son criaturas de stances. **Mantagale (actualizado 2026-07-18 con su kit final) añade los 7 huecos más caros: vuelo, desplazamiento de criaturas y fuerzas que actúan sobre objetos del mundo.** El **modelo elemental del 2026-07-29** añade otros 2 y **generaliza tres de los de Mantagale** (H12/H14/H15 dejan de ser suyos: los pide la capa base). Termina con el orden en 4 olas y las decisiones de locomoción/controles que los 3 kits obligan a tomar **antes de T0.3**.
> **Mapa:** §1 método · §2 desglose por habilidad · §3 veredicto · §4 los 18 huecos · §5 orden en 4 olas · §6 impacto en el scaffold · §7 cobertura de ejes · **§8 locomoción y controles (lo que T0.3–T0.5 deben respetar)**

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
| **Cambio de estado** | `ToggleStance(Libre↔Sujeción)` [S]. Al entrar: `GrabChunk` (ya estaba en el catálogo §3.7) [M] → resuelve **una vez** `Mineral + elemento del terreno` y guarda la **combinación sellada** en el blackboard [H2] + la reproduce en cada impacto [H9] + roca = **escudo con evento OnBroken → ExitStance** [H7] · stats y **kit override** por stance (previstos en StanceData) [M] · silueta bípedo/cuadrúpedo = contenido por stance. Coreografía de agarrar/romper [B]. ⚠️ *(2026-07-29)* **Solo se sella cruzando triángulos**: agarrar sobre **Rayo** o **Aire** dispara la **reacción** ahí mismo (sobre Aire, además **sin bando y contra él**) y el trozo queda **puro** → el `GrabChunk` tiene **dos salidas distintas**, no una. |
| **Embestidas (2 variantes)** | Mismo esqueleto: `DashMotion` + `MeleeHitbox` durante el dash [M]; payload por variante (Sujeción: daño 3 + `Stun` 6 + **reproducir la reacción sellada** / Libre: daño 5 + `Slow` 2) [M]. **CD y energía compartidos entre variantes → coste a nivel de SLOT** [H1]. |
| **Impacto a distancia / Mordisco** | Sujeción: `ThrowHeld` (ya estaba en el catálogo §3.7) [M] con payload daño 5 + `Stun` 2 + **reproducir la combinación sellada** [H9] + side-effect `ExitStance` (al salir se **pierde la combinación**) [M]. Libre: `MeleeHitbox` + daño 7 [M] + **`ElementalBurst` que estalla SIEMPRE** *(2026-07-29: conecte o no — el estallido lo dispara la fase, no el `ReportHit`; ver [ABILITY-SYSTEM §3.3b](ABILITY-SYSTEM.md))* [M]. |
| **Básicos por stance** | Cadenas distintas (Sujeción: 3, medio, muy lento, **reproduce la combinación sellada** por golpe / Libre: 5, corto, medio, **el 5º estalla siempre en Mineral**) → básicos con **override por stance** [S, H5]. ⚠️ *(2026-07-29)* La cadena de 3 en Sujeción **exige** que el **periodo de gracia** de [§0.4](ELEMENTAL-REACTIONS.md) deje **refrescar la MISMA reacción**: si no, los golpes 2 y 3 caen sobre su propia zona en gracia y no hacen nada — y su mecánica insignia queda decorativa. |
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

> ⚠️ **Tres de sus huecos dejaron de ser suyos el 2026-07-29.** El modelo elemental convirtió en **capa base** lo que aquí se leía como coste exclusivo de Mantagale: el **empuje/arrastre continuo** [H12] lo pide ahora la **corriente**; el **hazard con posición mutable** [H14], la **colada** (la única zona que avanza); y la **detonación por colisión** [H15] es literalmente la regla **§0.2b** — *dos entregas elementales que se tocan resuelven ahí*, sin enumerar casos, más el **velo de esporas**, que detona el proyectil que lo cruza. **Consecuencia de planificación:** su precio ya no se puede aplazar entero al Nivel B; parte cae con las combinaciones (ola 2–3). *No cambia el orden de olas, cambia quién paga.*

## 3. Veredicto

- **Núcleo modular confirmado (lo piden AMBOS):** `SpawnProjectile`+payload · `DealDamage` · `StatusData` (Quemadura/Slow/Stun) · `DashMotion`+`MeleeHitbox` · `RadiateElement` *(nombre real; la API es `ReactionSystem.Impact` — ✅ existe desde el 2026-07-29)* · `SpawnHazard` · **StanceSystem** — la pieza compartida más valiosa: Sigilo y Sujeción son el mismo esqueleto (stats + pasiva + kit/básicos override + condiciones de salida), y ABILITY-SYSTEM §6 ya tenía "Invisible" y "AgarrandoObjeto" en su lista de ejemplos · sistema de básicos con procs y override por stance · targeting `Self`/`LockedTarget`/`AimDirection`.
- **Parametrizable (1 uso hoy, genérico mañana):** **combinación sellada** en blackboard · escudo con `OnBroken` · drenaje de stance por evento · interrupción del bloqueo según stance · invisibilidad como pasiva de stance.
- **Bespoke (no generalizar):** qué "ve" la IA frente al sigilo (percepción) · `IgniteOwnedMarks` (semántica exacta del Signature de Pyrecko) · coreografía/anims del agarre de roca · condiciones de evolución (sistema futuro, GDD §9.2).
- **Sistemas aparte (no son efectos):** StanceSystem · marcas de especie · ataque básico · reacciones (diseñado en [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) — ✅ **base implementada el 2026-07-29**: `ReactionTable` + `ReactionSystem` + `ReactionEffect`) · **la anatomía de 3 tramos** del ataque ([ABILITY-SYSTEM §2.1](ABILITY-SYSTEM.md)), que es transversal a todo el kit y no un efecto de librería.

## 4. Los 18 huecos (lo que el catálogo no cubría)

### Los 9 originales (Pyrecko + Ottorock)

1. **Coste/CD a nivel de slot, no de `AbilityData`** — las variantes por stance comparten CD y energía. Hoy `AbilityCaster` tiene el cooldown keyed por `AbilityData` (`_readyAt`) → cambiar a por-slot.
2. **Blackboard a nivel de criatura/stance** — `AbilityCtx.bb` ya existe en `AbilityCore.cs`, pero muere con el cast; el trozo agarrado (y su elemento) debe sobrevivir **entre** habilidades → el blackboard vive en el stance.
3. **Marcas de especie** — sistema nuevo: spawn con rango/TTL/dueño, query «mis marcas de los últimos N s», expiración por TTL (10 s) y consumo por detonación. ⚠️ **Las reacciones elementales NO las tocan** — la regla de "limpieza por reacción" quedó **derogada** el 2026-07-18 ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §5).
4. **Targeting `OwnedMarks`** — «todas mis marcas vivas» como modo de targeting.
5. **Sistema de ataque básico** — cadenas (nº golpes, alcance, ritmo), proc `EveryNthHit`, override por stance. El catálogo solo cubría habilidades de slot.
6. **Drenaje/salida de stance por evento externo** (contacto con enemigo reduce la barra de sigilo).
7. **Escudo/segmento de vida extra con evento `OnBroken`** que puede forzar `ExitStance`.
8. **Reglas de stacking en `StatusData`** (el slow 20 % de Pyrecko es no acumulable).
9. **Reproducir una reacción ya resuelta** — ✅ *(reconciliado el **2026-07-29**: la versión vieja de este hueco asumía **elemento portado**; lo dictado es **roca sellada**, ver [CREATURE-OTTOROCK](CREATURE-OTTOROCK.md) «Resuelto», 2026-07-18)*. Son **dos caminos distintos**, no uno con un parámetro:
   - **Roca pura → camino normal.** `ReactionSystem.Impact(pos, radius, Mineral, fuerzaImpacto, instigador)` resuelve `(terreno, entrante)` por tabla y sale **una de las cinco salidas** ([§0.2](ELEMENTAL-REACTIONS.md)). El trozo **no transporta nada**: es Mineral y reacciona contra el sitio donde caiga.
   - **Roca sellada → reproducción literal.** Reproduce **la combinación que se selló al agarrar**, **saltándose la tabla y R5**, en cualquier superficie **incluida la neutra** (única excepción a R5, [§5](ELEMENTAL-REACTIONS.md) y §0.8). ⚠️ **Y NO vuelve a resolver contra el suelo donde impacta** — si lo hiciera, un mismo golpe produciría dos efectos y encadenaría, contra R1.
   - **Solo se sella cruzando triángulos.** Lo sellable son las **3 combinaciones de Mineral** — **arenas movedizas** (Agua), **colada** (Fuego), **cristalización** (Naturaleza) —, que son **el menú del jugador**. Con **Rayo** o **Aire** hay reacción, se **consume en el agarre** y el trozo queda **puro** *(y con Aire, además, la tormenta le cae a él **sin bando**)*.
   - **Cada combinación sellada lleva sus propios radio y duración**, y **muy distintos** entre sí (cristalización pequeña y larga · colada estrecha y direccional · arenas amplia y corta): si son parecidos el jugador elegirá siempre la misma y la mecánica insignia es decorativa. Es decir, lo sellado **no es un enum, es un payload** — incluida la `direction` de la colada.
   - **Depende del periodo de gracia (§0.4):** repetir la **misma** reacción tiene que **refrescarla**, o la cadena de 3 básicos de Sujeción se apaga sola a partir del golpe 2.
   - 🟡 **Sin resolver — el bando.** [§0.7](ELEMENTAL-REACTIONS.md) dice que **las combinaciones no son de nadie y te afectan a ti también**; §5 dice que Ottorock reproduce la suya **en cada impacto**, muchas veces y a corta distancia. Literalmente, su mecánica insignia le castiga a él cada vez que la usa. Lo que hoy le protege —el `forwardOffset` del estallido y el delay— puede bastar, o puede que la roca sellada necesite ser **la excepción**. **No lo resuelvo: decide Arzpal.**
   - 🟡 **Cristalización aparcada** hasta que exista el cancel ([ABILITY-SYSTEM §2.1](ABILITY-SYSTEM.md) / H17): sin él, una de las tres opciones del menú no hace nada.

### Huecos añadidos por Mantagale (2026-07-18) — los más caros del proyecto
10. **Locomoción aérea con eje vertical** ⚠️ *el más caro, y el único que hay que prever YA*. Vuelo permanente a altura variable, planeo, tope de altura. Toca `CreatureController`, cámara (T0.4) y lock-on (T0.5). Ver §8.
11. **Eventos del `ReactionSystem`** — la pasiva de altura necesita escuchar «se ha formado una reacción». Hoy `Impact()` no notifica nada. Barato si se prevé; caro si se retrofitea. ⚠️ *(2026-07-29: el `ReactionSystem` **ya existe** y **sigue sin emitir nada** — el hueco no se cerró solo, ahora hay dónde meterlo.)*
12. **Desplazamiento de criaturas** (`Knockback` puntual · `Pull` · **empuje continuo** de los tornados · **arrastre por el suelo** del Ciclón). Afecta al diseño físico del receptor de golpes y a cómo interactúa con el `CharacterController`. Era un punto ciego conocido (§7); ahora es requisito. ⚠️ *Ojo (2026-07-22): que la embestida de Ottorock distinga arrollar de rozar **no** cubre esto — se resolvió repartiendo **golpes** (multi-hit, [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md)), y un tornado empuja sin golpear a cadencia. El empuje continuo sigue entero por hacer.* ⚠️ **Y ya no es solo suyo (2026-07-29):** la **corriente** (Rayo + Agua) es *"el único arrastre"* del sistema, el **géiser** eleva desde una zona y la **amarra** da tirones periódicos hacia una estaca. Desplazar criaturas pasa a ser requisito **del modelo elemental**, no de una especie post-prototipo.
13. **Buff que modifica el ataque básico** durante N golpes (AoE + knockback + spawn). Extiende el sistema de básicos [H5] con una capa de override **temporal**, distinta del override **por stance** que piden Ottorock y Pyrecko.
14. **Fuerzas que actúan sobre hazards, no solo sobre criaturas** — el Ciclón atrae a los tornados. Implica que los hazards tengan posición mutable y respondan a fuerzas. ⚠️ **Generalizado el 2026-07-29:** la **colada** (Mineral + Fuego) es *"la única zona que AVANZA"* — nace en el impacto y repta en la **dirección del golpe** hasta solidificar, **y muere al llegar a una celda de Agua** —, y el **velo de esporas** es una zona que **cuelga en el aire**. O sea: la posición del hazard deja de ser fija **por diseño de la capa base**, no por una especie. Palancas concretas en [ABILITY-SYSTEM §3.7](ABILITY-SYSTEM.md).
15. **Detonación por colisión de dos efectos** — el tornado (empuje) y el Ciclón (atracción) se destruyen mutuamente y disparan la reacción en el punto de encuentro. Es interacción hazard↔hazard, que hoy no existe. ⚠️ **Ya no es un caso particular de Mantagale (2026-07-29):** [§0.2b](ELEMENTAL-REACTIONS.md) lo eleva a **regla general** — *dos entregas elementales que se tocan resuelven ahí, en el aire, sin terreno y sin bando* (mismo elemento → **amplificación**), y el **velo de esporas** funciona como un elemento estacionario que detona el proyectil que lo cruza. **Sube de prioridad**: es lo que le da uso **defensivo** a los ataques a distancia, así que hace falta en cuanto haya dos criaturas elementales enfrentadas, no en el Nivel B.
16. **Arrastre continuo que atraviesa hazards del trayecto** — el enemigo arrastrado debe **colisionar con lo que haya en el camino** (no teletransportarse), disparando cada tornado que cruce. ⚠️ *(2026-07-29)* Lo mismo pide la **corriente** (Rayo + Agua) y, en general, el verbo de Agua: **al salir, la inercia sigue llevándote** — el arrastre no se corta en el borde de la zona.

### Huecos añadidos por el modelo elemental (2026-07-29)

17. **Cancelar la recuperación de una habilidad con otra** — el tramo 3 de la anatomía universal ([ABILITY-SYSTEM §2.1](ABILITY-SYSTEM.md)). Hoy **no existe**: `AbilityCaster.CanCast` rechaza cualquier cast mientras `_active != null`. No es cosmético — es **el sistema de combos que alimenta la captura** (encadenar más seguido sube el combo y con él el aturdimiento), y sin él la **cristalización** (una de las 3 del menú de Ottorock) **no tiene función**. Toca `AbilityCaster` y el despacho de input, y encaja mejor en la **ola 2** que después.
18. **Efectos pegados con modelo de carga, y techo de 2** — [§0.3](ELEMENTAL-REACTIONS.md) y [§0.9](ELEMENTAL-REACTIONS.md): un efecto de zona **dura eternamente dentro y decrece fuera**, se **recarga al reentrar**, y **cuanto más tiempo estuviste dentro más te dura fuera**. `StatusData` hoy solo sabe de duración fija, y no hay tope global de efectos simultáneos. Es infraestructura, no un efecto: lo consumen **las 15 parejas a la vez**. Se solapa con las reglas de stacking de [H8].

## 5. Orden de construcción (4 olas)

> ✅ **Ratificado el 2026-07-29:** **Ottorock COMPLETO (con Sujeción) entra en el vertical slice**, y va **antes** que Pyrecko y que Mantagale. No cambia la tabla —ya era ese el orden—, pero deja de ser "post-gate, cuando toque": la ola 2 es **parte del slice**, no un extra.

| Ola | Cuándo | Qué se construye | Bespoke permitido |
|---|---|---|---|
| **1 · Gate** | Fase 1 (T1.4) | **Ottorock-Libre:** `DashMotion`, `MeleeHitbox`, `DealDamage`, `Slow`, cadena de 5 básicos, coste/CD por slot [H1] | Todo puede ser código directo — el objetivo es clavar el *feel*, no la arquitectura |
| **2 · Stances** | Fase 2–3 (**parte del slice**) | **StanceSystem + Sujeción completa:** `GrabChunk`/`ThrowHeld`, **combinación sellada** en blackboard [H2], **reproducción de la combinación sellada** [H9] con sus 3 payloads de radio/duración distintos, escudo-roca [H7], básicos por stance [H5], `Stun` — se apoya en `ElementalSurface`/`ReactionSystem`/`ReactionTable` de Fase 2 (✅ ya existen). **+ cancel de recuperación [H17]**, sin el cual la cristalización no hace nada | Coreografía del agarre |
| **3 · Pyrecko** | Fase 4 | **Reusa** el StanceSystem para Sigilo (+ [H6], invisibilidad) · marcas de especie [H3, H4] · `SpawnProjectile(homing)` · DoT/stacking [H8] · Signature | `IgniteOwnedMarks`, percepción de la IA |
| **4 · Mantagale** | Post-prototipo (Nivel B) | Vuelo y altura [H10, H11] · desplazamiento de criaturas [H12] · override temporal del básico [H13] · fuerzas sobre hazards e interacción hazard↔hazard [H14, H15, H16] | Pasiva de altura, coreografía de vuelo |

**El payoff del bespoke-primero, explícito:** el StanceSystem se extrae de Ottorock y Pyrecko lo consume gratis; las marcas se extraen de Pyrecko y quedan para cualquier especie futura. **Test de validez de la abstracción:** si en la ola 3 el Sigilo no encaja limpio en el StanceSystem extraído en la ola 2, la abstracción estaba mal — mejor descubrirlo con 2 criaturas que con 12.

## 6. Impacto en el scaffold actual

- Ya cubierto por **T1.0** (sync pendiente, ver [TASKS.md](TASKS.md)): slots ×3, TypeChart 1.5x, noqueo contextual, despacho del ataque básico.
- **Nuevo para la ola 1:** `AbilityCaster` → coste/CD por **slot** [H1]. Nada más se toca antes del gate.
- `AbilityCore.cs` ya trae fases + `PhaseExit` + blackboard → buena base para las olas 2–3; **no** añadir stances/marcas hasta su ola (regla anti-abstracción-prematura de ABILITY-SYSTEM).

## 7. Cobertura de ejes con los 3 kits

- **Verticalidad/vuelo** — ✅ **cubierto por Mantagale** [H10]. Sigue siendo lo **más caro de retrofitear**: por eso T0.3/T0.4/T0.5 tienen que preverlo aunque Mantagale no se construya hasta el Nivel B (§8).
- **Desplazar criaturas** (`Knockback`/`Pull`/empuje continuo/arrastre) — ✅ **cubierto por Mantagale** [H12], y es la mitad de su kit. Ya no es un "por si acaso": es requisito de una especie diseñada. ⚠️ *(2026-07-29)* **Y también de la capa base**: corriente, géiser y amarra lo piden sin Mantagale de por medio.
- **Cargar/canalizar** — ✅ barato, `PhaseExit` ya lo soporta.
- **Encadenar y cortar acciones** (los 3 tramos del ataque) — 🆕 **dictado el 2026-07-29** [H17], y es el primer eje que **no sale de ninguna criatura**: sale de la capa base y las toca a todas. El andamiaje está a medias (`interruptible`/`superArmor` sí, cancel no).
- **Interacción entre efectos del mundo** (hazard↔hazard) — ~~🆕 descubierto por Mantagale; ninguna otra especie lo pedía~~ → **derogado el 2026-07-29**: [§0.2b](ELEMENTAL-REACTIONS.md) lo convierte en **regla general del sistema** (*dos entregas elementales que se tocan resuelven ahí*) y el **velo de esporas** lo usa fuera de Mantagale. Ya no es un eje que cubre una especie: es capa base [H14, H15].
- **Soporte/objetivo aliado + taunt/aggro** — ❌ sin cobertura; irrelevante hasta la ola de aliados ([VITALITY-DEATH.md](VITALITY-DEATH.md) §6). No diseñar aún.
- **Summons/trampas** — ❌ sin señal de que el juego los pida.
- **Arquetipos** — los 3 cubren Zonificador, Luchador y Controlador. **Coloso y Ejecutor sin estresar** (especies 4–5, en papel, sin fecha).

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

✅ **Implementado el 2026-07-22** — `Data/LocomotionProfile.cs` (SO) + `EvolutionForm.locomotion`. Modelo elegido (ver 8.1b): **andares por deflexión del stick**, no inercia.

### 8.1b · Andares, no inercia — y por qué NO hay botón de correr 🔒 *(2026-07-22)*

Se planteó añadir **sprint** y se **descartó**. Razones:
- **Correr ya es identidad de especie, no acción universal:** Pyrecko va "muy rápido" en Sigilo, Mantagale vuela, Ottorock cambia de peso con el stance. Un botón genérico sería la respuesta enlatada a una pregunta que las fichas ya responden mejor.
- **No cabe:** §8.3 celebra que los 3 kits no piden ningún botón nuevo, y el presupuesto de inputs fue **la razón** de recortar el kit a 3 (GDD §6.3).

**El modelo — dos capas que NO hay que mezclar:**
- **Velocidad: lineal con la deflexión** del stick. Moverlo un poco más siempre te mueve un poco más rápido, sin escalones. `moveSpeed` de la forma = velocidad con el stick a fondo.
- **Andares: umbrales puramente VISUALES** — a qué deflexión cambia la animación, sin tocar la velocidad. Ottorock-Libre: `Paso corto (0.1) → Caminar (0.5) → Deslizamiento (0.75)`. Entre 0.75 y 1 te deslizas cada vez más rápido, pero **se ve igual**.

Bloquear no clava: recorta la deflexión a **paso corto** (`blockMaxInput`), usando todo el recorrido del stick en vez de convertirlo en un interruptor.

> 🔒 **La regla que lo hace funcionar: el deslizamiento es un ANDAR, no física.** Parece que patina, pero responde 1:1 — soltar el stick para en seco. *Se evaluó inercia real y se descartó: en un juego con lock-on y strafe, un cuerpo que resbala al esquivar arruina la precisión.* La única inercia del juego vive **dentro de habilidades concretas** (`DashMotion.easeOut`, el deslizamiento final de la embestida), donde el jugador ya ha comprometido la acción.

**Stances:** el mismo recorrido de stick, otros umbrales y otro tope. Ottorock-Sujeción no tiene umbral de deslizamiento (con la roca en brazos no patina) y su `maxSpeedMultiplier` es **0.75**: a fondo llega a lo que Libre da al 0.75 del stick. El asset ya se genera como referencia, aunque el stance sea post-gate.

**Teclado:** WASD es digital (0 o 1), así que la finura se pierde; lo único que queda es una **rampa corta** (`inputRamp`) que suaviza el salto. Con stick es imperceptible porque la deflexión ya es analógica. **Es el argumento técnico de por qué el mando es el control estrella**, y conviene decirlo en el pitch en vez de disimularlo.

**El caso que sí pide correr:** la **persecución del Tamer** ([VITALITY-DEATH §5](VITALITY-DEATH.md)) — ahí no hay monstruo ni kit, y escapar *es* la mecánica. Es locomoción **del Tamer**, otro perfil, post-gate.

### 8.2 · El eje vertical hay que preverlo, no construirlo
Mantagale es Nivel B, pero **lo que se hornee en Fase 0 decide si es viable**. Sin construir vuelo:
- **T0.3 (locomoción):** el movimiento y la máquina de estados no deben asumir que el personaje está **siempre pegado al suelo** (nada de dar por hecho `isGrounded` para poder actuar).
- **T0.4 (cámara):** el rig no puede asumir objetivos **a ras de suelo**; debe poder inclinar el pitch y alejarse.
- **T0.5 (lock-on):** el `TargetingSystem` debe contemplar objetivos **elevados** y no asumir un plano horizontal. ➕ *(2026-07-29)* **Y poder bloquear la ADQUISICIÓN sin romper el guiado**: la **niebla** (Aire + Agua) impide *fijar* a través de ella, no perder el lock ya fijado. Sale casi gratis porque `requireLineOfSight` / `sightGrace` ya separan las dos cosas.
- **[H11]:** cuando llegue el `ReactionSystem` (Fase 2), que **emita un evento** al resolver una reacción. Es una línea entonces; un retrofit después. ⚠️ *(2026-07-29: **llegó y no lo trae** — sigue siendo una línea, pero ya hay que ir a ponerla.)*

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
