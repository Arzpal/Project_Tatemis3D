using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Lock-on estilo BotW/TOTK (ver docs/add.md §7): adquiere el enemigo **más relevante en pantalla**
    /// (cono alrededor de la mira, prefiere el más centrado), permite **cambiar de objetivo** hacia un
    /// lado, y suelta el lock solo si el objetivo se aleja, cae o lleva un buen rato sin verse
    /// (la línea de visión hace falta para FIJAR, no para MANTENER: un pilar de por medio no rompe nada).
    public class TargetingSystem : MonoBehaviour
    {
        [Header("Adquisición")]
        public float range = 15f;
        [Tooltip("Ángulo TOTAL del cono de búsqueda alrededor de la mira de cámara (grados)")]
        public float acquisitionCone = 120f;
        [Tooltip("Cuánto pesa la distancia frente a lo centrado que esté (0 = solo importa el centrado)")]
        public float distanceWeight = 25f;
        public LayerMask targetMask = ~0;

        [Header("Ruptura del lock")]
        public float breakDistance = 20f;
        [Tooltip("Exigir línea de visión para FIJAR (no se fija a través de una pared). Mantener el lock ya fijado es otra cosa: ver sightGrace")]
        public bool requireLineOfSight = true;
        public LayerMask sightBlockers = ~0;
        [Tooltip("Segundos seguidos sin ver al objetivo antes de soltarlo. BotW no suelta al primer obstáculo: un pilar de por medio no debe romper el lock")]
        public float sightGrace = 2.5f;
        [Tooltip("Por debajo de esta distancia la oclusión NUNCA rompe el lock: estás encima del enemigo y perderlo se siente injusto")]
        public float closeRangeKeep = 8f;

        [Header("Cambio de objetivo")]
        [Tooltip("Separación angular mínima para considerar que otro objetivo está 'a un lado'")]
        public float switchMinSeparation = 4f;

        ICombatant _current;
        public ICombatant Current
        {
            get => _current;
            private set { _current = value; _blindTime = 0f; }
        }

        float _blindTime;   // segundos seguidos sin línea de visión con el objetivo actual

        struct Cand { public ICombatant c; public float signed; public float dist; }
        readonly List<Cand> _cands = new List<Cand>();

        public void ClearTarget() => Current = null;

        /// Fija el mejor objetivo en pantalla. **false** si no hay ninguno → quien llama puede
        /// recentrar la cámara (es el ZL de Zelda cuando no hay enemigo delante).
        public bool TryAcquire()
        {
            Scan();
            ICombatant best = null;
            float bestScore = float.MaxValue;
            foreach (var k in _cands)
            {
                float score = Mathf.Abs(k.signed) + k.dist * distanceWeight / Mathf.Max(1f, range);
                if (score < bestScore) { bestScore = score; best = k.c; }
            }
            Current = best;
            return Current != null;
        }

        /// Cambia al siguiente objetivo hacia la izquierda (-1) o la derecha (+1).
        public bool SwitchTarget(float dirSign)
        {
            if (Current == null) return false;
            Scan();
            float cur = SignedAngleTo(Current);
            ICombatant best = null;
            float bestDelta = float.MaxValue;
            foreach (var k in _cands)
            {
                if (k.c == Current) continue;
                float delta = (k.signed - cur) * Mathf.Sign(dirSign);
                if (delta < switchMinSeparation) continue;      // tiene que estar del lado pedido
                if (delta < bestDelta) { bestDelta = delta; best = k.c; }
            }
            if (best == null) return false;
            Current = best;
            return true;
        }

        void Update()
        {
            if (Current == null) return;
            if (Current is Object uo && uo == null) { Current = null; return; }
            if (Current is StaggerReceiver sr && sr.Defeated) { Current = null; return; }

            Vector3 d = Current.Transform.position - transform.position;
            if (d.sqrMagnitude > breakDistance * breakDistance) { Current = null; return; }

            // La oclusión NO rompe el lock al instante: en BotW puedes rodear un pilar sin perder el
            // objetivo. Solo se suelta tras `sightGrace` segundos seguidos sin verlo, y nunca de cerca.
            bool blind = requireLineOfSight
                      && d.sqrMagnitude > closeRangeKeep * closeRangeKeep
                      && !HasLineOfSight(Current);
            if (!blind) { _blindTime = 0f; return; }

            _blindTime += Time.deltaTime;
            if (_blindTime >= sightGrace) Current = null;
        }

        /// Cegado (verbo del Aire): mientras dure, no se fija objetivo nuevo.
        bool _blinded;
        StatusController _status;

        void Scan()
        {
            _cands.Clear();
            if (_status == null) _status = GetComponent<StatusController>();
            _blinded = _status != null && _status.Blinded;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 fwd = CameraForward();
            float half = acquisitionCone * 0.5f;

            foreach (var col in Physics.OverlapSphere(transform.position, range, targetMask))
            {
                if (!col.TryGetComponent(out ICombatant c)) continue;
                if (!c.IsCreature) continue;                               // no fijarse a un nodo de cristal
                if (c.Transform.root == transform.root) continue;          // no fijarse a sí mismo
                if (c is StaggerReceiver s && s.Defeated) continue;

                Vector3 to = c.Transform.position - origin; to.y = 0f;
                float dist = to.magnitude;
                if (dist < 0.01f || dist > range) continue;

                float signed = Vector3.SignedAngle(fwd, to / dist, Vector3.up);
                if (Mathf.Abs(signed) > half) continue;                    // fuera de pantalla
                if (requireLineOfSight && !HasLineOfSight(c)) continue;
                // NIEBLA (Aire + Agua): a través de ella no se puede FIJAR. Mantener el objetivo que ya
                // tenías sí — mismo criterio que la línea de visión, que hace falta para fijar y no para
                // conservar. Y si estás CEGADO, no fijas nada en absoluto.
                if (_blinded || ReactionEffect.TargetingBlocked(origin, c.Transform.position)) continue;

                _cands.Add(new Cand { c = c, signed = signed, dist = dist });
            }
        }

        float SignedAngleTo(ICombatant c)
        {
            Vector3 to = c.Transform.position - (transform.position + Vector3.up * 0.5f); to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return 0f;
            return Vector3.SignedAngle(CameraForward(), to.normalized, Vector3.up);
        }

        Vector3 CameraForward()
        {
            var cam = Camera.main;
            Vector3 f = cam != null ? cam.transform.forward : transform.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.0001f ? f.normalized : transform.forward;
        }

        bool HasLineOfSight(ICombatant c)
        {
            Vector3 a = transform.position + Vector3.up * 0.8f;
            Vector3 b = c.Transform.position + Vector3.up * 0.8f;
            Vector3 dir = b - a;
            float d = dir.magnitude;
            if (d < 0.1f) return true;

            foreach (var hit in Physics.RaycastAll(a, dir / d, d, sightBlockers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root == transform.root) continue;        // yo
                if (hit.transform.root == c.Transform.root) continue;      // el propio objetivo
                return false;                                              // algo se interpone
            }
            return true;
        }
    }
}
