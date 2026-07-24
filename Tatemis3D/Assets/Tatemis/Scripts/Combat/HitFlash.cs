using UnityEngine;

namespace Tatemis
{
    /// Flash de impacto: tiñe a quien recibe el golpe para que el hit se LEA en caja gris (sin VFX).
    /// El color dice la EFECTIVIDAD (cálido = le haces 1.5x · frío = resiste · blanco = neutral) y la
    /// duración dice la CONTUNDENCIA (ver HitFeedback.Weight): un jab destella seco, un mordisco al
    /// final de una cadena destella largo.
    ///
    /// La curva es **sostén + caída**, no un fade desde el frame 0: sin sostén el ojo se pierde el
    /// destello entero. Los números están en constantes (no en el inspector) a propósito — el maniquí
    /// lo regenera SandboxBuilder en cada rebuild y los ajustes del inspector se perderían.
    /// Usa MaterialPropertyBlock → no instancia materiales.
    [DisallowMultipleComponent]
    public class HitFlash : MonoBehaviour
    {
        public static readonly Color Weak = new Color(1f, 0.80f, 0.25f);      // el objetivo es débil (1.5x)
        public static readonly Color Neutral = Color.white;
        public static readonly Color Resist = new Color(0.45f, 0.70f, 1f);    // el objetivo resiste (0.5x)

        // ---- Afinado del destello (tocar aquí en playtest) ----
        // Rebajado con la llegada del hitstop (T1.6): el congelado ya sostiene el destello por sí solo,
        // así que lo que antes hacía falta para percibirlo ahora se siente pastoso.
        public const float MinDuration = 0.10f;   // golpe flojo
        public const float MaxDuration = 0.22f;   // golpe con todo el peso
        const float HoldFraction = 0.4f;          // qué parte de la duración se queda a tope antes de caer
        const float MinStrength = 0.85f;          // cuánto tapa el color base un golpe flojo (1 = del todo)

        public static Color ColorFor(float multiplier) =>
            multiplier > 1.01f ? Weak : multiplier < 0.99f ? Resist : Neutral;

        public static float DurationFor(float weight01) =>
            Mathf.Lerp(MinDuration, MaxDuration, Mathf.Clamp01(weight01));

        Renderer[] _renderers;
        Color[] _baseColors;
        MaterialPropertyBlock _mpb;
        Color _flash;
        float _timer, _dur = MinDuration, _hold, _strength = 1f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        /// Dispara el flash en quien recibe el golpe. Si el objeto lleva ActionColor —el jugador— delega
        /// en él para que los dos no se peleen por el mismo MaterialPropertyBlock; y si no tiene ninguno
        /// de los dos se añade solo, para que funcione también en escenas guardadas antes de existir.
        public static void Play(Transform target, float multiplier, float weight01 = 0f)
        {
            if (target == null) return;
            if (target.TryGetComponent(out HitFlash held) && held._holding) return;   // debilitado: no se pisa el tinte
            Color c = ColorFor(multiplier);
            float dur = DurationFor(weight01);
            if (target.TryGetComponent(out HitFlash flash)) { flash.Flash(c, dur, weight01); return; }
            if (target.TryGetComponent(out ActionColor color)) { color.Flash(c, dur); return; }
            target.gameObject.AddComponent<HitFlash>().Flash(c, dur, weight01);
        }

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i].sharedMaterial;
                _baseColors[i] = mat != null && mat.HasProperty(BaseColorId)
                    ? mat.GetColor(BaseColorId)
                    : Color.white;
            }
            _mpb = new MaterialPropertyBlock();
        }

        /// Un golpe nuevo REINICIA el destello (no lo alarga): en una cadena rápida cada impacto se lee aparte.
        public void Flash(Color c, float dur, float weight01 = 1f)
        {
            _flash = c;
            _dur = Mathf.Max(0.02f, dur);
            _hold = _dur * HoldFraction;
            _strength = Mathf.Lerp(MinStrength, 1f, Mathf.Clamp01(weight01));
            _timer = _dur;
            Paint(_strength);
        }

        /// Tinte SOSTENIDO (no un destello): lo usa el estado de debilitado para que se lea de lejos
        /// que ese enemigo está caído y puedes elegir. `Clear()` lo devuelve a su color.
        public void Hold(Color c)
        {
            _holding = true;
            _flash = c;
            _timer = 0f;
            Paint(1f);
        }

        public void Clear()
        {
            if (!_holding) return;
            _holding = false;
            Paint(0f);
        }

        bool _holding;

        void Update()
        {
            if (_holding || _timer <= 0f) return;
            _timer -= Time.deltaTime;

            float elapsed = _dur - _timer;
            float k = elapsed <= _hold
                ? 1f                                                   // sostén: el destello se queda
                : 1f - (elapsed - _hold) / Mathf.Max(0.0001f, _dur - _hold);
            Paint(Mathf.Clamp01(k) * _strength);                       // 0 al terminar → color base exacto
        }

        /// `k` = cuánto tapa el color de impacto al color base (1 = solo impacto, 0 = solo base).
        void Paint(float k)
        {
            if (_mpb == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(BaseColorId, Color.Lerp(_baseColors[i], _flash, k));
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
