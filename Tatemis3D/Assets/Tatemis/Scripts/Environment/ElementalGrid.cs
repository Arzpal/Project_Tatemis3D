using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **El tablero elemental del mundo** ([ARENA.md](../../docs/ARENA.md) §1). El mundo es
    /// semiabierto y continuo —se camina libremente—, y esta rejilla es la capa de LECTURA que dice
    /// qué elemento hay en cada punto. No es geometría: no se construye el terreno con hexágonos.
    ///
    /// El elemento de cada celda sale de su **bioma**: una laguna tiene la mayoría de celdas de Agua,
    /// una zona volcánica de Fuego. Composición objetivo **5/2/3** (elementales / neutras / con efecto
    /// ambiental) — ELEMENTAL-REACTIONS §0.5.
    ///
    /// ⚠️ El terreno **NO CAMBIA JAMÁS** (ER §1): esto se hornea una vez al arrancar y nadie lo muta.
    /// Todo lo que ocurre encima son efectos temporales que viven en `ReactionEffect`.
    public class ElementalGrid : MonoBehaviour
    {
        /// Región del mundo con un elemento predominante. ⚠️ **Un bioma NO pinta su elemento en todo
        /// su territorio** ✅ *(dictado 2026-07-30)*: siembra **manchas**. La Caldera no es fuego de
        /// borde a borde — es suelo neutro con **muchas hogueras**, un par de charcos de sus vecinos y
        /// alguna veta exótica. Lo único que define a un bioma es **cuántas manchas de cada elemento
        /// siembra**, y de ahí sale que su elemento sea el que casi siempre encuentras allí.
        [System.Serializable]
        public class Biome
        {
            public string name = "Bioma";
            public Vector3 center;
            public float radius = 40f;
            public Element element = Element.Naturaleza;
            [Tooltip("Color de caja gris del suelo del bioma")]
            public Color color = Color.gray;
            [Tooltip("Elementos de los biomas COLINDANTES. De ellos salen las manchas 'vecinas', que " +
                     "son las que hacen que el mundo se lea como un sitio real y no como un tablero")]
            public List<Element> neighbours = new List<Element>();
        }

        [Header("Manchas (ER §0.5 · ARENA §1)")]
        [Tooltip("Celdas por mancha, mín y máx. **Es el número que decide si el modelo funciona**: por " +
                 "debajo de 3 el tablero queda salpicado y no se puede apuntar a un elemento; por encima " +
                 "de ~10 una arena entera cae dentro de una sola mancha y vuelve a ser monocolor")]
        public Vector2Int patchCells = new Vector2Int(3, 6);
        [Tooltip("De las manchas de un bioma, qué fracción son de SU elemento. El resto se reparte " +
                 "entre vecinas y exóticas — pero SIEMPRE hay al menos una de cada")]
        [Range(0.4f, 1f)] public float ownPatchShare = 0.7f;

        /// Qué hay en una celda. `element == null` = neutra: ahí no reacciona nada **en el momento del
        /// golpe** (R5), pero **se carga a 2y y se vuelve elemental** (ER §0.10).
        public struct Cell
        {
            public Element? element;
            /// Marca de **efecto ambiental** (polvo, ceniza, niebla…), el 2º sistema ambiental del
            /// GDD §8.1, **aún sin dictar**. ✏️ *2026-07-31: dejó de ser una CLASE de celda y pasó a ser
            /// un **modificador** que puede ir encima de cualquiera, elemental o neutra* — así el clima
            /// no compite por espacio de tablero con el elemento y el tablero se lee con **dos**
            /// categorías en vez de tres. ⚠️ Su contenido **dependerá del bioma**.
            public bool ambient;
            /// A qué bioma pertenece. **El bioma es una identidad visual que tiñe TODAS sus celdas**,
            /// no solo las elementales: el suelo neutro de la Caldera es ceniza y el de la Laguna es
            /// arena mojada. Mecánicamente son lo mismo; visualmente, no. -1 = fuera de todo bioma.
            public int biome;
            /// 🔑 **A qué MANCHA pertenece** ([ER §0.10](../../../../docs/ELEMENTAL-REACTIONS.md)).
            /// La mancha —las celdas conectadas del mismo elemento— **es la batería**: la saturación se
            /// acumula por mancha entera, no por celda, y **la explosión es la mancha**. `0` = ninguna
            /// (celda neutra). Los ids empiezan en 1 a propósito: `default(Cell)` da 0 y así una celda
            /// desconocida nunca finge pertenecer a la mancha de otro.
            public int patch;
        }

        [Header("Rejilla (ARENA §1)")]
        [Tooltip("Radio del hexágono. La celda se mide en HUELLA: cara a cara = radius × √3 ≈ 6 u con " +
                 "3.5. ✏️ Doblado el 2026-07-29 por dictado de Arzpal: con 1.75 los hexágonos no se " +
                 "leían en pantalla. La celda es ahora del tamaño de una jugada, no de un paso")]
        public float cellRadius = 3.5f;

        [Header("Composición 5/5 (ER §0.5) ✏️ era 5/2/3 hasta el 2026-07-31")]
        [Tooltip("Fracción de celdas con elemento. El resto son NEUTRAS — que ya no son terreno muerto: " +
                 "se cargan a 2y y se vuelven elementales (ER §0.10)")]
        [Range(0f, 1f)] public float elementalShare = 0.5f;
        [Tooltip("Fracción de celdas que además llevan la marca AMBIENTAL. Es un MODIFICADOR encima de " +
                 "cualquier celda, elemental o neutra — ya no una tercera clase que compita por espacio")]
        [Range(0f, 1f)] public float ambientShare = 0.3f;

        [Header("Reversión (ER §0.10)")]
        [Tooltip("Lo que tarda una celda transformada en volver a su estado horneado. **Se REFRESCA** " +
                 "mientras se pelee encima —tanto al detonar como al SOLO cargar—: el suelo que se " +
                 "disputa se queda cambiado, el que se abandona se cura. Es lo que impide que la arena " +
                 "se agote a mitad de combate")]
        public float revertSeconds = 22f;
        [Tooltip("Multiplicador del tiempo de reversión para las celdas que NACIERON neutras ✅ (playtest " +
                 "2026-07-31). Fabricarte tablero es un proceso de DOS pasos —convertir la neutra y " +
                 "luego cargarla— así que si durara lo mismo que el terreno natural se te desharía a " +
                 "medio camino y el plan B no sería viable nunca")]
        public float neutralRevertMultiplier = 2f;

        public List<Biome> biomes = new List<Biome>();

        /// Una mancha puesta **a mano**, en coordenadas de celda. Es lo que convierte el laboratorio en
        /// un banco de medida: *«esta mancha tiene exactamente 4 celdas»* no se puede decir con el
        /// horneado por biomas, que produce amebas de tamaño impredecible — buenísimo para el mundo y
        /// **inservible para medir**.
        [System.Serializable]
        public class AuthoredPatch
        {
            public string label = "Banco";
            public Element element = Element.Naturaleza;
            [Tooltip("Celdas en axial (x = q, y = r). Deben ser contiguas o saldrán varias manchas")]
            public List<Vector2Int> cells = new List<Vector2Int>();
        }

        [Header("Modo autorado (laboratorio) — si hay manchas aquí, se IGNORAN los biomas")]
        public List<AuthoredPatch> authored = new List<AuthoredPatch>();
        [Tooltip("Medio ancho, en celdas, del suelo neutro que se genera alrededor de los bancos")]
        public int authoredSpan = 26;

        readonly Dictionary<HexCoord, Cell> _cells = new Dictionary<HexCoord, Cell>();
        /// El estado **horneado**: a esto vuelve una celda transformada. El mundo cambia, pero nunca
        /// queda pintado (ER §6).
        readonly Dictionary<HexCoord, Cell> _origin = new Dictionary<HexCoord, Cell>();
        /// Manchas vivas: id → sus celdas. Es el índice que convierte "la mancha es la batería" en una
        /// consulta O(1) desde el sistema de saturación.
        readonly Dictionary<int, List<HexCoord>> _patches = new Dictionary<int, List<HexCoord>>();
        /// Cuándo vuelve cada celda transformada a su estado horneado.
        readonly Dictionary<HexCoord, float> _revertAt = new Dictionary<HexCoord, float>();
        readonly List<HexCoord> _revertScratch = new List<HexCoord>();
        bool _baked;
        int _nextPatchId = 1;   // 0 queda reservado para "sin mancha"

        /// Avisa de que el tablero cambió (transformación o reversión) para que el overlay se repinte.
        /// Sin esto el jugador no ve que el suelo bajo sus pies dejó de ser lo que era.
        public event System.Action<IReadOnlyList<HexCoord>> CellsChanged;

        void Awake()
        {
            CombatServices.Grid = this;
            Bake();
        }

        /// La reversión corre aquí y no en el sistema de reacciones a propósito: **es una propiedad del
        /// terreno**, no de las reacciones, y tiene que seguir ocurriendo aunque no haya combate.
        void Update()
        {
            if (_revertAt.Count == 0) return;
            _revertScratch.Clear();
            foreach (var kv in _revertAt) if (Time.time >= kv.Value) _revertScratch.Add(kv.Key);
            if (_revertScratch.Count == 0) return;

            for (int i = 0; i < _revertScratch.Count; i++)
            {
                var c = _revertScratch[i];
                _revertAt.Remove(c);
                if (_origin.TryGetValue(c, out var baked)) _cells[c] = baked;
            }
            RebuildPatchesAround(_revertScratch);
            CellsChanged?.Invoke(_revertScratch);
        }

        /// Hornea las celdas de todos los biomas. Determinista (`HexCoord.Hash01`): el mismo mundo
        /// se ve igual en cada Play, que es lo que permite afinar la composición en playtest.
        /// Hornea el mundo en dos pasadas:
        ///  ① **Se siembran las manchas.** Cada bioma calcula cuántas necesita para llegar a su cuota
        ///     de celdas elementales, y cada una crece desde su **pieza central** hacia las vecinas.
        ///  ② **Todo lo que quede sin mancha es suelo sin elemento**, repartido entre **neutro** y
        ///     **ambiental** con el ratio dictado.
        /// Determinista de punta a punta: el mismo mundo se ve igual en cada Play, que es lo que
        /// permite afinar la composición en playtest en vez de perseguir un mundo distinto cada vez.
        public void Bake()
        {
            _cells.Clear();
            _origin.Clear();
            _patches.Clear();
            _revertAt.Clear();
            _nextPatchId = 1;
            // ⚠️ Se reinicia porque es la SEMILLA del ruido de siembra: sin esto, un segundo `Bake()`
            // (rebuild en el editor, o el horneado perezoso de `CellAt`) generaría un mundo distinto y
            // se rompería la promesa de "el mismo mundo se ve igual en cada Play".
            _patchIndex = 0;

            if (authored.Count > 0) { BakeAuthored(); RebuildAllPatches(); Snapshot(); return; }

            for (int bi = 0; bi < biomes.Count; bi++)
            {
                var b = biomes[bi];
                if (b == null) continue;

                // Cuántas manchas caben: las que hagan falta para cubrir la cuota elemental del bioma
                // (la mitad, ER §0.5) con el tamaño medio de mancha elegido.
                float cellArea = 1.5f * Mathf.Sqrt(3f) * cellRadius * cellRadius;
                int cellsInBiome = Mathf.Max(1, Mathf.RoundToInt(Mathf.PI * b.radius * b.radius / cellArea));
                float avgPatch = (patchCells.x + patchCells.y) * 0.5f;
                int patches = Mathf.Max(3, Mathf.RoundToInt(cellsInBiome * elementalShare / avgPatch));

                foreach (var el in PatchElements(b, bi, patches))
                    GrowPatch(b, bi, _patchIndex++, el, bi);
            }

            FillGaps();
            RebuildAllPatches();
            Snapshot();
        }

        /// A esto vuelve cada celda cuando expira su reversión.
        void Snapshot()
        {
            foreach (var kv in _cells) _origin[kv.Key] = kv.Value;
            _baked = true;
        }

        /// **Terreno puesto a mano** (laboratorio): un suelo neutro y encima las manchas autoradas, sin
        /// biomas ni azar. Sin marca ambiental a propósito: en un banco de medida es ruido.
        void BakeAuthored()
        {
            int span = Mathf.Max(1, authoredSpan);
            for (int q = -span; q <= span; q++)
                for (int r = -span; r <= span; r++)
                    _cells[new HexCoord(q, r)] = new Cell { element = null, biome = -1 };

            for (int i = 0; i < authored.Count; i++)
            {
                var a = authored[i];
                if (a == null) continue;
                for (int c = 0; c < a.cells.Count; c++)
                    _cells[new HexCoord(a.cells[c].x, a.cells[c].y)] =
                        new Cell { element = a.element, biome = -1 };
            }
        }

        int _patchIndex;

        /// Manchas = **componentes conexas del mismo elemento**. Se calcula al final del horneado y no
        /// durante el crecimiento a propósito: dos manchas sembradas por separado que acaban pegadas
        /// **son una sola mancha** para el jugador, y por tanto **una sola batería**. Dejar que los ids
        /// vinieran del bucle de siembra habría partido en dos una mancha que se ve como una.
        void RebuildAllPatches(bool resetIds = true)
        {
            _patches.Clear();
            // ⚠️ Los ids solo se reinician al hornear. En un recálculo en caliente (abrir o cerrar la
            // zona de combate) **tienen que seguir subiendo**: si volvieran a empezar en 1, una batería
            // huérfana con id 1 se pegaría a una mancha nueva que no tiene nada que ver con ella.
            if (resetIds) _nextPatchId = 1;

            var keys = new List<HexCoord>(_cells.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var cell = _cells[keys[i]];
                cell.patch = 0;
                _cells[keys[i]] = cell;
            }
            for (int i = 0; i < keys.Count; i++) FloodPatch(keys[i]);
        }

        /// Pinta una mancha entera desde una celda sin asignar. Devuelve el id, o 0 si no aplica.
        int FloodPatch(HexCoord start)
        {
            if (!_cells.TryGetValue(start, out var seed) || !seed.element.HasValue || seed.patch != 0)
                return 0;

            int id = _nextPatchId++;
            var cells = new List<HexCoord>();
            var stack = new Stack<HexCoord>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var c = stack.Pop();
                if (!_cells.TryGetValue(c, out var cell)) continue;
                if (cell.patch != 0 || cell.element != seed.element) continue;

                cell.patch = id;
                _cells[c] = cell;
                cells.Add(c);
                for (int i = 0; i < 6; i++)
                {
                    var n = c.Neighbor(i);
                    if (Connected(c, n)) stack.Push(n);   // el cerco corta la mancha (ver `Connected`)
                }
            }

            _patches[id] = cells;
            return id;
        }

        /// Qué elemento lleva cada mancha del bioma. **Mayoría suya · un par de vecinas · un par de
        /// exóticas**, y las dos últimas están *garantizadas*: son las que hacen que en mitad de un
        /// bioma, lejos de toda frontera, sigas teniendo al menos dos elementos con los que jugar.
        IEnumerable<Element> PatchElements(Biome b, int bi, int total)
        {
            int own = Mathf.Max(1, Mathf.RoundToInt(total * ownPatchShare));
            int rest = Mathf.Max(2, total - own);
            int neighbours = Mathf.Max(1, rest / 2);
            int exotic = Mathf.Max(1, rest - neighbours);

            for (int i = 0; i < own; i++) yield return b.element;

            for (int i = 0; i < neighbours; i++)
                yield return b.neighbours.Count > 0
                    ? b.neighbours[i % b.neighbours.Count]
                    : Exotic(b, bi, i);

            for (int i = 0; i < exotic; i++) yield return Exotic(b, bi, 100 + i);
        }

        /// Un elemento que no es el del bioma ni el de sus vecinos: la veta de mineral en mitad de la
        /// Caldera. Es lo que impide que el mundo sea predecible del todo.
        Element Exotic(Biome b, int bi, int salt)
        {
            var all = (Element[])System.Enum.GetValues(typeof(Element));
            for (int t = 0; t < 12; t++)
            {
                var e = all[Mathf.Abs(Hash(bi, salt, t)) % all.Length];
                if (e != b.element && !b.neighbours.Contains(e)) return e;
            }
            return all[Mathf.Abs(Hash(bi, salt, 99)) % all.Length];
        }

        /// **Pieza central + piezas subsecuentes.** La mancha nace en una celda y **crece por vecindad**
        /// hexagonal hasta su tamaño: eso da las formas de ameba del terreno real en vez de círculos, y
        /// no cuesta ninguna estructura nueva porque la rejilla ya es el grafo de vecindad.
        void GrowPatch(Biome b, int bi, int pi, Element element, int biomeIndex)
        {
            // Semilla en disco uniforme (√ del hash, o se apelotonarían en el centro).
            float ang = Hash01(bi, pi, 1) * Mathf.PI * 2f;
            float rad = Mathf.Sqrt(Hash01(bi, pi, 2)) * b.radius * 0.92f;
            Vector3 seedPos = b.center + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * rad;

            var frontier = new List<HexCoord> { HexCoord.FromWorld(seedPos, cellRadius) };
            int size = patchCells.x + Mathf.Abs(Hash(bi, pi, 3)) % Mathf.Max(1, patchCells.y - patchCells.x + 1);

            for (int grown = 0; grown < size && frontier.Count > 0; )
            {
                int pick = Mathf.Abs(Hash(bi, pi, 10 + grown)) % frontier.Count;
                var c = frontier[pick];
                frontier.RemoveAt(pick);

                if (_cells.ContainsKey(c) && _cells[c].element.HasValue) continue;   // ya es de otra mancha
                _cells[c] = new Cell { element = element, biome = biomeIndex };
                grown++;

                for (int i = 0; i < 6; i++) frontier.Add(c.Neighbor(i));
            }
        }

        /// Todo lo que no cayó en una mancha: **suelo sin elemento**, repartido entre neutro y
        /// ambiental con el ratio dictado. Aquí no reacciona nada (R5), y por eso el hueco entre
        /// manchas es tan importante como las manchas: es lo que hace que apuntar a una signifique algo.
        void FillGaps()
        {
            for (int bi = 0; bi < biomes.Count; bi++)
            {
                var b = biomes[bi];
                if (b == null) continue;
                int span = Mathf.CeilToInt(b.radius / (cellRadius * 1.5f)) + 2;
                var origin = HexCoord.FromWorld(b.center, cellRadius);

                for (int dq = -span; dq <= span; dq++)
                    for (int dr = -span; dr <= span; dr++)
                    {
                        var c = new HexCoord(origin.q + dq, origin.r + dr);
                        if (_cells.ContainsKey(c)) continue;
                        Vector3 p = c.ToWorld(cellRadius);
                        Vector3 d = p - b.center; d.y = 0f;
                        if (d.sqrMagnitude > b.radius * b.radius) continue;
                        _cells[c] = new Cell { element = null, biome = bi };
                    }
            }

            // La marca AMBIENTAL se reparte al final y sobre TODAS las celdas, elementales incluidas:
            // desde el 2026-07-31 es un modificador, no una clase de celda (ER §0.5).
            var keys = new List<HexCoord>(_cells.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var cell = _cells[keys[i]];
                cell.ambient = keys[i].Hash01(7) < ambientShare;
                _cells[keys[i]] = cell;
            }
        }

        static int Hash(int a, int b, int c)
        {
            unchecked
            {
                int h = a * 73856093 ^ b * 19349663 ^ c * 83492791;
                h = (h ^ (h >> 13)) * 1274126177;
                return h ^ (h >> 16);
            }
        }

        static float Hash01(int a, int b, int c) => (Mathf.Abs(Hash(a, b, c)) & 0xFFFFFF) / (float)0xFFFFFF;

        public HexCoord CoordAt(Vector3 world) => HexCoord.FromWorld(world, cellRadius);

        public Cell CellAt(Vector3 world)
        {
            if (!_baked) Bake();
            return _cells.TryGetValue(CoordAt(world), out var cell) ? cell : default;   // fuera de bioma = neutra
        }

        public Cell CellAt(HexCoord c)
        {
            if (!_baked) Bake();
            return _cells.TryGetValue(c, out var cell) ? cell : default;
        }

        /// La consulta que usa el sistema de reacciones (R1). `null` = suelo neutro → no reacciona nada.
        public Element? ElementAt(Vector3 world) => CellAt(world).element;

        /// Centro de la celda que contiene el punto — para que el efecto nazca **encajado en la
        /// celda** y no en un punto arbitrario: es lo que hace legible que el tablero es de celdas.
        public Vector3 CellCenter(Vector3 world)
        {
            Vector3 c = CoordAt(world).ToWorld(cellRadius);
            c.y = world.y;
            return c;
        }

        // ───────────────── El cerco corta las manchas ✅ (playtest 2026-07-31) ─────────────────

        /// Celdas de la zona de combate en curso. `null` = no hay combate y todo el mundo conecta.
        HashSet<HexCoord> _zone;

        /// 🔑 **El cerco es un muro también para la CONECTIVIDAD, no solo para los cuerpos.** Dos celdas
        /// vecinas del mismo elemento **no forman la misma mancha si el cerco pasa entre ellas**.
        ///
        /// Arregla dos cosas que se vieron en el playtest y que eran la misma:
        ///  ① al reventar una mancha cortada por el muro, **reaccionaba también la mitad de fuera** —
        ///     el jugador pagaba una detonación y media se perdía en terreno al que no puede llegar;
        ///  ② una mancha **partida en dos por el muro** seguía siendo una sola batería, así que cargar
        ///     un lado llenaba el otro a través de un puente que **no existe para el jugador**.
        /// La regla es la misma en los dos casos: *si no puedes ir andando de una celda a la otra sin
        /// salir de la arena, no son la misma mancha.*
        bool Connected(HexCoord a, HexCoord b) =>
            _zone == null || _zone.Contains(a) == _zone.Contains(b);

        /// La abre `CombatZone`. Recalcula todas las manchas con el cerco puesto.
        public void SetCombatZone(IEnumerable<HexCoord> cells)
        {
            _zone = cells != null ? new HashSet<HexCoord>(cells) : null;
            RebuildAllPatches(resetIds: false);
            CellsChanged?.Invoke(System.Array.Empty<HexCoord>());   // tira las baterías huérfanas y repinta
        }

        public void ClearCombatZone()
        {
            if (_zone == null) return;
            _zone = null;
            RebuildAllPatches(resetIds: false);
            CellsChanged?.Invoke(System.Array.Empty<HexCoord>());
        }

        // ───────────────── Manchas: la batería del sistema de saturación (ER §0.10) ─────────────────

        /// A qué mancha pertenece este punto. `0` = ninguna (celda neutra → acumula **por celda**,
        /// porque una neutra no forma parte de ninguna mancha; ver ER §0.10).
        public int PatchAt(Vector3 world) => CellAt(world).patch;
        public int PatchAt(HexCoord c) => _cells.TryGetValue(c, out var cell) ? cell.patch : 0;

        /// Las celdas de una mancha. **Esto es lo que revienta**: la explosión no es un círculo, es la
        /// mancha entera, y por eso hereda su forma de ameba en vez de un radio inventado.
        public IReadOnlyList<HexCoord> PatchCells(int id) =>
            _patches.TryGetValue(id, out var list) ? list : System.Array.Empty<HexCoord>();

        /// Cuántas celdas tiene una mancha. Gobierna lo grande que es su explosión y —vía el diseño de
        /// [ARENA §1b](../../../../docs/ARENA.md)— cuánto cuesta cargarla.
        public int PatchSize(int id) => _patches.TryGetValue(id, out var list) ? list.Count : 0;

        /// El **anillo** de una mancha: las celdas pegadas a su contorno que no son suyas. Es lo que se
        /// mancha al detonar, y es lo que hace que **ninguna detonación sea nula** — aunque el
        /// subproducto coincida con el elemento de la mancha, el anillo sí cambia y la mancha **crece**.
        public List<HexCoord> RingAround(IReadOnlyList<HexCoord> cells)
        {
            var ring = new List<HexCoord>();
            if (cells == null || cells.Count == 0) return ring;

            var inside = new HashSet<HexCoord>(cells);
            var seen = new HashSet<HexCoord>();
            for (int i = 0; i < cells.Count; i++)
                for (int d = 0; d < 6; d++)
                {
                    var n = cells[i].Neighbor(d);
                    if (inside.Contains(n) || !seen.Add(n)) continue;
                    if (!Connected(cells[i], n)) continue;    // el cerco corta también el manchado
                    if (_cells.ContainsKey(n)) ring.Add(n);   // solo suelo que existe
                }
            return ring;
        }

        /// **Transforma terreno** (ER §0.10): la detonación deja el subproducto en la mancha y en su
        /// anillo. `to == null` = neutro, que es lo que dejan las 9 combinaciones.
        ///
        /// 🔑 **La reversión se REFRESCA en cada llamada.** Aunque una celda ya estuviera transformada,
        /// volver a detonar encima reinicia su reloj — así *el suelo que se disputa se queda cambiado y
        /// el que se abandona se cura*, sin ninguna regla extra.
        public void TransformCells(IReadOnlyList<HexCoord> cells, Element? to)
        {
            if (cells == null || cells.Count == 0) return;
            if (!_baked) Bake();

            var touched = new List<HexCoord>(cells.Count);

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (!_cells.TryGetValue(c, out var cell)) continue;   // fuera del mundo horneado
                if (!_origin.ContainsKey(c)) _origin[c] = cell;       // por si nació fuera del snapshot
                cell.element = to;
                _cells[c] = cell;
                _revertAt[c] = Time.time + RevertSecondsFor(c);
                touched.Add(c);
            }

            if (touched.Count == 0) return;
            RebuildPatchesAround(touched);
            CellsChanged?.Invoke(touched);
        }

        /// ¿Esta celda está transformada ahora mismo? Lo usa el overlay para marcarla: el jugador tiene
        /// que poder distinguir la ceniza que hizo él del suelo que ya estaba (ER §6).
        public bool IsTransformed(HexCoord c) => _revertAt.ContainsKey(c);

        /// **El doble para las que nacieron neutras** ✅ *(playtest 2026-07-31)*. Fabricarte tablero son
        /// **dos pasos** —convertir la neutra (2y) y después cargarla (3y o 5y)—, así que con el mismo
        /// reloj que el terreno natural se te desharía a mitad del proceso y el plan B no sería viable.
        float RevertSecondsFor(HexCoord c) =>
            _origin.TryGetValue(c, out var baked) && !baked.element.HasValue
                ? revertSeconds * neutralRevertMultiplier
                : revertSeconds;

        /// **Pelear encima refresca el reloj, aunque no detone nada** ✅ *(playtest 2026-07-31)*.
        /// Antes solo lo refrescaba una detonación, y una mancha que estabas cargando activamente podía
        /// revertirse **en mitad de la carga** — el jugador veía deshacerse justo lo que estaba
        /// construyendo. Ahora basta con **mover el medidor**, para arriba o para abajo: el terreno
        /// disputado se queda cambiado mientras haya disputa, que es lo que §0.10 quería decir.
        public void TouchRevert(IReadOnlyList<HexCoord> cells)
        {
            if (cells == null) return;
            for (int i = 0; i < cells.Count; i++)
                if (_revertAt.ContainsKey(cells[i]))
                    _revertAt[cells[i]] = Time.time + RevertSecondsFor(cells[i]);
        }

        public void TouchRevert(HexCoord c)
        {
            if (_revertAt.ContainsKey(c)) _revertAt[c] = Time.time + RevertSecondsFor(c);
        }

        /// Segundos que le quedan a una celda para volver a su estado horneado, o **-1** si no está
        /// transformada. Lo lee el instrumental del laboratorio: sin este número, afinar `revertSeconds`
        /// y su ×2 neutro es adivinar.
        public float RevertRemaining(HexCoord c) =>
            _revertAt.TryGetValue(c, out float at) ? Mathf.Max(0f, at - Time.time) : -1f;

        /// Devuelve **todo** el terreno a su estado horneado, ya mismo. Es el botón de *reset* del
        /// laboratorio: sin él, cada medida obligaría a esperar a que revirtiera media arena.
        public void RevertAll()
        {
            if (_revertAt.Count == 0) return;
            var all = new List<HexCoord>(_revertAt.Keys);
            _revertAt.Clear();
            for (int i = 0; i < all.Count; i++)
                if (_origin.TryGetValue(all[i], out var baked)) _cells[all[i]] = baked;

            RebuildPatchesAround(all);
            CellsChanged?.Invoke(all);
        }

        /// Recalcula las manchas del vecindario tocado. Se limpia **la mancha entera** de cada celda
        /// afectada y de sus vecinas antes de re-inundar: si no, dos manchas que acaban de fundirse en
        /// una seguirían con ids distintos y serían **dos baterías donde el jugador ve una**.
        void RebuildPatchesAround(IReadOnlyList<HexCoord> seeds)
        {
            var dirty = new HashSet<HexCoord>();
            for (int i = 0; i < seeds.Count; i++)
            {
                AddWithItsPatch(seeds[i], dirty);
                for (int d = 0; d < 6; d++) AddWithItsPatch(seeds[i].Neighbor(d), dirty);
            }

            foreach (var c in dirty)
            {
                if (!_cells.TryGetValue(c, out var cell)) continue;
                if (cell.patch != 0) _patches.Remove(cell.patch);
                cell.patch = 0;
                _cells[c] = cell;
            }
            foreach (var c in dirty) FloodPatch(c);
        }

        void AddWithItsPatch(HexCoord c, HashSet<HexCoord> into)
        {
            if (!_cells.TryGetValue(c, out var cell)) return;
            into.Add(c);
            if (cell.patch != 0 && _patches.TryGetValue(cell.patch, out var list))
                for (int i = 0; i < list.Count; i++) into.Add(list[i]);
        }

        /// Todas las celdas cuyo centro cae dentro del radio dado. Lo usa la zona de combate para
        /// saber qué tablero tiene delante.
        public IEnumerable<KeyValuePair<HexCoord, Cell>> CellsWithin(Vector3 center, float radius)
        {
            if (!_baked) Bake();
            float sqr = radius * radius;
            foreach (var kv in _cells)
            {
                Vector3 p = kv.Key.ToWorld(cellRadius);
                Vector3 d = p - center; d.y = 0f;
                if (d.sqrMagnitude <= sqr) yield return kv;
            }
        }

        /// Paleta del tablero ✅ **dictada por Arzpal el 2026-07-29 con una maqueta**. Los seis
        /// elementos van a **color saturado** para que la celda se lea de un vistazo, y las dos clases
        /// de celda sin elemento se distinguen entre sí: **neutra = gris oscuro** (ahí no pasa nada,
        /// R5) y **ambiental = gris claro** (reservada al 2º sistema ambiental, aún sin dictar).
        /// ⚠️ **Mineral es marrón grisáceo, no azulado** — el azulado se confundía con el Agua.
        public static Color ColorOf(Element? e)
        {
            if (!e.HasValue) return Neutral;
            switch (e.Value)
            {
                case Element.Fuego:      return new Color(0.93f, 0.11f, 0.11f);   // rojo
                case Element.Agua:       return new Color(0.11f, 0.60f, 0.95f);   // azul marino
                case Element.Naturaleza: return new Color(0.09f, 0.72f, 0.25f);   // verde
                case Element.Mineral:    return new Color(0.66f, 0.58f, 0.44f);   // marrón grisáceo
                case Element.Rayo:       return new Color(1.00f, 0.80f, 0.06f);   // amarillo-naranja
                default:                 return new Color(0.34f, 0.90f, 0.80f);   // Aire: verde agua
            }
        }

        /// Celda **neutra**: gris oscuro. No reacciona nada (R5).
        public static readonly Color Neutral = new Color(0.36f, 0.36f, 0.36f);
        /// Gris claro **reservado** al 2º sistema ambiental (GDD §8.1). ⚠️ Ya **no se usa como color de
        /// celda**: desde el 2026-07-31 lo ambiental es un modificador encima de cualquier celda, así
        /// que cuando ese sistema se dicte necesitará una capa propia (textura, partículas, borde), no
        /// un color de relleno que compita con el del elemento.
        public static readonly Color Ambient = new Color(0.80f, 0.80f, 0.80f);

        /// **Cuánto del bioma se cuela en sus celdas sin elemento.** Bajo a propósito: es un lavado de
        /// tono, no un color — si subiera, una neutra de la Caldera se confundiría con una mancha de
        /// Fuego, y distinguir "aquí no pasa nada" de "aquí sí" es lo más importante del tablero.
        const float BiomeWash = 0.28f;

        /// Color de una celda **contando su bioma** ✅ *(2026-07-30)*. Las manchas elementales van al
        /// color puro de su elemento —un charco tiene que leerse como Agua esté donde esté—, pero las
        /// **neutras y ambientales se tiñen del bioma**: el suelo muerto de la Caldera es ceniza y el
        /// de la Laguna arena mojada. Mecánicamente idénticos, visualmente no.
        public Color ColorOfCell(Cell c)
        {
            // ✏️ 2026-07-31: **desaparece el gris claro**. Lo ambiental dejó de ser una clase de celda,
            // así que ya no tiene color propio — el tablero se lee con DOS grados (pasa algo aquí, o
            // no) en vez de tres, que era la ambigüedad de tener dos grises con significados distintos.
            if (c.element.HasValue) return ColorOf(c.element);
            if (c.biome < 0 || c.biome >= biomes.Count || biomes[c.biome] == null) return Neutral;
            return Color.Lerp(Neutral, biomes[c.biome].color, BiomeWash);
        }
    }
}
