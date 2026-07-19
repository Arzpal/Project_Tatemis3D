using UnityEngine;

namespace Tatemis
{
    /// HUD de debug con IMGUI (sin montar Canvas): barra de energía del jugador +
    /// vida/aturdimiento del objetivo con lock-on. Añádelo a cualquier GameObject de la escena.
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] ElementalEnergy energy;      // del jugador
        [SerializeField] TargetingSystem targeting;   // del jugador

        void Reset()
        {
            energy = Object.FindFirstObjectByType<ElementalEnergy>();
            targeting = Object.FindFirstObjectByType<TargetingSystem>();
        }

        void OnGUI()
        {
            const float w = 280f, h = 18f, pad = 10f, gap = 4f;
            float y = pad;

            if (energy != null)
            {
                Bar(new Rect(pad, y, w, h), energy.Normalized,
                    $"Energía  {Mathf.RoundToInt(energy.current)}/{Mathf.RoundToInt(energy.max)}",
                    new Color(0.3f, 0.8f, 0.95f));
                y += h + gap;
            }

            if (targeting != null && targeting.Current is StaggerReceiver sr)
            {
                Bar(new Rect(pad, y, w, h), sr.health / Mathf.Max(1f, sr.maxHealth),
                    $"Vida  {Mathf.RoundToInt(sr.health)}", new Color(0.85f, 0.25f, 0.25f));
                y += h + gap;

                Bar(new Rect(pad, y, w, h), sr.stagger / Mathf.Max(1f, sr.maxStagger),
                    $"Aturdimiento  {Mathf.RoundToInt(sr.stagger)}", new Color(0.95f, 0.75f, 0.2f));
                y += h + gap;

                if (sr.InKOWindow)
                    GUI.Label(new Rect(pad, y, w, h), "★ ¡NOQUEADO! — ventana de captura abierta");
            }
            else
            {
                GUI.Label(new Rect(pad, y, w, h), "Sin objetivo — haz lock-on (Q / R3)");
            }
        }

        static Texture2D _px;
        static Texture2D Px
        {
            get
            {
                if (_px == null)
                {
                    _px = new Texture2D(1, 1);
                    _px.SetPixel(0, 0, Color.white);
                    _px.Apply();
                }
                return _px;
            }
        }

        static void Bar(Rect r, float fill01, string label, Color color)
        {
            fill01 = Mathf.Clamp01(fill01);
            Color old = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);            // fondo
            GUI.DrawTexture(r, Px);

            GUI.color = color;                                   // relleno
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * fill01, r.height), Px);

            GUI.color = old;
            GUI.Label(new Rect(r.x + 6f, r.y, r.width, r.height), label);
        }
    }
}
