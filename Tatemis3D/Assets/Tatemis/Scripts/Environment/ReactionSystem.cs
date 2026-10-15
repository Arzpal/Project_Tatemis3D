using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// El árbitro del sistema elemental ([ELEMENTAL-REACTIONS §7](../../../../docs/ELEMENTAL-REACTIONS.md)).
    ///
    /// 🔑 **Desde §0.10 este sistema DEPOSITA, no resuelve.** Un impacto elemental ya no dispara nada:
    /// deja **saturación** en la mancha que pisa. Cuando se llena, la mancha queda **cebada**, y el
    /// siguiente impacto la revienta con una **onda** que la recorre, transforma el suelo y lo devuelve
    /// solo al cabo de un rato. Mismo daño total, **~6 acontecimientos grandes por combate en vez de
    /// 40 pequeños** — que era el problema que el playtest del 30/07 puso encima de la mesa.
    ///
    /// Reglas que siguen vigentes tal cual:
    ///  · R1 — siempre contra el TERRENO, nunca contra un efecto activo. Con terreno mutable, «el
    ///    terreno» pasa a significar **el de ahora mismo**: la matriz sigue siendo 6×6 porque lo que
    ///    cambia es la casilla, no la tabla.
    ///  · R2 — la duración de la zona residual es proporcional a la fuerza del impacto.
    ///  · R3 — **por DRENAJE** (rev. 31/07): golpear dentro de una zona viva con otro elemento le quita
    ///    duración, y ese golpe **no deposita**. Sin esto no habría forma de limpiar una zona.
    ///  · R4 — par ORDENADO: vive en la tabla (dominancia vs respuesta del terreno).
    ///  · R5 — superficie neutra → no reacciona… pero **se carga a 2y** y se vuelve elemental.
    ///  · R6 — el bando lo aplica el dato (`friendlyFire` / combinación).
    public class ReactionSystem : MonoBehaviour
    {
        [SerializeField] ReactionTable table;

        [Header("La unidad «y» (ER §0.10)")]
        [Tooltip("Fuerza de impacto que vale 1y ≈ el estallido de una habilidad media. **Es EL número de " +
                 "playtest de todo el sistema**: el ratio 1/2/3/5 está dictado, esto no. Un básico " +
                 "aporta ≈0,3-0,5y.\n\n⚠️ Se mide en `impactStrength` y NO en daño infligido: si " +
                 "dependiera del daño, cargar rápido sería un chequeo del stat de la especie en vez de " +
                 "una decisión de diseño por habilidad")]
        [SerializeField] float referenceImpact = 15f;

        [Header("Escalera de umbrales — el ratio 1/2/3/5 está DICTADO")]
        [SerializeField] float costAmplificacion = 1f;
        [SerializeField] float costCargarNeutra = 2f;
        [SerializeField] float costCombinacion = 3f;
        [SerializeField] float costReaccion = 5f;

        [Header("Saturación")]
        [Tooltip("Lo que tarda una batería llena en vaciarse sola. Sin decaimiento, al minuto 2 el " +
                 "tablero entero está cebado y vuelve el ruido que §0.10 vino a curar")]
        [SerializeField] float decaySeconds = 16f;
        [Tooltip("Lo que aguanta cebada antes de ventearse (fogonazo flojo, sin efecto)")]
        [SerializeField] float primedSeconds = 5f;
        [Tooltip("Cuánta saturación derrama la amplificación a cada mancha vecina. Es lo que convierte " +
                 "golpear tu propio elemento en la herramienta de carga del sistema")]
        [SerializeField] float spillPerNeighbour = 0.35f;
        [Tooltip("Cuánto se sale de su celda el estallido de la amplificación")]
        [SerializeField] float spillRadiusScale = 1.35f;

        [Header("R2 — duración de la zona ∝ impacto")]
        [SerializeField] float minDurationScale = 0.5f;
        [SerializeField] float maxDurationScale = 2.5f;

        [Header("R3 — drenaje")]
        [Tooltip("Segundos de vida que le quita a una zona viva un impacto de otro elemento caído DENTRO")]
        [SerializeField] float drainSeconds = 1.2f;

        [Header("Excepciones instantáneas (ER §0.10)")]
        [Tooltip("Delay del estallido de las tres excepciones que NO pasan por saturación: choques en " +
                 "el aire, roca sellada de Ottorock y minas encendidas de Pyrecko")]
        [SerializeField] float instantDelay = 0.25f;

        TerrainSaturation _sat;

        /// La capa de saturación, para quien la necesite pintar (el overlay del tablero).
        public TerrainSaturation Saturation => EnsureSat();

        /// La unidad `y`, para que el instrumental del laboratorio pueda mostrar **cuánto vale un golpe**
        /// en lugar de obligar a deducirlo. Es el número que más se va a tocar.
        public float ReferenceImpact => referenceImpact;

        /// La tabla en uso, para que el laboratorio pueda mostrar **la receta** de la pareja apuntada
        /// (*terreno X + entrante Y = nombre*) sin duplicar el contenido en otro sitio.
        public ReactionTable Table => table;

        void Awake() => CombatServices.Reactions = this;
        void OnDestroy()
        {
            _sat?.Dispose(); _sat = null;
            if (_visual != null) { Destroy(_visual.gameObject); _visual = null; }
        }

        void Update() => _sat?.Tick();

        /// Perezoso a propósito: `ElementalGrid` se registra en su propio `Awake` y el orden entre
        /// componentes no está garantizado.
        TerrainSaturation EnsureSat()
        {
            if (_sat != null) return _sat;
            var grid = CombatServices.Grid;
            if (grid == null) return null;
            _sat = new TerrainSaturation(grid)
            {
                costAmplificacion = costAmplificacion,
                costCargarNeutra = costCargarNeutra,
                costCombinacion = costCombinacion,
                costReaccion = costReaccion,
                decaySeconds = decaySeconds,
                primedSeconds = primedSeconds,
            };
            _sat.Vented += OnVented;
            // La carga TIENE que verse, y en cualquier escena — no solo donde `CombatZone` haya pintado
            // celdas. Se crea aquí para que exista sin tocar ningún builder.
            _visual = SaturationVisual.Create(grid, _sat);
            return _sat;
        }

        SaturationVisual _visual;

        // ───────────────────────── Punto de entrada ─────────────────────────

        /// `incoming` es el elemento irradiado (los golpes neutros no llegan aquí — GDD §7),
        /// `impactStrength` la fuerza del golpe que lo irradió (de ahí sale `y`), `instigator` quien lo
        /// provocó (null = sin dueño) y `echoDamage` lo que repite la **amplificación**.
        public void Impact(Vector3 pos, float radius, Element incoming, float impactStrength,
                           ICombatant instigator, float echoDamage = 0f, Vector3 direction = default)
        {
            // La dirección apunta las zonas direccionales (corriente, colada). Si no llega, el encarado.
            if (direction.sqrMagnitude < 0.0001f && instigator != null && instigator.Transform != null)
                direction = instigator.Transform.forward;

            var grid = CombatServices.Grid;
            var sat = EnsureSat();
            if (grid == null || sat == null) { ResolveNow(pos, radius, incoming, impactStrength, instigator, echoDamage, direction); return; }

            Vector3 ground = GroundUnder(pos);
            var coord = grid.CoordAt(ground);
            var cell = grid.CellAt(coord);

            // R3 por drenaje. Va PRIMERO y consume el golpe: limpiar una zona cuesta el impacto que
            // habrías usado para cargar. Ese coste es lo que R3 siempre quiso.
            if (ReactionEffect.DrainAt(ground, incoming, drainSeconds)) return;

            float y = Mathf.Max(0.05f, impactStrength / Mathf.Max(1f, referenceImpact));

            if (!Classify(cell, incoming, out var kind, out var entry)) return;   // par sin autorar

            var ev = sat.Deposit(coord, cell.patch, incoming, kind, y, instigator, direction, out var battery);
            RefreshRevert(grid, coord, cell.patch);

            switch (ev)
            {
                case SaturationEvent.Detona:
                    Detonate(grid, battery);
                    break;

                case SaturationEvent.Lleno:
                    if (kind == ChargeKind.Amplificacion)
                        Amplify(grid, coord, ground, radius, incoming, echoDamage, instigator);
                    else if (kind == ChargeKind.CargarNeutra)
                        ConvertNeutral(grid, coord, incoming);
                    else
                        Telegraph(ground, ReactionEffect.TintOf(entry), grid.cellRadius * 1.1f, 0.35f);
                    break;

                default:
                    // El golpe que no llena HABLA pero no hace: destello mudo de la mezcla correcta,
                    // sin daño, sin estado y sin zona — y **crece con la saturación**. Enseña las 15
                    // parejas cuando las mezclas son raras, y es el telegraph de la mancha.
                    MuteFlash(ground, entry, incoming, grid.cellRadius, battery.Fill01);
                    break;
            }
        }

        /// **Las tres excepciones instantáneas** (ER §0.10): choques en el aire (§0.2b), roca sellada de
        /// Ottorock y minas encendidas de Pyrecko. **No pasan por saturación** y resuelven en el acto —
        /// más flojas que una detonación, a cambio de ser inmediatas. Sin esta puerta, los choques en el
        /// aire desaparecerían del juego (no hay suelo donde acumular) y la mecánica insignia de
        /// Ottorock se apagaría.
        public void Instant(Vector3 pos, float radius, Element incoming, float impactStrength,
                            ICombatant instigator, float echoDamage = 0f, Vector3 direction = default)
        {
            if (direction.sqrMagnitude < 0.0001f && instigator != null && instigator.Transform != null)
                direction = instigator.Transform.forward;
            ResolveNow(pos, radius, incoming, impactStrength, instigator, echoDamage, direction);
        }

        // ───────────────────────── Clasificación ─────────────────────────

        /// Qué clase de salida es esto y cuánto cuesta. Es la escalera de §0.10 traducida a código.
        bool Classify(ElementalGrid.Cell cell, Element incoming, out ChargeKind kind, out ReactionEntry entry)
        {
            entry = null;
            if (!cell.element.HasValue) { kind = ChargeKind.CargarNeutra; return true; }
            if (cell.element.Value == incoming) { kind = ChargeKind.Amplificacion; return true; }

            if (table == null || !table.TryResolve(cell.element.Value, incoming, out entry))
            {
                kind = ChargeKind.Combinacion;
                return false;   // par sin autorar: el impacto ocurre, la reacción no
            }
            kind = entry.style == ReactionStyle.Dominancia ? ChargeKind.Reaccion : ChargeKind.Combinacion;
            return true;
        }

        // ───────────────────────── Salidas ─────────────────────────

        /// Revienta una batería cebada. La reacción que sale es **la que se cargó** (el dueño del
        /// medidor contra el terreno), no la del golpe que la disparó: montar la bomba sigue siendo
        /// mérito de quien la montó, pero el rival puede hacerla estallar antes de tiempo.
        void Detonate(ElementalGrid grid, TerrainSaturation.Battery b)
        {
            var origin = b.lastHit;
            var cell = grid.CellAt(origin);

            // ⚠️ Consumir SIEMPRE y ANTES de transformar: **una detonación no deposita saturación**
            // (ER §0.10). Sin esto, el anillo que acaba de crear se cargaría con su propio estallido y
            // tendrías dominó infinito.
            _sat.Consume(b);

            if (!cell.element.HasValue) return;
            if (table == null || !table.TryResolve(cell.element.Value, b.owner, out var entry)) return;

            // Lo que revienta es LA MANCHA. Una neutra convertida que aún no se ha unido a ninguna
            // revienta sola — el terreno fabricado es pequeño y débil, que es el incentivo correcto.
            var cells = cell.patch != 0
                ? new List<HexCoord>(grid.PatchCells(cell.patch))
                : new List<HexCoord> { origin };

            if (entry.style == ReactionStyle.Dominancia)
                DetonationWave.Launch(entry, grid, cells, origin, b.instigator, b.direction);
            else
                // Las combinaciones no encadenan: **se extienden**. Son transformaciones, no explosiones.
                DetonationWave.Settle(entry, grid, cells, b.instigator, b.direction);
        }

        /// **Amplificación** (1y): tu explosión vuelve a sonar, **sin cebado y sin delay** — es el único
        /// carril rápido del sistema. Y su radio **se sale de la celda**, así que de paso **deposita
        /// saturación en las manchas vecinas**: golpear tu propio elemento pasa de ser la salida más
        /// aburrida a ser la herramienta de carga, y «pelear en tu bioma» gana identidad estratégica.
        void Amplify(ElementalGrid grid, HexCoord coord, Vector3 ground, float radius,
                     Element incoming, float echoDamage, ICombatant instigator)
        {
            if (echoDamage > 0f)
                Echo.Spawn(ground, radius * spillRadiusScale, incoming, echoDamage, 0f, instigator);

            for (int i = 0; i < 6; i++)
            {
                var n = coord.Neighbor(i);
                var cell = grid.CellAt(n);
                if (cell.element.HasValue && cell.element.Value == incoming) continue;   // más de lo mismo
                if (!Classify(cell, incoming, out var kind, out _)) continue;

                var ev = _sat.Deposit(n, cell.patch, incoming, kind, spillPerNeighbour,
                                      instigator, Vector3.zero, out var nb);
                RefreshRevert(grid, n, cell.patch);
                // El derrame puede reventar una mancha vecina que ya estuviera cebada. Es emergente y
                // se queda: te da una forma de detonar a distancia usando tu propio elemento.
                if (ev == SaturationEvent.Detona) Detonate(grid, nb);
            }
        }

        /// **Cargar una neutra** (2y): el plan B cuando no hay tablero. Nace como mancha de una celda —
        /// pequeña y débil frente al terreno natural, y con su propio reloj de reversión.
        void ConvertNeutral(ElementalGrid grid, HexCoord coord, Element incoming)
        {
            grid.TransformCells(new List<HexCoord> { coord }, incoming);
            Telegraph(coord.ToWorld(grid.cellRadius), ElementalGrid.ColorOf(incoming), grid.cellRadius, 0.3f);
        }

        /// **Mover el medidor refresca el reloj de reversión** ✅ *(playtest 2026-07-31)*. Antes solo lo
        /// refrescaba una detonación, así que una mancha que estabas cargando activamente podía
        /// revertirse **a mitad de la carga**: veías deshacerse justo lo que estabas construyendo. Vale
        /// tanto si subes la tuya como si bajas la del rival — las dos cosas son disputa.
        static void RefreshRevert(ElementalGrid grid, HexCoord coord, int patch)
        {
            if (patch != 0) grid.TouchRevert(grid.PatchCells(patch));
            else grid.TouchRevert(coord);
        }

        void OnVented(Vector3 where, Element owner)
        {
            // Se ventea: fogonazo débil, sin efecto y sin daño. Es el guardarraíl que impide que el
            // tablero se llene de bombas dormidas.
            var c = ElementalGrid.ColorOf(owner); c.a = 0.25f;
            HitboxDebug.Show(where + Vector3.up * 0.2f, 1.5f, 0.3f, c);
        }

        // ───────────────────────── Caja gris ─────────────────────────

        void MuteFlash(Vector3 ground, ReactionEntry entry, Element incoming, float cellRadius, float fill01)
        {
            var c = entry != null ? ReactionEffect.TintOf(entry) : ElementalGrid.ColorOf(incoming);
            c.a = Mathf.Lerp(0.06f, 0.4f, fill01);   // casi imperceptible al principio, escandaloso al final
            HitboxDebug.Show(ground + Vector3.up * 0.15f, cellRadius * Mathf.Lerp(0.4f, 0.9f, fill01),
                             0.12f + 0.15f * fill01, c);
        }

        void Telegraph(Vector3 ground, Color tint, float radius, float seconds)
        {
            tint.a = 0.5f;
            HitboxDebug.Show(ground + Vector3.up * 0.2f, radius, seconds, tint);
        }

        // ───────────────────────── Resolución inmediata (excepciones y respaldo) ─────────────────────────

        /// El camino de antes de §0.10: resolver en el acto contra el terreno. Lo usan **las tres
        /// excepciones instantáneas** y, como respaldo, las escenas sin rejilla (parches de
        /// `ElementalSurface` puestos a mano).
        void ResolveNow(Vector3 pos, float radius, Element incoming, float impactStrength,
                        ICombatant instigator, float echoDamage, Vector3 direction)
        {
            if (!TerrainAt(pos, out Element terrain, out Vector3 ground)) return;   // R5

            if (terrain == incoming)
            {
                if (echoDamage > 0f) Echo.Spawn(ground, radius, incoming, echoDamage, 0f, instigator);
                return;
            }
            if (table == null || !table.TryResolve(terrain, incoming, out var entry)) return;

            float scale = Mathf.Clamp(impactStrength / Mathf.Max(1f, referenceImpact),
                                      minDurationScale, maxDurationScale);
            float duration = entry.baseDuration > 0f ? entry.baseDuration * scale : 0.7f;
            ReactionEffect.Spawn(entry, ground, radius, duration, instantDelay, instigator, direction);
        }

        /// R1: qué elemento hay bajo este punto. **La rejilla del mundo manda**; el `ElementalSurface`
        /// suelto queda como respaldo para parches puestos a mano en escenas de prueba.
        bool TerrainAt(Vector3 pos, out Element terrain, out Vector3 ground)
        {
            var grid = CombatServices.Grid;
            if (grid != null)
            {
                var cell = grid.CellAt(pos);
                if (cell.element.HasValue)
                {
                    terrain = cell.element.Value;
                    ground = grid.CellCenter(GroundUnder(pos));
                    return true;
                }
                terrain = default; ground = pos;
                return false;
            }

            var surface = SurfaceAt(pos, out ground);
            if (surface == null) { terrain = default; return false; }
            terrain = surface.element;
            return true;
        }

        /// Proyecta el punto al suelo para que el efecto no nazca flotando cuando el golpe fue aéreo.
        Vector3 GroundUnder(Vector3 pos)
        {
            if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out var hit, 60f,
                                ~0, QueryTriggerInteraction.Ignore))
                return hit.point;
            var p = pos; p.y = 0f;
            return p;
        }

        /// ⚠️ El alcance es largo a propósito: el kit está construido para pelear EN EL AIRE (juggle,
        /// embestida en diagonal), así que un techo corto haría que estallar arriba no produjera
        /// reacción y se leyera como suelo neutro (R5) sin ningún aviso.
        ElementalSurface SurfaceAt(Vector3 pos, out Vector3 ground)
        {
            ground = pos;
            ElementalSurface found = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics.RaycastAll(pos + Vector3.up * 2f, Vector3.down, 60f,
                                                   ~0, QueryTriggerInteraction.Ignore))
            {
                var s = hit.collider.GetComponentInParent<ElementalSurface>();
                if (s == null || hit.distance >= bestDist) continue;
                bestDist = hit.distance;
                found = s;
                ground = hit.point;
            }
            return found;
        }
    }
}
