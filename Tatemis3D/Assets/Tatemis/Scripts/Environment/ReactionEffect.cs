using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// UNA reacción viva sobre el terreno ([ELEMENTAL-REACTIONS §4](../../../../docs/ELEMENTAL-REACTIONS.md)).
    /// Nace resuelta —la entrada de la tabla ya dice qué es—, se dispersa sola y el terreno vuelve a
    /// ser lo que era: el mundo nunca queda pintado.
    ///
    ///  · **Reacción** (dominancia) → **explosión**: fogonazo al nacer, daño **del elemento que manda**.
    ///  · **Combinación** → **transformación**: zona que dura, daño **neutro**, y **sin bando**.
    ///
    /// **Todo aparece con delay** (§0.2): mientras gesta se ve el aviso pero no aplica nada, y esa
    /// ventana es lo que hace real elegir entre pegar directo (seguro) y apuntar al suelo (más daño y
    /// efecto, pero esquivable). Vale en los dos sentidos: si provocas la reacción del terreno bajo
    /// tus pies, ese instante es tu margen para apartarte.
    public class ReactionEffect : MonoBehaviour
    {
        const float PulseInterval = 0.5f;   // reloj de la zona, nunca por frame

        ReactionEntry _entry;
        float _radius, _duration, _dieAt, _bornAt, _nextPulse;
        Transform _instigatorRoot;          // null = sin dueño (p. ej. un nodo de cristal descargado)
        Vector3 _direction;                 // corriente / colada: hacia dónde
        Material _mat;
        Transform _telegraph, _disc;
        bool _hatched, _deflagrated;

        /// Frame en el que nació. R3 habla de un impacto NUEVO sobre un efecto ACTIVO — no de dos
        /// efectos de la MISMA habilidad pisándose (el daño crea y el estallido borraría).
        public int BornFrame { get; private set; }
        public ReactionEntry Entry => _entry;
        public float Age => Mathf.Max(0f, Time.time - _bornAt);
        public Vector3 Center => transform.position;
        public float Radius => _radius;

        /// AMARRA: a quién ha atado y dónde estaba cuando le alcanzó. La estaca es de cada víctima,
        /// no del centro de la zona — "te devuelve al punto donde estabas", que es más legible.
        readonly Dictionary<Transform, Vector3> _anchors = new Dictionary<Transform, Vector3>();
        /// Objetivos que ya recibieron el estado (solo si `zoneStatusOnce`).
        readonly HashSet<Transform> _statusApplied = new HashSet<Transform>();

        /// Registro de zonas vivas, para las consultas que hacen otros sistemas (el eco de las mini
        /// explosiones, la niebla que corta el fijado, el velo que detona proyectiles).
        static readonly List<ReactionEffect> Live = new List<ReactionEffect>();

        public void Refresh() => _dieAt = Mathf.Max(_dieAt, Time.time + _duration);

        /// `spreadBudget` = cuántos saltos de propagación le quedan (solo lo usa el incendio). Cada hijo
        /// nace con uno menos, así que **se apaga solo** sin ninguna regla de tope aparte.
        public static ReactionEffect Spawn(ReactionEntry entry, Vector3 groundPos, float radius,
                                           float duration, float delay, ICombatant instigator,
                                           Vector3 direction, int spreadBudget = 3)
        {
            var go = new GameObject("Reaccion_" + entry.reactionName);
            go.transform.position = groundPos;
            var fx = go.AddComponent<ReactionEffect>();
            fx._spreadBudget = spreadBudget;
            fx.Init(entry, radius, duration, delay, instigator, direction);
            return fx;
        }

        int _spreadBudget;
        bool _hasSpread;

        void Init(ReactionEntry entry, float radius, float duration, float delay, ICombatant instigator,
                  Vector3 direction)
        {
            _entry = entry;
            _radius = radius * Mathf.Max(0.1f, entry.radiusScale);
            _duration = duration;
            _bornAt = Time.time + Mathf.Max(0f, delay);
            _dieAt = _bornAt + duration;
            _instigatorRoot = instigator != null && instigator.Transform != null
                ? instigator.Transform.root : null;
            _nextPulse = _bornAt + PulseInterval;
            BornFrame = Time.frameCount;

            direction.y = 0f;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;

            Live.Add(this);
            if (delay > 0f) BuildTelegraph(); else Hatch();
        }

        void OnDestroy()
        {
            Live.Remove(this);
            if (_mat != null) Destroy(_mat);
        }

        void Hatch()
        {
            _hatched = true;
            if (_telegraph != null) Destroy(_telegraph.gameObject);
            BuildVisual();
            // La MECHA no estalla al nacer: acumula y deflagra al morir. Es la única que invierte el
            // delay, y esa inversión es su identidad.
            if (_entry.style == ReactionStyle.Dominancia && !_entry.deflagrates) Burst();
        }

        // ---------------- Consultas para otros sistemas ----------------

        /// MINI EXPLOSIONES: ¿hay que hacer eco de un golpe dado en este punto? Devuelve el % a repetir.
        public static float EchoPercentAt(Vector3 point)
        {
            float best = 0f;
            for (int i = 0; i < Live.Count; i++)
            {
                var fx = Live[i];
                if (fx == null || !fx._hatched || fx._entry.echoPercent <= 0f) continue;
                if (fx.Contains(point)) best = Mathf.Max(best, fx._entry.echoPercent);
            }
            return best;
        }

        /// NIEBLA: ¿el segmento entre dos puntos cruza una zona que corta el fijado?
        public static bool TargetingBlocked(Vector3 from, Vector3 to)
        {
            for (int i = 0; i < Live.Count; i++)
            {
                var fx = Live[i];
                if (fx == null || !fx._hatched || !fx._entry.breaksTargeting) continue;
                if (fx.SegmentCrosses(from, to)) return true;
            }
            return false;
        }

        /// VELO DE ESPORAS: ¿una entrega elemental que pasa por aquí debe detonar? ⚠️ Hoy no hay
        /// proyectiles en el slice (llegan con Ottorock-Sujeción y Pyrecko), así que esto queda
        /// cableado y sin consumidor: es la costura, no una mecánica muerta.
        public static bool StopsProjectileAt(Vector3 point)
        {
            for (int i = 0; i < Live.Count; i++)
            {
                var fx = Live[i];
                if (fx == null || !fx._hatched || !fx._entry.blocksProjectiles) continue;
                if (fx.Contains(point)) return true;
            }
            return false;
        }

        bool Contains(Vector3 p)
        {
            Vector3 d = p - Center; d.y = 0f;
            return d.sqrMagnitude <= _radius * _radius;
        }

        /// Distancia punto-segmento aplanada: sirve para saber si una línea de visión atraviesa la nube.
        bool SegmentCrosses(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            Vector3 c = Center; c.y = 0f;
            Vector3 ab = b - a;
            float len2 = ab.sqrMagnitude;
            float t = len2 < 0.0001f ? 0f : Mathf.Clamp01(Vector3.Dot(c - a, ab) / len2);
            return (a + ab * t - c).sqrMagnitude <= _radius * _radius;
        }

        // ---------------- Daño ----------------

        /// Una **reacción** pega con el elemento que manda y pasa por la tabla de tipos; una
        /// **combinación** pega NEUTRO, porque ahí no se impone ninguno de los dos (§0.2). Es la
        /// diferencia que hace que las dos se sientan distintas jugando.
        float TypeMultiplier(ICombatant target)
        {
            var e = _entry.DamageElement;
            if (!e.HasValue || CombatServices.TypeChart == null || target == null) return 1f;
            return CombatServices.TypeChart.Multiplier(e.Value, target.Element);
        }

        Color Tint => _entry.friendlyFire
            ? Color.Lerp(_entry.debugColor, new Color(1f, 0.15f, 0.1f, _entry.debugColor.a), 0.45f)
            : _entry.debugColor;

        void Burst()
        {
            Vector3 origin = Center + Vector3.up * 0.4f;
            HitboxDebug.Show(origin, _radius, 0.3f, Tint);
            ImpactVFX.Spawn(origin, 1f, 1f);

            bool hitSomeone = false;
            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(origin, _radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                var root = t.Transform.root;
                if (!seen.Add(root)) continue;
                // Sin bando = golpea a todos: la respuesta del terreno y TODAS las combinaciones (§0.7).
                if (!_entry.NoBando && _instigatorRoot != null && root == _instigatorRoot) continue;

                Vector3 point = col.ClosestPoint(origin);
                Vector3 radial = t.Transform.position - Center; radial.y = 0f;
                radial = radial.sqrMagnitude > 0.001f ? radial.normalized : Vector3.forward;

                float mult = TypeMultiplier(t);
                float dealt = _entry.burstDamage * mult;
                t.TakeHit(dealt, _entry.Stagger * mult);   // el aturdimiento va con el daño (GDD §8.2)

                if (t.TakesDamage)
                {
                    HitFeedback.Report(t, dealt, mult, point, radial, 0.55f);
                    hitSomeone = true;
                }

                if (t.IsCreature && _entry.burstStatus != StatusType.None)
                    StatusController.Of(t.Transform)?.Apply(_entry.burstStatus, _entry.burstStatusSeconds);

                // El GÉISER eleva: es lo que abre el juego aéreo y deja que un melee alcance a un volador.
                Vector3 push = radial * _entry.burstKnockback + Vector3.up * _entry.burstLift;
                if (push.sqrMagnitude > 0.01f) Push(t, push, _entry.burstLift > 0f ? 0.3f : 0.18f);
            }
            if (hitSomeone) ImpactFeel.Shockwave(0.08f);
        }

        /// Empuje por el camino que cada cuerpo entienda. El jugador NO usa `HitReactor` (aplicaría su
        /// propia gravedad sobre el mismo CharacterController y se duplicaría la caída): va por su
        /// desplazamiento externo, que es su único punto de movimiento.
        static void Push(ICombatant t, Vector3 velocity, float seconds)
        {
            if (t.Transform.TryGetComponent(out HitReactor reactor))
            {
                reactor.ApplyKnockback(velocity, seconds);
                reactor.ApplyFlinch(0.12f);
                if (velocity.y > 1f) reactor.ApplyHang(0.5f);   // que el elevado se sostenga un instante
                return;
            }
            var creature = t.Transform.root.GetComponent<CreatureController>();
            if (creature != null) creature.ApplyExternalPush(velocity, seconds);
        }

        // ---------------- Ciclo ----------------

        void Update()
        {
            if (!_hatched)
            {
                if (_telegraph != null)
                {
                    float t = Mathf.PingPong(Time.time * 6f, 1f);
                    _telegraph.localScale = Vector3.one * (_radius * 2f * Mathf.Lerp(0.75f, 1f, t));
                }
                if (Time.time >= _bornAt) Hatch();
                return;
            }

            if (Time.time >= _dieAt)
            {
                // MECHA: su peligro es el final. Deflagra en todo el volumen justo antes de morir.
                if (_entry.deflagrates && !_deflagrated) { _deflagrated = true; Burst(); }
                Destroy(gameObject);
                return;
            }

            if (_entry.advanceSpeed > 0f) Advance();
            if (_entry.deflagrates) { PaintCharge(); return; }   // no hace nada mientras dura

            if (Time.time < _nextPulse) return;
            _nextPulse = Time.time + PulseInterval;
            Pulse();
        }

        /// COLADA: la única zona que avanza. Repta en la dirección del golpe comiéndose el suelo
        /// jugable, y **se apaga al llegar a una celda de Agua** — no es una reacción encadenada (R1
        /// sigue intacta), es su muerte, y le da al rival una respuesta que se entiende sin explicar.
        void Advance()
        {
            Vector3 next = transform.position + _direction * _entry.advanceSpeed * Time.deltaTime;
            var grid = CombatServices.Grid;
            if (grid != null)
            {
                var cell = grid.CellAt(next);
                if (cell.element == Element.Agua)
                {
                    Debug.Log($"[Reacción] {_entry.reactionName} se apaga al llegar al agua.");
                    Destroy(gameObject);
                    return;
                }
            }
            transform.position = next;
        }

        /// La zona, por pulsos de reloj: aplica sus efectos a quien esté dentro. Los estados van con el
        /// **modelo de carga** (§0.3): re-aplicarlos mientras estás dentro los mantiene llenos, y al
        /// salir decaen solos — el `StatusController` se encarga.
        void Pulse()
        {
            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(Center + Vector3.up * 0.4f, _radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                var root = t.Transform.root;
                if (!seen.Add(root)) continue;
                if (!t.IsCreature) continue;      // una piedra no se ralentiza ni se quema

                var status = StatusController.Of(t.Transform);

                if (_entry.zoneStatus != StatusType.None && (!_entry.zoneStatusOnce || _statusApplied.Add(root)))
                {
                    // CORRIENTE: el estado lleva la dirección, que es lo que arrastra.
                    Vector3 drag = _entry.dragSpeed > 0f ? _direction * _entry.dragSpeed : Vector3.zero;
                    status?.Apply(_entry.zoneStatus, _entry.statusSeconds, _entry.statusTickDamage, drag);
                    // Segundo verbo, para las parejas que aplican los dos (corriente = arrastre + tick).
                    if (_entry.zoneStatus2 != StatusType.None)
                        status?.Apply(_entry.zoneStatus2, _entry.statusSeconds);
                }

                if (_entry.zoneTickDamage > 0f)
                {
                    float mult = TypeMultiplier(t);
                    float dealt = _entry.zoneTickDamage * mult;
                    t.TakeHit(dealt, 0f);
                    if (t.TakesDamage)
                        HitFeedback.Report(t, dealt, mult,
                            t.Transform.position + Vector3.up * 1.2f, Vector3.zero, 0.1f);
                }

                // AMARRA: clava la estaca la primera vez que te pilla.
                if (_entry.tetherPull > 0f && !_anchors.ContainsKey(root))
                    _anchors[root] = t.Transform.position;
            }

            if (_entry.tetherPull > 0f) Tether();
            if (_entry.propagates && !_hasSpread && Age > _duration * 0.35f) Spread();
        }

        /// INCENDIO: se extiende **poco a poco y solo por terreno de su elemento**, hasta apagarse.
        /// La rejilla ya es el grafo de vecindad, así que no hace falta nada más: el parche de
        /// Naturaleza define el alcance del fuego, y el presupuesto de saltos garantiza que muera.
        void Spread()
        {
            _hasSpread = true;
            var grid = CombatServices.Grid;
            if (grid == null || _spreadBudget <= 0) return;

            var here = grid.CoordAt(Center);
            int spawned = 0;
            for (int i = 0; i < 6 && spawned < 2; i++)
            {
                var n = here.Neighbor(i);
                Vector3 p = n.ToWorld(grid.cellRadius);
                if (grid.CellAt(p).element != _entry.propagateThrough) continue;   // el fuego no salta al vacío
                if (AlreadyBurning(p)) continue;

                p.y = Center.y;
                // Cada salto dura menos y hereda un salto menos: el incendio se abre y se apaga solo.
                Spawn(_entry, p, _radius, _duration * 0.7f, 0.25f, null, _direction, _spreadBudget - 1);
                spawned++;
            }
        }

        static bool AlreadyBurning(Vector3 p)
        {
            for (int i = 0; i < Live.Count; i++)
            {
                var fx = Live[i];
                if (fx != null && fx._entry.propagates && fx.Contains(p)) return true;
            }
            return false;
        }

        /// AMARRA: tirones **a intervalos**, no una fuerza continua. Es deliberado — una fuerza
        /// constante contra tu input se lee como que los mandos no responden; un tirón se lee como un
        /// acontecimiento, y entre tirón y tirón corres normal. Tira aunque ya hayas salido: la correa
        /// dura lo que dura la zona, así que es un ring, no un charco.
        void Tether()
        {
            foreach (var kv in _anchors)
            {
                if (kv.Key == null) continue;
                Vector3 d = kv.Value - kv.Key.position; d.y = 0f;
                float dist = d.magnitude;
                if (dist < 1f) continue;                       // ya estás donde te ataron
                if (!kv.Key.TryGetComponent(out ICombatant t)) continue;
                Push(t, d.normalized * _entry.tetherPull, 0.22f);
            }
        }

        /// R3: un impacto nuevo dentro de la zona la convierte — pasada su gracia (§0.4).
        public void CancelledByImpact()
        {
            HitboxDebug.Show(Center + Vector3.up * 0.2f, _radius * 0.8f, 0.15f,
                             new Color(0.6f, 0.6f, 0.6f, 0.35f));
            Destroy(gameObject);
        }

        // ---------------- Caja gris ----------------

        void BuildTelegraph()
        {
            _telegraph = MakeDisc("Aviso", 0.04f, 0.28f).transform;
        }

        void BuildVisual()
        {
            _disc = MakeDisc("Zona", 0.05f, Tint.a).transform;
        }

        /// MECHA: mientras acumula no daña, pero **satura de color** — es su único aviso, y con arte
        /// será lo mismo. Es además la más fácil de leer en caja gris precisamente por esto.
        void PaintCharge()
        {
            if (_disc == null || _mat == null) return;
            float t = Mathf.InverseLerp(_bornAt, _dieAt, Time.time);
            var c = Tint; c.a = Mathf.Lerp(0.2f, 0.9f, t);
            _mat.SetColor("_BaseColor", c);
            _mat.SetColor("_Color", c);
            _disc.localScale = new Vector3(_radius * 2f, 0.01f, _radius * 2f) * Mathf.Lerp(0.8f, 1f, t);
        }

        GameObject MakeDisc(string name, float height, float alpha)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            // DestroyImmediate: Destroy es diferido a fin de frame y, con esta escala, la cápsula queda
            // como una ESFERA sólida del radio de la zona donde acaba de estallar el jugador — un frame
            // de empujón fantasma, intermitente y muy difícil de diagnosticar.
            DestroyImmediate(disc.GetComponent<Collider>());
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = Vector3.up * height;
            disc.transform.localScale = new Vector3(_radius * 2f, 0.01f, _radius * 2f);

            if (_mat != null) Destroy(_mat);
            var tint = Tint; tint.a = alpha;
            _mat = Transparent(tint);
            disc.GetComponent<Renderer>().sharedMaterial = _mat;
            return disc;
        }

        static Material Transparent(Color tint)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var m = new Material(shader != null ? shader : Shader.Find("Universal Render Pipeline/Lit"));
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            if (m.HasProperty("_SrcBlend")) m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
            if (m.HasProperty("_Color")) m.SetColor("_Color", tint);
            return m;
        }

        static GUIStyle _style;

        void OnGUI()
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(Center + Vector3.up * 0.9f);
            if (sp.z <= 0f) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Overflow, wordWrap = false,
                };
            _style.fontSize = Mathf.RoundToInt(Mathf.Clamp(14f * (12f / Mathf.Max(1f, sp.z)), 9f, 18f));

            string txt = _hatched ? _entry.reactionName : _entry.reactionName + "…";
            var r = new Rect(sp.x - 120f, Screen.height - sp.y, 240f, 20f);
            _style.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
            GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), txt, _style);
            var tint = Tint; tint.a = 1f;
            _style.normal.textColor = tint;
            GUI.Label(r, txt, _style);
        }
    }
}
