using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **La detonación encadenada** ([ELEMENTAL-REACTIONS §0.10](../../../../docs/ELEMENTAL-REACTIONS.md)).
    /// Una mancha cebada **no revienta de golpe**: revienta **la celda que recibió el impacto** y la onda
    /// avanza por vecindad al resto, dejando terreno transformado detrás.
    ///
    /// No es mecanismo nuevo — es el del **incendio**, que ya se propagaba por celdas de su elemento,
    /// generalizado a las seis reacciones. La rejilla ya era el grafo de vecindad.
    ///
    /// Tres reglas que la sostienen, y sin las que se rompe:
    ///  · 🔑 **El jugador APUNTA la onda.** Nace donde cae el golpe: detonar por el borde cercano la
    ///    manda lejos de ti; por el lejano te la trae encima, pero alcanza al que huye. Decisión nueva
    ///    **sin input nuevo**.
    ///  · 🔑 **La onda ES la ventana de esquiva.** El viejo `appearDelay` existía solo para dar tiempo a
    ///    apartarse; ahora ese tiempo lo da **el viaje**, y encima se ve.
    ///  · ⚠️ **Golpea a cada criatura UNA sola vez** (`_hit`), y **va más rápido que correr**, para que
    ///    se esquive de lado o hacia atrás — con decisión, no con velocidad.
    public class DetonationWave : MonoBehaviour
    {
        /// Lo que tarda la onda en saltar de anillo a anillo. Con la celda a ~6 u de cara a cara esto
        /// son ~75 u/s: **muy por encima de la carrera**, así que huir hacia delante no funciona.
        /// Suficientemente lento para leerse como frente que viaja y no como fogonazo.
        public const float StepSeconds = 0.08f;

        ReactionEntry _entry;
        ElementalGrid _grid;
        List<List<HexCoord>> _rings;
        List<HexCoord> _patch;
        ICombatant _instigator;
        Transform _instigatorRoot;
        Vector3 _direction;
        float _nextStep;
        int _ring;

        /// 🔑 Compartido por TODA la onda: es lo que garantiza un solo golpe por criatura.
        readonly HashSet<Transform> _hit = new HashSet<Transform>();

        /// Lanza la onda. `origin` es la celda impactada — de ahí sale, y por eso el jugador la apunta.
        public static void Launch(ReactionEntry entry, ElementalGrid grid, IReadOnlyList<HexCoord> patch,
                                  HexCoord origin, ICombatant instigator, Vector3 direction)
        {
            if (entry == null || grid == null || patch == null || patch.Count == 0) return;

            var go = new GameObject("Detonacion_" + entry.reactionName);
            go.transform.position = origin.ToWorld(grid.cellRadius);
            var w = go.AddComponent<DetonationWave>();
            w._entry = entry;
            w._grid = grid;
            w._patch = new List<HexCoord>(patch);
            w._instigator = instigator;
            w._instigatorRoot = instigator?.Transform != null ? instigator.Transform.root : null;
            w._direction = direction;
            w._rings = GroupByDistance(w._patch, origin);
            w._nextStep = Time.time;   // el primer anillo sale en el acto: el golpe se siente ya
        }

        /// Agrupa las celdas de la mancha por **distancia hexagonal** a la celda impactada. Cada grupo
        /// es un frente de onda, así que la explosión hereda la **forma de ameba** de la mancha en vez
        /// de expandirse como un círculo: un brazo largo llega lejos por un lado y no por el otro, y esa
        /// geografía pasa de estética a táctica.
        static List<List<HexCoord>> GroupByDistance(List<HexCoord> patch, HexCoord origin)
        {
            var rings = new List<List<HexCoord>>();
            for (int i = 0; i < patch.Count; i++)
            {
                int d = patch[i].DistanceTo(origin);
                while (rings.Count <= d) rings.Add(new List<HexCoord>());
                rings[d].Add(patch[i]);
            }
            return rings;
        }

        void Update()
        {
            if (Time.time < _nextStep) return;
            _nextStep = Time.time + StepSeconds;

            // Anillos vacíos (una mancha en U puede tener huecos de distancia): se saltan sin gastar
            // tiempo, o la onda parecería trabarse.
            while (_ring < _rings.Count && _rings[_ring].Count == 0) _ring++;

            if (_ring >= _rings.Count) { Settle(_entry, _grid, _patch, _instigator, _direction); Destroy(gameObject); return; }

            var front = _rings[_ring++];
            float cellReach = _grid.cellRadius * 0.95f;
            var tint = ReactionEffect.TintOf(_entry);

            for (int i = 0; i < front.Count; i++)
            {
                Vector3 p = front[i].ToWorld(_grid.cellRadius);
                p.y = transform.position.y;
                ReactionEffect.ApplyBurst(_entry, p, cellReach, _instigatorRoot, _hit, tint);
            }
        }

        /// Lo que queda **después** de la onda, y también el camino que siguen las combinaciones (que no
        /// encadenan: *se extienden*, ocupan la mancha de una vez). Tres pasos, en este orden:
        ///  ① **borrar los efectos vivos** del alcance — el suelo que los sostenía dejó de ser ese
        ///    elemento; ② **transformar** la mancha y su **anillo** al subproducto; ③ dejar la **zona
        ///    residual** si la entrada la tiene.
        ///
        /// ⚠️ El orden importa: limpiar **antes** de crear, o la zona nueva se borraría a sí misma.
        public static void Settle(ReactionEntry entry, ElementalGrid grid, List<HexCoord> patch,
                                  ICombatant instigator, Vector3 direction)
        {
            if (entry == null || grid == null || patch == null || patch.Count == 0) return;

            Vector3 center = Centroid(patch, grid);
            float radius = Extent(patch, grid, center);

            ReactionEffect.ClearLiveIn(center, radius);

            // 🔑 La mancha adopta el subproducto y el ANILLO se mancha con él. Cuando subproducto y
            // mancha coinciden (crecimiento explosivo sobre pasto) la mancha no cambia pero el anillo
            // sí: su función ahí es **expandirla**. Por eso ninguna detonación es nula.
            var affected = new List<HexCoord>(patch);
            affected.AddRange(grid.RingAround(patch));
            grid.TransformCells(affected, entry.TerrainAfter);

            // Zona residual, si la entrada deja una. Sin delay: el aviso ya lo dio la carga, y para las
            // reacciones el fogonazo lo cobró la onda (`suppressBurst`).
            if (entry.baseDuration > 0f)
                ReactionEffect.Spawn(entry, center, radius, entry.baseDuration, 0f,
                                     instigator, direction, 3, suppressBurst: true);
        }

        public static Vector3 Centroid(List<HexCoord> cells, ElementalGrid grid)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < cells.Count; i++) sum += cells[i].ToWorld(grid.cellRadius);
            return sum / Mathf.Max(1, cells.Count);
        }

        /// Radio que cubre la mancha entera: es lo que hace que **la explosión sea la mancha** y que
        /// una mancha grande valga más que una pequeña sin ninguna regla extra.
        public static float Extent(List<HexCoord> cells, ElementalGrid grid, Vector3 center)
        {
            float best = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 d = cells[i].ToWorld(grid.cellRadius) - center; d.y = 0f;
                best = Mathf.Max(best, d.magnitude);
            }
            return best + grid.cellRadius;
        }
    }
}
