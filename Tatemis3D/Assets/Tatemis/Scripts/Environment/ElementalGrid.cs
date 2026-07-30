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

        /// Qué hay en una celda. `element == null` = neutra: ahí no reacciona nada (R5).
        public struct Cell
        {
            public Element? element;
            /// Celda de **efecto ambiental** (polvo, ceniza, niebla…). Es el 2º sistema ambiental del
            /// GDD §8.1, **aún sin dictar**. ⚠️ Su contenido **dependerá del bioma**: la ambiental de
            /// la Caldera no es la misma que la de la Laguna (dictado 2026-07-30).
            public bool ambient;
            /// A qué bioma pertenece. **El bioma es una identidad visual que tiñe TODAS sus celdas**,
            /// no solo las elementales: el suelo neutro de la Caldera es ceniza y el de la Laguna es
            /// arena mojada. Mecánicamente son lo mismo; visualmente, no. -1 = fuera de todo bioma.
            public int biome;
        }

        [Header("Rejilla (ARENA §1)")]
        [Tooltip("Radio del hexágono. La celda se mide en HUELLA: cara a cara = radius × √3 ≈ 6 u con " +
                 "3.5. ✏️ Doblado el 2026-07-29 por dictado de Arzpal: con 1.75 los hexágonos no se " +
                 "leían en pantalla. La celda es ahora del tamaño de una jugada, no de un paso")]
        public float cellRadius = 3.5f;

        [Header("Composición 5/2/3 (ER §0.5)")]
        [Range(0f, 1f)] public float elementalShare = 0.5f;
        [Range(0f, 1f)] public float neutralShare = 0.2f;
        // El resto (0.3) son celdas de efecto ambiental.

        public List<Biome> biomes = new List<Biome>();

        readonly Dictionary<HexCoord, Cell> _cells = new Dictionary<HexCoord, Cell>();
        bool _baked;

        void Awake()
        {
            CombatServices.Grid = this;
            Bake();
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

            for (int bi = 0; bi < biomes.Count; bi++)
            {
                var b = biomes[bi];
                if (b == null) continue;

                // Cuántas manchas caben: las que hagan falta para que la mitad del bioma (la cuota
                // elemental del 5/2/3) quede cubierta, con el tamaño medio de mancha elegido.
                float cellArea = 1.5f * Mathf.Sqrt(3f) * cellRadius * cellRadius;
                int cellsInBiome = Mathf.Max(1, Mathf.RoundToInt(Mathf.PI * b.radius * b.radius / cellArea));
                float avgPatch = (patchCells.x + patchCells.y) * 0.5f;
                int patches = Mathf.Max(3, Mathf.RoundToInt(cellsInBiome * elementalShare / avgPatch));

                foreach (var el in PatchElements(b, bi, patches))
                    GrowPatch(b, bi, _patchIndex++, el, bi);
            }

            FillGaps();
            _baked = true;
        }

        int _patchIndex;

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
            float neutralOfGap = neutralShare / Mathf.Max(0.001f, neutralShare + (1f - elementalShare - neutralShare));

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
                        _cells[c] = new Cell { element = null, biome = bi, ambient = c.Hash01(7) >= neutralOfGap };
                    }
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
        /// Celda **ambiental**: gris claro. Sitio reservado al 2º sistema ambiental (GDD §8.1).
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
            if (c.element.HasValue) return ColorOf(c.element);
            var basecol = c.ambient ? Ambient : Neutral;
            if (c.biome < 0 || c.biome >= biomes.Count || biomes[c.biome] == null) return basecol;
            return Color.Lerp(basecol, biomes[c.biome].color, BiomeWash);
        }
    }
}
