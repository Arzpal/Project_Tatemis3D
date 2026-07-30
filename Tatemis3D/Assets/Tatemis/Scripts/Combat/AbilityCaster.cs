using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Valida cooldown + energía, crea la habilidad en ejecución y la tickea fase a fase.
    [RequireComponent(typeof(ElementalEnergy))]
    public class AbilityCaster : MonoBehaviour
    {
        ElementalEnergy _energy;
        readonly Dictionary<AbilityData, float> _readyAt = new Dictionary<AbilityData, float>();
        ActiveAbility _active;

        public bool IsBusy => _active != null;
        public AbilityPhase CurrentPhase => _active?.Current;

        void Awake() => _energy = GetComponent<ElementalEnergy>();

        public bool CanCast(AbilityData a)
        {
            if (a == null || _active != null) return false;
            if (_readyAt.TryGetValue(a, out float t) && Time.time < t) return false;
            return _energy.current >= a.energyCost;
        }

        /// Segundos de cooldown que le quedan a una habilidad (0 = lista). Para el HUD de debug.
        public float CooldownRemaining(AbilityData a)
        {
            if (a == null) return 0f;
            return _readyAt.TryGetValue(a, out float t) ? Mathf.Max(0f, t - Time.time) : 0f;
        }

        public bool TryStart(AbilityData a, AbilityCtx ctx)
        {
            if (!CanCast(a)) return false;

            // INTERFERIDO (verbo de Rayo): el tick corta el **arranque** de un ataque, nunca un golpe
            // ya comprometido (anatomía de §0.6). Aquí es exactamente el arranque, así que este es su
            // sitio. Se cobra la energía igual: el intento se ha gastado — es lo que hace que no
            // puedas encadenar, y por eso Rayo no te quita vida, te quita la captura.
            if (_status == null) _status = GetComponent<StatusController>();
            if (_status != null && _status.ConsumeInterrupt())
            {
                _energy.TrySpend(a.energyCost * 0.5f);
                Debug.Log($"[Rayo] Interferencia corta el arranque de {a.displayName}.");
                return false;
            }

            if (!_energy.TrySpend(a.energyCost)) return false;
            _readyAt[a] = Time.time + a.cooldown;
            _active = new ActiveAbility(a, ctx);
            return true;
        }

        StatusController _status;

        // El CreatureController y los carriers alimentan estado a la habilidad activa.
        public void SetButtonHeld(bool held) { if (_active != null) _active.Ctx.buttonHeld = held; }
        public void NotifyPressAgain()       { if (_active != null) _active.Ctx.pressedAgain = true; }
        public void ReportHit(HitInfo hit)   => _active?.ReportHit(hit);

        void Update()
        {
            if (_active == null) return;
            if (!_active.Tick(Time.deltaTime)) _active = null;
        }
    }
}
