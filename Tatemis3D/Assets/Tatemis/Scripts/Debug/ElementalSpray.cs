using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// Spray elemental de debug (ELEMENTAL-REACTIONS §8): dispara cualquier elemento contra el
    /// terreno sin necesitar una criatura que lo genere — es la herramienta para probar las 30
    /// casillas de la tabla. **F2** cicla el elemento · **G** dispara · **H** alterna el modo.
    ///
    /// ⚠️ **Adaptado a la saturación (§0.10):** desde el rediseño, un disparo **ya no resuelve nada** —
    /// deposita. Para ver una pareja concreta hacen falta varios (5y una reacción, 3y una combinación),
    /// así que el spray tiene ahora **dos modos**:
    ///  · **Saturar** (por defecto): el camino real del juego. Sirve para ver cargarse la mancha, el
    ///    latido del cebado y la onda al reventar.
    ///  · **Instantáneo**: entra por `ReactionSystem.Instant`, la misma puerta de las tres excepciones,
    ///    y resuelve la pareja de un golpe. Es lo que hace que el checklist de las 30 casillas siga
    ///    siendo practicable sin vaciar medio combate en cada una.
    /// Va en el jugador del sandbox; teclas directas (Keyboard.current), patrón de DebugHUD.
    public class ElementalSpray : MonoBehaviour
    {
        public float radius = 2.5f;
        [Tooltip("Fuerza del impacto simulado. 15 ≈ un estallido de habilidad = 1y (ER §0.10)")]
        public float impactStrength = 15f;
        [Tooltip("A cuántas unidades por delante del cuerpo cae el disparo")]
        public float reach = 3f;
        public Element current = Element.Fuego;
        [Tooltip("true = resuelve de un golpe (para el checklist) · false = deposita saturación (el juego real)")]
        public bool instantMode;

        ICombatant _self;

        void Awake() => _self = GetComponent<ICombatant>();

        Vector3 Muzzle => transform.position + transform.forward * reach;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f2Key.wasPressedThisFrame)
                current = (Element)(((int)current + 1) % System.Enum.GetValues(typeof(Element)).Length);

            if (kb.hKey.wasPressedThisFrame) instantMode = !instantMode;

            if (kb.gKey.wasPressedThisFrame)
            {
                Vector3 pos = Muzzle;
                HitboxDebug.Show(pos, radius, 0.2f, HitboxDebug.ColorOf(current));
                // El spray SÍ tiene dueño (tú): así también sirve para comprobar R6 — tu dominancia
                // no te golpea; la respuesta del terreno y las combinaciones, sí. El último parámetro
                // es el daño que repite la AMPLIFICACIÓN si disparas tu propio elemento.
                var rs = CombatServices.Reactions;
                if (rs == null) return;
                if (instantMode) rs.Instant(pos, radius, current, impactStrength, _self, impactStrength);
                else             rs.Impact(pos, radius, current, impactStrength, _self, impactStrength);
            }
        }

        void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            style.normal.textColor = HitboxDebug.ColorOf(current);
            string modo = instantMode ? "INSTANTÁNEO" : "saturar";
            GUI.Label(new Rect(12f, Screen.height - 48f, 700f, 22f),
                      $"Spray elemental: {current}  ·  modo {modo}   (F2 cicla · G dispara · H cambia modo)", style);

            // Lectura de la batería bajo el punto de disparo: sin esto no hay forma de saber si un
            // golpe hizo algo, que es exactamente el fallo que §0.10 vino a arreglar.
            var grid = CombatServices.Grid;
            var sat = CombatServices.Reactions != null ? CombatServices.Reactions.Saturation : null;
            if (grid == null || sat == null) return;

            var c = grid.CoordAt(Muzzle);
            var cell = grid.CellAt(c);
            var owner = sat.OwnerAt(c);
            string terreno = cell.element.HasValue ? cell.element.Value.ToString() : "neutro";
            string mancha = cell.patch != 0 ? $"mancha #{cell.patch} ({grid.PatchSize(cell.patch)} celdas)" : "celda suelta";
            string carga = owner.HasValue
                ? $"{owner.Value} {sat.Fill01(c) * 100f:0}%{(sat.IsPrimed(c) ? "  ¡CEBADA!" : "")}"
                : "vacía";

            var s2 = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            s2.normal.textColor = sat.IsPrimed(c) ? Color.white : new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(12f, Screen.height - 28f, 700f, 22f),
                      $"Bajo el punto: {terreno} · {mancha} · batería: {carga}" +
                      (grid.IsTransformed(c) ? "  · TRANSFORMADA" : ""), s2);
        }
    }
}
