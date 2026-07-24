using UnityEngine;

namespace Tatemis
{
    /// **Peso del golpe (T1.6):** hitstop + sacudida de cámara. Los dos escalan con el MISMO
    /// `HitFeedback.Weight` que ya gobierna el destello y el número, así que un jab tiembla poco y
    /// un mordisco a combo lleno para el mundo un instante. Una sola noción de contundencia, no
    /// tres afinados que se contradicen.
    ///
    /// Se auto-crea al primer impacto: nada que cablear en la escena.
    ///
    /// ⚠️ `DefaultExecutionOrder` alto **a propósito**: la sacudida se suma a la posición que fija
    /// `PlayerCameraRig` en su `LateUpdate`, así que tiene que correr DESPUÉS o el rig la pisaría.
    [DefaultExecutionOrder(1000)]
    public class ImpactFeel : MonoBehaviour
    {
        public static bool Enabled = true;

        // ---- Afinado (tocar aquí en playtest) ----
        const float MinFreeze = 0.02f, MaxFreeze = 0.09f;   // s de congelado, por peso
        const float MinShake = 0.025f, MaxShake = 0.18f;    // desplazamiento (u) de la sacudida
        const float MinRoll = 0.15f, MaxRoll = 0.9f;        // sacudida ANGULAR (grados) — la que se nota
        const float ShakeTime = 0.18f;                      // cuánto tarda en apagarse
        const float ShakeFreq = 34f;                        // vibraciones por segundo

        /// Perlin devuelve [0,1] pero se queda en ~[0.2,0.8]: casi nunca toca los extremos, así que
        /// centrarlo sin más daba **el 60% de la amplitud pedida**. Aquí el número declarado es el real.
        const float NoiseGain = 1.7f;
        static float Noise(float t) => Mathf.Clamp((Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * NoiseGain, -1f, 1f);

        static ImpactFeel _host;
        static float _freezeUntil;                          // en tiempo REAL, no escalado
        static float _shakeAmp, _rollAmp, _shakeUntil, _shakeStart;

        /// Lo llama `HitFeedback` en cada impacto. `weight01` = contundencia (0..1).
        public static void Impact(float weight01)
        {
            weight01 = Mathf.Clamp01(weight01);
            Play(Mathf.Lerp(MinFreeze, MaxFreeze, weight01), weight01);
        }

        /// Canal aparte para **eventos gordos** (un estallido elemental, una reacción): pueden pedir
        /// más congelado del que da el máximo por daño, porque su peso no viene de cuánto quitan de
        /// vida sino de lo que son. Sin esto habría que mentir con el daño para conseguir el parón.
        public static void Shockwave(float freezeSeconds, float weight01 = 1f) =>
            Play(freezeSeconds, Mathf.Clamp01(weight01));

        static void Play(float freeze, float weight01)
        {
            if (!Enabled) return;
            if (_host == null) _host = new GameObject("~ImpactFeel").AddComponent<ImpactFeel>();

            // Congelar: se toma el MÁS LARGO en curso, no se acumula. Encadenar golpes rápidos no
            // debe convertirse en cámara lenta permanente.
            _freezeUntil = Mathf.Max(_freezeUntil, Time.realtimeSinceStartup + freeze);

            _shakeAmp = Mathf.Max(_shakeAmp, Mathf.Lerp(MinShake, MaxShake, weight01));
            _rollAmp = Mathf.Max(_rollAmp, Mathf.Lerp(MinRoll, MaxRoll, weight01));
            _shakeStart = Time.realtimeSinceStartup;
            _shakeUntil = _shakeStart + ShakeTime;
        }

        Vector3 _applied;    // sacudida aplicada el frame anterior
        bool _frozen;

        void Update()
        {
            // 1) Deshacer la sacudida ANTES de que el rig recalcule su posición. Si no, el muelle del
            //    rig partiría de la posición ya sacudida y se tragaría el temblor como movimiento real.
            var cam = Camera.main;
            if (cam != null && _applied != Vector3.zero)
            {
                cam.transform.position -= _applied;
                _applied = Vector3.zero;
            }

            // 2) El hitstop se mide en tiempo REAL: con timeScale a 0, Time.time no avanza y nunca
            //    saldríamos del congelado. Solo se escribe al cambiar de estado, para no pisar a otro
            //    sistema que quiera su propio timeScale (el slow-mo del menú del Tamer, más adelante).
            bool frozen = Time.realtimeSinceStartup < _freezeUntil;
            if (frozen != _frozen)
            {
                _frozen = frozen;
                Time.timeScale = frozen ? 0f : 1f;
            }
        }

        /// La sacudida se suma ENCIMA de lo que fijó el rig, y se deshace en el Update siguiente:
        /// así el rig nunca ve la cámara movida y no hay que tocar su estado.
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null || _shakeAmp <= 0.0001f) return;

            float now = Time.realtimeSinceStartup;
            if (now >= _shakeUntil) { _shakeAmp = 0f; _rollAmp = 0f; return; }

            float k = 1f - (now - _shakeStart) / ShakeTime;      // se apaga hacia el final
            float fade = k * k;

            // Frecuencias distintas por eje → no es una vibración regular de máquina.
            float t = now * ShakeFreq;
            float nx = Noise(t), ny = Noise(t * 1.3f + 37f), nz = Noise(t * 0.9f + 91f);

            Vector3 offset = (cam.transform.right * nx + cam.transform.up * ny) * (_shakeAmp * fade);
            cam.transform.position += offset;
            _applied = offset;

            // Sacudida ANGULAR: en tercera persona un grado de giro se nota mucho más que mover la
            // cámara de sitio, y no arrastra el encuadre. No hace falta deshacerla — el rig reasigna
            // la rotación entera cada frame, al contrario que la posición (que va con muelle).
            float roll = _rollAmp * fade;
            cam.transform.rotation *= Quaternion.Euler(ny * roll * 0.6f, nx * roll * 0.6f, nz * roll);
        }

        void OnDisable() { Time.timeScale = 1f; _frozen = false; }   // salir de Play congelado sería un lío
    }
}
