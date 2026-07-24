using UnityEngine;

namespace Tatemis
{
    /// Va en las criaturas salvajes. Vida + barra de aturdimiento tipo "postura" (ref. Sekiro):
    /// - maxStagger es PROPORCIONAL a la vida (staggerRatio) → los tanques aguantan más aturdimiento.
    /// - cada golpe suma stagger × un multiplicador de COMBO que sube con golpes continuos.
    /// - si dejas de golpear, el stagger DECAE → hay que mantener la presión.
    /// Al llenarse abre la VENTANA DE NOQUEO: durante ella NO recibe más aturdimiento y este decae;
    /// cuando llega a 0, si le queda vida sigue peleando, y si no, cae (huye / se retira).
    public class StaggerReceiver : MonoBehaviour, ICombatant
    {
        public CreatureSpeciesData species;
        [Tooltip("Salvaje = huye al caer · si es de un tamer, se retira / cambia de tatemis")]
        public bool isWild = true;

        [Header("Vida")]
        public float maxHealth = 100f;
        public float health = 100f;

        [Header("Aturdimiento (proporcional a la vida)")]
        [Tooltip("maxStagger = staggerRatio × maxHealth")]
        public float staggerRatio = 0.15f;
        public float comboMax = 3f;
        public float comboStep = 0.2f;      // +mult por golpe encadenado
        public float comboWindow = 1.5f;    // s sin golpear → se rompe el combo
        public float decayDelay = 2f;       // s sin golpear antes de decaer
        public float decayPerSec = 0.15f;   // fracción de maxStagger que decae por segundo
        [Tooltip("A qué ritmo baja el aturdimiento DURANTE el noqueo (fracción de maxStagger/seg)")]
        public float koDecayPerSec = 0.3f;

        [Header("Noqueo total (aturdimiento + vida 0) — ver TAMER.md §4.1")]
        [Tooltip("Segundos INVULNERABLE al caer del todo. Es el margen para ELEGIR: sin él, el siguiente golpe del combo en curso remataría por inercia justo al que querías capturar")]
        public float graceSeconds = 4f;
        [Tooltip("Cuánto dura la ventana total. DEBE ser mayor que la gracia, o no queda tiempo para rematar después de ella")]
        public float downedSeconds = 9f;

        public float maxStagger { get; private set; }
        public float stagger { get; private set; }
        public float ComboMultiplier { get; private set; } = 1f;
        public bool InKOWindow { get; private set; }
        public bool Defeated { get; private set; }

        /// Caído del todo: vida a 0 y esperando desenlace (rematar / capturar / se retira).
        public bool Downed { get; private set; }
        /// Dentro del margen invulnerable: los golpes en curso no cuentan.
        public bool InGrace => Downed && Time.time < _graceUntil;
        /// 0→1 de lo que queda de ventana, para pintarlo en el HUD.
        public float DownedProgress =>
            Downed ? Mathf.Clamp01((Time.time - _downedAt) / Mathf.Max(0.01f, downedSeconds)) : 0f;

        float _lastHitTime = -999f;
        float _graceUntil, _downedAt;

        public Element Element => species != null ? species.element : Element.Naturaleza;
        public Transform Transform => transform;

        // Colores del estado caído (caja gris): la gracia se distingue del "ya puedes rematar".
        static readonly Color ColGrace = new Color(0.95f, 0.95f, 1f);      // inmune: casi blanco
        static readonly Color ColDowned = new Color(0.45f, 0.45f, 0.5f);   // rematable: gris apagado
        HitFlash _flash;
        bool _wasInGrace;

        void Awake()
        {
            maxStagger = Mathf.Max(1f, maxHealth * staggerRatio);
            _flash = GetComponent<HitFlash>();
        }

        /// El estado de debilitado tiene que LEERSE, o el margen de gracia no sirve de nada: el jugador
        /// no sabría que se ha abierto la decisión. En caja gris = tinte sostenido; con arte, animación.
        void PaintState()
        {
            if (_flash == null) return;
            if (!Downed) { _flash.Clear(); return; }
            bool grace = InGrace;
            if (grace != _wasInGrace || !_wasPainted)
            {
                _flash.Hold(grace ? ColGrace : ColDowned);
                _wasInGrace = grace;
                _wasPainted = true;
            }
        }
        bool _wasPainted;

        void Update()
        {
            PaintState();

            if (Downed)
            {
                // Caído del todo: no se recupera solo. Espera un desenlace hasta que se acabe el tiempo,
                // y entonces se retira (que es lo que pasa si el jugador no decide nada).
                if (Time.time - _downedAt >= downedSeconds) Retire();
                return;
            }

            if (InKOWindow)
            {
                // El noqueo dura lo que tarda el aturdimiento en bajar a 0.
                stagger = Mathf.Max(0f, stagger - maxStagger * koDecayPerSec * Time.deltaTime);
                if (stagger <= 0f) ResolveKO();
                return;
            }

            float since = Time.time - _lastHitTime;
            if (since > comboWindow) ComboMultiplier = 1f;
            if (since > decayDelay && stagger > 0f)
                stagger = Mathf.Max(0f, stagger - maxStagger * decayPerSec * Time.deltaTime);
        }

        public void TakeHit(float damage, float staggerAmount)
        {
            if (Defeated) return;

            if (Downed)
            {
                // Ya está en el suelo: aquí el golpe no hace daño, DECIDE. Durante la gracia no cuenta
                // (es el margen para elegir); pasada la gracia, seguir pegando es rematar.
                if (!InGrace) Finish();
                return;
            }

            health = Mathf.Max(0f, health - damage);
            RegisterStagger(staggerAmount);
            if (health <= 0f) GoDown();       // sin vida → cae, y ahí se decide qué hacer con él
        }

        /// Punto de entrada del aturdimiento (golpes y CC). Aplica el multiplicador de combo.
        /// Ignorado durante el noqueo: un enemigo ya noqueado no recibe más aturdimiento.
        public void RegisterStagger(float amount)
        {
            if (InKOWindow || Downed || Defeated) return;

            float since = Time.time - _lastHitTime;
            ComboMultiplier = since <= comboWindow
                ? Mathf.Min(comboMax, ComboMultiplier + comboStep)
                : 1f;
            _lastHitTime = Time.time;

            stagger = Mathf.Min(maxStagger, stagger + amount * ComboMultiplier);
            if (stagger >= maxStagger) OpenKOWindow();
        }

        void OpenKOWindow()
        {
            if (InKOWindow) return;
            InKOWindow = true;
            ComboMultiplier = 1f;
            Debug.Log($"[KO] {name} aturdido — ventana abierta (aturdimiento {stagger:0}→0).");
        }

        void ResolveKO()
        {
            InKOWindow = false;
            Debug.Log($"[KO] {name} se recupera y sigue peleando (vida {health:0}).");
        }

        /// Cae del todo. **No hay prompt ni botón** (TAMER.md §4): a partir de aquí, seguir atacándolo
        /// lo remata y el menú del Tamer lo captura — pero primero hay un margen invulnerable para decidir.
        void GoDown()
        {
            if (Downed) return;
            Downed = true;
            InKOWindow = false;
            ComboMultiplier = 1f;
            _downedAt = Time.time;
            _graceUntil = Time.time + graceSeconds;
            Debug.Log($"[KO] {name} DEBILITADO — {graceSeconds:0.#}s de gracia; después, atacar remata.");
        }

        /// Golpe de gracia: el jugador ha elegido matar (VITALITY-DEATH §3).
        void Finish()
        {
            if (Defeated) return;
            Defeated = true;
            Downed = false;
            Debug.Log($"[KO] {name} rematado — muerte por elección del jugador.");
        }

        /// Nadie decidió nada a tiempo: se lo llevan / huye.
        void Retire()
        {
            Downed = false;
            Defeated = true;
            Debug.Log(isWild
                ? $"[KO] {name} se recupera y huye (nadie lo remató ni lo capturó)."
                : $"[KO] {name} es retirado por su tamer.");
        }
    }
}
