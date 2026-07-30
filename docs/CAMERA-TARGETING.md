# Tatemis3D — Cámara y apuntado (lock-on)

> **TL;DR:** La cámara/apuntado **replica la de BotW / Tears of the Kingdom** (Z-targeting). Se **descartó** el intento previo de cámara TPS al hombro estilo Marvel Rivals: funcionaba, pero **no se notaba la diferencia entre libre y lock** y su fuerte (puntería de shooter) no es nuestro bucle. ✅ **v1 implementada** en `Combat/PlayerCameraRig.cs` (script propio, sin Cinemachine) — **falta afinar parámetros midiendo metraje real** (§3).
> **Mapa:** §1 por qué · §2 spec de comportamiento · §3 qué investigar/medir · §4 implementación actual · §5 estado del código · §6 abiertas · §7 encuadre por habilidad

---

## 1. Por qué BotW/TOTK y no lo que había

**Qué se probó (2026-07-18) y por qué se descartó:** cámara TPS al hombro (`CinemachineThirdPersonFollow`), cuerpo siempre encarando la cámara, strafe lateral, lock-on como asistencia de puntería, y encuadre variable (centrado en libre / al hombro en lock). Resultado: **no se sentía diferencia entre libre y lock**.

**Diagnóstico:** una cámara al hombro está optimizada para **puntería precisa a distancia** (shooter). Por eso se ve casi igual apuntando o no — el cambio de estado no comunica nada. El Z-targeting de Zelda, en cambio, cambia el **modelo de control** (encarar-movimiento ↔ encarar-enemigo + strafe), y *eso* sí se siente.

**Por qué encaja con este juego:**
- Nuestro bucle es **acción melee + habilidades ambientales**, no disparo de precisión.
- **TOTK es la referencia más cercana:** UltraHand y Fusion obligan al jugador a **leer y manipular el mundo** constantemente — exactamente el problema que tenemos con terreno elemental y reacciones ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)). Su cámara resuelve "ver el tablero" y "pelear" con el mismo sistema.
- Está **probadísimo con usuario común**: aguanta 1 enemigo y varios sin incomodar.

**Caveat conocido:** BotW/TOTK resuelven el **apuntado a distancia** (arco) con un **modo de apuntado dedicado** (primera persona / cámara lenta), no con la cámara normal. Nuestras criaturas de rango (**Pyrecko** proyectiles, **Mantagale** colocación del Ciclón) necesitarán un modo análogo **encima** de este sistema — es un diseño aparte, no un bloqueo. Ver §6.

---

## 2. Spec de comportamiento — la referencia, rasgo a rasgo

> El **porqué** (los tres estados, el invariante, el principio de libertad) vive en el ADD → **[add.md §7](add.md)** y su **diagrama** en **[add.html](add.html)**. Aquí está el detalle de la referencia.

### 2.1 · Libre (sin objetivo)
| Rasgo | Comportamiento |
|---|---|
| Seguimiento | Por detrás, algo por encima de la cabeza, mirando levemente abajo. Posición **amortiguada** (muelle), nunca rígida |
| Órbita manual | Yaw libre 360°, pitch con topes |
| ~~**Trail-behind**~~ | ❌ **Divergencia consciente con BotW (2026-07-22, tras playtest).** Estaba implementado (perezoso, con rampa) y se **desactivó** (`autoRecenterSpeed = 0`): en BotW corres en línea recta por un mundo abierto, pero **aquí se combate**, cambiando de dirección a cada segundo — y como el movimiento es relativo a la cámara, una cámara que persigue al movimiento cierra un **lazo de realimentación** que marea. El recentrado **manual** (lock-on sin enemigo) cubre la necesidad sin pelear con el jugador. *Volver a subir el parámetro si algún día hay exploración sostenida.* |
| **Vuelta del picado** | Al **soltar el apuntado**, el picado regresa solo a `freeDefaultPitch` (`aimExitPitchSpeed`): apuntando el pitch es la **mira**, no la cámara, y heredarlo te dejaba mirando al cielo sin haberlo pedido. Se cancela en cuanto tocas la cámara. *El yaw no se toca: apuntando el cuerpo ya encara a la cámara, así que ya viene bien orientado.* |
| Personaje | **Encara su dirección de movimiento** (movimiento relativo a cámara) y **ataca hacia donde encara** |
| Oclusión | Se acerca para no atravesar paredes y **desvanece** lo que se interponga |

### 2.2 · Lock (mantener ZL)
| Rasgo | Comportamiento |
|---|---|
| Adquisición | El enemigo **más relevante en pantalla**, no simplemente el más cercano del mundo |
| **Sin enemigo** | ZL **recentra la cámara** detrás del jugador — la función "resetear cámara", muy usada |
| Encuadre | Cámara por detrás **sobre el eje jugador→enemigo**, con **ambos en pantalla**: enemigo hacia el centro, jugador más abajo |
| Adaptación | Al separarse **se aleja y sube** para que sigan cabiendo los dos |
| Libertad | Órbita H/V **dentro de límites**, y **vuelve al encuadre** al soltar |
| Personaje | **Encara siempre al enemigo**; el movimiento pasa a **strafe** → moverte de lado lo **rodea** |
| Cambio de objetivo | **Flick del stick** hacia otro enemigo |
| Ruptura | Soltar ZL · demasiado lejos · objetivo muerto · **llevar un buen rato sin verlo**. ⚠️ *La línea de visión hace falta para **fijar**, no para **mantener**: en BotW rodear un pilar no rompe el lock. Romperlo al primer obstáculo se siente injusto — corregido el 2026-07-21 con una **gracia** (`sightGrace` 2.5 s) y una **distancia corta** (`closeRangeKeep` 8 u) en la que la oclusión nunca rompe nada.* |

---

## 3. Qué investigar (el trabajo pendiente)

### 3.1 Referencias externas
- **Z-targeting de Ocarina of Time**: es el origen del sistema; buscar la intención de diseño documentada por Nintendo (por qué encarar+strafe, por qué la cámara encuadra a los dos).
- **Breakdowns técnicos/GDC de la cámara de BotW/TOTK** (charlas, análisis de la comunidad, ingeniería inversa de parámetros).
- Comparar con implementaciones equivalentes bien documentadas (Souls-like lock-on, Genshin, Horizon) para ver qué resuelven distinto.

### 3.2 Parámetros a MEDIR (lo más importante)
> Método sugerido: grabar gameplay de BotW/TOTK, avanzar **frame a frame** y usar una **referencia de escala conocida** (la altura de Link) para estimar distancias y ángulos.

**Libre:**
- Distancia de seguimiento y altura de la cámara; **pitch por defecto**.
- ¿Hay **auto-recentrado detrás** del jugador? Con qué **retardo** y **velocidad**, y si solo ocurre al moverse.
- Velocidad de orbitado manual; **límites de pitch** (arriba/abajo).
- **Cuándo sube y cuándo baja** la cámara (¿en función de la velocidad? ¿de la pendiente del terreno? ¿de mirar hacia arriba/abajo?).
- Manejo de **colisión/oclusión** con paredes y FOV.

**Lock:**
- **Cómo encuadra a los dos**: ¿coloca al enemigo en una **posición fija de pantalla**? ¿La cámara se sitúa sobre el **eje jugador→enemigo**, por detrás del jugador?
- **Distancia** en función de la **separación** jugador↔enemigo.
- **Altura/pitch** en función de la separación **y** de la diferencia de altura (importante para voladores, §6).
- **Límites de giro H y V** permitidos mientras se apunta, y **cómo vuelve** al encuadre al soltar el input.
- **Transición** libre↔lock: duración y curva del blend.

**Adquisición y cancelación del objetivo:**
- **Rango** de adquisición y **cono/FOV** de búsqueda; criterio de prioridad (¿el más cercano? ¿el más centrado en pantalla?).
- **Cambio de objetivo** (flick del stick) y su tolerancia.
- **Distancia y condiciones de ruptura** del lock (demasiado lejos, sin línea de visión, objetivo muerto).

**Personaje:**
- Libre: **velocidad de giro** hacia la dirección de movimiento; cómo se orientan los ataques.
- Lock: cómo mantiene el encarado al enemigo mientras hace **strafe**, y cómo se siente **orbitar**.

### 3.3 Preguntas a resolver con la investigación
- ¿El lock es **toggle** (pulsar) o **mantener**? BotW usa **mantener ZL** para el strafe-lock; confirmar y decidir el nuestro (nuestro `InputReader` hoy tiene un **toggle** en `Q` / R3).
- ¿Qué pasa exactamente cuando el objetivo se **pierde de vista** tras un obstáculo?
- ¿Cómo se comporta con **varios enemigos** alrededor (encuadre, cambio, cancelación)?

---

## 4. Implementación actual (v1)

**Decisión: cámara escrita a mano, sin los rigs de Cinemachine.** La de BotW es **bespoke** — ningún rig enlatado la replica — y Cinemachine nos dio fricción repetida (el `Deoccluder` colapsaba la cámara sobre el jugador al detectar su propio collider; nombres de campos a ciegas; el encuadre al hombro no se notaba). Control total = poder clavar el feel e iterar rápido. **Portable a Cinemachine cuando los parámetros estén cerrados.**

`Combat/PlayerCameraRig.cs` (va en el jugador, mueve la Main Camera en `LateUpdate`):
- **Libre:** orbita por detrás con ratón/stick. El *trail-behind* existe en el código (`autoRecenterDelay` + `autoRecenterSpeed`) pero va **desactivado** por decisión de playtest (§2.1); el picado sí vuelve solo al soltar el apuntado.
- **Lock:** se alinea al **eje jugador→enemigo** por detrás del jugador; mira a un punto **entre los dos** (`lockLookBias`) y la **distancia crece con la separación** (`lockDistanceBase + sep·lockDistanceFactor`, tope `lockDistanceMax`) para que quepan ambos. Giro manual H/V limitado (`lockYawLimit`/`lockPitchLimit`) que **vuelve al encuadre** al soltar (`lockRecenterSpeed`).
- **Picado en lock** = `lockBasePitch` + diferencia de altura **real** (mismo suelo → no se dispara hacia arriba; sube/baja solo si el enemigo vuela o está en alto).
- **Suavizado del giro** (`lookResponse`) para matar el jitter del ratón, y **colisión** por SphereCast ignorando al propio jugador.

**Orientación del personaje** (`CreatureController`): lock → encara **siempre** al enemigo (moverse de lado lo **rodea**); libre → encara su **dirección de avance**. El movimiento es **siempre relativo a la cámara**.

**Targeting** (`TargetingSystem`): adquiere el **más relevante en pantalla** — cono `acquisitionCone` alrededor de la mira, puntuando por **lo centrado que esté** con `distanceWeight` de desempate — exige **línea de visión**, y **cambia de objetivo** con `SwitchTarget(±1)` (flick lateral detectado en el rig). **Cancelación automática** por distancia (`breakDistance`), objetivo caído o pérdida de línea de visión. Si no hay nadie a la vista, el botón **recentra la cámara** (`RecenterBehind`).

**Feel:** el giro del cuerpo usa `SmoothDampAngle` (acelera y frena, no es un giro plano) y la posición de la cámara un **muelle** (`SmoothDamp`).

> 🔒 **Principio destilado de tres correcciones seguidas (22/07):** *el jugador manda sobre la cámara; todo automatismo que compita con él marea.* El movimiento es relativo a la cámara, así que **cualquier cosa que mueva la cámara en respuesta al jugador cierra un lazo de realimentación**. Ya nos mordió tres veces: el temblor del apuntado (cámara↔cuerpo), el trail-behind (cámara↔movimiento) y el encuadre del dash alineado al cuerpo. La cámara puede reaccionar a cosas **que el jugador no controla** (un enemigo fijado, el eje de una habilidad ya lanzada), nunca a su propio input.

> ⚠️ **El apuntado NO lleva muelle** (`aimPositionSmoothTime` ≈ 0) y responde más directo al giro (`aimLookResponse`). *Corregido el 2026-07-22: con muelle se columpiaba.* El motivo es estructural y conviene no olvidarlo — **libre y lock recalculan la mirada hacia `lookAtPoint`**, así que si la posición va retrasada la rotación **corrige sola** y el personaje sigue encuadrado; **apuntando la mirada es rígida** (`rotation = rot`, la retícula es el centro), así que el retraso de posición no lo corrige nadie: al girar en círculos el personaje se columpia por el encuadre y la distancia efectiva baila. Una cámara de precisión va **clavada al rig**; lo que se suaviza es el giro, no el sitio.

---

## 5. Estado actual del código (lo que hay que retocar o quitar)

Proyecto: **Unity 6000.3.9f1**, **Cinemachine 3.1.7**, sin `.asmdef` (todo en `Assembly-CSharp`).
Escena y contenido se generan por código: menú **`Tatemis ▸ Sandbox ▸ Construir escena de prueba`** (`Editor/SandboxBuilder.cs`, rebuild limpio).

| Archivo | Estado | Qué hacer |
|---|---|---|
| `Combat/PlayerCameraRig.cs` | ✅ **Nuevo (v1)** — la cámara BotW descrita en §4 | Afinar parámetros con lo medido en §3 |
| `Combat/CreatureController.cs` | ✅ Encarado por modo (lock→enemigo · libre→movimiento) + movimiento relativo a cámara | — |
| `Combat/TargetingSystem.cs` | ✅ Adquisición en pantalla (cono + centrado) · línea de visión **solo para fijar** · cambio de objetivo · cancelación automática con gracia ante la oclusión (§2.2) | Afinar `sightGrace` / `closeRangeKeep` en playtest |
| `Editor/SandboxBuilder.cs` | ✅ Cámara simple (`Camera` + `PlayerCameraRig`), sin Cinemachine · **3 maniquíes** para probar multi-enemigo | — |
| `Input/InputReader.cs` | Lock-on en `Q` / R3 como **toggle** | ⚠️ Revisar **toggle vs mantener** (§3.3) — es la última diferencia de control |

**Eliminados:** `PlayerAimController.cs`, `ThirdPersonCamera.cs`, `CinemachineCameraRig.cs` (intentos previos).
**Lo que dejó de hacer falta:** la **convergencia de retícula** (raycast para compensar el offset del hombro) era para puntería de shooter. Guardar la idea para el **modo de apuntado a distancia** del futuro (§6), no para el melee.

---

## 6. Abiertas

> ✅ **Resuelto:** el *modo de apuntado a distancia* que faltaba para Pyrecko y Mantagale — se generalizó al **tercer estado de cámara** (apuntado sostenido con retícula y convergencia), en vez de atarlo a una habilidad. Ver [add.md §7](add.md).

1. **Validar el multi-enemigo en playtest.** El sandbox ya trae **3 maniquíes** y están la adquisición en pantalla y el cambio de objetivo, pero **no se ha probado a fondo** una pelea contra varios.
2. **Fade de la oclusión:** hoy la cámara solo hace *pull-in* al chocar; falta **desvanecer** lo que se interponga (BotW lo hace).
3. **Voladores (Mantagale):** el encuadre de lock con **gran diferencia de altura** es el caso peor cubierto. El pitch ya reacciona a la altura real, pero está **sin probar** con un enemigo volando.
4. **Toggle vs mantener** para el lock: Arzpal lo dictó **toggle** (2026-07-18); BotW usa *mantener*. Queda como la última divergencia consciente con la referencia.
5. **Afinar parámetros midiendo metraje real** (§3.2): lo actual son valores razonados, no medidos.
6. **Riesgo a vigilar:** que se viva en el estado de apuntado y libre/lock queden vestigiales → la palanca es `aimMoveMultiplier` (que la lentitud muerda).
7. **Lo que el ambiente le pide a esta capa** *(nuevo 2026-07-29, nada implementado)*: tras la reformulación elemental, **dos efectos atacan la vista y el apuntado** ([ELEMENTAL-REACTIONS §4](ELEMENTAL-REACTIONS.md)) — la combinación **Niebla** (Aire + Agua) **rompe la ADQUISICIÓN de lock-on a través de ella** *(no el guiado de lo ya fijado: se corta fijar, no mantener)*, y la reacción **Tormenta de arena** (Aire › Mineral) **tapa la visión**. Lo primero sale casi gratis porque `TargetingSystem` ya separa **fijar** de **mantener** (§2.2); lo segundo es un efecto de pantalla, no de cámara. *(Ojo al renombre: el romper-lock-on era antes del "golpe de vapor" y ahora vive en la Niebla.)*

---

## 7. Encuadre por habilidad (una habilidad puede pedir cámara)

> Añadido el 2026-07-21 con la **embestida de Ottorock**, el primer caso. Sienta el precedente: habrá más.

**La regla:** mientras dura, el encuadre de la habilidad **manda con valores absolutos**; la *transición* se mezcla en los dos sentidos. La cámara no sabe qué habilidades existen y la habilidad no sabe en qué modo estabas — al soltar, se vuelve sola a donde estaba.

> ⚠️ **Se intentó primero sumando** (encuadre = modo + incremento) y **no vale**: partiendo del apuntado —cámara al hombro y cerca— el mismo encuadre seguía sintiéndose de apuntado, y la habilidad se veía distinta en cada modo. *Corregido el 2026-07-22 tras playtest.* Una cámara de habilidad es **cinemática**: su gracia es que se ve **igual siempre**.

| Pieza | Qué hace |
|---|---|
| `PlayerCameraRig.Shot` | Lo que pide una habilidad: **distancia** y **picado** (absolutos), **colocarse detrás** (y a qué velocidad) |
| `PlayerCameraRig.SetShot/ClearShot` | Lo impone sobre el modo activo con un **peso** (`_shotWeight`): entrada seca (`shotBlendIn`), **regreso suave** (`shotBlendOut`). El `ttl` permite dejarlo colgado un rato tras la fase |
| Efecto `CameraShot` | El que lo pide desde los datos de la habilidad — se pone como un efecto más de la fase |

**Durante el encuadre se anula el desplazamiento al hombro** (`lateral → 0`). Es lo que hace que apuntando también se vea la habilidad y no una cámara de apuntado un poco más lejos.

**Decisiones que evitan bugs clásicos:**
- **El encuadre caduca solo.** El efecto lo *renueva cada tick*; si la habilidad muere sin cerrar su fase (interrupción, muerte, cancelación futura), la cámara vuelve sola. Nada puede dejarla enganchada.
- **El picado no toca `_targetPitch`.** Ese es estado persistente del modo: escribirlo dejaría la cámara del jugador movida al terminar la habilidad. Se mezcla solo en la rotación final.
- **El encuadre se coloca sobre el EJE DE LA ACCIÓN, no sobre el cuerpo.** El efecto captura la dirección **una vez** al lanzarse (del `aimPoint`) y la cámara se alinea a ella en yaw *y* en picado — subir 30° pone la cámara por debajo mirando arriba, en el mismo eje del viaje. Dos bugs que esto arregla de raíz:
>   · **El temblor del apuntado.** Alinear con el cuerpo realimenta: en apuntado el cuerpo persigue a la cámara, así que si la cámara persigue al cuerpo el lazo oscila. El eje de la acción es fijo, así que no hay lazo. *(Si algún día un `Shot` no trae eje, alinear con el cuerpo queda prohibido mientras apuntas.)*
>   · **La vuelta de campana a media embestida.** Recalcular el eje cada frame lo invierte en cuanto el caster pasa el punto al que apuntaba. Por eso se captura una vez, no por ahorrar.
- **El encuadre sobrevive a la fase, el secuestro no.** `holdAfter` mantiene la cámara fuera un poco más tras el golpe (que la habilidad no se sienta cortada de golpe), pero en ese resto se **suelta el *fromBehind***: la habilidad terminó y la cámara vuelve a ser del jugador.

### La coreografía: la cámara viaja cuando el cuerpo NO se mueve

> Lo que de verdad marea no es la cámara, es **mover el mundo y el encuadre a la vez**. La embestida se reparte en 3 fases para que nunca coincidan:

| # | Fase | Cuerpo | Cámara |
|---|---|---|---|
| ① | **Carga** (0.30 s) | **clavado** (`rooted`, también en el aire) | **viaja** hasta el encuadre |
| ② | **Embestida** (0.35 s) | se desplaza (6 u en 0.28 s) | ya colocada, quieta |
| ③ | **Deslizamiento** (0.36 s) | **estalla y sigue patinando** hasta pararse (`easeOut`) | hace el **viaje de vuelta** |

*Los tiempos de ① y ③ mandan sobre el blend de la cámara, no al revés: si el viaje no cabe en la fase, se alarga la fase — nunca se acelera la cámara.* **⚠️ La fase ② debe durar más que su `DashMotion`**: si acabara antes, la fase siguiente (`rooted`) cortaría el desplazamiento a medias. Valores afinados en playtest (2026-07-22): 0.22 s de carga se leía como que no había telegrafiado y 0.38 s ya se sentía lento — **0.30 s es la ventana**.

La fase ① estrenó el flag **`rooted`** de `AbilityPhase`, que estaba declarado pero muerto: lo lee `CreatureController.IsRooted` y congela control, inercia **y gravedad** — de ahí que se quede suspendido si carga en el aire. Sirve para cualquier habilidad con tiempos, no solo para esta.

> **La ③ no es una frenada** *(corregido el 2026-07-22)*: frenar en seco es lo menos natural que puede hacer un cuerpo con inercia. El estallido revienta con **su propio parón** (`ElementalBurst.hitstop`, más fuerte que el de un golpe porque un estallido es un evento gordo aunque quite poca vida) y el cuerpo **continúa por donde iba**, patinando hasta pararse (`DashMotion.easeOut + continueForward`). Se descartó el **arco tipo salto de tigre**: el *pounce* es un gesto felino y Ottorock es una nutria — el **deslizamiento** logra lo mismo (completar el gesto en vez de cortarlo) y encima es el movimiento más icónico del animal.

**La otra causa del mareo era el propio dash:** apuntando al cielo, `DashMotion` salía en vertical puro. Ahora tiene **`maxPitch` (35°)** — sigue pudiendo impulsarse hacia un volador, pero no como un cohete. Es un tope del *desplazamiento*, no de la cámara.

**Uso actual:** Embestida Garra (distancia **9.5**, picado **18°**, se coloca detrás a 260°/s) — valores ajustados en playtest el 2026-07-22. Candidatas futuras: el vuelo de Mantagale y las Signature.
