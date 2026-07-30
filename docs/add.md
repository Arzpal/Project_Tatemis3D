# Tatemis3D — ADD · Architecture Design Document

> **TL;DR:** El **mapa mental del código**: cómo encajan las piezas y **por qué** están separadas así. Nace de haber diseñado 3 criaturas deliberadamente complejas ([CREATURES.md](CREATURES.md)) antes de programar — cada decisión de aquí la obliga un kit real.
> **📐 Versión visual con los diagramas: [add.html](add.html)** — abrir ahí cuando haga falta *ver* algo. Los diagramas se acumulan en ese archivo en vez de improvisarse cada vez.
> **Reparto con [ARCHITECTURE.md](ARCHITECTURE.md):** ARCHITECTURE es la **especificación** (structs, clases, firmas, qué script hace qué). Este doc es el **mapa y el porqué**. Si buscas código concreto → ARCHITECTURE. Si buscas entender cómo encaja → aquí.
> **Mapa:** §1 las 5 capas · §2 actuador vs payload · §3 las 10 primitivas · §4 locomoción · §5 kit activo · §6 orden de construcción · §7 cámara y apuntado (modelo BotW/TOTK) · §8 reglas vigentes

---

## 1. Las 5 capas del controlador

```
1 · ENTRADA      InputReader ......... teclado · gamepad · rebinding
                        ↓ intención cruda
2 · DECISIÓN     IBrain .............. PlayerBrain · AIBrain · Órdenes
                        ↓ órdenes
3 · CRIATURA     CreatureController .. máquina de estados
                   ├── LocomotionProfile   ← DATO intercambiable
                   ├── Kit activo          ← DATO, lo reescribe StanceSystem
                   ├── AbilityCaster       coste/CD por SLOT
                   └── BasicChain          cadena + procs
                        ↓ efectos, disparados por ANIM-EVENTS
4 · EFECTOS      AbilityEffect[]
                        ↓
5 · MUNDO        ReactionSystem · Hazards · Marcas · Combatientes
```

**Solo dos piezas son intercambiables, y ambas son datos, no código:** el `LocomotionProfile` y el `Kit activo`. Todo lo demás es fijo.

**Las dos costuras que hay que dejar abiertas desde el principio:**
- **`IBrain`** — hoy solo `PlayerBrain`. La IA salvaje, los aliados y las órdenes entran por esta misma interfaz sin tocar el controller ([VITALITY-DEATH](VITALITY-DEATH.md) §6).
- **Anim-events** — los efectos se disparan por el **reloj de la fase**, no en el frame del input, y cuando existan clips ese reloj lo releva un **anim-event** sin tocar los efectos. La costura ya está abierta (el multi-hit dispara por tiempo); falta solo el emisor de animación. Es lo que pone el game feel en manos del diseñador sin recompilar.

---

## 2. Actuador vs payload — la distinción que ordena todo

Casi nada se aplica directamente: **algo tiene que llevarlo**. Los efectos de una fase son de dos naturalezas y **no se conocen entre sí**:

| | Qué hace | Ejemplos |
|---|---|---|
| **Actuador** (entrega) | crea el vehículo y **reporta impactos** con `ctx.reportHit(HitInfo)` | `MeleeHitbox` · `SweepHitbox` · `SpawnProjectile` · `ThrowHeld` · `SpawnHazard` · `ElementalBurst` |
| **Payload** (consecuencia) | declara **cuándo** aplica (`ApplyWhen { OnCast, OnHit }`) | `DealDamage` · `ApplyStatus` · `Knockback` · `RadiateElement` · `DropMark` |
| **Presentación** *(añadida 22/07)* | ni entrega ni causa: **encuadra**. La habilidad pide cámara; la cámara no sabe de habilidades | `CameraShot` |

**El pegamento es `reportHit`**, y el payload **no sabe quién lo entregó**: `DealDamage{when=OnHit}` es idéntico venga de un puñetazo, de un proyectil teledirigido o de una zona pisada. Por eso una habilidad se arma en el inspector como **un actuador + N payloads**, sin escribir código.

> **Corolario que costó una iteración:** un ataque que *dura* no se modela como "daño mientras te toca", sino como un actuador que **reparte golpes** (`SweepHitbox.hitInterval`). Así la consecuencia sigue siendo un `HitInfo` y los payloads de siempre valen — en vez de inventar payloads "sostenidos" que duplican a los normales. Ver [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md).

### 2.1 · Tres tiempos de vida
| Vida | Resuelve | Ejemplos |
|---|---|---|
| **Instantánea** | dentro de la animación | `MeleeHitbox` · `AreaBurst` |
| **En vuelo** | segundos después, al impactar | proyectil de Pyrecko · roca lanzada de Ottorock |
| **Persistente** | ciclo propio en el mundo; aplica al **entrar** o al **permanecer** | zonas ardiendo · tornados · marcas de especie |

⚠️ **Los dos últimos sobreviven a la habilidad que los creó.** Por eso el actuador entrega al vehículo **una copia del payload + un snapshot del contexto** (elemento, lanzador, stats y —para Ottorock— la reacción sellada), nunca una referencia viva al estado del lanzador.

*Los dos casos que lo obligan:* Ottorock **lanza la roca y sale de Sujeción en el mismo gesto** — si el proyectil leyera su estado al impactar, la reacción sellada ya no existiría, pero su ficha dice que el impacto sí la aplica. Y si noquean a Pyrecko después de detonar, **sus zonas siguen ardiendo**.

---

## 3. Las 10 primitivas

Las 9 habilidades de las 3 criaturas se descomponen en **10 primitivas**. Esta lista **es** la librería de efectos:

| # | Primitiva | Tipo | Estado |
|---|---|---|---|
| 1 | Daño | payload | ✅ en scaffold |
| 2 | Estado (quemadura, slow, stun, parálisis) | payload | ✅ parcial (falta stacking) |
| 3 | Movimiento propio (dash, impulso vertical) | actuador sobre uno mismo | ✅ `DashMotion` |
| 4 | Desplazar ajeno (knockback, pull, arrastre, empuje continuo) | payload + actuador | ❌ ola Mantagale |
| 5 | Proyectil (recto, homing, objeto lanzado) | actuador | ❌ olas Pyrecko/Ottorock |
| 6 | Hazard (zona estática o móvil) | actuador persistente | ❌ ola Pyrecko |
| 7 | Marca de especie | payload persistente | ❌ ola Pyrecko |
| 8 | Elemento irradiado → reacción / reacción sellada | payload | ✅ `RadiateElement` (2026-07-29) · ⚠️ la **tabla** es anterior al rediseño → reautorar |
| 9 | Buff temporal (incl. reescribir el básico) | modificador propio | ❌ ola Mantagale |
| 10 | Stance (override de kit y locomoción) | sistema | ❌ ola Ottorock |

> **Señal buena:** tres criaturas diseñadas *a propósito* para romper el sistema no piden nada fuera de esta lista. El catálogo no se va a disparar con las especies 4 y 5.

> ⚠️ **Lo que el rediseño elemental del 2026-07-29 le añade a la primitiva 8:** las mezclas ya **no necesitan suelo** — dos entregas elementales que se cruzan **en el aire** resuelven ahí mismo y sin bando ([ELEMENTAL-REACTIONS §0.2b](ELEMENTAL-REACTIONS.md)). O sea que `ReactionSystem.Impact(pos, …)` **deja de ser la única puerta**: hace falta un camino **entrega ↔ entrega**, que es el mismo `hazard↔hazard` que la ola 4 tenía apuntado para Mantagale (§6) — solo que ahora lo pide el **sistema base**, no una criatura. Es la costura que conviene no cerrar al reautorar la tabla.

---

## 4. Locomoción: no existe "la locomoción del jugador"

Las tres se mueven de forma radicalmente distinta, y **dos cambian de modo en pleno combate**:

| Especie | Locomoción |
|---|---|
| **Ottorock** | cuadrúpedo ágil (Libre) ↔ **bípedo pesado** (Sujeción) |
| **Pyrecko** | normal ↔ **muy rápido** (Sigilo) |
| **Mantagale** | **vuelo permanente**, altura variable como eje propio — nunca camina |

➡️ El `CreatureController` **no puede** llevar la locomoción hardcodeada: necesita un **perfil intercambiable** (SO de parámetros: velocidad, aceleración, giro, gravedad, salto, eje Y libre) por **especie** y sobrescribible por **stance**. Barato ahora, carísimo después.

**El eje vertical hay que preverlo, no construirlo.** Mantagale es Nivel B, pero lo que se hornee en Fase 0 decide si es viable: T0.3 no debe asumir que el personaje está siempre pegado al suelo · T0.4 no debe asumir objetivos a ras de suelo · T0.5 debe contemplar objetivos elevados · y cuando llegue el `ReactionSystem`, que **emita un evento** al resolver una reacción (la pasiva de altura lo necesita).

---

## 5. Kit activo: el input se resuelve contra el stance

El mapeo de mando (GDD §6.3) **aguanta los 3 kits sin pedir un botón nuevo** — buena señal. Pero:

- Ottorock y Pyrecko **reescriben su kit al cambiar de stance**: el mismo botón hace cosas distintas → el despacho de input debe resolverse contra el **kit activo**, no contra una `AbilityData` fija.
- **Coste y cooldown viven en el SLOT**, no en la habilidad — si no, las dos embestidas de Ottorock no comparten CD como deben.
- El ataque básico admite **dos capas de override**: por stance (Ottorock, Pyrecko) y **temporal por buff** (Mantagale).

---

## 6. Orden de construcción

| Ola | Cuándo | Qué se extrae | Bespoke permitido |
|---|---|---|---|
| **1 · Gate** | Fase 1 | Ottorock-Libre: dash, hitbox, daño, slow, cadena de básicos, **coste/CD por slot** | **todo** — el objetivo es el *feel*, no la arquitectura |
| **2 · Stances** | Fase 2–3 | `StanceSystem`, `GrabChunk`/`ThrowHeld`, reacción sellada, escudo-roca, básicos por stance | coreografía del agarre |
| **3 · Pyrecko** | Fase 4 | reusa StanceSystem para Sigilo · marcas de especie · proyectil homing · DoT | `IgniteOwnedMarks`, percepción IA |
| **4 · Mantagale** | Nivel B | vuelo y altura · desplazamiento · override temporal del básico · hazard↔hazard | pasiva de altura |

**El payoff del bespoke-primero:** el `StanceSystem` se extrae de Ottorock y Pyrecko lo consume gratis. **Test de validez:** si en la ola 3 el Sigilo no encaja limpio en la abstracción de la ola 2, la abstracción estaba mal — mejor descubrirlo con 2 criaturas que con 12.

---

## 7. Cámara y apuntado — el modelo BotW/TOTK

> Referencia elegida para nuestra cámara. Spec de proyecto, estado y **parámetros a medir** → [CAMERA-TARGETING.md](CAMERA-TARGETING.md).

**La idea que lo ordena todo:** el lock **no es un modo de puntería, es un modo de relación con un enemigo**. Al activarlo no cambia *dónde apuntas*, cambia **a qué están anclados tu cuerpo y tu cámara**. Por eso se nota — y por eso una cámara al hombro, que solo cambia el encuadre, se sentía **igual** en ambos modos (el intento descartado).

### 7.1 · Los tres estados de cámara

Los tres **los elige el jugador**, y comparten un **invariante**: *el ataque va siempre hacia donde encara el cuerpo* — lo único que cambia es **quién decide ese encarado**.

| Estado | Entrada | El cuerpo encara… | Sirve para |
|---|---|---|---|
| **Libre** | por defecto | tu **movimiento** | desplazarte y pelear de forma física, sin retícula |
| **Lock** | toggle | el **enemigo** | comprometerte con un **rival** (strafe, rodearlo) |
| **Apuntado** | mantener | la **cámara** | comprometerte con un **punto del mundo** |

**Libre** — cámara por detrás, amortiguada, que **se recoloca sola** detrás del avance de forma perezosa. El personaje **encara su movimiento**: el stick *es* la mira. Para que se sienta bien **sin retícula**, al golpear **se orienta hacia un enemigo cercano** dentro de un cono y **se abalanza** un poco (el step-in de cada golpe).

**Lock** — encuadra a **los dos** sobre el eje jugador→enemigo y se aleja al separarse; órbita limitada que **vuelve al encuadre** al soltar. El personaje **encara siempre al enemigo**, así que moverte de lado lo **rodea**. Adquiere el **más relevante en pantalla**, cambia de objetivo con **flick**, y **sin enemigo el botón recentra la cámara**.

**Apuntado** — cámara **al hombro**, **retícula** al centro, el cuerpo encara la cámara y **movilidad reducida** como precio; dentro puedes usar básico y cualquier habilidad. Es un **estado** y no "apuntar por habilidad": si unas apuntaran y otras no volvería la ambigüedad de *"¿este botón apunta?"*, mientras que así la regla es una — **si mantienes apuntado, todo apunta**.

➡️ Rasgo a rasgo de la referencia → **[CAMERA-TARGETING.md](CAMERA-TARGETING.md) §2** · diagrama → **[add.html](add.html)**.

### 7.2 · Por qué hacen falta los tres

Que **lock y apuntado** coexistan no es duplicación: son **dos compromisos distintos** — uno con un **rival**, otro con un **punto del mundo**. En un juego cuyo eje es *habilidad + elemento + **ambiente***, el tablero importa tanto como el enemigo, así que hay que poder fijar **cualquiera de los dos**.

El tercero además nos lo pedía el roster: Zelda resuelve el apuntado fino con un **modo dedicado** (el arco), y nosotros llegamos a la misma necesidad por **Pyrecko** (proyectiles) y **Mantagale** (colocar el Ciclón) — pero lo **generalizamos a un estado** en vez de atarlo a una habilidad. *Descartada la cámara lenta al apuntar: difícil de sostener en online y ningún monstruo la aprovecha.*

> **🔑 Principio: la libertad genera ingenio.** Apuntando, la **Movilidad se lanza en la dirección de la mira, también en vertical**. No es un detalle técnico: las habilidades **dejan de estar atadas al plano del suelo** y las soluciones las inventa el jugador — que la forma de que un Ottorock alcance a una Mantagale sea **impulsarse al aire apuntando hacia arriba** es una jugada que nadie diseñó. Ese es el segundo USP de Tatemis.

⚠️ **El riesgo a vigilar:** que se viva en apuntado (como el ADS de los hero shooters) y los otros dos queden vestigiales. **La lentitud tiene que morder**, y el premio debe ser *solo precisión* — nunca más daño ni más alcance.

### 7.3 · Dónde estamos respecto a la referencia

**Ya equivalente:** encuadre de ambos y distancia adaptativa · adquisición del más relevante **en pantalla** · **cambio de objetivo** por flick · botón **sin enemigo → recentra** · ruptura por distancia/caída/**línea de visión** · trail-behind perezoso · **muelle** de posición y giro **con aceleración**.

**Diferencias vivas:** el lock es **toggle** y no *mantener* (decisión pendiente) · la oclusión solo hace *pull-in*, sin **fade** de lo que tapa · el **multi-enemigo** está construido pero **sin validar en playtest**.

> Tabla completa y diagrama → **[add.html](add.html)** · estado del código y parámetros a medir → **[CAMERA-TARGETING.md](CAMERA-TARGETING.md)**.

---

## 8. Reglas vigentes

1. **Bespoke primero, extraer después.** No se generaliza nada hasta tener ≥2 usos reales ([ABILITY-SYSTEM](ABILITY-SYSTEM.md)).
2. **Datos, no código.** Añadir una criatura o una reacción = editar assets. Si obliga a tocar C#, es un hueco del catálogo → anotarlo en [ABILITY-EXTRACTION](ABILITY-EXTRACTION.md) §4.
3. **Los efectos se disparan por el TIEMPO de la fase, no en el frame del input** — y el día que haya animaciones, ese reloj lo sustituye un **anim-event** sin tocar los efectos. *(Decisión 2026-07-22: en caja gris no hay clips que emitan eventos, así que el reloj de `AbilityPhase`/`SweepHitbox.hitInterval` **es** el mecanismo — no un placeholder pendiente. El multi-hit se diseñó ya para que migrar sea cambiar quién dispara el golpe, no cómo. Ver [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md).)*
4. **El payload viaja desacoplado** del lanzador (§2.1).
5. **Ante duda entre docs y código, mandan los docs.** El scaffold arrastra deuda conocida → ticket T1.0.
6. **Diagrama nuevo → va a [add.html](add.html)**, no al chat. Si hace falta explicar algo visualmente y no está ahí, se añade.
