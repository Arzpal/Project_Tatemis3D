using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    // ============ PAYLOAD (causan la consecuencia) ============

    [System.Serializable]
    public class DealDamage : AbilityEffect
    {
        public float amount = 10f;
        public float staggerAmount = 5f;
        [Tooltip("OnCast = al objetivo del lock-on · OnHit = a lo que golpee un carrier (hitbox/proyectil/zona)")]
        public ApplyWhen when = ApplyWhen.OnCast;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Apply(c, c.lockTarget); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Apply(c, hit.target); }

        void Apply(AbilityCtx c, ICombatant t)
        {
            if (t == null) return;
            float mult = CombatServices.TypeChart != null ? CombatServices.TypeChart.Multiplier(c.element, t.Element) : 1f;
            t.TakeHit(amount * mult, staggerAmount * mult);
        }
    }

    [System.Serializable]
    public class ApplyStatus : AbilityEffect
    {
        public StatusType type = StatusType.Ralentizado;
        public float duration = 3f;
        public ApplyWhen when = ApplyWhen.OnHit;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Log(c.lockTarget); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Log(hit.target); }

        void Log(ICombatant t)
        {
            if (t == null) return;
            // TODO(Pasada 2): enrutar a un StatusController real en el objetivo.
            Debug.Log($"[Ability] {type} {duration}s -> {t.Transform?.name}");
        }
    }

    [System.Serializable]
    public class GainEnergy : AbilityEffect
    {
        public float amount = 20f;
        public override void OnEnter(AbilityCtx c)
        {
            if (c.casterEnergy != null)
                c.casterEnergy.current = Mathf.Min(c.casterEnergy.max, c.casterEnergy.current + amount);
        }
    }

    // ============ ACTUADORES (hacen la acción) — versión base; se amplían en la Pasada 2 ============

    [System.Serializable]
    public class DashMotion : AbilityEffect
    {
        public float distance = 5f;
        public float time = 0.16f;
        public override void OnEnter(AbilityCtx c)
        {
            if (c.caster == null) return;
            c.caster.RequestDash(c.caster.transform.forward * distance, time);
        }
    }

    /// Delivery melee: al entrar la fase, solapa un volumen frente al caster y reporta
    /// cada ICombatant golpeado -> dispara el OnHit de los payloads de la fase (DealDamage OnHit, etc.).
    [System.Serializable]
    public class MeleeHitbox : AbilityEffect
    {
        public float range = 2f;      // alcance frente al caster
        public float radius = 1.5f;   // grosor del arco

        public override void OnEnter(AbilityCtx c)
        {
            if (c.caster == null || c.reportHit == null) return;
            Vector3 center = c.caster.transform.position
                           + c.caster.transform.forward * range * 0.5f
                           + Vector3.up * 0.5f;

            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(center, radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                if (t.Transform.root == c.caster.transform.root) continue;   // no autogolpe
                if (!seen.Add(t.Transform.root)) continue;                   // 1 vez por objetivo
                c.reportHit(new HitInfo { target = t, point = col.ClosestPoint(center), normal = Vector3.up });
            }
        }
    }

    [System.Serializable]
    public class StainTerrain : AbilityEffect
    {
        public float radius = 3f;
        public override void OnEnter(AbilityCtx c)
        {
            CombatServices.Reactions?.Stain(c.aimPoint, radius, c.element);
        }
    }
}
