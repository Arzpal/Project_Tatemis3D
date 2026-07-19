# Tatemis3D — Backlog Fase 0–1 (Prototipo de feel)

> Tickets repartibles para arrancar el Nivel A. Ver el plan completo en [ROADMAP.md](ROADMAP.md) y la base técnica en [ARCHITECTURE.md](ARCHITECTURE.md).
> **Roles:** **PA** = Arzpal (prog + diseño) · **PB** = 2º programador · **DS** = diseñador. Estimaciones en **horas de trabajo** (part-time).

## Estado
> **Mantener al día al cerrar cada sesión** — es lo primero que se lee para retomar contexto. Este doc solo contiene la fase en curso: al completar una fase, mover sus tickets a `docs/archive/TASKS-fase<N>.md` y dejar aquí una línea-resumen.

> **🔄 Para retomar (última sesión: 2026-07-18):** leer este bloque + las **dos tablas de decisiones** + la **cola de ratificación**. Para profundizar: [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) (**el sistema elemental, reescrito y cerrado**), [CREATURES.md](CREATURES.md) (índice de las 3 fichas), [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md), [VITALITY-DEATH.md](VITALITY-DEATH.md), [ROADMAP.md](ROADMAP.md) §"Recalibración de alcance".
> **Qué se hizo el 2026-07-17:** se dictaron las **3 criaturas del slice** (Pyrecko, Ottorock, Mantagale), se decidió el **gate** (Ottorock-Libre) y **voladores: sí**, análisis de modularidad de los kits, principio **"toda especie es autosuficiente"**, y una primera tabla elemental de 15 parejas.
> **Qué se hizo el 2026-07-18:** un **stress-test de 46 agentes** sobre esa tabla encontró que su mayor problema era que las reacciones creaban estados nuevos que se encadenaban sin reglas. Arzpal lo resolvió **cambiando el modelo de base**: el terreno es fijo y las reacciones son efectos temporales encima. `ELEMENTAL-REACTIONS.md` se **reescribió entero** con ese modelo y el sistema quedó ✅ **cerrado**; se propagó a GDD §8.1, ARCHITECTURE §5, las 3 fichas y CREATURES §3. **Todo es diseño en docs; no se tocó código.**
> **Y después (misma sesión):** se **rediseñó el kit de Mantagale** entero (el anterior no cerraba como bucle — auditoría de 5 lentes en su ficha), se cerraron **nombres y animales definitivos** (Pyrecko=gecko *(antes Geflame, nombre ya en uso por otro indie)* · Ottorock=nutria · Mantagale=mantarraya aérea *(antes Cyclowl)*), se formalizó **actuador vs payload** en ARCHITECTURE §2.0, se añadió **ABILITY-EXTRACTION §8** (locomoción y controles que T0.3–T0.5 deben respetar) y se creó el **ADD** → [add.md](add.md) + **[add.html](add.html)**, que es desde ahora **el hogar de todos los diagramas**. El trío entró al GDD (§9.0) y a `gdd.html`.
> **Lo siguiente que toca:** la cola de ratificación de abajo. **Nada de eso bloquea el código: Fase 0 (T0.1/T0.2) lleva pendiente toda la sesión y es lo que decide el gate** — necesita Unity (abrir proyecto + crear escena), no lo cubre una sesión de solo-código. **Ya aplicado en solo-código: T1.0** (sincronización del scaffold con el diseño vigente; falta compilar en Unity para confirmar) y la **báscula de arquetipos** (6 ejes + Σ=37/especie + hexágonos del trío → [CREATURES.md](CREATURES.md) §4).

- **Fase actual:** Fase 0 (fundaciones) — scaffold de scripts creado, nada cableado en escena aún.
- **Hecho (código):** scaffold `Assets/Tatemis/Scripts/` completo; `InputReader` ya migrado al new Input System (T0.6 casi resuelto, falta verificar). **T1.0 aplicado (2026-07-18):** el scaffold ya refleja el diseño vigente — kit de 3 (`AbilitySlot { Movilidad, Flex, Signature }`), ataque básico universal (`EvolutionForm.basic` + `InputReader.BasicPressed`, sin coste), acción **contextual** en vez de botón de noqueo (`ContextualPressed` → `TryContextualAction`), `TypeChart.Multiplier` = 1.5x, mapeo de input alineado con GDD §6.3. ⚠️ **Falta compilar en Unity para confirmar** (no verificable fuera del editor).
- **⚠️ Deuda conocida (restante):** el modelo elemental **viejo** sigue en `Environment/ElementalSurface.cs` + `ReactionSystem.cs` (sin `ReactionTable`, sin las 9 combinaciones, sin anulación ni duración por impacto) → reescribir en **Fase 2** según [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) §7. Y `docs/gdd.html` (pitch visual) está obsoleto (kit de 4, ×2, modelo elemental viejo): regenerar desde el GDD antes de presentarlo. Mientras tanto, **ante cualquier duda mandan los docs, no el código**.
- **Siguiente (código):** T0.1 (setup Unity/LFS) y T0.2 (escena Sandbox grey-box) — sin dependencias, arrancables en paralelo; luego T0.3 locomoción.
- **Siguiente (diseño) — 👉 EL TRABAJO INMEDIATO:** ✅ fichas de Pyrecko y Ottorock dictadas ([CREATURE-PYRECKO.md](CREATURE-PYRECKO.md), [CREATURE-OTTOROCK.md](CREATURE-OTTOROCK.md)). **Gate decidido:** T1.4 = Ottorock en modo Libre; Pyrecko → Fase 4. **Análisis de modularidad hecho** → [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) (modular vs bespoke, 9 huecos, 3 olas). **Voladores en el slice: SÍ** → 3ª especie en borrador: [CREATURE-MANTAGALE.md](CREATURE-MANTAGALE.md). Quedan: ① **ratificar el borrador de Mantagale** campo a campo · ② **ratificar la tabla completa de reacciones** (6 dominancias + 9 combinaciones + regla direccional, [P]) en [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) — desbloquea el campo 14 de Ottorock · ③ las "Abiertas" menores de cada ficha. Con eso, los campos 1–10 de ambas ya permiten discutir el código (estrategia *bespoke*-primero de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)).
- **Bloqueos:** ninguno.

### 📋 Cola de ratificación
> ✅ **Sistema elemental cerrado el 2026-07-18** (ver la tabla de decisiones). ✅ **Báscula de arquetipos fijada el 2026-07-18** → 6 ejes (Vida/Daño/Velocidad/Alcance/Control/Sinergia elemental) + presupuesto **Σ=37/especie** + hexágonos del trío en [CREATURES.md](CREATURES.md) §4. Queda:
1. **Mantagale** — kit ✅ cerrado (§C de su ficha), nombre/animal ✅ cerrados, **ficha de 18 campos ✅ reescrita** al kit nuevo, **stats base ✅ fijados** ([CREATURES.md](CREATURES.md) §4). Quedan solo los **números de habilidad** (altura máxima, escalón por reacción, planeo, costes/CD), qué cuesta en altura un golpe recibido fuera del bloqueo, y las dos microdecisiones (tornados atraídos en secuencia vs a la vez · FIFO) — todo listado en §C.6.
2. **Abiertas menores por ficha:** telegraphs de IA salvaje (las 3), escala de Ottorock, 2ª evolución de Pyrecko.
> ✅ *Nombres verificados por Arzpal el 2026-07-18: «Pyrecko» y «Mantagale» están **libres**. Los tres nombres quedan firmes.*
3. **Números de la duración ∝ impacto** (regla R2 del sistema elemental): qué duración base por reacción y cómo escala con el daño. Se afina en playtest, no bloquea.

> ✅ *Reconciliación de [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) §4.9/H9 (elemento portado → roca sellada): **hecha** el 2026-07-18, junto con el barrido completo de rastros del modelo viejo en los 13 docs.*

### Decisiones de diseño de la sesión del 2026-07-17

| Decisión | Estado | Dónde vive |
|---|---|---|
| **Muerte real** de tatemis: salvajes por elección del jugador (rematar a un noqueado); propios por **Vitalidad** (barra externa al combate) que solo drena con **heridas graves** al caído | 🟡 ratificar con PB/DS | [VITALITY-DEATH.md](VITALITY-DEATH.md) |
| **Regla de oro:** un tatemis propio muere por *negligencia acumulada, nunca por un solo error* — toda regla nueva debe preservarlo | 🟡 | VITALITY-DEATH §1 |
| **Swap de emergencia** sin cooldown al noqueo · cuerpo caído en el suelo → **defender al compañero** · con aliados en campo el swap = **posesión** (cambio de control) | 🟡 | VITALITY-DEATH §4, §6.1 |
| **Derrota total:** caen los 3 → cuerpos vuelven a Tótems → el enemigo **persigue al Tamer** (barra de vida invisible) → escapar o despertar en la base | 🟡 | VITALITY-DEATH §5 |
| **Aliados por órdenes** (ref. Guardianes de la Galaxia): la IA aliada no pelea sola, ejecuta órdenes mid-combat. Coste = **exposición, no XP**. Manadas de salvajes como compensación | 🟡 experimento post-gate | VITALITY-DEATH §6 |
| **Kit simplificado:** 3 habilidades (**Movilidad / Flex / Signature**) + ataque básico gratis + **noqueo contextual** (sin botón dedicado) | ✅ docs · ⚠️ **código pendiente (T1.0)** | GDD §6.3, ARCHITECTURE §1 |
| **Tabla elemental:** debilidad **1.5x** normal → **2x** solo en forma elemental pura; resistencia 0.5x; neutral 1x | ✅ docs · ⚠️ **código pendiente (T1.0)** | GDD §7.1, ARCHITECTURE (`TypeChart`), T1.1 |
| **Alcance recalibrado** en 4 olas (matriz valor×viabilidad). El gate y las Fases 0–2 **no cambian**; lo nuevo va a olas post-gate | ✅ | [ROADMAP.md](ROADMAP.md) §"Recalibración de alcance" |
| **Pareja del prototipo dictada:** Pyrecko (Zonificador de Fuego) + Ottorock (Luchador de Mineral), **compleja a propósito** para descubrir los límites del sistema antes de estandarizar el código | ✅ fichas | [CREATURE-PYRECKO.md](CREATURE-PYRECKO.md) · [CREATURE-OTTOROCK.md](CREATURE-OTTOROCK.md) |
| **Marcas de especie** (1er caso: «combustible» de Pyrecko): **tercera capa**, aparte del terreno y de las reacciones; inertes | ✅ *(regla de limpieza **derogada** el 18/07 — ver tabla siguiente)* | ELEMENTAL-REACTIONS §1, §5 |
| **Gate (T1.4) = Ottorock, solo modo Libre** (melee puro, sin reacciones); Sujeción post-gate como stress-test de stances; **Pyrecko → Fase 4** | ✅ | CREATURES §3 · T1.4 |
| **Voladores en el slice: SÍ.** 3ª especie = Controlador de Aire volador (mini-tornados que empujan; vuelo como stance que drena energía) — cámara/lock-on/locomoción **no deben asumir "solo suelo"** | 🟡 borrador a ratificar | [CREATURE-MANTAGALE.md](CREATURE-MANTAGALE.md) |
| **Principio de kits: toda especie es autosuficiente** — su CC prepara su propio daño, puede ganar sola (ref. Aurelion Sol para Mantagale) | ✅ | CREATURES §2 |

### Decisiones de la sesión del 2026-07-18 — sistema elemental ✅ CERRADO

| Decisión | Dónde vive |
|---|---|
| **Modelo de 3 capas:** ① el **terreno tiene su elemento y no cambia nunca** (es el tablero) · ② las habilidades **irradian su elemento al impactar** → reacción = **efecto temporal** · ③ las **marcas de especie** (combustible) son otra cosa aparte | [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §1 |
| **R1 · Siempre se resuelve contra el TERRENO**, nunca contra un efecto activo → mata la explosión combinatoria (la matriz es 6×6 y punto) | ER §3 |
| **R2 · La duración del efecto es proporcional al impacto** (regla nueva) | ER §3 |
| **R3 · Un impacto nuevo cancela el efecto activo** → counterplay: atacar una zona para cancelarla | ER §3 |
| **R4 · Regla direccional** ratificada: el predador dispara la dominancia; la presa se **anula**. *"Fizzle" pasa a llamarse **anulación**.* | ER §2–3 |
| **R5 · Terreno neutro → nada.** La arena del gate es gris y sin reacciones; la de Fase 2 **necesita parches elementales** | ER §3, §8 · T0.2 |
| **R6 · El estallido no golpea a quien lo provoca** (el efecto de zona sí es neutral) | ER §3 |
| **Ottorock — combinación sellada portátil:** la mezcla se decide **al agarrar** y cada impacto la reproduce en cualquier superficie, incluida la neutra. Elige su combinación eligiendo dónde agarra | ER §5 · [ficha](CREATURE-OTTOROCK.md) campo 14 |
| **Pyrecko — combustible:** solo desaparece al **expirar o detonarse**; las reacciones **ya NO lo limpian**; pertenece **al lugar, no al material** (Ottorock arranca el terreno, no la marca) | ER §5 · [ficha](CREATURE-PYRECKO.md) |
| **Mantagale — tornado generador de reacciones:** sustituye a "mover/dispersar zonas" (incompatible con terreno fijo); recorre el suelo encadenando reacciones distintas | ER §5 · [ficha](CREATURE-MANTAGALE.md) campo 9 |
| **Ajustes de coste/legibilidad:** púas sin colisión física · maleza cargada → daña al pisar (sin carga diferida) · Rayo›Aire = AoE genérica (relámpagos uno a uno → 🔮) · root con guardarraíles · vapor **rompe el lock-on** (no "baja la precisión") · aturdimiento de dominancia = barra de captura, no stun | ER §4 |

**Eje de diseño en 3 capas:** ① *feel* de combate (en curso) → ② *vínculo con riesgo real* (la nueva identidad del meta) → ③ *equipo en campo* (apuesta cara, condicionada). ⚠️ El sistema de olas solo protege el proyecto **si se respetan las condiciones**: si la ola 3 se cuela antes del gate, el prototipo de feel muere de scope creep.

## Convención de ticket
`ID · Título` — descripción · **AC** = criterios de aceptación · **Dep** = depende de · **Owner** · **est.**

---

## Fase 0 — Fundaciones
> **Meta / DoD:** te mueves por una arena grey-box y haces lock-on a un maniquí.

**T0.1 · Setup del proyecto** — Unity LTS + URP, estructura de carpetas (ya scaffoldeada en `Tatemis3D/Assets/Tatemis/`), `Active Input Handling = Input System (New)` (default del template), Git LFS para binarios (fbx/png). · **AC:** el repo abre en Unity sin errores de consola. · **Owner:** PB · **est.** 2–3h

**T0.2 · Escena de pruebas grey-box** — arena con suelo, unas paredes/obstáculos, luz direccional, skybox neutro, un **maniquí dummy** (cápsula) con collider. *(Para Fase 0–1 el suelo va **neutro**: el gate es Ottorock-Libre sin reacciones. Dejar sitio previsto para los parches elementales de la Fase 2 — ver T2.x abajo.)* · **AC:** existe `Scenes/Sandbox.unity` navegable. · **Owner:** DS · **est.** 2–3h

**T0.2b · Parches de terreno elemental (preparar para Fase 2)** — quads etiquetados con su `Element` en la arena: charco (Agua), matorral (Naturaleza), veta de roca (Mineral), poza de lava (Fuego) + un **nodo de cristal junto al charco** que suelta su carga **al ser golpeado** (único modo de ver *agua electrificada*, la ★ del pitch, con el bestiario del slice). Sin esto **no hay nada que demostrar** del USP ambiental (regla R5). · **AC:** las zonas existen y son visualmente distinguibles en caja gris. · **Dep:** T0.2 · **Owner:** DS · **est.** 2–3h *(hacer al llegar a Fase 2, no antes)*

**T0.3 · Locomoción de la criatura** — cablear `CreatureController` (ya en scaffold): `CharacterController`, movimiento relativo a cámara, salto, gravedad, giro suave. ⚠️ **Dos cosas que hay que hornear bien ahora** ([ABILITY-EXTRACTION §8](ABILITY-EXTRACTION.md)): ① **perfil de locomoción intercambiable** (SO de parámetros + interfaz) por especie y sobrescribible por stance — las 3 criaturas se mueven distinto y dos cambian de modo en combate; solo se implementa el perfil terrestre. ② **no asumir que el personaje está siempre pegado al suelo** (habrá voladores en Nivel B; retrofitear el eje vertical es carísimo). *Sugerencia:* dejar las órdenes tras una interfaz *brain* (PlayerBrain hoy, AIBrain futuro) — ver [VITALITY-DEATH.md](VITALITY-DEATH.md) §6. · **AC:** la cápsula-jugador se mueve y salta con buen feel base. · **Dep:** T0.1 · **Owner:** PA · **est.** 4–6h

**T0.4 · Cámara 3ª persona** — Cinemachine FreeLook siguiendo al jugador; colisión con paredes. *(⚠️ Habrá **voladores** en el slice: no hornear un rig que asuma objetivos a ras de suelo.)* · **AC:** cámara orbital estable, sin clipping grave. · **Dep:** T0.1 · **Owner:** PB · **est.** 3–5h

**T0.5 · Lock-on** — `TargetingSystem` (ya en scaffold): seleccionar objetivo más cercano, la cámara/personaje encara al objetivo, toggle. *(⚠️ Voladores en el slice: contemplar objetivos elevados.)* · **AC:** con una tecla fijas/sueltas el maniquí; el personaje lo encara. · **Dep:** T0.3, T0.4 · **Owner:** PA · **est.** 4–6h

**T0.6 · Input (new Input System)** — el `InputReader` ya está migrado al new Input System (teclado + cualquier gamepad, rebinding en runtime + guardado de overrides en PlayerPrefs). Este ticket = verificar en editor que llegan todos los inputs con teclado y con mando, y (opcional) montar la UI de rebinding en el menú de settings usando `StartRebind` / `GetBindingDisplayString` / `ResetAllBindings`. · **AC:** todos los inputs llegan a `CreatureController` con teclado y con gamepad; el rebinding persiste entre sesiones. · **Dep:** T0.1 · **Owner:** PB · **est.** 3–5h

---

## Fase 1 — Core feel 🚩 (EL GATE)
> **Meta / DoD:** *"¿es divertido pegarle al maniquí y encadenar habilidades, aunque no haya arte?"* Si no → iterar aquí antes de la Fase 2.

**T1.0 · Sincronizar el scaffold con el diseño vigente** ✅ **APLICADO (2026-07-18)** — hecho todo lo que no requiere Unity ni Fase 2: enum `AbilitySlot` a 3, `EvolutionForm` (`flex` + `basic`), default de `AbilityData.slot`, `TypeChart` 1.5x, `CreatureController` (kit de 3 + despacho de básico + contextual), `InputReader` (3 abilities, `Basic`/`Contextual`, mapeo GDD §6.3), `ExampleContentGenerator` y comentario de `StaggerReceiver`. **Pendiente:** ① compilar en Unity para confirmar el AC; ② `ElementalSurface`/`ReactionSystem` (modelo viejo — se difiere a Fase 2 a propósito); ③ regenerar `docs/gdd.html`. La lista verificada original (auditoría del 2026-07-17), como referencia:
> **Ojo al orden:** hacerlo **después** de rellenar las fichas de [CREATURES.md](CREATURES.md) — la estrategia acordada es *bespoke*-primero ([ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)): diseñar criaturas reales → programarlas → **extraer** el sistema. Este ticket es el mínimo para que el scaffold no contradiga al diseño, no el refactor grande.
>
> - `Core/Enums.cs:7` — `AbilitySlot { Movilidad, Efecto, Ataque, Signature }` → `{ Movilidad, Flex, Signature }` *(raíz: romperá los 3 call sites de abajo, migrar en el mismo commit)*.
> - `Data/CreatureSpeciesData.cs:28-29` — campos `efecto` + `ataque` → un único `flex`.
> - `Data/AbilityData.cs:13` — default `= AbilitySlot.Ataque` → `= AbilitySlot.Flex`.
> - `Data/TypeChart.cs:13` — `return 2f` → `return 1.5f` *(el 2x lo aplica `BlockParams.weaknessDamageMultiplier`, que ya vale 2f — ese sí es correcto)*.
> - `Combat/CreatureController.cs` — summary L5–6 y comentario L80 ("kit de 4 habilidades"); L84 array con `form.efecto, form.ataque` → `form.flex`; bucle `i < 4` → `i < kit.Length`; L89 `if (_input.KnockoutPressed) TryKnockout();` → **prompt contextual**; falta el despacho del **ataque básico** (acción universal, sin coste).
> - `Input/InputReader.cs` — L29/L51 acción `Knockout` (`<Keyboard>/f`, `<Gamepad>/buttonNorth`) → rebautizar a **contextual** (finisher/Sludge/interactuar); L56-57 comentarios `// Efecto` / `// Ataque` → Flex/Signature; **añadir** binding de ataque básico. Mapeo objetivo en GDD §6.3.
> - `Editor/ExampleContentGenerator.cs:24,56,78` — `AbilitySlot.Ataque` / `AbilitySlot.Efecto` y `efecto = rafaga, ataque = golpe` → slots nuevos.
> - `Combat/StaggerReceiver.cs:30` — comentario "Intento manual de noqueo (botón)" → contextual. *(L6 "Nunca «mueren»" queda pendiente de [VITALITY-DEATH.md](VITALITY-DEATH.md) — no tocar hasta ratificar.)*
> - `Environment/ElementalSurface.cs:8` y `Environment/ReactionSystem.cs:25` — implementan el modelo **viejo** (`baseElement` + reacción derivada del `TypeChart` con `Multiplier>1`): sin `ReactionTable`, sin las 9 combinaciones, sin anulación, sin duración por impacto. Reescribir según [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md) §7 y ARCHITECTURE §5. *(No urge: es material de Fase 2, pero que no se olvide.)*
> - **Fuera del código:** `docs/gdd.html` (pitch visual) está obsoleto en **dos frentes**: (a) kit de 4, noqueo como botón y "×2" en L300/303/304/307/329/447; (b) **el modelo elemental viejo** en L379 ("manchar el terreno", "elemento base", "una reacción por terreno"). **No bloquea nada**, pero **no presentarlo** sin regenerarlo desde el GDD actual.
>
> **AC:** el proyecto compila; `AbilitySlot` tiene 3 valores; ningún archivo menciona un botón de noqueo dedicado; `Multiplier` devuelve 1.5. · **Owner:** PA · **est.** 2–3h

**T1.1 · TypeChart + elementos** — crear el asset `TypeChart` y validar la tabla de los 2 triángulos con tests rápidos. · **AC:** `Multiplier(Fuego, Naturaleza)==1.5`, `(Agua,Fuego)==1.5`, resistencias `==0.5`, cruces neutrales `==1` (el 2x solo existe contra forma elemental, vía `BlockParams`). · **Owner:** PA · **est.** 1–2h

**T1.2 · Habilidades data-driven** — `AbilityData` + efectos `DealDamage`/`Dash` (ya en scaffold); crear 2–3 assets de habilidad. · **AC:** una `AbilityData` con efectos editables en el inspector se ejecuta. · **Dep:** T0.6 · **Owner:** PA · **est.** 4–6h

**T1.3 · Energía elemental + HUD debug** — `ElementalEnergy` (regen tipo tinta) + barra de debug en pantalla; el `AbilityCaster` gasta y respeta cooldown. · **AC:** lanzar habilidades vacía la barra; se regenera; bloquear sube la regen. · **Dep:** T1.2 · **Owner:** PB · **est.** 3–5h

**T1.4 · Criatura de prueba: Ottorock (solo modo Libre)** — crear un `CreatureSpeciesData` (1 forma) de **Ottorock** según su ficha ([CREATURE-OTTOROCK.md](CREATURE-OTTOROCK.md)), implementando **solo el kit del modo Libre** (Embestida Garra, Mordisco Ferroso, cadena de 5 básicos; el Cambio de estado y Sujeción llegan **después del gate** como stress-test de stances). · **AC:** el jugador usa a Ottorock-Libre con ese kit. · **Dep:** T1.2 · **Owner:** DS · **est.** 2–4h

**T1.5 · Maniquí golpeable + feedback** — `StaggerReceiver` en el dummy: recibe daño con multiplicador de tipo, hit-react visible (flash/knockback), número de daño debug. · **AC:** pegarle al maniquí se siente y se lee. · **Dep:** T1.2 · **Owner:** PA · **est.** 4–6h

**T1.6 · Game feel** — hitstop, screen-shake ligero, VFX de impacto placeholder, ejecutar efectos en **anim-events** (no en el frame del input). · **AC:** los golpes tienen "peso". *(Este ticket es el corazón del gate — presupuéstenle iteración.)* · **Dep:** T1.5 · **Owner:** PA + DS · **est.** 6–10h

**T1.7 · Noqueo (esqueleto)** — **prompt contextual** (no botón dedicado): al llenarse el aturdimiento o con vida muy baja se ofrece el finisher → `StaggerReceiver.TryKnockout()` con probabilidad por vida/aturdimiento → log de "KO window abierta". · **AC:** al debilitar, aparece el prompt; al confirmarlo se abre la ventana (aún sin captura). · **Dep:** T1.5, T1.0 · **Owner:** PB · **est.** 2–3h

---

## Reparto sugerido
- **PA (Arzpal):** locomoción, lock-on, habilidades, maniquí, game feel (T0.3, T0.5, T1.1, T1.2, T1.5, T1.6).
- **PB:** setup, cámara, input, energía/HUD, noqueo (T0.1, T0.4, T0.6, T1.3, T1.7).
- **DS:** escena grey-box, criatura de prueba, apoyo en game feel (T0.2, T1.4, T1.6).

**Total estimado Fase 0–1:** ~50–75h de trabajo → a ritmo part-time, coincide con las ~4–6 semanas del gate en el roadmap.
