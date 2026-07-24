# Tatemis3D — Anatomía de un tatemis jugable (referencia: Ottorock-Libre)

> **TL;DR:** Cómo se **siente y funciona** un tatemis controlado por el jugador, consolidado en un solo sitio a partir de la primera especie jugable de verdad (**Ottorock, modo Libre**). No sustituye a los docs de cada sistema: es el **mapa de qué encaja con qué**, con los números reales del gate y punteros al detalle. Todo lo aquí descrito ya está implementado y jugado (2026-07-22).
> **Mapa:** §1 movimiento · §2 cámara · §3 habilidades · §4 elemento · §5 feedback visual · §6 feedback físico y combos aéreos · §7 aturdimiento y noqueo · §8 tabla de referencia de Ottorock-Libre · §9 principios transversales

> **Regla de lectura:** un tatemis = **datos, no código** ([ARCHITECTURE](ARCHITECTURE.md)). Casi todo lo de abajo son campos de un `ScriptableObject` que el diseñador ajusta. Cuando algo necesita C# nuevo, es un hueco del catálogo → [ABILITY-EXTRACTION §4](ABILITY-EXTRACTION.md).

---

## 1. Movimiento — [detalle: ABILITY-EXTRACTION §8.1b](ABILITY-EXTRACTION.md)

**La velocidad la manda el stick, no la inercia.** Dos capas independientes que **no** hay que mezclar:
- **Velocidad = lineal con la deflexión** (0..1). `moveSpeed` de la forma = velocidad con el stick a fondo. Soltar el stick para *en seco* — innegociable con lock-on y strafe.
- **Andares = umbrales puramente visuales** (a qué deflexión cambia la animación), no tocan la velocidad. Ottorock-Libre: `Paso corto (0.1) → Caminar cuadrúpedo (0.5) → Deslizamiento (0.75)`.

🔒 **El deslizamiento es un ANDAR, no física:** parece que patina, pero responde 1:1. La única inercia real del juego vive **dentro de habilidades** (`DashMotion.easeOut`). **No hay botón de correr:** correr es identidad de especie (stances), no acción universal — y el presupuesto de inputs no da para más (GDD §6.3).

- **Bloqueo** no clava: recorta la deflexión a **paso corto** (`blockMaxInput`) usando todo el recorrido del stick.
- **Stances** sobrescriben el perfil entero: Ottorock-Sujeción quita la banda de deslizamiento y limita a `maxSpeedMultiplier 0.75` (con la roca en brazos no patina).
- **Salto + gravedad** propios del perfil; el eje vertical **no** se da por pegado al suelo (voladores).
- **Teclado:** WASD es digital → la finura se pierde, solo queda una rampa corta (`inputRamp`). Es el argumento técnico de por qué **el mando es el control estrella**.

## 2. Cámara — [detalle: CAMERA-TARGETING](CAMERA-TARGETING.md)

**Tres estados, los tres elegidos por el jugador**, unidos por un invariante: *el ataque va siempre hacia donde encara el cuerpo*; solo cambia quién lo decide.

| Estado | Quién orienta | Notas |
|---|---|---|
| **Libre** | tu movimiento | orbita por detrás; el *trail-behind* está **desactivado** (perseguir al jugador marea) |
| **Lock** (toggle) | el enemigo | encuadra a ambos; la línea de visión hace falta para **fijar**, no para mantener (gracia 2.5 s, y de cerca nunca rompe) |
| **Apuntado** (mantener) | la cámara | al hombro con retícula, **sin muelle de posición** (rígida = precisa); al soltar, el picado vuelve solo |

🔒 **Principio:** *el jugador manda sobre la cámara; todo automatismo que compita con su input marea.* La cámara puede reaccionar a lo que el jugador **no** controla (un enemigo fijado, el eje de una habilidad ya lanzada), nunca a su propio movimiento.

**Encuadre por habilidad** ([§7](CAMERA-TARGETING.md)): una habilidad puede pedir cámara (efecto `CameraShot`) con valores **absolutos** que mandan mientras dura y se sueltan al terminar. Se coloca sobre el **eje de la acción**, no sobre el cuerpo. Rig propio (`PlayerCameraRig`), sin Cinemachine.

## 3. Habilidades — [detalle: ARCHITECTURE §2](ARCHITECTURE.md) · [add §2](add.md)

**Kit de 3** (con coste) + **cadena de básicos** (gratis) + acciones universales (salto, bloqueo, contextual). Cada habilidad = **fases**, y cada fase = una lista de **efectos** que no se conocen entre sí:

| Categoría | Qué hace | Ejemplos |
|---|---|---|
| **Actuador** | entrega y **reporta impactos** (`reportHit`) | `MeleeHitbox` · `SweepHitbox` · `DashMotion` · `ElementalBurst` |
| **Payload** | la consecuencia (`OnCast`/`OnHit`) | `DealDamage` · `Knockback` · `CarryToFront` · `ApplyStatus` |
| **Presentación** | ni entrega ni causa: encuadra | `CameraShot` |

🔒 **Reglas que se pagan si se olvidan:**
- **Los payloads son de la FASE, no del actuador** → dos actuadores que reporten en la misma fase duplican el `DealDamage`. Un actuador con payload propio va en **su propia fase**.
- **Un ataque que dura reparte GOLPES, no daño continuo** (`SweepHitbox.hitInterval`, ref. Smash) — cada golpe es un `HitInfo` normal, así valen los payloads de siempre. [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md).
- **Detección se muestrea, consecuencias se cronometran:** la cadencia va por reloj (nunca por frame) y las hitboxes que se mueven **barren el tramo** (no muestrean un punto), o a poco framerate un enemigo se cuela entre muestras.
- **Estado por activación en el blackboard (`ctx.bb`), nunca en un campo del efecto** — el efecto vive en un asset compartido por todas las criaturas.
- **Anim-events:** los efectos se disparan por el **reloj de la fase**; el día que haya clips, ese reloj lo releva un anim-event sin tocar los efectos.

## 4. Elemento — [detalle: GDD §7](GDD.md) · [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)

- **Los básicos van NEUTROS** (x1, sin reacción): si la acción que más repites llevara elemento, pelear contra quien te supera sería imposible. El elemento es palanca del **kit**, no impuesto de cada golpe.
- **Excepción:** el **último eslabón** de la cadena puede **estallar en el elemento de la especie** (Ottorock: 4 garras neutras + remate que estalla en Mineral).
- **`ElementalBurst`** siempre usa el elemento de la **especie**, no el del golpe → un básico neutro puede rematar estallando en Mineral sin contradecir la regla.
- Multiplicador: débil **1.5x** · resiste **0.5x** · neutral **1x** (el 2x solo existe contra forma elemental pura). `Vulnerable` lo multiplica aparte (×1.35).

## 5. Golpear — feedback visual — [código: `Combat/HitFeedback.cs`]

Un solo punto (`HitFeedback.Report`) convierte cada golpe en feedback, y todo escala con **una** noción: el **peso** del golpe (`Weight` = 60% daño relativo a *ese* monstruo + 40% avance del combo).

| Canal | Qué dice | Cómo |
|---|---|---|
| **Flash** (`HitFlash`) | efectividad (color) + contundencia (duración) | tinte del cuerpo golpeado, `MinDuration` 0.10 → `Max` 0.22; sostén + caída |
| **Número** (`DamagePopups`) | resultado (el daño ya multiplicado, sin la cuenta), efectividad (color/tamaño) | vuela en la dirección del knockback, se abre a un costado alternando lado |
| **VFX** (`ImpactVFX`) | dónde y con qué fuerza | "pop" núcleo+anillo aditivo, color de efectividad, 1.3–3.2 u |
| **Estado** (`StatusController`) | qué condición sufre | **etiqueta anclada** sobre el enemigo (no vuela: un estado es condición, no evento) |

Colores de efectividad, una sola tabla (`HitFlash.ColorFor`): **cálido = 1.5x · blanco = 1x · frío = 0.5x**.

## 6. Golpear — feedback físico y combos aéreos — [código: `Combat/HitReactor.cs`]

**Contundencia (T1.6):** hitstop + sacudida, con el **mismo peso** del punto 5.
- **Hitstop** en tiempo real (0.02–0.09 s por peso), no se acumula al encadenar; canal aparte `Shockwave` para eventos gordos (un estallido pide 0.14 s aunque quite poca vida).
- **Sacudida:** desplazamiento (0.025–0.18 u) **+ giro angular** (0.15°–0.9°) — el giro se nota más en 3ª persona y no arrastra el encuadre. Suave a propósito.

**Empuje y recolocación:**
- **`Knockback`** — empujón por golpe. `useMotionDirection` (arrastra en la dirección **3D** del dash → embestir en diagonal se lleva al enemigo arriba, ref. *side B* de Meta Knight), `verticalScale` (afina solo la vertical), `lift` (elevación, en u/**seg**: la altura real es `lift × duration`), `hang` (ver abajo).
- **`CarryToFront`** — **correa de seguridad:** mantiene al enemigo a ≤ distancia y (con `matchHeight`) a tu altura, pase lo que pase con el empuje. Es lo que hace que el remate aéreo **llegue siempre** en vez de depender de que las velocidades cuadren. ⚠️ Regla: *el empuje no puede ir más rápido que tu propio dash*, o el enemigo se escapa por delante.

🔒 **Suspensión en el aire (juggle / hang time):** golpear y ser golpeado **suspenden la caída** un instante, en los dos bandos — sin eso, un combo aéreo se cae solo a media cadena. Piezas que lo hacen jugable:
- **El hang cubre las fases de recuperación**, no solo el hueco entre golpes: tras una embestida pasan ~0.7 s antes de poder encadenar, así que el hang es de ~1 s. Si dura menos, caes antes de rematar.
- **`hangDamp` alto (160)**: frena la caída casi al instante. Bajo, "frena un poco" en vez de sostener y no se siente.
- **Guardarraíl:** presupuesto acumulable (`maxHang ≈ 3 s`) que mide **tiempo suspendido**, no golpes, y **se recarga al tocar suelo**. Impide el juggle infinito, el fallo clásico del género.
- **Cola al terminar** (`hangTailAfterAbility ≈ 0.45 s`): al dejar de encadenar caes aunque quedara hang — el hang largo es para cubrir habilidades, no para flotar cuando ya paraste.
- **La embestida abre la ventana aérea; el resto solo sostiene** (`lift = 0` en mordisco y remate). Si cada golpe elevara, el combo subiría solo hasta el techo.

## 7. Aturdimiento y noqueo — [detalle: TAMER §4](TAMER.md) · [VITALITY-DEATH](VITALITY-DEATH.md)

- **Aturdimiento tipo postura** (`StaggerReceiver`, ref. Sekiro): proporcional a la vida, sube con **combo**, decae si dejas de golpear. Al llenarse abre la ventana de KO.
- **Noqueo total** (aturdimiento + vida 0) → **`Downed`** con **gracia invulnerable de 3–5 s**: el margen para decidir, sin el cual el combo en curso remataría por inercia.
- **Sin prompt ni botón:** pasada la gracia, **atacar remata** (muerte por elección); el **menú del Tamer** captura. Si no decides, se retira/huye.
- **Vitalidad** (barra externa, propios): drena **poco al ser noqueado** y **mucho si le pegan caído**; a 0, muerte permanente pero siempre evitable. 🟡 sin ratificar.

## 8. Tabla de referencia — Ottorock-Libre (números del gate)

> Placeholders de caja gris, afinados en playtest. `Editor/OttorockContent.cs` los genera como assets reales.

**Stats:** Vida **4000** (8/10 × 500 HP/punto) · daño base **14** · `moveSpeed` **6** (a stick a fondo).

| Habilidad | Coste/CD | Daño | Elemento | Clave de feel |
|---|---|---|---|---|
| **Cadena básicos 1–4** | gratis | 14/14/18/20 | Neutro | step-in 0.9–1.2 > empuje (0.72) → cierra distancia; `hang` 0.5 sostiene |
| **5º · Garra sísmica** | gratis | 26 + estallido 16 | Neutro + **estallido Mineral** | remate lento (0.62 s); no eleva |
| **Movilidad · Embestida Garra** | 12 / 2.5 s | 8×~4 (multi-hit) + estallido 14 | Neutro + **estallido Mineral** | 3 fases (carga *rooted* → embestida → deslizamiento); **abre la ventana aérea**; cámara propia; slow 2 s del estallido |
| **Flex · Mordisco Ferroso** | 20 / 3 s | 50 + estallido 15 | Especie (Mineral) | golpe fuerte; sostiene, no eleva |
| **Signature** | — | — | — | Cambio de estado Libre↔Sujeción, **post-gate** (null) |

**Locomoción Libre:** andares `Paso corto 0.1 · Caminar 0.5 · Deslizamiento 0.75`, `maxSpeedMultiplier 1`, `blockMaxInput 0.25`.

## 9. Principios transversales (lo que aplica a CUALQUIER tatemis futuro)

1. **Datos, no código.** Una especie nueva = editar assets. Si obliga a tocar C#, es un hueco → anotarlo.
2. **Bespoke primero, extraer después.** No se generaliza hasta ≥2 usos reales. Ottorock ya extrajo: multi-hit, empuje continuo, perfil de locomoción, cámara por habilidad.
3. **Toda especie es autosuficiente** (CREATURES §2): su CC prepara su propio daño, puede ganar sola.
4. **Una sola noción de contundencia** (el `Weight`): destello, número, VFX, hitstop y shake beben de ahí. Nunca afinar cinco cosas por separado que deberían decir lo mismo.
5. **El jugador manda; los automatismos que compiten con su input marean** — vale para cámara, para inercia y para el juggle (que se corta al parar).
6. **Los umbrales de "cuánto" y "cómo se ve" son capas separadas** — velocidad vs. andar, daño vs. VFX. Acoplarlos hace que afinar una cosa rompa la otra.
