using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// Spray elemental de debug (ELEMENTAL-REACTIONS §8): dispara cualquier elemento contra el
    /// terreno sin necesitar una criatura que lo genere — es la herramienta para probar las 30
    /// casillas de la tabla. F2 cicla el elemento · G lo dispara delante del jugador.
    /// Va en el jugador del sandbox; teclas directas (Keyboard.current), patrón de DebugHUD.
    public class ElementalSpray : MonoBehaviour
    {
        public float radius = 2.5f;
        [Tooltip("Fuerza del impacto simulado (R2). 15 ≈ un estallido de habilidad")]
        public float impactStrength = 15f;
        [Tooltip("A cuántas unidades por delante del cuerpo cae el disparo")]
        public float reach = 3f;
        public Element current = Element.Fuego;

        ICombatant _self;

        void Awake() => _self = GetComponent<ICombatant>();

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f2Key.wasPressedThisFrame)
                current = (Element)(((int)current + 1) % System.Enum.GetValues(typeof(Element)).Length);

            if (kb.gKey.wasPressedThisFrame)
            {
                Vector3 pos = transform.position + transform.forward * reach;
                HitboxDebug.Show(pos, radius, 0.2f, HitboxDebug.ColorOf(current));
                // El spray SÍ tiene dueño (tú): así también sirve para comprobar R6 — tu dominancia
                // no te golpea; la respuesta del terreno y las combinaciones, sí. El último parámetro
                // es el daño que repite la AMPLIFICACIÓN si disparas tu propio elemento.
                CombatServices.Reactions?.Impact(pos, radius, current, impactStrength, _self, impactStrength);
            }
        }

        void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            style.normal.textColor = HitboxDebug.ColorOf(current);
            GUI.Label(new Rect(12f, Screen.height - 28f, 500f, 22f),
                      $"Spray elemental: {current}   (F2 cicla · G dispara)", style);
        }
    }
}
