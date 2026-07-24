using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Estados temporales sobre un combatiente (el destino real de `ApplyStatus`, que antes solo
    /// logueaba). Cada tipo tiene un efecto distinto y quien lo sufre lo LEE — el status no empuja
    /// nada, expone consultas y los consumidores deciden:
    ///   · Ralentizado → `SpeedMultiplier` (lo lee CreatureController)
    ///   · Aturdido    → `Stunned` + velocidad 0 (lo lee CreatureController)
    ///   · Vulnerable  → `DamageTakenMultiplier` (lo lee DamageResolver → se ve YA: recibe más)
    ///   · Quemadura   → daño por pulsos (DoT), con el patrón de reloj de ABILITY-SYSTEM §4.1
    ///
    /// Se auto-añade al aplicar (como HitFlash), así funciona en escenas guardadas sin recablear.
    [DisallowMultipleComponent]
    public class StatusController : MonoBehaviour
    {
        struct Active { public float until; public float tickDamage; public float nextTick; }
        readonly Dictionary<StatusType, Active> _active = new Dictionary<StatusType, Active>();

        // Valores del efecto (provisionales de caja gris; se afinan en playtest).
        const float SlowFactor = 0.5f;         // Ralentizado: mitad de velocidad
        const float VulnerableFactor = 1.35f;  // Vulnerable: +35% de daño recibido
        const float BurnInterval = 0.4f;       // s entre pulsos de Quemadura

        ICombatant _self;

        void Awake() => _self = GetComponent<ICombatant>();

        /// Punto de entrada. `tickDamage` solo lo usa Quemadura (DoT). Refresca si ya estaba: se queda
        /// con el vencimiento MÁS lejano, no los suma (dos slows no ralentizan el doble).
        public void Apply(StatusType type, float duration, float tickDamage = 0f)
        {
            if (type == StatusType.None || duration <= 0f) return;
            float until = Time.time + duration;
            if (_active.TryGetValue(type, out var a))
            {
                a.until = Mathf.Max(a.until, until);
                a.tickDamage = Mathf.Max(a.tickDamage, tickDamage);
                _active[type] = a;
            }
            else
            {
                _active[type] = new Active { until = until, tickDamage = tickDamage, nextTick = Time.time + BurnInterval };
            }
        }

        public bool Has(StatusType type) => _active.ContainsKey(type);

        public float SpeedMultiplier =>
            Has(StatusType.Aturdido) ? 0f : Has(StatusType.Ralentizado) ? SlowFactor : 1f;

        public bool Stunned => Has(StatusType.Aturdido);

        public float DamageTakenMultiplier => Has(StatusType.Vulnerable) ? VulnerableFactor : 1f;

        /// Los estados activos y cuánto les queda. (El tiempo no se muestra al jugador — ver `OnGUI` —,
        /// pero se expone por si un HUD de debug lo quiere.)
        public IEnumerable<KeyValuePair<StatusType, float>> ActiveRemaining()
        {
            foreach (var kv in _active)
                yield return new KeyValuePair<StatusType, float>(kv.Key, kv.Value.until - Time.time);
        }

        // De instancia, no estático: con varios enemigos afectados a la vez, una lista compartida es
        // un accidente esperando (hoy Update no es reentrante, pero no hay que confiar en eso).
        readonly List<StatusType> _expired = new List<StatusType>();

        void Update()
        {
            if (_active.Count == 0) return;

            _expired.Clear();
            foreach (var kv in _active)
            {
                if (Time.time >= kv.Value.until) { _expired.Add(kv.Key); continue; }

                // Quemadura: DoT en pulsos (por reloj, no por frame — ABILITY-SYSTEM §4.1).
                if (kv.Key == StatusType.Quemadura && kv.Value.tickDamage > 0f && Time.time >= kv.Value.nextTick)
                {
                    var a = kv.Value;
                    a.nextTick = Time.time + BurnInterval;
                    _active[kv.Key] = a;
                    // TODO(Pyrecko/Fase 2): el DoT debería llevar el elemento de quien lo prendió.
                    // Hoy va neutro y reutiliza el pipeline de feedback (número + destello).
                    if (_self != null)
                    {
                        _self.TakeHit(a.tickDamage, 0f);
                        HitFeedback.Report(_self, a.tickDamage, 1f,
                            transform.position + Vector3.up * 1.2f, Vector3.zero, 0.1f);
                    }
                }
            }
            for (int i = 0; i < _expired.Count; i++) _active.Remove(_expired[i]);
        }

        /// Resuelve el StatusController del objetivo, añadiéndolo si falta (patrón de HitFlash).
        public static StatusController Of(Transform t)
        {
            if (t == null) return null;
            var root = t.root;
            return root.TryGetComponent(out StatusController sc) ? sc : root.gameObject.AddComponent<StatusController>();
        }

        // ---------------- Etiqueta sobre el afectado ----------------

        [Tooltip("Altura de la etiqueta sobre el pivote de la criatura")]
        public float labelHeight = 2.3f;

        static GUIStyle _style;

        /// Etiqueta **anclada al enemigo**, no flotante: dice qué le pasa y desaparece cuando se acaba.
        /// A diferencia de los números de daño —que son un evento y por eso vuelan—, un estado es una
        /// condición: tiene que quedarse quieta encima de quien la sufre para poder leerla de un vistazo.
        void OnGUI()
        {
            if (_active.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;

            Vector3 sp = cam.WorldToScreenPoint(transform.position + Vector3.up * labelHeight);
            if (sp.z <= 0f) return;                                   // detrás de la cámara

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Overflow,
                    wordWrap = false,
                };
            _style.fontSize = Mathf.RoundToInt(Mathf.Clamp(15f * (12f / Mathf.Max(1f, sp.z)), 10f, 20f));

            float y = Screen.height - sp.y;
            foreach (var kv in _active)
            {
                var r = new Rect(sp.x - 100f, y, 200f, 20f);
                _style.normal.textColor = new Color(0f, 0f, 0f, 0.75f);        // sombra
                GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), kv.Key.ToString(), _style);
                _style.normal.textColor = ColorOf(kv.Key);
                GUI.Label(r, kv.Key.ToString(), _style);
                y -= 18f;                                              // varios estados se apilan
            }
        }

        static Color ColorOf(StatusType t)
        {
            switch (t)
            {
                case StatusType.Ralentizado: return new Color(0.55f, 0.8f, 1f);
                case StatusType.Quemadura:   return new Color(1f, 0.5f, 0.2f);
                case StatusType.Vulnerable:  return new Color(1f, 0.45f, 0.7f);
                case StatusType.Aturdido:    return new Color(1f, 0.9f, 0.3f);
                default:                     return Color.white;
            }
        }
    }
}
