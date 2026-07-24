using UnityEngine;

namespace Tatemis
{
    /// Reacción a los golpes: knockback (empuje con colisión de paredes), CARRY (mantener al enemigo
    /// por delante del atacante para encadenar el combo) y flinch (breve hitstun). El carry sigue al
    /// atacante mientras se mueve, así los golpes lo re-centran al frente en vez de alejarlo, y una
    /// embestida lo lleva consigo hasta chocar una pared. Todo respeta la colisión vía CharacterController.
    public class HitReactor : MonoBehaviour
    {
        [Header("Gravedad y suspensión (juggle)")]
        public float gravity = -25f;
        [Tooltip("Segundos MÁXIMOS de suspensión acumulables sin tocar el suelo. Es el guardarraíl contra el juggle infinito: se recarga al aterrizar. ~3 s ≈ una cadena aérea completa, no más")]
        public float maxHang = 3f;
        [Tooltip("A qué ritmo se frena la caída al entrar en suspensión (u/s²). ALTO a propósito: tiene que frenar mucho más rápido de lo que dura el hang, o el golpe solo 'frena un poco' la caída en vez de sostener, y no se siente nada")]
        public float hangDamp = 160f;

        CharacterController _cc;

        Vector3 _knockVel;
        float _knockTimer;

        Transform _carrySource;
        float _carryDist, _carrySpeed, _carryTimer;

        float _fallVel;               // velocidad vertical propia
        float _hangUntil, _hangUsed;  // suspensión: hasta cuándo y cuánta se ha gastado en este vuelo

        public bool IsFlinching => FlinchTimer > 0f;
        public float FlinchTimer { get; private set; }
        public bool IsKnockedBack => _knockTimer > 0f;
        public bool IsHanging => Time.time < _hangUntil;
        public bool IsAirborne => _cc != null && !_cc.isGrounded;

        void Awake() => _cc = GetComponent<CharacterController>();

        /// **Suspende la caída** un instante (hang time). Es lo que permite encadenar en el aire: cada
        /// golpe renueva el margen, así que mientras sigas conectando, el enemigo no cae. El tope
        /// (`maxHang`) evita el juggle infinito y **se recarga solo al tocar suelo**.
        public void ApplyHang(float seconds)
        {
            if (seconds <= 0f || _hangUsed >= maxHang) return;
            // El presupuesto mide TIEMPO SUSPENDIDO, no número de golpes: solo se cobra lo que este
            // golpe añade sobre lo que ya había. Si no, una cadena multi-hit agotaría el margen en
            // cuatro impactos que apenas se solapan.
            float from = Mathf.Max(_hangUntil, Time.time);
            float extension = Mathf.Min(Time.time + seconds - from, maxHang - _hangUsed);
            if (extension <= 0f) return;
            _hangUsed += extension;
            _hangUntil = from + extension;
        }

        /// Empuje sostenido: velocidad (u/seg) durante `duration`.
        public void ApplyKnockback(Vector3 velocity, float duration)
        {
            _knockVel = velocity;
            _knockTimer = Mathf.Max(_knockTimer, duration);
        }

        /// Mantener al objetivo a `distance` por DELANTE de `source` (re-centra el combo / carry de
        /// embestida). `matchHeight` lo lleva además a la ALTURA del atacante: sin eso, una embestida
        /// en diagonal lo deja arriba pero fuera de alcance, y el remate aéreo es imposible.
        public void ApplyCarry(Transform source, float distance, float speed, float duration,
                               bool matchHeight = false, float heightOffset = 0f)
        {
            _carrySource = source;
            _carryDist = distance;
            _carrySpeed = speed;
            _carryMatchHeight = matchHeight;
            _carryHeightOffset = heightOffset;
            _carryTimer = Mathf.Max(_carryTimer, duration);
        }

        bool _carryMatchHeight;
        float _carryHeightOffset;

        public void ApplyFlinch(float duration) => FlinchTimer = Mathf.Max(FlinchTimer, duration);

        void Update()
        {
            float dt = Time.deltaTime;
            if (FlinchTimer > 0f) FlinchTimer -= dt;

            Vector3 move = Vector3.zero;

            if (_knockTimer > 0f)
            {
                _knockTimer -= dt;
                move += _knockVel * dt;
            }

            if (_carryTimer > 0f && _carrySource != null)
            {
                _carryTimer -= dt;
                // Radial: acerca al objetivo en SU MISMA dirección respecto al atacante (sin desvío
                // lateral → no hace girar la cámara), y solo si está más lejos que la distancia objetivo.
                Vector3 toSelf = transform.position - _carrySource.position; toSelf.y = 0f;
                float dist = toSelf.magnitude;
                float heightGap = _carryMatchHeight
                    ? Mathf.Abs(transform.position.y - (_carrySource.position.y + _carryHeightOffset))
                    : 0f;

                // Actúa si se ha alejado DE MÁS, en horizontal o —con matchHeight— en altura: es el
                // tope que impide que un golpe fuerte lo mande fuera del alcance del siguiente.
                if (dist > _carryDist + 0.01f || heightGap > 0.01f)
                {
                    Vector3 anchor = dist > 0.01f
                        ? _carrySource.position + (toSelf / dist) * Mathf.Min(dist, _carryDist)
                        : _carrySource.position;
                    anchor.y = _carryMatchHeight
                        ? _carrySource.position.y + _carryHeightOffset
                        : transform.position.y;
                    Vector3 next = Vector3.MoveTowards(transform.position, anchor, _carrySpeed * dt);
                    move += next - transform.position;
                }
            }

            // Gravedad propia. Sin esto, un golpe que eleva dejaría al enemigo flotando para siempre:
            // los maniquíes no tienen locomoción que los haga caer.
            if (_cc != null && _cc.enabled)
            {
                if (_cc.isGrounded && _fallVel <= 0f)
                {
                    _fallVel = -2f;      // pegado al suelo
                    _hangUsed = 0f;      // aterrizar recarga el presupuesto de suspensión
                }
                else if (IsHanging) _fallVel = Mathf.MoveTowards(_fallVel, 0f, hangDamp * dt);
                else _fallVel += gravity * dt;

                move.y += _fallVel * dt;
            }

            if (move != Vector3.zero)
            {
                if (_cc != null && _cc.enabled) _cc.Move(move);   // colisiona con paredes → se detiene solo
                else transform.position += move;
            }
        }
    }
}
