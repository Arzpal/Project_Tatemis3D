using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Estados temporales sobre un combatiente. Cada uno **es el verbo de un elemento**
    /// ([ELEMENTAL-REACTIONS §0.1](../../../../docs/ELEMENTAL-REACTIONS.md)) y quien lo sufre lo LEE:
    /// el status no empuja nada, expone consultas y los consumidores deciden.
    ///
    /// 🔑 **Modelo de carga (§0.3): el efecto se pega.** Un estado tiene **intensidad 0..1**, no solo
    /// duración. Mientras la zona te re-aplica (o sea, mientras estás dentro) la carga se mantiene
    /// llena y **el rabo se alarga**; al salir, la carga **decae** hasta apagarse. De ahí salen las
    /// cuatro propiedades que pedía el diseño con un solo número: dentro es eterno · fuera decrece ·
    /// reentrar recarga · **cuanto más tiempo estuviste dentro, más te dura fuera**.
    ///
    /// 🔑 **Techo de 2 (§0.9).** Nunca más de dos a la vez, y el objetivo real es 0-1: si estar bajo
    /// un efecto fuera el estado normal, dejarían de leerse como acontecimientos. Al llegar un tercero
    /// se cae el más débil.
    [DisallowMultipleComponent]
    public class StatusController : MonoBehaviour
    {
        public const int MaxSimultaneous = 2;

        /// Ventana para considerar que "sigues dentro". Mayor que el pulso de la zona (0,5 s) para que
        /// un pulso perdido no se lea como haber salido.
        const float InsideWindow = 0.8f;
        const float TickInterval = 0.4f;      // pulsos de quemadura y de interferencia
        const float MaxTail = 6f;             // tope de lo que puede crecer el rabo

        class Active
        {
            public float charge = 1f;         // intensidad 0..1
            public float tail = 1.5f;         // segundos que tarda en caer de 1 a 0 fuera
            public float lastRefresh;
            public float tickDamage, nextTick;
            public Vector3 drag;              // solo Arrastrado: hacia dónde y a qué velocidad
        }

        readonly Dictionary<StatusType, Active> _active = new Dictionary<StatusType, Active>();

        // Valores del efecto (provisionales de caja gris; se afinan en playtest).
        const float SlowFactor = 0.5f;         // Ralentizado a plena carga
        const float HeavySlow = 0.65f;         // Pesado: ralentiza menos que un slow puro…
        const float HeavyGravity = 2.6f;       // …pero te clava al suelo (anti-aéreo)
        const float VulnerableFactor = 1.35f;

        ICombatant _self;

        void Awake() => _self = GetComponent<ICombatant>();

        /// Punto de entrada. `tail` = cuánto tarda en apagarse **fuera** de la zona. Llamarlo de nuevo
        /// mientras estás dentro **recarga y alarga**, no acumula un segundo estado.
        public void Apply(StatusType type, float tail, float tickDamage = 0f, Vector3 drag = default)
        {
            if (type == StatusType.None || tail <= 0f) return;

            if (_active.TryGetValue(type, out var a))
            {
                a.charge = 1f;
                // Seguir dentro alarga el rabo: es lo que hace que forzar la permanencia siga siendo
                // la jugada premium en vez de dar lo mismo que pasar de refilón.
                a.tail = Mathf.Min(MaxTail, Mathf.Max(a.tail, tail) + (Time.time - a.lastRefresh));
                a.lastRefresh = Time.time;
                a.tickDamage = Mathf.Max(a.tickDamage, tickDamage);
                if (drag.sqrMagnitude > 0.001f) a.drag = drag;
                return;
            }

            EnforceCap();
            _active[type] = new Active
            {
                charge = 1f, tail = tail, lastRefresh = Time.time,
                tickDamage = tickDamage, nextTick = Time.time + TickInterval, drag = drag,
            };
        }

        /// Techo de 2: si van a ser tres, se va **el más débil** (menos carga). Así el efecto nuevo
        /// —el que el jugador acaba de provocar y por tanto está mirando— siempre entra.
        void EnforceCap()
        {
            while (_active.Count >= MaxSimultaneous)
            {
                StatusType weakest = StatusType.None;
                float min = float.MaxValue;
                foreach (var kv in _active)
                    if (kv.Value.charge < min) { min = kv.Value.charge; weakest = kv.Key; }
                if (weakest == StatusType.None) return;
                _active.Remove(weakest);
            }
        }

        public bool Has(StatusType type) => _active.ContainsKey(type);

        /// Intensidad actual 0..1. Todo efecto escala con ella: es lo que hace que salir de la zona
        /// se **sienta** como que el efecto se va soltando en vez de cortarse de golpe.
        public float Charge(StatusType type) => _active.TryGetValue(type, out var a) ? a.charge : 0f;

        // ---------------- Consultas que leen los consumidores ----------------

        /// Enraizado (Naturaleza) clava los pies **pero deja atacar y bloquear** — es la firma del
        /// verbo. Pesado (Mineral) ralentiza en proporción a su carga. Aturdido lo para todo.
        public float SpeedMultiplier
        {
            get
            {
                if (Has(StatusType.Aturdido) || Has(StatusType.Enraizado)) return 0f;
                float m = 1f;
                if (Has(StatusType.Ralentizado)) m *= Mathf.Lerp(1f, SlowFactor, Charge(StatusType.Ralentizado));
                if (Has(StatusType.Pesado)) m *= Mathf.Lerp(1f, HeavySlow, Charge(StatusType.Pesado));
                return m;
            }
        }

        /// **Solo Aturdido impide actuar.** Enraizado no: puedes pegar y bloquear clavado en el sitio.
        public bool Stunned => Has(StatusType.Aturdido);

        /// Pesado = "más gravedad": imanta hacia abajo y acorta la suspensión del juggle. No impide
        /// despegar, hace que despegar **valga menos** — que es lo dictado.
        public float GravityMultiplier =>
            Has(StatusType.Pesado) ? Mathf.Lerp(1f, HeavyGravity, Charge(StatusType.Pesado)) : 1f;

        public float DamageTakenMultiplier =>
            Has(StatusType.Vulnerable) ? Mathf.Lerp(1f, VulnerableFactor, Charge(StatusType.Vulnerable)) : 1f;

        /// Cegado (Aire): no puedes **fijar** objetivo nuevo. Mantener el que ya tenías sí — igual que
        /// la línea de visión, que hace falta para fijar y no para conservar.
        public bool Blinded => Has(StatusType.Cegado);

        /// Arrastrado (Agua): velocidad que la corriente te impone, escalada por la carga. Al salir de
        /// la zona la carga cae, y eso *es* la inercia que te sigue llevando.
        public Vector3 DragVelocity =>
            _active.TryGetValue(StatusType.Arrastrado, out var a) ? a.drag * a.charge : Vector3.zero;

        /// Interferido (Rayo): ¿ha caído un tick justo ahora? Lo consume quien arranca un ataque —solo
        /// muerde el **arranque**, nunca un golpe ya comprometido (§0.6). Devuelve true una sola vez
        /// por tick, así que el que pregunta primero se lo come.
        public bool ConsumeInterrupt()
        {
            if (!_active.TryGetValue(StatusType.Interferido, out var a)) return false;
            if (!_interruptPending) return false;
            _interruptPending = false;
            return true;
        }
        bool _interruptPending;
        /// Para pintarlo: el tick de interferencia acaba de sonar.
        public bool InterruptFlash => Time.time - _lastInterrupt < 0.15f;
        float _lastInterrupt = -99f;

        public IEnumerable<KeyValuePair<StatusType, float>> ActiveCharges()
        {
            foreach (var kv in _active) yield return new KeyValuePair<StatusType, float>(kv.Key, kv.Value.charge);
        }

        // ---------------- Ciclo ----------------

        readonly List<StatusType> _expired = new List<StatusType>();
        readonly List<StatusType> _keys = new List<StatusType>();
        readonly List<float> _pendingTicks = new List<float>();

        /// ⚠️ Nunca recorrer `_active` directamente: el bucle escribe en el diccionario y en el runtime
        /// de Unity eso invalida el enumerador; y `TakeHit` puede acabar aplicando otro estado a este
        /// mismo objetivo (reentrada). Se itera una COPIA de las claves y el daño se cobra al final.
        void Update()
        {
            if (_active.Count == 0) return;

            _expired.Clear();
            _pendingTicks.Clear();
            _keys.Clear();
            foreach (var k in _active.Keys) _keys.Add(k);

            for (int i = 0; i < _keys.Count; i++)
            {
                var type = _keys[i];
                if (!_active.TryGetValue(type, out var a)) continue;

                // Fuera de la zona → la carga decae. Dentro (te siguen re-aplicando) se mantiene.
                if (Time.time - a.lastRefresh > InsideWindow)
                {
                    a.charge -= Time.deltaTime / Mathf.Max(0.1f, a.tail);
                    if (a.charge <= 0f) { _expired.Add(type); continue; }
                }

                if (Time.time < a.nextTick) continue;
                a.nextTick = Time.time + TickInterval;

                // Quemadura: DoT por pulsos, escalado por la carga (al salir quema cada vez menos).
                if (type == StatusType.Quemadura && a.tickDamage > 0f)
                    _pendingTicks.Add(a.tickDamage * a.charge);

                // Interferencia: el tick que corta el arranque. Por RELOJ, no por azar — así el
                // jugador aprende el ritmo y cuela el combo entre tick y tick.
                if (type == StatusType.Interferido)
                {
                    _interruptPending = true;
                    _lastInterrupt = Time.time;
                }
            }

            for (int i = 0; i < _expired.Count; i++) _active.Remove(_expired[i]);

            if (_self == null) return;
            for (int i = 0; i < _pendingTicks.Count; i++)
            {
                _self.TakeHit(_pendingTicks[i], 0f);
                if (_self.TakesDamage)
                    HitFeedback.Report(_self, _pendingTicks[i], 1f,
                        transform.position + Vector3.up * 1.2f, Vector3.zero, 0.1f);
            }
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

        /// Etiqueta **anclada al afectado**: dice qué le pasa y **cuánta carga le queda** (la barra),
        /// que es lo que hace visible el modelo de §0.3 — se ve cómo el efecto se va soltando al
        /// salir de la zona. Y §0.9 exige que todo efecto se vea en el cuerpo, o no entra en el juego.
        void OnGUI()
        {
            if (_active.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;

            Vector3 sp = cam.WorldToScreenPoint(transform.position + Vector3.up * labelHeight);
            if (sp.z <= 0f) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Overflow, wordWrap = false,
                };
            _style.fontSize = Mathf.RoundToInt(Mathf.Clamp(15f * (12f / Mathf.Max(1f, sp.z)), 10f, 20f));

            float y = Screen.height - sp.y;
            foreach (var kv in _active)
            {
                var r = new Rect(sp.x - 100f, y, 200f, 20f);
                _style.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
                GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), Label(kv.Key), _style);
                var c = ColorOf(kv.Key);
                _style.normal.textColor = c;
                GUI.Label(r, Label(kv.Key), _style);

                // Barra de carga: se vacía al salir de la zona.
                float w = 46f * kv.Value.charge;
                GUI.color = new Color(c.r, c.g, c.b, 0.75f);
                GUI.DrawTexture(new Rect(sp.x - 23f, y + 15f, w, 2.5f), Texture2D.whiteTexture);
                GUI.color = Color.white;
                y -= 20f;
            }
        }

        static string Label(StatusType t)
        {
            switch (t)
            {
                case StatusType.Quemadura:   return "Quemadura";
                case StatusType.Arrastrado:  return "Arrastrado";
                case StatusType.Enraizado:   return "Enraizado";
                case StatusType.Pesado:      return "Pesado";
                case StatusType.Interferido: return "Interferido";
                case StatusType.Cegado:      return "Cegado";
                default:                     return t.ToString();
            }
        }

        /// Cada estado con el color de SU elemento: así el jugador ata el efecto a su causa sin que
        /// nadie se lo explique.
        static Color ColorOf(StatusType t)
        {
            switch (t)
            {
                case StatusType.Quemadura:   return new Color(1f, 0.5f, 0.2f);      // Fuego
                case StatusType.Arrastrado:  return new Color(0.35f, 0.6f, 1f);     // Agua
                case StatusType.Enraizado:   return new Color(0.4f, 0.9f, 0.4f);    // Naturaleza
                case StatusType.Pesado:      return new Color(0.7f, 0.72f, 0.8f);   // Mineral
                case StatusType.Interferido: return new Color(0.95f, 0.85f, 0.3f);  // Rayo
                case StatusType.Cegado:      return new Color(0.75f, 0.9f, 0.95f);  // Aire
                case StatusType.Ralentizado: return new Color(0.55f, 0.8f, 1f);
                case StatusType.Vulnerable:  return new Color(1f, 0.45f, 0.7f);
                case StatusType.Aturdido:    return new Color(1f, 0.9f, 0.3f);
                default:                     return Color.white;
            }
        }
    }
}
