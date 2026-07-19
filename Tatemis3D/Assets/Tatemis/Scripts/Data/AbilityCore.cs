using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Cuándo aplica un efecto de payload: al lanzar (al objetivo del lock-on) o al impactar (un carrier).
    public enum ApplyWhen { OnCast, OnHit }

    /// Info de un impacto que un carrier (hitbox/proyectil/zona) reporta a la habilidad.
    public struct HitInfo
    {
        public ICombatant target;
        public Vector3 point;
        public Vector3 normal;
    }

    /// Contexto vivo de UNA activación de habilidad. Lo mutan la habilidad y los carriers.
    public class AbilityCtx
    {
        public CreatureController caster;
        public ElementalEnergy casterEnergy;
        public ICombatant lockTarget;    // objetivo del lock-on al lanzar
        public Vector3 aimPoint;
        public Element element;

        public bool buttonHeld;          // para OnButtonReleased
        public bool pressedAgain;        // para OnSecondPress
        public HitInfo lastHit;          // último impacto reportado en la fase actual
        public System.Action<HitInfo> reportHit;   // lo cablea ActiveAbility; los actuadores lo llaman al impactar

        public readonly Dictionary<string, object> bb = new Dictionary<string, object>(); // blackboard (p.ej. objeto agarrado)
    }

    /// Efecto: se engancha a los hooks del ciclo de vida de la fase que le interesen.
    [System.Serializable]
    public abstract class AbilityEffect
    {
        public virtual void OnEnter(AbilityCtx c) { }
        public virtual void OnTick(AbilityCtx c, float dt) { }
        public virtual void OnHit(AbilityCtx c, HitInfo hit) { }
        public virtual void OnExit(AbilityCtx c) { }
    }

    /// Condición que decide cuándo termina una fase. Módulos elegibles en el inspector.
    [System.Serializable]
    public abstract class PhaseExit
    {
        public abstract bool Done(AbilityCtx c, float phaseTime);
    }

    [System.Serializable] public class Immediate       : PhaseExit { public override bool Done(AbilityCtx c, float t) => true; }
    [System.Serializable] public class AfterSeconds    : PhaseExit { public float seconds = 0.3f; public override bool Done(AbilityCtx c, float t) => t >= seconds; }
    [System.Serializable] public class OnButtonReleased: PhaseExit { public override bool Done(AbilityCtx c, float t) => !c.buttonHeld; }
    [System.Serializable] public class OnSecondPress   : PhaseExit { public override bool Done(AbilityCtx c, float t) => c.pressedAgain; }
    [System.Serializable] public class OnHitLanded     : PhaseExit { public override bool Done(AbilityCtx c, float t) => c.lastHit.target != null; }

    /// Una fase = efectos + cuándo termina + cómo se comporta el cuerpo (flags de ejecución).
    /// 1 fase = habilidad simple; varias = multi-paso (cargar, agarrar-y-lanzar).
    [System.Serializable]
    public class AbilityPhase
    {
        public string name = "Fase";
        public string animTrigger;

        [SerializeReference, SubclassSelector] public List<AbilityEffect> effects = new List<AbilityEffect>();
        [SerializeReference, SubclassSelector] public PhaseExit exit = new AfterSeconds();

        [Header("Flags de ejecución (cómo se comporta el cuerpo)")]
        public bool rooted;              // no puede moverse durante la fase
        public bool iFrames;             // invulnerable
        public bool superArmor;          // no lo interrumpen los golpes
        public bool interruptible = true;
    }
}
