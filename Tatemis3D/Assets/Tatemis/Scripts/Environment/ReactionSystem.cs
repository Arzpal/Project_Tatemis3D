using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// El árbitro del sistema elemental (ELEMENTAL-REACTIONS §7): un impacto IRRADIA su elemento en
    /// un punto y aquí se resuelve qué pasa. Reglas ratificadas:
    ///  · R1 — siempre contra el TERRENO (fijo), nunca contra un efecto activo → la matriz es 6×6.
    ///  · R2 — la duración de la reacción es proporcional a la fuerza del impacto.
    ///  · R3 — un impacto nuevo cancela el efecto vivo del punto y resuelve de cero.
    ///  · R4 — par ORDENADO: vive en la tabla (dominancia vs respuesta del terreno).
    ///  · R5 — superficie neutra → nada. (El feedback mínimo de "no pasa nada" es VFX futuro.)
    ///  · R6 — el bando lo aplica el efecto: fogonazo con dueño salvo `friendlyFire`; zona neutral.
    public class ReactionSystem : MonoBehaviour
    {
        [SerializeField] ReactionTable table;

        [Header("R2 — duración ∝ impacto (se afina en playtest)")]
        [Tooltip("Fuerza de impacto que da duración ×1 (≈ el daño de un estallido de habilidad)")]
        [SerializeField] float referenceImpact = 15f;
        [SerializeField] float minDurationScale = 0.5f;
        [SerializeField] float maxDurationScale = 2.5f;

        [Header("Delay y gracia (ER §0.2 y §0.4)")]
        [Tooltip("Lo que tarda una reacción/combinación/amplificación en aparecer. Es la ventana de " +
                 "esquiva que hace real elegir entre pegar directo y apuntar al suelo")]
        [SerializeField] float appearDelay = 0.45f;
        [Tooltip("Tiempo durante el cual un efecto vivo NO se puede convertir en otro. Debe ser MENOR " +
                 "que la duración del efecto, o nunca se podría interrumpir")]
        [SerializeField] float graceSeconds = 1.5f;

        readonly List<ReactionEffect> _active = new List<ReactionEffect>();

        void Awake() => CombatServices.Reactions = this;

        /// Punto único de entrada: `incoming` es el elemento irradiado (los golpes neutros no llegan
        /// aquí — GDD §7), `impactStrength` la fuerza del golpe que lo irradió (≈ su daño, R2) e
        /// `instigator` quien lo provocó (null = sin dueño, p. ej. la descarga de un nodo).
        /// `echoDamage` = el daño del estallido entrante, que es lo que la **amplificación** repite
        /// (ER §0.2: "19 de fuego → otro 19 de fuego"). 0 = no amplifica.
        public void Impact(Vector3 pos, float radius, Element incoming, float impactStrength,
                           ICombatant instigator, float echoDamage = 0f, Vector3 direction = default)
        {
            // La dirección del golpe es lo que APUNTA las zonas direccionales (corriente, colada): el
            // jugador decide a dónde arrastra la cinta o hacia dónde repta la lava. Si no llega, se usa
            // el encarado del instigador.
            if (direction.sqrMagnitude < 0.0001f && instigator != null && instigator.Transform != null)
                direction = instigator.Transform.forward;

            if (!TerrainAt(pos, out Element terrain, out Vector3 ground)) return;   // R5: neutro → nada

            // Mismo elemento → AMPLIFICACIÓN: no hay mezcla que produzca efecto, así que lo único que
            // pasa es que tu propia explosión vuelve a sonar. Es lo que hace que tu bioma valga.
            if (terrain == incoming)
            {
                if (echoDamage > 0f) Echo.Spawn(ground, radius, incoming, echoDamage, appearDelay, instigator);
                return;
            }

            if (table == null || !table.TryResolve(terrain, incoming, out var entry))
                return;                    // par sin autorar: el impacto ocurre, la reacción no

            // R3 con periodo de gracia (§0.4): un impacto nuevo convierte el efecto vivo del punto,
            // pero no antes de que pase su gracia. Repetir la MISMA reacción lo refresca en vez de
            // bloquearse — si no, la cadena de Sujeción de Ottorock se anularía a sí misma.
            if (!ConvertActiveAt(ground, entry)) return;

            float scale = Mathf.Clamp(impactStrength / Mathf.Max(1f, referenceImpact),
                                      minDurationScale, maxDurationScale);
            // Dominancia sin zona residual: un suspiro visible igualmente, para leer DÓNDE pegó.
            float duration = entry.baseDuration > 0f ? entry.baseDuration * scale : 0.7f;

            _active.Add(ReactionEffect.Spawn(entry, ground, radius, duration, appearDelay, instigator, direction));
        }

        /// R1: el elemento del punto es el del TERRENO bajo él. RaycastAll porque el rayo puede
        /// atravesar criaturas u obstáculos antes de llegar al suelo: se busca la superficie entre
        /// todos los hits, no en el primero.
        /// R1: qué elemento hay bajo este punto. **La rejilla del mundo manda** (ARENA §1: el terreno
        /// es continuo y la celda dice qué elemento tiene); el `ElementalSurface` suelto queda como
        /// respaldo para parches puestos a mano en escenas de prueba.
        /// El efecto nace en el **centro de la celda**, no en el punto exacto: es lo que hace legible
        /// que el tablero está hecho de celdas y no de manchas arbitrarias.
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
                // Con rejilla en escena, una celda sin elemento es NEUTRA de verdad — no se cae al
                // respaldo, o los parches a mano contradirían al tablero.
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
        /// reacción y se leyera como suelo neutro (R5) sin ningún aviso. R1 no dicta cota de altura.
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

        /// R3 + periodo de gracia. Devuelve **false** si el efecto vivo del punto está protegido y
        /// por tanto la reacción nueva no debe nacer.
        ///
        /// Se exige que el impacto caiga DENTRO de la zona, no que las dos áreas se rocen: R3 habla
        /// de "impactar en un punto donde ya hay una reacción viva", y su propósito es counterplay
        /// —atacar la zona PARA limpiarla—, o sea puntería, no proximidad.
        bool ConvertActiveAt(Vector3 pos, ReactionEntry incoming)
        {
            _active.RemoveAll(fx => fx == null);   // los expirados se destruyen solos
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var fx = _active[i];

                // ⚠️ La distancia va PRIMERO. Al revés, un efecto nacido en la otra punta del mapa
                // bloqueaba la reacción de aquí solo por haber nacido en el mismo frame — y con
                // varias criaturas peleando a la vez eso pasaría constantemente.
                Vector3 d = fx.Center - pos; d.y = 0f;
                if (d.sqrMagnitude > fx.Radius * fx.Radius) continue;

                // Nacido en este frame Y aquí mismo = no es "un impacto nuevo sobre un efecto
                // activo", son dos efectos de la misma habilidad pisándose (el daño crea, el
                // estallido borraría).
                if (fx.BornFrame == Time.frameCount) return false;

                // Repetir la MISMA reacción no la convierte: la refresca. La gracia existe para
                // impedir CAMBIAR de efecto, no para castigar la insistencia.
                if (fx.Entry == incoming) { fx.Refresh(); return false; }

                // Dentro de su gracia, el efecto aguanta y el impacto nuevo no cuaja.
                if (fx.Age < graceSeconds) return false;

                fx.CancelledByImpact();
                _active.RemoveAt(i);
            }
            return true;
        }
    }
}
