using UnityEngine;

namespace Tatemis
{
    /// Lock-on básico: fija/suelta el ICombatant más cercano dentro de rango.
    public class TargetingSystem : MonoBehaviour
    {
        public float range = 15f;
        public LayerMask targetMask = ~0;

        public ICombatant Current { get; private set; }

        public void ToggleLockOn()
        {
            Current = Current == null ? FindNearest() : null;
        }

        ICombatant FindNearest()
        {
            var hits = Physics.OverlapSphere(transform.position, range, targetMask);
            ICombatant best = null;
            float bestSqr = float.MaxValue;

            foreach (var h in hits)
            {
                if (!h.TryGetComponent(out ICombatant c)) continue;
                if (c.Transform.root == transform.root) continue; // no fijarse a sí mismo
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = c; }
            }
            return best;
        }
    }
}
