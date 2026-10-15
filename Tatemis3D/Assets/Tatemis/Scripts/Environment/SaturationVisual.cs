using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **La carga, vista** ([ELEMENTAL-REACTIONS §0.10](../../../../docs/ELEMENTAL-REACTIONS.md) y §6).
    ///
    /// 🔑 **Esto no es pulido: es la mitad de la mecánica.** Si el hexágono no se ve llenarse, el
    /// jugador no percibe carga — percibe que **golpear el suelo dejó de hacer nada**, que es
    /// exactamente el fallo que la saturación vino a arreglar. En el primer playtest (2026-07-31) la
    /// carga solo existía como texto de debug, y el sistema entero se leía como roto.
    ///
    /// Se dibuja **al margen de `CombatZone`** a propósito: la zona de combate solo pinta celdas
    /// mientras hay un encuentro abierto, y la saturación tiene que verse **siempre y en cualquier
    /// escena** (mundo, sandbox, laboratorio). Se crea sola desde `ReactionSystem`.
    ///
    /// Dos señales, para dos distancias:
    ///  · **De cerca — el hexágono se llena.** Crece desde el centro y se enciende con el color del
    ///    elemento que lo está cargando: ves *quién* carga y *cuánto falta*, en el sitio donde vas a
    ///    tener que pelear.
    ///  · **De lejos — la columna.** Una barra vertical en el centro de la mancha cuya altura *es* el
    ///    medidor. Es lo que permite decidir "voy a esa zona" sin estar encima.
    /// Cebada, las dos **laten en blanco**: ese latido es lo que hace justa una explosión enorme.
    public class SaturationVisual : MonoBehaviour
    {
        [Tooltip("Altura a la que flota el relleno. Por encima de las celdas de CombatZone (0.07) para " +
                 "que se vea encima del color del terreno en vez de pelearse con él por el z-fighting")]
        public float height = 0.16f;
        [Tooltip("Tamaño del hexágono con la batería vacía, como fracción de la celda. Crecer desde " +
                 "pequeño es lo que se lee como MEDIDOR; con solo cambiar el color parecería otro terreno")]
        [Range(0.1f, 0.9f)] public float minScale = 0.34f;
        [Tooltip("Altura de la columna con el medidor lleno (unidades del mundo)")]
        public float pillarHeight = 5f;
        public float pillarWidth = 0.55f;
        [Tooltip("Latido de la mancha CEBADA, en ciclos por segundo")]
        public float primedPulseHz = 3.2f;

        ElementalGrid _grid;
        TerrainSaturation _sat;
        Material _mat;
        MaterialPropertyBlock _mpb;

        readonly List<Transform> _hexPool = new List<Transform>();
        readonly List<Transform> _pillarPool = new List<Transform>();
        int _hexUsed, _pillarUsed;

        public static SaturationVisual Create(ElementalGrid grid, TerrainSaturation sat)
        {
            var go = new GameObject("SaturacionVisual");
            var v = go.AddComponent<SaturationVisual>();
            v._grid = grid;
            v._sat = sat;
            return v;
        }

        void OnDestroy() { if (_mat != null) Destroy(_mat); }

        /// `LateUpdate` para pintar **después** de que el sistema de reacciones haya depositado en este
        /// frame: si se pintara en `Update`, la carga iría siempre un frame por detrás del golpe y el
        /// impacto se sentiría desacoplado de su consecuencia.
        void LateUpdate()
        {
            if (_grid == null || _sat == null) return;
            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            _hexUsed = 0;
            _pillarUsed = 0;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * primedPulseHz * Mathf.PI * 2f);

            foreach (var kv in _sat.LivePatches)
            {
                var cells = _grid.PatchCells(kv.Key);
                if (cells.Count == 0) continue;
                DrawBattery(kv.Value, cells, pulse);
            }
            foreach (var kv in _sat.LiveCells)
                DrawBattery(kv.Value, new[] { kv.Key }, pulse);

            for (int i = _hexUsed; i < _hexPool.Count; i++) _hexPool[i].gameObject.SetActive(false);
            for (int i = _pillarUsed; i < _pillarPool.Count; i++) _pillarPool[i].gameObject.SetActive(false);
        }

        void DrawBattery(TerrainSaturation.Battery b, IReadOnlyList<HexCoord> cells, float pulse)
        {
            float fill = b.Fill01;
            if (fill <= 0.001f) return;

            var col = ElementalGrid.ColorOf(b.owner);
            float scale = Mathf.Lerp(minScale, 1f, fill);
            col.a = Mathf.Lerp(0.30f, 0.85f, fill);

            if (b.primed)
            {
                // Cebada: blanco que late y se pasa un poco del hexágono. Tiene que gritar.
                col = Color.Lerp(col, Color.white, 0.35f + 0.45f * pulse);
                col.a = Mathf.Lerp(0.75f, 1f, pulse);
                scale = 1f + 0.12f * pulse;
            }

            Vector3 centroid = Vector3.zero;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 p = cells[i].ToWorld(_grid.cellRadius);
                centroid += p;
                var t = NextHex();
                t.position = new Vector3(p.x, height, p.z);
                t.localScale = Vector3.one * (_grid.cellRadius * 0.93f * scale);
                Paint(t, col);
            }
            centroid /= cells.Count;

            // La columna: su ALTURA es el medidor. Es la señal de largo alcance — la que deja decidir
            // "voy a esa mancha" sin tener que estar encima leyendo colores.
            var pil = NextPillar();
            float h = pillarHeight * (b.primed ? 1f : fill);
            pil.position = new Vector3(centroid.x, h * 0.5f, centroid.z);
            pil.localScale = new Vector3(pillarWidth, h * 0.5f, pillarWidth);   // el cilindro mide 2 de alto
            var pcol = col; pcol.a *= 0.55f;                                    // más tenue: no debe tapar
            Paint(pil, pcol);
        }

        void Paint(Transform t, Color c)
        {
            var r = t.GetComponent<Renderer>();
            if (r == null) return;
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            r.SetPropertyBlock(_mpb);
        }

        Transform NextHex()
        {
            if (_hexUsed < _hexPool.Count)
            {
                var t = _hexPool[_hexUsed++];
                t.gameObject.SetActive(true);
                return t;
            }
            var go = new GameObject("Carga");
            go.transform.SetParent(transform, true);
            go.AddComponent<MeshFilter>().sharedMesh = CombatZone.HexMesh();
            Setup(go.AddComponent<MeshRenderer>());
            _hexPool.Add(go.transform);
            _hexUsed++;
            return go.transform;
        }

        Transform NextPillar()
        {
            if (_pillarUsed < _pillarPool.Count)
            {
                var t = _pillarPool[_pillarUsed++];
                t.gameObject.SetActive(true);
                return t;
            }
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "CargaColumna";
            // DestroyImmediate y no Destroy: el collider de un primitivo vive hasta fin de frame, y un
            // cilindro de 5 u en mitad de la arena es un muro fantasma con el que el jugador choca.
            DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(transform, true);
            Setup(go.GetComponent<MeshRenderer>());
            _pillarPool.Add(go.transform);
            _pillarUsed++;
            return go.transform;
        }

        void Setup(MeshRenderer r)
        {
            if (_mat == null) _mat = Transparent();
            r.sharedMaterial = _mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        /// Un solo material para todo: el color va por `MaterialPropertyBlock`, porque aquí **cambia
        /// cada frame** y crear materiales por celda sería una fuga garantizada.
        Material Transparent()
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            var m = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Lit"));
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
            if (m.HasProperty("_Cull")) m.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3050;   // por encima de las celdas del tablero (3002)
            return m;
        }
    }
}
