# Tatemis3D — Arquitectura del sistema de habilidades y combate

> Arquitectura técnica inicial del prototipo (Nivel A). Objetivo: **data-driven** para iterar diseño sin recompilar, y que **añadir una criatura sea añadir datos, no código**. Pensado para Unity + URP. Es un punto de partida pragmático, no una biblia — ajústenlo al construir.
>
> **Mapa:** Principios · Mapa de sistemas (qué script hace qué) · §1 Datos (ScriptableObjects) · §2 Efectos componibles · §3 Runtime (componentes) · §4 Aturdimiento y captura · §5 Reacciones ambientales · Orden de construcción · Notas pragmáticas. → Ir a la sección que pida la tarea, no leer entero.

## Principios
1. **Datos separados de comportamiento.** Criaturas, habilidades y elementos son `ScriptableObject`. El diseñador ajusta en el inspector; el programador solo añade *tipos de efecto* nuevos cuando de verdad hace falta.
2. **Efectos de habilidad componibles.** Una habilidad = una lista de efectos pequeños y reutilizables (daño, dash, aplicar estado, irradiar elemento…). El 90% de las habilidades se arma combinando efectos existentes.
3. **No sobre-ingenierizar el resto.** Cámara, mundo y arte se quedan en caja gris hasta validar el feel. La inversión data-driven se concentra en el sistema de habilidades porque es lo que se reutiliza en cada criatura.

## Mapa de sistemas

```
CreatureController (MonoBehaviour, criatura activa)
├── InputReader            → mover / saltar / ataque básico / lock-on / 3 habilidades / contextual (interactuar) / bloqueo / órdenes / swap
├── LocomotionProfile (SO) → velocidad LINEAL con el stick + andares visuales por umbral; por forma, sobrescribible por stance (ABILITY-EXTRACTION §8.1b)
├── Locomotion + StateMachine (Locomotion, Ability, Block/ElementForm, Hitstun, KO)
│                            `IsRooted` ← fases `rooted`: clava el cuerpo (sin control, inercia NI gravedad)
│                            `GrantHang` ← suspensión en el aire (juggle): golpear/ser golpeado no te desploma; se corta al parar
├── AbilityCaster          → valida cooldown + energía, dispara animación, ejecuta efectos fase a fase
├── ElementalEnergy        → recurso tipo tinta (Splatoon), regenera por segundo
├── TargetingSystem        → lock-on (la línea de visión hace falta para FIJAR, no para MANTENER)
└── PlayerCameraRig        → los 3 estados de cámara + encuadre por habilidad (CAMERA-TARGETING §7)

CombatantHealth / StaggerReceiver (en enemigos salvajes; noqueo total → Downed con gracia invulnerable, TAMER §4)
StatusController           → estados temporales (slow/vulnerable/aturdido/quemadura-DoT), etiqueta anclada al afectado
HitReactor                 → knockback (3D) / carry (correa, matchHeight) / flinch / gravedad propia / hang (juggle)
HitFeedback                → punto único golpe→feedback; calcula el PESO del golpe (daño relativo + combo)
├── HitFlash               → destello del cuerpo golpeado (color = efectividad, duración = peso)
└── DamagePopups           → número de daño (color = efectividad, tamaño = peso, vuela con el knockback)
CaptureSystem              → KO → ventana → Sludge → Tótem
ReactionSystem + ElementalSurface + ReactionTable (SO) → el impacto irradia elemento → 5 salidas (§5)
│                            reacción = ReactionEffect (explosión / zona por pulsos) sobre terreno fijo
│                            ⚠️ el CONTENIDO de la tabla es anterior al rediseño del 29/07 → reautorar
├── CrystalNode            → golpeable: suelta su carga elemental donde está plantado (★ del slice)
├── HexGridOverlay         → hexágonos leves sobre cada parche (ARENA §1; el marco de combate, con la IA)
└── ElementalSpray (debug) → F2/G: dispara cualquier elemento sin criatura que lo genere
TypeChart (SO)             → los 2 triángulos elementales
CombatBootstrap            → cablea los servicios globales (TypeChart, Reactions) en CombatServices
TeamManager                → equipo de 3, swap con cooldown
```

---

## 1. Datos (ScriptableObjects)

```csharp
public enum Element { Naturaleza, Agua, Fuego, Mineral, Rayo, Aire }
public enum Archetype { Coloso, Luchador, Ejecutor, Zonificador, Controlador }
public enum AbilitySlot { Movilidad, Flex, Signature }
public enum AbilityElement { Neutro, Especie, Secundario }   // con qué elemento golpea una habilidad;
                                                            // los BÁSICOS van Neutro por regla (GDD §7)

[CreateAssetMenu(menuName = "Tatemis/Type Chart")]
public class TypeChart : ScriptableObject
{
    // Dos triángulos. X supera a Y (x1.5; sube a x2 si el defensor está en forma elemental — lo aplica BlockParams).
    // Naturaleza>Agua>Fuego>Naturaleza   |   Mineral>Rayo>Aire>Mineral
    public float Multiplier(Element atk, Element def)
    {
        if (Beats(atk, def)) return 1.5f;
        if (Beats(def, atk)) return 0.5f;
        return 1f;
    }

    static bool Beats(Element a, Element b) =>
        (a == Element.Naturaleza && b == Element.Agua)  ||
        (a == Element.Agua       && b == Element.Fuego) ||
        (a == Element.Fuego      && b == Element.Naturaleza) ||
        (a == Element.Mineral    && b == Element.Rayo)  ||
        (a == Element.Rayo       && b == Element.Aire)  ||
        (a == Element.Aire       && b == Element.Mineral);
}

[CreateAssetMenu(menuName = "Tatemis/Ability")]
public class AbilityData : ScriptableObject
{
    public string id;
    public string displayName;
    public AbilitySlot slot;
    public float energyCost;
    public float cooldown;
    public string animTrigger;
    [SerializeReference] public List<AbilityEffect> effects = new();
}

[CreateAssetMenu(menuName = "Tatemis/Creature Species")]
public class CreatureSpeciesData : ScriptableObject
{
    public string id;
    public string displayName;
    public Element element;
    public Archetype archetype;
    public List<EvolutionForm> forms; // [0]=base(bebé) … [n]=final
}

[System.Serializable]
public class EvolutionForm
{
    public string formName;
    public GameObject modelPrefab;      // placeholder por ahora
    public BaseStats stats;             // define el arquetipo
    public AbilityData movilidad, flex, signature;
    public List<AbilityData> basicChain; // cadena de básicos EN ORDEN: cada golpe encadenado avanza
                                         // un eslabón y vuelve al primero al completarla (BasicAt)
    public BlockParams block;           // params de la forma elemental pura
    public Element secondaryElement;    // solo forma final: utilidad, NO cambia debilidades
    public int levelsInForm = 2;        // 2–3 niveles de evolución progresiva
}

[System.Serializable] public struct BaseStats { public float maxHealth, damage, moveSpeed; }
[System.Serializable] public struct BlockParams { public float energyRegenMultiplier; public float weaknessDamageMultiplier; } // p.ej. 2.5x regen; la debilidad pasa de 1.5x a 2x en forma pura
```

## 2. Efectos de habilidad componibles

Polimórficos con `[SerializeReference]` → se combinan en el inspector sin crear un asset por efecto.

### 2.0 · La distinción que ordena todo: ACTUADOR vs PAYLOAD
> ✅ Ya implementado en el scaffold (`Data/AbilityCore.cs`, `Data/AbilityEffects.cs`). Formalizado 2026-07-18.

Casi nada se aplica directamente: **algo tiene que llevarlo**. Por eso los efectos de una fase son de dos naturalezas y **no se conocen entre sí**:

| | Qué hace | Ejemplos |
|---|---|---|
| **Actuador (entrega)** | Crea el vehículo y **reporta impactos** vía `ctx.reportHit(HitInfo)` | `MeleeHitbox` · **`SweepHitbox`** · `SpawnProjectile` · `ThrowHeld` · `SpawnHazard` · **`ElementalBurst`** · `PullField` |
| **Payload (consecuencia)** | Declara **cuándo** aplica con `ApplyWhen { OnCast, OnHit }` y se ejecuta al dispararse ese hook | `DealDamage` · `ApplyStatus` · **`Knockback`** (3D: `useMotionDirection`/`verticalScale`/`lift`/`hang`) · **`CarryToFront`** (correa, `matchHeight`) · `RadiateElement` |
| **Presentación** *(3ª categoría, 2026-07-22)* | Ni entrega ni causa daño: **encuadra**. La habilidad pide cámara y la cámara no sabe de habilidades | **`CameraShot`** |

> **Combate aéreo (juggle):** `Knockback.hang` / `CreatureController.GrantHang` **suspenden la caída** al golpear y al ser golpeado, en los dos bandos — sin eso un combo aéreo se cae a media cadena. Guardarraíl: presupuesto acumulable (`maxHang`) que mide tiempo suspendido y se recarga al tocar suelo; cola corta al dejar de encadenar. `CarryToFront.matchHeight` es la **correa** que impide que el enemigo salga del alcance del siguiente golpe. Detalle → [TATEMIS-BASE §6](TATEMIS-BASE.md).

> 🔒 **Los payloads son de la FASE, no del actuador.** `reportHit` los dispara a todos por igual, así que **dos actuadores en una misma fase comparten sus payloads** — y si los dos reportan, el `DealDamage` se cobra dos veces. Regla práctica: *un actuador cuyos impactos necesiten payload propio va en **su propia fase***. Es lo que hace la embestida de Ottorock: el arrollamiento va en la fase ②, y el estallido en la ③ con el `slow` colgado de **sus** impactos, no de los del arrollamiento. *(`ElementalBurst.reportHits` está por defecto en `false` justo por esto: si estalla dentro de una fase que ya tiene un `DealDamage`, reportar duplicaría el daño.)*
>
> ⚠️ *Deuda conocida: `ElementalBurst` aplica su propio daño en vez de delegarlo a un `DealDamage` — es actuador y payload a la vez. La reescritura elemental de Fase 2 (2026-07-29) NO la saldó a propósito: la limpieza exige llevar el elemento en el `HitInfo` y tocar el flujo de payloads compartidos, y no era necesaria para irradiar al terreno. Queda para un refactor de `HitInfo` con calma.*

> **Un ataque que dura reparte GOLPES, no daño continuo.** `SweepHitbox.hitInterval` emite un golpe cada X segundos mientras dura la fase (multi-hit): cada uno es un `HitInfo` normal, así que los payloads sirven tal cual y **no hacen falta efectos de "daño sostenido"**. La cadencia va por **reloj** (nunca por frame) y cada golpe **barre el tramo desde el anterior**. Razones y regla de oro del muestreo → [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md).

**El pegamento es `reportHit`.** El payload **no sabe** quién lo entregó: `DealDamage{when=OnHit}` es idéntico venga de un hitbox melee, de un proyectil teledirigido o de una zona que alguien pisó. Eso es lo que permite componer en el inspector sin escribir código nuevo: **un actuador + N payloads**.

### 2.1 · Tres tiempos de vida (y por qué el payload viaja *desacoplado*)
| Vida | Resuelve | Ejemplos |
|---|---|---|
| **Instantánea** | dentro de la animación | `MeleeHitbox`, `AreaBurst` |
| **En vuelo** | segundos después, al impactar | proyectil de Pyrecko · roca lanzada de Ottorock |
| **Persistente** | vive en el mundo con ciclo propio y aplica **al entrar o al permanecer** | zonas encendidas de Pyrecko · tornados de Mantagale · marcas de especie |

⚠️ **Consecuencia crítica:** los dos últimos **sobreviven a la habilidad que los creó**. Así que el actuador entrega al vehículo una **copia del payload + un snapshot del contexto** (elemento, lanzador, stats y —para Ottorock— la reacción sellada). No puede quedarse con una referencia viva al estado del lanzador.

*Por qué importa, con dos casos reales:* Ottorock **lanza la roca y sale de Sujeción**; si el proyectil leyera su estado actual al impactar, la reacción sellada ya no existiría — y la ficha dice que el impacto **sí** la aplica. Ídem Pyrecko: si lo noquean después de detonar, sus zonas **siguen ardiendo**.

> ⚠️ **El estado por activación va en el blackboard (`ctx.bb`), nunca en un campo del efecto.** La instancia del efecto vive dentro del `AbilityData`, que es un **asset**: la comparten todas las activaciones y todas las criaturas que usen esa habilidad. Un contador en un campo se pisaría entre ellas. Clave única por instancia: `"loQueSea." + GetHashCode()`.

### 2.2 · Estado del catálogo de actuadores
- **Ya en el scaffold:** `MeleeHitbox` (reporta con `reportHit`) · **`SweepHitbox`** (multi-hit con barrido continuo) · `DashMotion` (con `maxPitch`, tope de inclinación) · **`ElementalBurst`** (estalla en el elemento de la **especie**, aunque el golpe sea neutro — y desde Fase 2 **irradia al terreno** al reventar) · **`CameraShot`** · **`RadiateElement`** ✅ *(2026-07-29, ex-StainTerrain: irradia el elemento del GOLPE al terreno con la API nueva; una vez por fase en OnHit)*.
- **Faltan, por olas:** `SpawnProjectile` (recto/homing) y `SpawnHazard` con `applyOn {Enter, Stay}` → **ola Pyrecko** · `ThrowHeld` → **ola Ottorock** · `PullField` y fuerzas que mueven *otros hazards* → **ola Mantagale** ([ABILITY-EXTRACTION](ABILITY-EXTRACTION.md) H14–H16).

```csharp
public class AbilityCtx            // vivo durante UNA activación; lo mutan la habilidad y los carriers
{
    public CreatureController caster;
    public Vector3 aimPoint;
    public ICombatant lockTarget;  // puede ser null (habilidades sin objetivo)
    public Element? element;       // NULL = golpe NEUTRO (x1, sin reacción) — así van los básicos, GDD §7
    public System.Action<HitInfo> reportHit;
    public readonly Dictionary<string, object> bb;   // blackboard: estado por activación (ver abajo)
}

[System.Serializable]
public abstract class AbilityEffect
{
    public abstract void Execute(in AbilityContext ctx);
}

// --- Ejemplos de efectos reutilizables ---
[System.Serializable]
public class DealDamage : AbilityEffect
{
    public float amount;
    public float staggerAmount;      // cuánto llena el aturdimiento del enemigo
    public override void Execute(in AbilityContext ctx)
    {
        if (ctx.target == null) return;
        float mult = CombatServices.TypeChart.Multiplier(ctx.element, ctx.target.Element);
        ctx.target.TakeHit(amount * mult, staggerAmount * mult);
    }
}

[System.Serializable]
public class Dash : AbilityEffect { public float distance; public float time; /* mueve al caster */ 
    public override void Execute(in AbilityContext ctx) { /* … */ } }

[System.Serializable]
public class ApplyStatus : AbilityEffect { public StatusType type; public float duration;
    public override void Execute(in AbilityContext ctx) { /* … */ } }

[System.Serializable]
public class RadiateElement : AbilityEffect { public float radius, impactStrength;  // irradia ctx.element al impactar
    public override void Execute(in AbilityContext ctx) =>
        CombatServices.Reactions.Impact(ctx.aimPoint, radius, ctx.element, impactStrength, ctx.caster); }  // R2 + R6
```

> **Regla práctica:** cuando una habilidad no se puede armar con efectos existentes, se crea **un** tipo de efecto nuevo — no una clase por habilidad. Así el catálogo de criaturas crece con datos.

## 3. Runtime (componentes)

```csharp
// Instancia viva de una criatura del equipo
[System.Serializable]
public class CreatureInstance
{
    public CreatureSpeciesData species;
    public int formIndex;        // forma evolutiva actual
    public int level;            // nivel dentro de la forma (evolución progresiva)
    public float evoProgress;    // avance de mini-misiones → sube nivel/forma
    public EvolutionForm Form => species.forms[formIndex];
}

public class ElementalEnergy : MonoBehaviour
{
    public float max = 100f, current, regenPerSec = 10f;
    float regenMult = 1f;
    public bool TrySpend(float c) { if (current < c) return false; current -= c; return true; }
    public void SetRegenMultiplier(float m) => regenMult = m; // Bloqueo la sube
    void Update() => current = Mathf.Min(max, current + regenPerSec * regenMult * Time.deltaTime);
}

public class AbilityCaster : MonoBehaviour
{
    [SerializeField] ElementalEnergy energy;
    readonly Dictionary<AbilityData, float> _readyAt = new();

    public bool TryCast(AbilityData a, in AbilityContext ctx)
    {
        if (_readyAt.TryGetValue(a, out float t) && Time.time < t) return false;
        if (!energy.TrySpend(a.energyCost)) return false;
        _readyAt[a] = Time.time + a.cooldown;
        // Disparar animación; los efectos normalmente se ejecutan en un anim-event / al impactar.
        foreach (var e in a.effects) e.Execute(in ctx);
        return true;
    }
}
```

El `CreatureController` es una **máquina de estados** simple: `Locomotion → Ability → Hitstun → Block(ElementForm) → KO`. El bloqueo entra en un estado propio donde **no se puede atacar**, sube el multiplicador de regen de energía y marca la vulnerabilidad ×2 a la debilidad.

## 4. Aturdimiento y captura

```csharp
public interface ICombatant { Element Element { get; } void TakeHit(float dmg, float stagger); }

public class StaggerReceiver : MonoBehaviour, ICombatant   // en criaturas salvajes
{
    public Element Element => _species.element;
    public float maxHealth, health, maxStagger, stagger;

    public void TakeHit(float dmg, float staggerAmt)
    {
        health  = Mathf.Max(0, health - dmg);      // nunca "muere": al mínimo se agota
        stagger = Mathf.Min(maxStagger, stagger + staggerAmt);
        if (stagger >= maxStagger || health <= 0) OpenKOWindow();
    }
}
```

- Las **reacciones ambientales** llaman a `TakeHit` → recompensan el USP. **Presupuesto ✅ *(2026-07-29)*:** el reparto del daño total de un duelo es **30% básicos / 40% habilidades / 30% terreno**, y una reacción vale **casi lo que una habilidad promedio** — ese es el número contra el que se afinan sus `amount`/`staggerAmount`, no "aturdimiento alto porque sí".
- Al KO: fin del combate → **ventana temporizada** → lanzar Sludge → probabilidad de resistir `f(health, rareza)` → si resiste o expira, huye.

## 5. Reacciones ambientales

> Modelo **reformulado de raíz ✅ 2026-07-29** — detalle completo en **[ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md)**, **empezando por su §0**, que es de donde cuelga todo lo demás.
>
> ⚠️ **El código que describe esta sección es ANTERIOR al rediseño** (se escribió el mismo día, contra el modelo viejo de dominancia/anulación). Los **sistemas siguen valiendo** —`ElementalSurface`, `ReactionSystem`, `ReactionTable`, `ReactionEffect`, `RadiateElement`—, pero **la tabla hay que REAUTORARLA ENTERA**: cambian los nombres, los efectos, de qué elemento es el daño y qué pares tienen entrada. `Editor/ReactionTableContent.cs` es un asset a rehacer, no a parchear.
>
> **Lo que no cambió:** el terreno **no muda** (sin `currentState`) · la duración es proporcional al impacto (R2) · un impacto resuelve **contra el terreno**, nunca contra un efecto activo (R1).
>
> **Lo que sí cambió, y hay que llevar al código:**
> - **Un elemento puro solo hace daño.** Los efectos nacen **únicamente de la mezcla**.
> - **Cinco salidas, no tres:** neutro (nada) · **mismo elemento → amplificación** (la explosión se repite tras un delay corto; ya no es "no pasa nada") · elemento al que superas → **reacción tuya** (R6 te respeta) · elemento que te supera → **reacción del terreno, sin bando** · cruzando triángulos → **combinación**.
> - **Reacción = explosión con daño DEL ELEMENTO dominante**, que **sí pasa por el `TypeChart`** · **combinación = transformación con daño NEUTRO**, que **no pasa por el `TypeChart`**. Hoy `ReactionEffect` hace daño neutro en los dos casos.
> - **Delay obligatorio:** reacción, combinación y amplificación **tardan un poco en aparecer**. Es lo que las hace esquivables y lo que hace real elegir entre pegar directo o apuntar al suelo. No existe en el código.
> - **El efecto se pega al afectado:** dentro de la zona dura mientras dure la zona; **al salir decrece** en vez de cortarse (modelo de carga: sube dentro, baja fuera, volver a entrar recarga). **Techo de 2 efectos** pegados a la vez, con objetivo de 0–1.
> - **R3 pasa a ser un periodo de gracia**, no cancelación instantánea — y **repetir la MISMA reacción la refresca** en vez de bloquearse (sin esto la cadena de Ottorock-Sujeción no funciona).
> - **Las mezclas no necesitan suelo:** dos entregas elementales que se cruzan en el aire resuelven ahí, sin bando (§0.2b) → `Impact(pos,…)` deja de ser el único punto de entrada.

```csharp
public class ElementalSurface : MonoBehaviour { public Element element; } // fijo: río=Agua, roca volcánica=Fuego…

// Par ORDENADO (terreno, entrante) → payload. Ordenado porque R4 lo exige: (Agua,Fuego) = GÉISER
// sin bando (reacción del terreno, friendlyFire en el dato) y (Fuego,Agua) = el mismo géiser como
// reacción del Agua entrante, que sí respeta a su bando.
// ⚠️ REAUTORAR: las 30 entradas que hay hoy son del modelo viejo. El reparto nuevo es
// 6 reacciones + 6 reacciones del terreno + 9 combinaciones x 2 direcciones + los 6 pares
// mismo-elemento, que ya NO son "nada" sino AMPLIFICACION. Par sin entrada = sin reacción.
public class ReactionTable : ScriptableObject
{
    public List<ReactionEntry> entries;
    public bool TryResolve(Element terrain, Element incoming, out ReactionEntry e) { /* lineal */ }
}

public class ReactionSystem : MonoBehaviour        // Environment/ReactionSystem.cs, tal cual
{
    [SerializeField] ReactionTable table;          // + referenceImpact/min/max de la curva R2
    public void Impact(Vector3 pos, float radius, Element incoming, float impactStrength, ICombatant instigator)
    {
        CancelActiveAt(pos, radius);               // R3 PRIMERO: limpiar lo vivo es counterplay, haya o no reacción nueva
                                                   // ⚠️ R3 pasa a PERIODO DE GRACIA: no se convierte un efecto vivo
                                                   //    hasta que expira, y repetir el MISMO efecto lo REFRESCA
        var surface = SurfaceAt(pos, out var ground);   // RaycastAll: busca la superficie entre TODOS los hits
        if (surface == null) return;                                   // neutro → nada (R5)
        if (!table.TryResolve(surface.element, incoming, out var e)) return;
        float duration = e.baseDuration * Clamp(impactStrength / referenceImpact);   // R2
        ReactionEffect.Spawn(e, ground, radius, duration, instigator);               // R6 lo aplica el efecto
    }
}
```

`ReactionEffect` es el genérico único: **reacción** = explosión al nacer (± estado/empuje; respeta bando salvo `friendlyFire`) · **combinación** = zona por pulsos de reloj (neutral SIEMPRE). Lo bespoke (el root que deja atacar y bloquear, la niebla que rompe la **adquisición** de lock-on, la propagación del incendio, la colada que avanza) llega por olas con su criatura.

> ⚠️ **Lo que le falta a `ReactionEffect` para cumplir el modelo de 2026-07-29:** el **delay** antes de aparecer · que el daño de reacción sea **del elemento dominante** y pase por el `TypeChart` (hoy es neutro en los dos casos) · que el estado **decrezca al salir** en vez de cortarse.
>
> 🔑 **R6 protege del ESTALLIDO, no de la ZONA.** Tu propia reacción no te golpea al nacer, pero si deja una zona que dura, **esa zona no distingue a nadie**: te afecta si entras. Es una regla de gameplay, no un bug — no "arreglarla" poniéndole bando a la zona.

---

## Orden de construcción (mapea a las fases del ROADMAP)

| Fase | Qué de esta arquitectura se implementa |
|---|---|
| **0** | `CreatureController` (locomoción + salto), `TargetingSystem` (lock-on), cámara |
| **1** | `AbilityData` + `AbilityEffect` (DealDamage, Dash), `AbilityCaster`, `ElementalEnergy`, `StaggerReceiver` básico, `TypeChart` |
| **2** | Estado de Bloqueo/forma elemental, `ElementalSurface` + `ReactionSystem` + **`ReactionTable`** + **`ReactionEffect` genérico** (explosión/zona) + `RadiateElement`; parches de terreno elemental en la arena. ✅ *(sistemas, 2026-07-29)* · ⚠️ **la tabla y `ReactionEffect` hay que reautorarlos contra el modelo nuevo (§5)** antes de dar la fase por buena |
| **3** | `CaptureSystem` (ventana + Sludge), IA mínima del enemigo, HUD de aturdimiento |
| **4** | `CreatureInstance` + `TeamManager` (equipo de 3, swap con cooldown), 2ª criatura como **datos** |
| **5** | Pulido, `BlockParams`/naturalezas afinados, integrar placeholders del concept art |

## Notas pragmáticas
- **`[SerializeReference]`** (Unity 2019.3+) para la lista polimórfica de efectos. Alternativa: efectos como sub-assets `ScriptableObject` (más reutilizables/compartibles, pero más fricción). Para un equipo chico, `[SerializeReference]` es más simple.
- `CombatServices` = un pequeño localizador de servicios (o inyección) para que los efectos alcancen `TypeChart` y `ReactionSystem` sin acoplarse a escenas.
- Ejecutar los efectos en **anim-events** (no en el frame del input) para que el *game feel* (timing, hitstop) quede en manos del diseñador.
