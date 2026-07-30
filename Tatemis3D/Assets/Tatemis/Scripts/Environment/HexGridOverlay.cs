using UnityEngine;

namespace Tatemis
{
    /// La rejilla hexagonal LEVE de la arena (ARENA.md §1): overlay de LECTURA sobre un parche de
    /// terreno elemental — no es geometría, el mundo sigue continuo. Marca visualmente "esta sección
    /// tiene elemento" con hexágonos sutiles; el marco de la zona de combate llegará con los
    /// encuentros (hoy no hay IA que los inicie).
    /// Celda ≈ 2× un monstruo grande / 3× un bebé (circumradius ~2.2 en caja gris, afinable).
    /// Se genera en runtime (Start): la escena guardada no acumula cientos de líneas.
    public class HexGridOverlay : MonoBehaviour
    {
        [Tooltip("Área (X×Z) que cubre la rejilla, centrada en este transform")]
        public Vector2 area = new Vector2(8f, 8f);
        [Tooltip("Radio del hexágono (centro a vértice). La celda se mide en HUELLA (es una rejilla " +
                 "de suelo): cara a cara = radius × √3 ≈ 3 u con 1.75, que es 2× un monstruo grande " +
                 "(~1,5 u de ancho) y 3× un bebé (~1 u). ARENA §1 — provisional, a validar en playtest")]
        public float cellRadius = 1.75f;
        public Color lineColor = new Color(1f, 1f, 1f, 0.28f);
        public float lineWidth = 0.035f;
        [Tooltip("Altura de las líneas sobre el suelo del parche")]
        public float height = 0.07f;

        void Start()
        {
            var mat = LineMaterial();
            float dx = cellRadius * 1.5f;                 // hexágonos flat-top
            float dz = cellRadius * Mathf.Sqrt(3f);
            int cols = Mathf.CeilToInt(area.x / dx) + 1;
            int rows = Mathf.CeilToInt(area.y / dz) + 1;

            for (int cx = 0; cx <= cols; cx++)
                for (int cz = 0; cz <= rows; cz++)
                {
                    // Centrado: la retícula se reparte a los dos lados del parche en vez de arrancar
                    // en el borde y desbordar por el otro.
                    float x = (cx - cols * 0.5f) * dx;
                    float z = (cz - rows * 0.5f) * dz + (cx % 2 == 1 ? dz * 0.5f : 0f);
                    // Recorte por EXTENSIÓN, no por centro: un hexágono se dibuja entero, así que
                    // filtrar su centro dejaba medio hexágono pintado sobre suelo NEUTRO, donde por
                    // R5 no reacciona nada — justo la lectura contraria a la que ARENA §1 pide.
                    if (Mathf.Abs(x) + cellRadius > area.x * 0.5f) continue;
                    if (Mathf.Abs(z) + cellRadius * 0.866f > area.y * 0.5f) continue;
                    MakeHex(new Vector3(x, height, z), mat);
                }
        }

        void MakeHex(Vector3 localCenter, Material mat)
        {
            var go = new GameObject("Hex");
            go.transform.SetParent(transform, false);   // el padre NO va escalado (los hexágonos no se deforman)
            go.transform.localPosition = localCenter;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 6;
            lr.widthMultiplier = lineWidth;
            lr.sharedMaterial = mat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i);
                lr.SetPosition(i, new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * cellRadius);
            }
        }

        /// Transparente de verdad: sin configurar la mezcla, el material sale Opaque y el alpha de
        /// `lineColor` es código muerto — la rejilla "leve" que pide ARENA §1 se dibujaría sólida.
        Material LineMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var m = new Material(shader != null ? shader : Shader.Find("Universal Render Pipeline/Lit"));
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);     // 1 = Transparent
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);         // 0 = Alpha
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3001;    // por encima del disco de reacción (3000): la rejilla no se entierra
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", lineColor);
            if (m.HasProperty("_Color")) m.SetColor("_Color", lineColor);
            return m;
        }
    }
}
