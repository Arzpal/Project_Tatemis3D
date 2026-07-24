# Tatemis3D — Sistema de habilidades (catálogo + arquitectura)

> **Objetivo:** que **cualquier** habilidad dentro del rango que definamos se cree en **un par de clics**, cubriendo **todo** (mecánica, animación, VFX, sonido). Este doc es para **triagear**: marca `[x]` lo que SÍ, tacha ~~lo que NO~~, deja `[ ]` lo dudoso. Con el set acordado se refactoriza el scaffold. Complementa [ARCHITECTURE.md](ARCHITECTURE.md).
>
> **Mapa:** §1 principios · §2 anatomía de una habilidad · §3 catálogo de efectos (3.1 movilidad · 3.2 control · 3.3 daño/entrega · 3.4 CC · 3.5 buffs · 3.6 recursos · 3.7 terreno · 3.8 spawns · 3.9 condicionales · 3.10 juice · 3.11 escape hatch) · §4 PhaseExit · §5 targeting · §6 stances · §7 cómo triagear. → Ir a la subsección que pida la tarea.

> ⚠️ **ESTRATEGIA (según investigación de arquitectura):** este es el sistema **OBJETIVO**, **no** el punto de partida. Para un equipo pequeño el mayor riesgo es la **abstracción prematura** — construir el framework antes de tener habilidades reales. **Plan:** en el prototipo de feel, programar **3–4 habilidades bespoke** (código directo, aunque se repita) para clavar el *feel*; **luego extraer** este sistema de ellas. Los juegos de este tipo (Pokémon, Dota 2, Path of Exile) sí usan sistemas modulares — pero se **destilan de habilidades reales**, no se diseñan por adelantado. NO adoptar Unreal GAS ni un clon completo (su valor es la replicación de red multijugador, no aplica ahora).

---

## 1. Los 3 principios que ordenan todo

### 1.1 Comportamiento (inline) vs Contenido (assets) — la clave de la reutilización
- **Comportamiento + números → inline** (`[SerializeReference]` dentro de la habilidad). Ej: "`DealDamage` con amount=12". Se re-teclea por habilidad; es barato y da control fino.
- **Contenido compartido → assets** que los efectos *referencian*: `StatusData` (Quemadura, Ralentizado…), prefabs de **VFX**, **AudioClips/eventos de sonido**, `ProjectileData`, `HazardData`.
- **Regla de decisión:** *"si cambio esto, ¿quiero que cambie en TODAS las habilidades a la vez?"* → **asset**. *"¿quiero tunearlo por habilidad?"* → **inline**.
- Así "Quemadura" o "el golpe de impacto de fuego" se definen **una vez** y se reúsan en 50 habilidades; pero los números de cada habilidad son suyos.

### 1.2 La animación es la línea de tiempo — todo se dispara por anim events
Los efectos **no** se ejecutan en el frame del input, sino en **eventos de animación** (exactamente tu intuición para los sonidos). La animación fira eventos con nombre (`"hit"`, `"active"`, `"vfx"`, `"sfx"`, `"end"`) → un `AbilityAnimationBridge` se los pasa al runtime, que dispara los efectos correspondientes en ese frame.
- **VFX y SFX son solo efectos más** (`SpawnVFX`, `PlaySound`) disparados en su anim event. Un solo sistema para timing de mecánica + visual + audio → cubre el "todo" que pediste.
- El diseñador coloca el evento "hit" en el frame exacto del impacto → *game feel* en manos del diseñador, sin tocar código.

### 1.3 Válvula de escape para lo irrepetible
Para la habilidad única que no vale la pena modularizar, el efecto **`CustomBehaviour`** referencia un script propio. Así el sistema modular **nunca bloquea** una idea rara — pero el 95% se arma con la librería.

---

## 2. Anatomía de una habilidad (cómo llega a "pocos clics")

```
AbilityData (asset)
├── Meta:      id · nombre · icono · slot · energyCost · cooldown
├── Targeting: cómo apunta (§5)
└── Phases[]:  1 fase = habilidad simple; varias = multi-paso (cargar, agarrar-y-lanzar)
     └── Phase
         ├── animation   (trigger/clip)   ← la línea de tiempo
         ├── effects[]    (de la librería, §3, se enganchan a OnEnter/OnTick/OnHit/OnExit)
         └── exit         (condición de salida, §4)
```

**Habilidad típica = 1 fase:** eliges animación + 2–3 efectos + condición de salida. Los VFX/SFX son efectos disparados en anim events. Hecho en pocos clics.

---

## 3. Catálogo de EFECTOS (la librería — TRIAGEAR)

> Sugerencia de alcance: 🟢 core (prototipo/USP) · 🟡 útil · ⚪ luego/avanzado. Reescribe a gusto.

### 3.1 Movilidad (self)
- [ ] **DashMotion** — desplazamiento rápido en una dirección 🟢
- [ ] **Lunge** — dash hacia el objetivo (lock-on) 🟢
- [ ] **Dodge/Backstep** — evasión con i-frames 🟡
- [ ] **LiftImpulse** — impulso hacia arriba (elevarse) 🟡
- [ ] **Glide** — planeo (baja gravedad mientras dura) 🟡
- [ ] **Hover** — mantener altura 🟡
- [ ] **Teleport/Blink** — parpadeo a un punto / detrás del objetivo 🟡
- [ ] **GroundSlam** — caída rápida con impacto ⚪
- [ ] **RootSelf** — anclarse (para canalizadas) 🟡

### 3.2 Control del enemigo (movimiento)
- [x] **Knockback** — empujar 🟢 *(un empujón por golpe recibido)*. Tres palancas más, añadidas el 2026-07-22: **`useMotionDirection`** (arrastra en la dirección **3D** del dash, no en el frente horizontal → embestir en diagonal se lleva al enemigo hacia arriba contigo, ref. *side B* de Meta Knight), **`lift`** (elevación extra) y **`hang`** (ver abajo).
- [x] **LaunchAirborne** — elevar al enemigo (juggles) 🟢 *(`Knockback.lift` + `hang`)*

> 🔒 **Suspensión en el aire (juggle), 2026-07-22.** Golpear y ser golpeado **suspenden la caída** un instante, en los dos bandos: sin eso, un combo aéreo se cae solo a media cadena y no existe. Lo lleva `HitReactor.ApplyHang` (enemigos) y `CreatureController.GrantHang` (jugador). **El guardarraíl es un presupuesto acumulable (`maxHang` ≈ 1.2 s) que solo se recarga al tocar suelo** — si no, cualquiera se sostendría indefinidamente a base de pegar, y el juggle infinito es el fallo clásico del género.
>
> ⚠️ Efecto secundario a vigilar: elevar enemigos exige que la **cámara y el lock-on** aguanten objetivos en alto. Ya está previsto (era requisito de Mantagale, [§8.2](ABILITY-EXTRACTION.md)), pero ahora deja de ser hipotético y hay que probarlo.
- [ ] **Empuje continuo** (tornados de Mantagale, arrastre del Ciclón) 🟡 — sigue pendiente, ver [H12](ABILITY-EXTRACTION.md). *No confundir con el multi-hit: arrollar se resuelve repartiendo golpes (§4.1), pero un tornado empuja sin golpear a cadencia.*
- [ ] **Pull** — atraer 🟡
- [ ] **Interrupt** — romper su acción actual 🟢
- [ ] **Reposition/Grab** — agarrar y colocar ⚪

### 3.3 Daño / impacto
- [x] **DealDamage** — daño con multiplicador de tipo 🟢
- [x] **MultiHit** — N golpes durante la duración 🟢 *(2026-07-22: `SweepHitbox.hitInterval`, ver §4.1)*
- [x] **AreaDamage** — AoE por radio 🟢 *(`ElementalBurst`: estalla en el elemento de la especie)*
- [ ] **AddStagger** — llenar aturdimiento (clave para captura) 🟢 *(hoy va dentro de los efectos de daño)*
- [ ] **DamageOverTime** — daño por tick 🟡 *(cadencia por reloj, como el multi-hit — nunca por frame)*
- [ ] **MultiHit** — N golpes durante la duración 🟡
- [ ] **ConeSweep** — daño en cono/barrido 🟡
- [ ] **Execute** — bonus vs vida baja / aturdido (apoya el noqueo) 🟡
- [ ] **Lifesteal** — cura % del daño (elemento Sangre, a futuro) ⚪
- [ ] **Pierce** — ignora defensa ⚪

### 3.3b Entrega (melee / rango / teledirigido)
El **daño —y cualquier payload: estado, knockback…— es agnóstico de la entrega.** *Cómo* llega al enemigo se elige aparte, con un módulo de entrega. Así el mismo `DealDamage` sirve para un espadazo, un proyectil o una reacción ambiental.
- [ ] **MeleeHitbox** — volumen de golpe durante los frames activos de la animación 🟢
- [ ] **Projectile (recto)** — dispara un proyectil que lleva el payload 🟢
- [ ] **Projectile (arco/lobbed)** — proyectil con parábola 🟡
- [ ] **Projectile (teledirigido/homing)** — persigue al objetivo 🟡
- [ ] **Hitscan** — línea instantánea (tipo francotirador) 🟡
- [ ] **AreaAtPoint** — AoE en el objetivo/punto 🟢
- [ ] **Aura/Self** — aplica alrededor del lanzador 🟡

### 4.1 · Ataques que duran: multi-hit 🔒

> **Un ataque que se prolonga no hace "daño mientras te toca": reparte GOLPES.** Ref. el Drill Rush de Meta Knight (Smash Ultimate). La embestida *tiene* ~4 golpes repartidos por su recorrido; lo que cambia es **cuántos te alcanzan** — rozarla al esquivar cuesta uno, comerla entera cuesta todos.
>
> Por qué este modelo y no "daño por tiempo de contacto" *(decidido el 2026-07-22)*:
> - **Converge con los anim-events** (T1.6, regla 3 del [ADD](add.md)): un golpe en un momento concreto del ataque es exactamente lo que dispara una animación. El reloj de hoy lo sustituye un anim-event mañana **sin tocar nada más**. "Daño por tiempo tocando" no se convierte en un anim-event nunca.
> - **Es autoral y determinista:** el diseñador coloca los golpes y el balance se lee solo (4 × 8 = 32 si te alcanzan todos).
> - **Los payloads no se enteran:** cada golpe es un `HitInfo` normal, así que `DealDamage`, `Knockback` y compañía sirven tal cual. Sin efectos especiales de "daño sostenido" — se probaron y sobraban.
>
> 🔒 **Regla de oro del muestreo:** *la detección se muestrea; las consecuencias se cronometran.* El framerate decide cuántas veces se **mira**, nunca cuántas veces **pasa** algo. En la práctica: ① la **cadencia** de los golpes va por reloj de la habilidad (como los frames de animación de un juego de lucha), nunca uno por frame; ② cada golpe **barre el tramo recorrido desde el golpe anterior** (`SphereCast`), no muestrea un punto suelto — si no, a poco framerate un enemigo se cuela **entre dos muestras** y no lo golpea nadie.
>
> ❌ **Descartado (2026-07-22):** el **último golpe distinto** del modelo de Smash (*finishing hit* con knockback grande). Se valoró y **no hace falta**: con golpes iguales el empuje ya se siente bien, y el remate lo pone el **estallido elemental** al frenar. Si alguna habilidad futura lo pide, se resuelve con una fase extra, no con un caso especial en el multi-hit.

> Regla: los efectos de §3.3–3.6 son el **payload** (qué le pasa a quien lo reciba); la **entrega** decide **a quién y cómo**. Un proyectil teledirigido que solo ralentiza = `Projectile(homing)` + `Slow`, sin `DealDamage`. Un espadazo que empuja = `MeleeHitbox` + `DealDamage` + `Knockback`.

### 3.4 Estados / CC
- [ ] **ApplyStatus** — aplicar estado genérico (referencia a `StatusData`) 🟢
- [ ] **Slow** 🟢
- [ ] **Stun** — no puede actuar 🟡
- [ ] **Root** — no puede moverse 🟡
- [ ] **Vulnerable** — recibe más daño 🟡
- [ ] **Taunt** — fuerza al enemigo a atacarte (Guardián) 🟡
- [ ] **Cleanse** — quita estados propios/aliados 🟡
- [ ] **Silence** — no puede usar habilidades ⚪

### 3.5 Buff / defensa (self o aliado)
- [ ] **ModifyStat** — def/daño/velocidad ±X por Y seg 🟢
- [ ] **Shield/Barrier** — absorbe N daño 🟡
- [ ] **DamageReduction %** 🟡
- [ ] **Invuln/i-frames** — invulnerabilidad breve 🟡
- [ ] **Parry/Counter** — ventana de contra 🟡
- [ ] **Reflect** — devuelve % del daño ⚪

### 3.6 Recursos
- [ ] **GainEnergy** 🟢
- [ ] **DrainEnergy** — propio extra o al enemigo 🟡
- [ ] **Heal / HealOverTime** 🟡
- [ ] **HealthToEnergy** — convertir vida en energía (Sangre) ⚪

### 3.7 Terreno / ambiente (USP)
- [ ] **RadiateElement** — irradiar el elemento al impactar → resuelve la reacción contra el terreno ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §3; parámetros: radio, fuerza de impacto → duración R2, instigador R6) 🟢
- [ ] **SpawnHazard** — zona AoE persistente (charco de fuego, hielo…) 🟢
- [ ] **ReadSurface** — **lee** el elemento del terreno (sin alterarlo: el terreno nunca cambia) para potenciar la habilidad 🟡
- [ ] **GrabChunk** — arrancar un trozo de terreno; **resuelve y sella una vez** `elemento propio + elemento del terreno` en el blackboard del stance (las marcas de especie NO se llevan: pertenecen al lugar) 🟡
- [ ] **ThrowHeld** — lanzar lo agarrado como proyectil 🟡
- [ ] **CreateWall/Cover** — generar obstáculo/cobertura 🟡
- [ ] **SpawnPlatform** — plataforma/puente ⚪

### 3.8 Spawns / proyectiles
- [ ] **SpawnProjectile** — recto / arco / teledirigido (referencia a `ProjectileData`) 🟢
- [ ] **SpawnZone** — AoE persistente que aplica efectos al entrar 🟡
- [ ] **Trap** — trampa armada 🟡
- [ ] **Boomerang** — proyectil que vuelve ⚪
- [ ] **Summon** — minion/torreta ⚪

### 3.9 Lógica / condicional (ramas "si…")
- [ ] **ChanceEffect** — % de disparar otro efecto 🟡
- [ ] **ConditionalBranch** — si el objetivo tiene estado X → efecto extra 🟡
- [ ] **RequireSurface** — solo si estás sobre terreno de elemento X 🟡
- [ ] **ComboGate** — solo si la habilidad previa fue Y ⚪

### 3.10 Presentación / juice (feel)
- [ ] **SpawnVFX** — instanciar VFX (referencia a prefab) 🟢
- [ ] **PlaySound** — reproducir SFX (referencia a AudioClip/evento) 🟢
- [ ] **Hitstop** — congelar frames al impactar 🟢
- [ ] **CameraShake** 🟢
- [ ] **TimeDip** — breve cámara lenta 🟡
- [ ] **Rumble** — vibración del mando 🟡

### 3.11 Escape hatch
- [ ] **CustomBehaviour** — referencia a un script propio para la habilidad única e irrepetible 🟢 *(mantener siempre)*

---

## 4. Condiciones de salida de fase (`PhaseExit` — TRIAGEAR)
- [ ] **Immediate** — la fase termina al instante (efectos instantáneos) 🟢
- [ ] **AfterSeconds** — tras X segundos 🟢
- [ ] **OnAnimationEvent** — un evento con nombre en la animación marca el fin 🟢
- [ ] **OnButtonReleased** — al soltar (cargar/mantener) 🟡
- [ ] **OnSecondPress** — al volver a pulsar (multi-paso) 🟡
- [ ] **WhileButtonHeld** — mientras se mantiene (canalizar) 🟡
- [ ] **OnHitLanded** — al conectar un golpe 🟡
- [ ] **OnGrounded** — al tocar el suelo (tras planear/saltar) 🟡
- [ ] **OnStaggerFull** — cuando el objetivo llena aturdimiento ⚪
- [ ] **OnResourceEmpty** — cuando se acaba la energía ⚪

## 5. Modos de targeting (TRIAGEAR)
- [ ] **Self** 🟢
- [ ] **LockedTarget** — el objetivo del lock-on 🟢
- [ ] **AimDirection** — hacia donde mira la criatura 🟢
- [ ] **AllInRadius** — todos en un radio 🟢
- [ ] **PointOnGround** — retícula en el suelo 🟡
- [ ] **NearestInCone** — el más cercano en un cono 🟡

---

## 6. Formas / Stances (modos) — es otro sistema

Las tres capas de duración del sistema: **Efecto** (instantáneo) → **Fase** (secuencia con timing) → **Stance** (modo persistente). Un **Stance** es un modo en el que **entra** la criatura y que cambia sus reglas **hasta salir**. Persiste, y puede cambiar el **kit**, los **stats** y el **movimiento**. *(El **bloqueo / forma elemental pura** que ya existe ES un stance — este sistema lo generaliza.)*

**`StanceData` (asset)** define:
- Modificadores de **stats** (ej. +def / −daño) y de **movimiento** (volar, más lento…).
- **Override del kit** (opcional): en modo "agarrando bloque" el mismo botón ahora **lanza**; en "stance de daño", otro set de habilidades.
- **Aura/pasiva** mientras dura (invisible = no te pueden hacer lock-on; envuelto en agua = reduce daño).
- Efectos **OnEnter / OnTick / OnExit** (el tick permite **coste por segundo**).
- **Condición de salida:** toggle, temporizador, **coste de energía por segundo** (volar/invisibilidad drenan), al recibir daño, etc.
- **VFX/SFX persistentes** del modo.

Se entra/sale con efectos de habilidad (se unifica con todo lo anterior):
- [ ] **EnterStance** — entra a un modo 🟢
- [ ] **ExitStance** — sale del modo actual 🟢
- [ ] **ToggleStance** — alterna (defensa ↔ daño) 🟢

**Modos de ejemplo (contenido — TRIAGEAR):**
- [ ] **FormaElementalPura** — el bloqueo actual (defiende + recarga, ×2 debilidad) 🟢
- [ ] **AgarrandoObjeto** — sostienes un bloque; el kit cambia a lanzar/soltar 🟡
- [ ] **EnvueltoEnAgua** — defensivo: reduce daño, quizá refleja 🟡
- [ ] **Vuelo** — modo aéreo persistente (drena energía por seg) 🟡
- [ ] **Invisible** — sigilo: sin lock-on enemigo, drena energía 🟡
- [ ] **StanceDaño / StanceDefensa** — alternar perfil ofensivo/defensivo 🟡

> **Relación con habilidades:** "transformarse" = un efecto `EnterStance(X)`. Salir puede ser otra habilidad, soltar el botón, o que se agote el recurso. El **kit override** es lo que hace natural el "agarrar y luego lanzar" (mismo botón, contexto distinto) — más limpio que multi-fases para modos que **duran**.

## 7. Cómo triagear este doc
1. Marca `[x]` los efectos/condiciones que **sí** quieres, tacha ~~los que no~~, deja `[ ]` los dudosos.
2. Con el set acordado, se refactoriza el scaffold: `AbilityEffect` con hooks, `AbilityPhase` + `PhaseExit`, `ActiveAbility` (runtime con blackboard), `AbilityAnimationBridge`, y **un stub por cada efecto elegido** (el diseñador ya solo compone).
3. Los assets de contenido (`StatusData`, `ProjectileData`, VFX, SFX) se crean como ScriptableObjects/prefabs a medida que se necesiten.

> **Recomendación de orden:** implementar primero **todo lo 🟢** (cubre el prototipo y el USP), dejar 🟡/⚪ como stubs vacíos o sin crear hasta que una habilidad los pida.
