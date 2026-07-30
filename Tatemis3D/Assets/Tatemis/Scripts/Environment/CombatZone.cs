using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **La zona de combate** ([ARENA.md](../../../../docs/ARENA.md) §1). Al empezar un encuentro se
    /// acota un trozo de mundo y ese trozo se convierte en el tablero: las celdas se hacen visibles y
    /// pasan a estar en juego. Fuera se sigue caminando libremente.
    ///
    /// 🔑 **La zona es un conjunto de CELDAS COMPLETAS, no un círculo** ✅ *(dictado 2026-07-29)*. El
    /// cerco se levanta sobre las **aristas de los hexágonos del borde**, así que **no corta ninguna
    /// celda por la mitad** — el área de combate sigue el patrón hexagonal de punta a punta.
    ///
    /// 🔑 **El cerco llega hasta el cielo**: su colisión es altísima para que **un volador no pueda
    /// salirse por arriba**, pero solo se dibuja la parte baja — una pared visible de 40 u taparía la
    /// cámara en tercera persona.
    public class CombatZone : MonoBehaviour
    {
        [Header("Tamaño (en CELDAS, no en unidades)")]
        [Tooltip("Anillos de celdas alrededor del centro. 4 = 9 celdas de punta a punta, que es la " +
                 "proporción de la maqueta dictada")]
        public int rings = 4;
        [Tooltip("Anillos extra si los combatientes no caben. Nunca se recorta por debajo de `rings`")]
        public int maxRings = 7;

        [Header("Cerco")]
        [Tooltip("Altura de la COLISIÓN: tiene que ser inalcanzable para un volador")]
        public float fenceCollisionHeight = 45f;
        [Tooltip("Altura de lo que SE VE. Baja a propósito: una pared visible altísima taparía la cámara")]
        public float fenceVisualHeight = 7f;
        [Tooltip("Color del cerco. Translúcido pero llamativo — es un campo de fuerza, no una pared")]
        public Color fenceColor = new Color(1f, 0.25f, 0.95f, 0.30f);

        [Header("Celdas")]
        [Tooltip("Opacidad del relleno de cada celda")]
        [Range(0f, 1f)] public float cellAlpha = 0.55f;
        [Tooltip("Hueco entre celdas: <1 deja ver la junta y hace que se lean como celdas y no como manchas")]
        [Range(0.7f, 1f)] public float cellInset = 0.93f;
        public float cellHeight = 0.07f;

        public Vector3 Center { get; private set; }
        /// Radio aproximado en unidades (para quien necesite una cifra: cámara, IA, HUD).
        public float Radius { get; private set; }
        public bool IsOpen { get; private set; }

        readonly List<GameObject> _spawned = new List<GameObject>();
        readonly HashSet<HexCoord> _cells = new HashSet<HexCoord>();
        readonly List<Material> _mats = new List<Material>();
        static Mesh _hexMesh;

        /// Abre la zona alrededor de un grupo de combatientes: el centro es la **celda** que contiene
        /// su centroide, y se añaden anillos hasta que todos quepan dentro.
        public void Open(IReadOnlyList<Transform> combatants)
        {
            if (combatants == null || combatants.Count == 0) return;
            Close();

            var grid = CombatServices.Grid;
            if (grid == null) { Debug.LogWarning("[Arena] No hay ElementalGrid: la zona no puede alinearse a celdas."); return; }

            Vector3 c = Vector3.zero;
            for (int i = 0; i < combatants.Count; i++) c += combatants[i].position;
            c /= combatants.Count;
            c.y = 0f;

            var centerCell = grid.CoordAt(c);
            Center = centerCell.ToWorld(grid.cellRadius);

            // Crecer en anillos hasta que el combatiente más lejano quede dentro. Se mide en celdas,
            // no en unidades: la zona es un conjunto de celdas y su tamaño se cuenta en celdas.
            int need = rings;
            for (int i = 0; i < combatants.Count; i++)
            {
                int d = grid.CoordAt(combatants[i].position).DistanceTo(centerCell);
                need = Mathf.Max(need, d + 1);
            }
            int used = Mathf.Clamp(need, rings, maxRings);

            _cells.Clear();
            foreach (var h in HexCoord.Within(centerCell, used)) _cells.Add(h);

            // Cara a cara de un hexágono = radius·√3; `used` anillos llegan a esa distancia × anillos.
            Radius = grid.cellRadius * Mathf.Sqrt(3f) * (used + 0.5f);
            transform.position = Center;
            IsOpen = true;

            BuildCells(grid);
            BuildFence(grid);

            int elementales = 0, ambientales = 0;
            foreach (var h in _cells)
            {
                var cell = grid.CellAt(h.ToWorld(grid.cellRadius));
                if (cell.element.HasValue) elementales++;
                else if (cell.ambient) ambientales++;
            }
            Debug.Log($"[Arena] Zona abierta: {_cells.Count} celdas ({used} anillos, ~{Radius * 2f:0} u de punta a punta) " +
                      $"— {elementales} elementales, {ambientales} ambientales, " +
                      $"{_cells.Count - elementales - ambientales} neutras.");
        }

        public void Close()
        {
            for (int i = 0; i < _spawned.Count; i++) if (_spawned[i] != null) Destroy(_spawned[i]);
            for (int i = 0; i < _mats.Count; i++) if (_mats[i] != null) Destroy(_mats[i]);
            _spawned.Clear();
            _mats.Clear();
            _cells.Clear();
            IsOpen = false;
        }

        void OnDestroy() => Close();

        public bool Contains(Vector3 p)
        {
            if (!IsOpen) return true;
            var grid = CombatServices.Grid;
            if (grid == null) return true;
            return _cells.Contains(grid.CoordAt(p));
        }

        // ---------------- Construcción ----------------

        /// Cada celda, **rellena** y con su color: elemental a color saturado, neutra gris oscuro,
        /// ambiental gris claro. Rellenas y no en contorno porque el contorno fino no se leía; el hueco
        /// entre ellas (`cellInset`) es lo que las mantiene reconocibles como celdas.
        void BuildCells(ElementalGrid grid)
        {
            var byColor = new Dictionary<Color, Material>();
            foreach (var h in _cells)
            {
                Vector3 p = h.ToWorld(grid.cellRadius);
                var cell = grid.CellAt(p);
                // El color lo pide la rejilla, no una tabla estática: las celdas sin elemento se tiñen
                // del BIOMA, así que hace falta saber a cuál pertenecen.
                var col = grid.ColorOfCell(cell);
                col.a = cellAlpha;

                if (!byColor.TryGetValue(col, out var mat))
                {
                    mat = Unlit(col);
                    byColor[col] = mat;
                    _mats.Add(mat);
                }

                var go = new GameObject("Celda");
                go.transform.SetParent(transform, true);
                go.transform.position = new Vector3(p.x, cellHeight, p.z);
                go.AddComponent<MeshFilter>().sharedMesh = HexMesh();
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                go.transform.localScale = Vector3.one * (grid.cellRadius * cellInset);
                _spawned.Add(go);
            }
        }

        /// El cerco: un panel por cada **arista de borde** — las aristas de celdas de la zona cuyo
        /// vecino queda fuera. Así el perímetro es exactamente el contorno hexagonal y no parte
        /// ninguna celda. Cada panel lleva **dos piezas**: la colisión (altísima, invisible, para que
        /// los voladores no se escapen) y lo visible (bajo y translúcido).
        void BuildFence(ElementalGrid grid)
        {
            var mat = Unlit(fenceColor);
            _mats.Add(mat);
            float side = grid.cellRadius;                     // el lado de un hexágono regular = su radio

            foreach (var h in _cells)
            {
                Vector3 center = h.ToWorld(grid.cellRadius);
                for (int i = 0; i < 6; i++)
                {
                    if (_cells.Contains(h.Neighbor(i))) continue;   // vecino dentro → no es borde

                    Vector3 off = HexCoord.NeighborOffset(i, grid.cellRadius);
                    Vector3 mid = center + off * 0.5f;               // justo sobre la arista
                    var rot = Quaternion.LookRotation(off.normalized, Vector3.up);

                    // Colisión invisible que llega al cielo: es lo que encierra a un volador.
                    var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    blocker.name = "Cerco (colisión)";
                    Destroy(blocker.GetComponent<MeshRenderer>());
                    blocker.transform.SetParent(transform, true);
                    blocker.transform.position = mid + Vector3.up * (fenceCollisionHeight * 0.5f);
                    blocker.transform.rotation = rot;
                    blocker.transform.localScale = new Vector3(side * 1.04f, fenceCollisionHeight, 0.3f);
                    _spawned.Add(blocker);

                    // Lo que se ve: translúcido y llamativo, pero bajo para no comerse la cámara.
                    var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    panel.name = "Cerco";
                    Destroy(panel.GetComponent<Collider>());
                    panel.transform.SetParent(transform, true);
                    panel.transform.position = mid + Vector3.up * (fenceVisualHeight * 0.5f);
                    panel.transform.rotation = rot;
                    panel.transform.localScale = new Vector3(side * 1.02f, fenceVisualHeight, 0.12f);
                    panel.GetComponent<MeshRenderer>().sharedMaterial = mat;
                    _spawned.Add(panel);
                }
            }
        }

        /// Hexágono relleno de radio 1 (se escala al usarlo), plano en XZ y mirando hacia arriba.
        static Mesh HexMesh()
        {
            if (_hexMesh != null) return _hexMesh;
            var verts = new Vector3[7];
            verts[0] = Vector3.zero;
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * 60f * i;
                verts[i + 1] = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            }
            var tris = new int[18];
            for (int i = 0; i < 6; i++)
            {
                // Orden invertido para que la normal apunte a +Y (si no, el hexágono se ve por abajo).
                tris[i * 3] = 0;
                tris[i * 3 + 1] = ((i + 1) % 6) + 1;
                tris[i * 3 + 2] = i + 1;
            }
            _hexMesh = new Mesh { name = "Hex" };
            _hexMesh.vertices = verts;
            _hexMesh.triangles = tris;
            _hexMesh.RecalculateNormals();
            _hexMesh.RecalculateBounds();
            return _hexMesh;
        }

        static Material Unlit(Color c)
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            var m = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Lit"));
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
            // Sin culling: la malla del hexágono se genera a mano y si el orden de los triángulos
            // quedara invertido, la celda sería invisible desde arriba y costaría entender por qué.
            // Dibujar las dos caras cuesta nada en caja gris y quita ese modo de fallo entero.
            if (m.HasProperty("_Cull")) m.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3002;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }
    }
}
