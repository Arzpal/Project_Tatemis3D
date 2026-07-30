# Tatemis3D — Sistema de habilidades (catálogo + arquitectura)

> **Objetivo:** que **cualquier** habilidad dentro del rango que definamos se cree en **un par de clics**, cubriendo **todo** (mecánica, animación, VFX, sonido). Este doc es para **triagear**: marca `[x]` lo que SÍ, tacha ~~lo que NO~~, deja `[ ]` lo dudoso. Con el set acordado se refactoriza el scaffold. Complementa [ARCHITECTURE.md](ARCHITECTURE.md).
>
> **Mapa:** §1 principios · §2 anatomía de una habilidad · **§2.1 anatomía universal del ataque: los 3 tramos 🔒** · §3 catálogo de efectos (3.1 movilidad · 3.2 control · 3.3 daño/entrega · 3.4 CC · 3.5 buffs · 3.6 recursos · 3.7 terreno · 3.8 spawns · 3.9 condicionales · 3.10 juice · 3.11 escape hatch) · §4 PhaseExit · §5 targeting · §6 stances · §7 cómo triagear. → Ir a la subsección que pida la tarea.
>
> ⚠️ **Revisado el 2026-07-29 contra el modelo elemental vigente** ([ELEMENTAL-REACTIONS §0 y §4](ELEMENTAL-REACTIONS.md)): §2.1 es nueva y §3.1/§3.2/§3.4/§3.7 incorporan los efectos que las 15 parejas cerradas ese día **necesitan y el catálogo no tenía**.

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

### 2.1 · Anatomía universal del ataque: los 3 tramos 🔒 *(dictada por Arzpal el 2026-07-29)*

Al margen de cuántas fases tenga la habilidad, **todo ataque del juego —básicos incluidos— se lee en tres tramos**, y de ahí sale qué se puede parar y qué no:

1. **Arranque — interrumpible.** Lo cortan los golpes, los **ticks de Rayo** y cualquier otra cosa. **Este tramo *es* el "te corté a media animación".**
2. **El golpe — comprometido.** Movimiento del golpe + hitbox. A partir de aquí **entra sí o sí** (es lo que hoy expresa `superArmor`).
3. **Recuperación — cancelable.** El jugador puede **cortar la cola con otra habilidad** para encadenar más seguido.

**Por qué importa más allá del feel:** la recuperación cancelable **es el sistema de combos que alimenta la captura** (encadenar más seguido sube el multiplicador de combo, y con él el aturdimiento — [GDD §8.2](GDD.md)), y es lo que le da a Rayo un **contrajuego real**: sus ticks solo muerden el **arranque**, así que la respuesta es entrar ya atacando y no dejar huecos. Origen y consecuencias completas en [ELEMENTAL-REACTIONS §0.6](ELEMENTAL-REACTIONS.md).

> ⚠️ **El andamiaje existe a medias.** `AbilityPhase` ya tiene `interruptible` y `superArmor` (tramos 1 y 2), y la **Embestida Garra** de Ottorock ya tiene esta forma exacta (carga → embestida → deslizamiento). **Lo que NO existe es el tramo 3: `AbilityCaster.CanCast` rechaza cualquier cast mientras `_active != null`**, así que hoy una habilidad no se puede cortar con otra. Mientras siga así, **todo lo que dependa del cancel queda aparcado** — empezando por la **cristalización** (Mineral + Naturaleza), cuya única función es *quitártelo* ([ELEMENTAL-REACTIONS §4](ELEMENTAL-REACTIONS.md)).
>
> 🟡 **Sin dictar (decide Arzpal):** la **ventana** del cancel (¿toda la recuperación o solo su último tramo?), si **cuesta recurso**, y si los **básicos** se cancelan entre sí o solo con habilidades de slot.

---

## 3. Catálogo de EFECTOS (la librería — TRIAGEAR)

> Sugerencia de alcance: 🟢 core (prototipo/USP) · 🟡 útil · ⚪ luego/avanzado. Reescribe a gusto.

### 3.1 Movilidad (self)
- [ ] **DashMotion** — desplazamiento rápido en una dirección 🟢
- [ ] **Lunge** — dash hacia el objetivo (lock-on) 🟢
- [ ] **Dodge/Backstep** — evasión con i-frames 🟢 *(sube a core el **2026-07-29**: [ELEMENTAL-REACTIONS §0.9](ELEMENTAL-REACTIONS.md) dicta que **todo tiene que ser esquivable** — el techo de efectos apunta a 0-1 y el **delay** de §0.2 existe precisamente para que haya ventana. La esquiva deja de ser un lujo: es la respuesta esperada del jugador ante una reacción.)*
  > 🟡 **Sin dictar:** hoy la esquiva **no es un botón**, es el slot de **Movilidad** ([GDD §6.3](GDD.md): *"hace las veces de dodge"*) — y por tanto **tiene coste y cooldown**. Falta decidir si eso basta para sostener "todo es esquivable", o si hace falta una evasión barata/sin recurso. **Arzpal decide** (afecta al presupuesto de inputs, que fue la razón de recortar el kit a 3).
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
- [ ] **Empuje continuo** (tornados de Mantagale, arrastre del Ciclón) 🟡 — sigue pendiente, ver [H12](ABILITY-EXTRACTION.md). *No confundir con el multi-hit: arrollar se resuelve repartiendo golpes (§4.1), pero un tornado empuja sin golpear a cadencia.* ⚠️ **Ya no es solo de Mantagale (2026-07-29):** la **corriente** (Rayo + Agua) es *"el único arrastre"* del sistema y el verbo de Agua es literalmente **Corriente**, así que el empuje continuo pasa a ser requisito **del modelo elemental**, no de una especie post-prototipo.
- [ ] **Pull** — atraer 🟡
- [ ] **ReelIn / Tether** — **tirón periódico que devuelve al punto de anclaje** 🟡 *(nuevo 2026-07-29: lo pide la **amarra**, Rayo + Naturaleza. **No es un pull continuo**: es discreto a propósito — una fuerza constante contra tu input se lee como mandos rotos, un tirón se lee como acontecimiento. Cada víctima lleva **su propia estaca** en el punto donde la alcanzó, y la correa **se debilita** cuanto más aguantas fuera.)*
- [ ] **AnchorDown / GravityWell** — **más gravedad en una zona: acorta la suspensión y arranca del aire** 🟡 *(nuevo 2026-07-29: es el verbo **Peso** de Mineral y el cuerpo de la **energía geo**, la reacción anti-aérea del juego. No impide despegar, hace que despegar **valga menos** → toca el presupuesto de `hang` del juggle, ver el 🔒 de arriba.)*
- [ ] **Interrupt** — romper su acción actual 🟢 *(el modelo del 2026-07-29 lo precisa: interrumpir muerde el **arranque** (§2.1, tramo 1) y **no** el tramo comprometido. Es el mecanismo de los **ticks de Rayo** y de la **descarga de tormenta**.)*
- [ ] **Reposition/Grab** — agarrar y colocar ⚪

> ⚠️ **Elevar desde una ZONA, no desde un golpe** *(2026-07-29)*: el **géiser** (Agua › Fuego) es *"la reacción que abre el juego aéreo"* — eleva a quien pilla y, al caer, la inercia de la corriente sigue arrastrando. El payload es `LaunchAirborne` tal cual, pero **el emisor es una reacción ambiental**, no un `MeleeHitbox`: la entrega de §3.3b tiene que servir igual a las zonas de [ELEMENTAL-REACTIONS §4](ELEMENTAL-REACTIONS.md).

### 3.3 Daño / impacto
- [x] **DealDamage** — daño con multiplicador de tipo 🟢 *(⚠️ **2026-07-29: necesita modo NEUTRO.** Las **reacciones** hacen daño **del elemento dominante y sí pasan por la tabla de tipos**; las **combinaciones** hacen daño **neutro, de ningún elemento, y NO pasan por la tabla** ([ELEMENTAL-REACTIONS §0.2](ELEMENTAL-REACTIONS.md)). Hoy el efecto asume que todo daño tiene tipo.)*
- [x] **MultiHit** — N golpes durante la duración 🟢 *(2026-07-22: `SweepHitbox.hitInterval`, ver §4.1)*
- [x] **AreaDamage** — AoE por radio 🟢 *(`ElementalBurst`: estalla en el elemento de la especie)*
- [ ] **AddStagger** — llenar aturdimiento (clave para captura) 🟢 *(hoy va dentro de los efectos de daño)*
- [ ] **DamageOverTime** — daño por tick 🟡 *(cadencia por reloj, como el multi-hit — nunca por frame)*
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

> 🔑 **El estallido elemental es ENTREGA PROPIA, no un payload de `OnHit`** ✅ *(2026-07-29)*. Dictado con Ottorock: su **Mordisco Ferroso** y el **5º básico** deben **estallar SIEMPRE**, conecten o no — si solo estallan al acertar, fallar te deja además sin sembrar tu elemento, y el USP entero depende de poder **apuntar al suelo**. En la práctica: el estallido lo dispara la **fase/anim-event**, nunca el `ReportHit`. Y **nace por delante del cuerpo** (`ElementalBurst.forwardOffset`), no a los pies — lo que además protege al melee del radio de una mezcla imparcial ([ELEMENTAL-REACTIONS §0.2b](ELEMENTAL-REACTIONS.md)).

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
- [ ] **Stun** — no puede actuar 🟡 *(⚠️ no confundir con el **Aturdimiento** de captura, que es una barra y no te quita el mando — [GDD §8.2](GDD.md))*
- [ ] **Root** — no puede moverse 🟡 ⚠️ *(precisado el 2026-07-29: el **Entrelazar** de Naturaleza —y su **crecimiento explosivo**— **bloquea el movimiento pero DEJA atacar y bloquear**. No es un stun con otro nombre. Hoy va **aproximado con `Paralizado`** en `ReactionSystem` y el "puede atacar y bloquear" es bespoke pendiente.)*
- [ ] **Paralizar (ticks por reloj)** — **interrumpe el ARRANQUE, no el movimiento** 🟡 *(nuevo 2026-07-29: es el verbo de **Rayo**. No te impide moverte ni pegar; te impide **encadenar**, porque cada tick corta el tramo 1 de §2.1. **Cronometrado, nunca aleatorio** — el jugador aprende el ritmo y cuela el combo entre ticks. Nombre a no reciclar: **no** es `Stun`.)*
- [ ] **NoCancel** — **quita el cancel de recuperación** 🟡 *(nuevo 2026-07-29: es toda la función de la **cristalización**, Mineral + Naturaleza. Te deja pelear, bloquear y moverte a velocidad normal, pero **pagas cada acción entera**. Espejo exacto de Rayo: él te corta el golpe, esta no te deja cortarlo a ti.)* ⚠️ **Aparcada**: depende del cancel, que **no existe** en `AbilityCaster` — ver §2.1.
- [ ] **RecoveryPenalty** — **alarga la recuperación de dash/salto al aterrizar** 🟡 *(nuevo 2026-07-29: es cómo funcionan las **arenas movedizas** (Mineral + Agua), la única **anti-fuga**. 🔑 El dash y el salto salen **enteros y a distancia normal** —recortar la distancia se leería como un bug, no como barro—; lo que se paga es **el aterrizaje**. Escapar funciona; escapar te deja vendido. **Muere el "barro = ralentiza"**: era redundante con el verbo Peso.)*
- [ ] **Vulnerable** — recibe más daño 🟡
- [ ] **Taunt** — fuerza al enemigo a atacarte (Coloso) 🟡
- [ ] **Cleanse** — quita estados propios/aliados 🟡
- [ ] **Silence** — no puede usar habilidades ⚪

> 🔑 **`StatusData` necesita un modelo de CARGA, no solo una duración** ✅ *(2026-07-29, [ELEMENTAL-REACTIONS §0.3](ELEMENTAL-REACTIONS.md))*. Los efectos de zona **se pegan al cuerpo**: dentro de la zona duran **eternamente** (hasta que la zona muera) y **al salir DECRECEN**. Se modela con **una sola carga que sube dentro y baja fuera** — y de ahí salen gratis las cuatro propiedades: se mantiene dentro, decae fuera, **recarga al reentrar**, y **cuanto más tiempo estuviste dentro, más te dura fuera**. Es lo que hace que empujar al enemigo un instante a una zona ya pague, sin necesidad de CC.
>
> ⚠️ **Techo duro: máximo 2 efectos pegados a la vez**, y el objetivo real es **0-1** ([§0.9](ELEMENTAL-REACTIONS.md)). No es una regla de balance opcional: es lo que impide que "estar bajo algún efecto" sea el estado por defecto y los efectos dejen de leerse como acontecimientos. Va junto a las reglas de stacking de [H8](ABILITY-EXTRACTION.md).
>
> 📐 **Y si no se ve en el cuerpo del afectado, no entra**: todo estado pegado necesita lectura en el propio monstruo (en caja gris, tinte y escala de cápsula). Es lo que separa *"el barro me está castigando"* de *"la esquiva de este juego a veces falla"*.

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
- [x] **RadiateElement** — irradiar el elemento al impactar → resuelve **una de las cinco salidas** contra el terreno ([ELEMENTAL-REACTIONS §0.2](ELEMENTAL-REACTIONS.md); parámetros: radio, fuerza de impacto → duración R2, instigador R6) 🟢 ✅ *(implementado el 2026-07-29 en `Data/AbilityEffects.cs` + `Environment/ReactionSystem.cs` + `ReactionTable`)*. ⚠️ **Las cinco salidas, no tres**: neutro (nada) · **tu mismo elemento → amplificación** (tu explosión se repite tras un delay corto — ya **no** es "no pasa nada") · elemento que superas → **reacción tuya** · elemento que te supera → **reacción del terreno, sin bando** · cruzando triángulos → **combinación** (daño **neutro**). **Todas tardan un poco en aparecer**: ese *delay* es lo que hace real elegir entre pegar directo (seguro) y apuntar al suelo (más daño + efecto, pero esquivable).
- [ ] **SpawnHazard** — zona AoE persistente (charco de fuego, hielo…) 🟢 — ⚠️ **las 9 combinaciones cerradas el 2026-07-29 le piden cinco palancas que hoy no tiene** *(cada una la usa ≥1 de las 15, y ninguna es cosmética)*:
  - [ ] **`direction` — vector de dirección de la zona** 🟡 *(lo piden la **corriente** —el arrastre lo apunta el jugador con el vector de su golpe aplanado al suelo— y la **colada**. Ya está dictado que **se paga una vez y lo comparten**.)*
  - [ ] **`advance` — zona que AVANZA** 🟡 *(solo la **colada**: nace en el impacto y repta despacio en la dirección del golpe hasta solidificar. Es **lo único del sistema que devuelve distancia**, y **muere al llegar a una celda de Agua** — no es reacción encadenada, es su final. Emparenta con el `SpawnHazard` **móvil** que ya pedía Mantagale [H14](ABILITY-EXTRACTION.md): la posición del hazard deja de ser fija.)*
  - [ ] **`airborne` — zona que cuelga en el aire** 🟡 *(el **velo de esporas**; y la **mecha**, que deflagra en todo su volumen y por eso es **la única que alcanza a un volador**.)*
  - [ ] **`detonateAtEnd` — temporizador que deflagra al morir** 🟡 *(la **mecha**: no hace **nada** mientras dura y suelta todo al final — **invierte el delay** de §0.2. **Detona solo por su reloj, nunca por golpes**, o Pyrecko tendría un detonador gratis sobre su kit de marcas.)*
  - [ ] **`echo` — la zona ENGANCHA los golpes que ocurren dentro** 🟡 *(las **mini explosiones**: cada impacto dentro lo sigue un eco que repite un **%** del golpe original. Es la única zona del sistema que **sube el daño ajeno** — y, con la niebla, una de las dos que **quieres pisar**; hasta ahora el ambiente solo sabía castigar. Es un enganche nuevo: la zona escucha eventos de daño, no solo presencia.)* ⚠️ **El eco NO dispara reacciones**: ya *es* la combinación, y encadenar rompería R1.
- [ ] **BreakLockOnAcquisition** — **a través de la zona no se puede FIJAR objetivo** 🟡 *(nuevo 2026-07-29: es la función de la **niebla** (Aire + Agua), y **tapar la visión** es el verbo de Aire en general —también la **tormenta de arena**. Se corta la **adquisición**, no el guiado: un proyectil ya guiado no se pierde. Barato: `TargetingSystem` ya separa fijar de mantener (`requireLineOfSight` / `sightGrace`).)* ⚠️ *Esto **ya no es del vapor/géiser**: el romper-lock-on se movió a la niebla en la reformulación del 2026-07-29.*
- [ ] **DetonateCrossingProjectiles** — la zona **detona el proyectil elemental que la cruza** 🟡 *(el **velo de esporas**. No es mecánica nueva: el velo **se comporta como un elemento estacionario** para la regla de choques ([§0.2b](ELEMENTAL-REACTIONS.md)), que dice que dos entregas elementales que se tocan resuelven ahí. Solo detona **lo elemental**; se atraviesa andando.)*
- [ ] **ReadSurface** — **lee** el elemento del terreno (sin alterarlo: el terreno nunca cambia) para potenciar la habilidad 🟡
- [ ] **GrabChunk** — arrancar un trozo de terreno; **resuelve y sella una vez** `elemento propio + elemento del terreno` en el blackboard del stance (las marcas de especie NO se llevan: pertenecen al lugar) 🟡 — ⚠️ **precisado el 2026-07-29:** lo que se sella es una **combinación** (cruzando triángulos). Si el agarre dispara una **reacción** (dentro del triángulo, en cualquiera de los dos sentidos), **esta se consume ahí y el trozo queda puro**. Detalle y consecuencias en [H9](ABILITY-EXTRACTION.md).
- [ ] **ThrowHeld** — lanzar lo agarrado como proyectil 🟡
- [ ] **CreateWall/Cover** — generar obstáculo/cobertura 🟡
- [ ] **SpawnPlatform** — plataforma/puente ⚪

> ⚠️ **Las zonas no tienen bando (salvo la reacción propia)** ✅ *(2026-07-29, [§0.7](ELEMENTAL-REACTIONS.md))*: **R6 protege del ESTALLIDO, no de la ZONA**. Una reacción tuya no te golpea al nacer, pero si deja zona **esa zona no distingue a nadie**; y las **combinaciones** y los **choques de elementos** no son de nadie en absoluto. Consecuencia para el catálogo: la entrega de zona necesita el flag `sinBando`, y **no** basta con marcar al instigador.

### 3.8 Spawns / proyectiles
- [ ] **SpawnProjectile** — recto / arco / teledirigido (referencia a `ProjectileData`) 🟢
- [ ] **SpawnZone** — AoE persistente que aplica efectos al entrar 🟡 *(🔒 regla de la auditoría del 2026-07-29: un **CC duro** de zona se aplica **al ENTRAR**, no por pulsos —re-aplicarlo mientras estás dentro no es un evento, es un bloqueo del que no se sale andando—; los **estados blandos** (slow, quemadura) **sí** van por pulsos. Con el modelo de carga de §3.4, "salir" es empezar a decrecer, no perderlo de golpe.)*
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
- [ ] **PointOnGround** — retícula en el suelo 🟡 *(2026-07-29: **apuntar al suelo es jugada de primer orden**, no un extra — el terreno aporta el **30% del daño total** y "un combo bueno consiste en encadenar básicos y habilidades apuntando a zonas elementales" ([§0.2](ELEMENTAL-REACTIONS.md)).)* 🟡 **Sin dictar:** si eso pide una **retícula** propia o basta con el `forwardOffset` del estallido + la orientación de la criatura. Decide Arzpal.
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
