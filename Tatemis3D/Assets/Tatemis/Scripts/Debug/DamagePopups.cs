using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Números de daño flotantes (caja gris): IMGUI proyectado desde el mundo, sin Canvas ni prefabs.
    /// Se auto-crea con el primer Spawn, así que nadie tiene que cablearlo en la escena.
    ///
    /// El número cuenta tres cosas a la vez — y es siempre el **resultado**, el daño que de verdad
    /// ha entrado (multiplicador ya aplicado), nunca la cuenta que lo produjo:
    ///  · **color** = efectividad elemental (la misma tabla que el destello, ver HitFlash.ColorFor),
    ///  · **tamaño y vida** = peso del golpe (daño relativo a ESE monstruo + avance del combo → HitFeedback),
    ///  · **trayectoria** = sale despedido en la dirección del knockback y cae, en vez de subir recto:
    ///    el número acompaña al cuerpo, así se lee de dónde vino el golpe. Además se abre **a un
    ///    costado** (perpendicular al empuje, alternando lado) para no taparse con el cuerpo ni con
    ///    el número anterior — sin eso, en un combo todos se apilan sobre la misma línea.
    /// Sustituible por UI real sin tocar a quien lo llama.
    public class DamagePopups : MonoBehaviour
    {
        public static bool Enabled = true;

        // ---- Afinado (tocar aquí en playtest) ----
        const float MinLife = 0.7f, MaxLife = 1.1f;      // s en pantalla: golpe flojo → golpe gordo
        const int MinFont = 17, MaxFont = 40;
        const float RiseSpeed = 2.2f;                    // u/seg de impulso hacia arriba
        const float Gravity = 4.5f;                      // u/seg² que lo hace caer al final
        const float MinPush = 0.8f, MaxPush = 3.5f;      // u/seg en la dirección del knockback
        const float MinSide = 0.7f, MaxSide = 2.0f;      // u/seg perpendiculares: abre el número a un lado
        const int MaxLive = 48;

        struct Popup
        {
            public Vector3 pos, vel;
            public string text;
            public Color color;
            public float life, age;
            public int fontSize;
        }

        static readonly List<Popup> _live = new List<Popup>();
        static DamagePopups _host;
        static float _sideFlip = 1f;   // alterna el costado de cada número

        public static void Spawn(Vector3 worldPos, float damage, float multiplier,
                                 Vector3 knockDir, float weight01)
        {
            if (!Enabled) return;
            if (_host == null) _host = new GameObject("~DamagePopups").AddComponent<DamagePopups>();

            weight01 = Mathf.Clamp01(weight01);

            // Solo el resultado: el daño que ha entrado, con el multiplicador YA aplicado. Nada de
            // enseñar la cuenta ("8 ×1.5") — la efectividad ya la dicen el color y el tamaño.
            string text = Mathf.RoundToInt(damage).ToString();

            // La efectividad modula el tamaño que ya fijó el peso: lo que más manda es la contundencia.
            float effScale = multiplier > 1.01f ? 1.12f : multiplier < 0.99f ? 0.88f : 1f;

            knockDir.y = 0f;
            bool hasDir = knockDir.sqrMagnitude > 0.0001f;
            if (hasDir) knockDir.Normalize();

            Vector3 push = hasDir ? knockDir * Mathf.Lerp(MinPush, MaxPush, weight01) : Vector3.zero;

            // Costado: perpendicular al empuje (con la cámara detrás, eso es el eje horizontal de
            // pantalla) y ALTERNANDO lado en cada golpe → en un combo los números se abren en abanico
            // en vez de apilarse sobre el cuerpo.
            var cam = Camera.main;
            Vector3 sideAxis = hasDir
                ? Vector3.Cross(Vector3.up, knockDir)
                : (cam != null ? Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized : Vector3.right);
            _sideFlip = -_sideFlip;
            Vector3 side = sideAxis * (_sideFlip * Mathf.Lerp(MinSide, MaxSide, weight01) * Random.Range(0.75f, 1.25f));

            _live.Add(new Popup
            {
                pos = worldPos + Vector3.up * 0.2f,
                vel = push + side + Vector3.up * RiseSpeed,
                text = text,
                color = HitFlash.ColorFor(multiplier),
                life = Mathf.Lerp(MinLife, MaxLife, weight01),
                fontSize = Mathf.RoundToInt(Mathf.Lerp(MinFont, MaxFont, weight01) * effScale),
            });
            if (_live.Count > MaxLive) _live.RemoveAt(0);
        }

        // La lista es estática: si se juega con el domain reload desactivado, sobreviviría al Play anterior.
        void Awake() => _live.Clear();

        void Update()
        {
            // unscaled: que sigan volando aunque el hitstop congele el juego (T1.6).
            float dt = Time.unscaledDeltaTime;
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var p = _live[i];
                p.age += dt;
                if (p.age >= p.life) { _live.RemoveAt(i); continue; }
                p.vel.y -= Gravity * dt;      // sale despedido y cae, como el cuerpo al que golpeas
                p.pos += p.vel * dt;
                _live[i] = p;
            }
        }

        GUIStyle _style;
        readonly GUIContent _gc = new GUIContent();

        void OnGUI()
        {
            var cam = Camera.main;
            if (cam == null || _live.Count == 0) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Overflow,   // que nunca recorte, pase lo que pase con el Rect
                    wordWrap = false,
                };

            foreach (var p in _live)
            {
                Vector3 sp = cam.WorldToScreenPoint(p.pos);
                if (sp.z <= 0f) continue;                                   // detrás de la cámara

                float t = p.age / p.life;
                float alpha = 1f - t * t * t;                               // opaco casi todo, se apaga al final
                float scale = Mathf.Clamp(12f / Mathf.Max(1f, sp.z), 0.6f, 1.4f);   // lejos = más pequeño
                _style.fontSize = Mathf.RoundToInt(p.fontSize * scale);

                // El Rect se mide a partir del texto YA con su fontSize: con un Rect fijo, los números
                // grandes (el Flex a combo lleno) no cabían y GUI.Label los recortaba.
                _gc.text = p.text;
                Vector2 size = _style.CalcSize(_gc) + new Vector2(6f, 6f);
                var r = new Rect(sp.x - size.x * 0.5f, Screen.height - sp.y - size.y * 0.5f, size.x, size.y);
                _style.normal.textColor = new Color(0f, 0f, 0f, alpha * 0.7f);      // sombra: legible sobre suelo claro
                GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), p.text, _style);
                _style.normal.textColor = new Color(p.color.r, p.color.g, p.color.b, alpha);
                GUI.Label(r, p.text, _style);
            }
        }
    }
}
