namespace Tatemis
{
    /// Instancia viva de una habilidad en ejecución. La tickea el AbilityCaster fase a fase.
    public class ActiveAbility
    {
        public readonly AbilityData data;
        readonly AbilityCtx _ctx;
        int _phase;
        float _t;
        bool _entered;

        public ActiveAbility(AbilityData data, AbilityCtx ctx)
        {
            this.data = data;
            _ctx = ctx;
            _ctx.reportHit = ReportHit;   // los actuadores (hitbox/proyectil) reportan impactos por aquí
        }

        public AbilityCtx Ctx => _ctx;

        public AbilityPhase Current =>
            (data.phases != null && _phase >= 0 && _phase < data.phases.Count) ? data.phases[_phase] : null;

        /// Devuelve false cuando la habilidad terminó (todas sus fases).
        public bool Tick(float dt)
        {
            var p = Current;
            if (p == null) return false;

            if (!_entered)
            {
                _entered = true;
                if (p.effects != null) foreach (var e in p.effects) e?.OnEnter(_ctx);
            }

            _t += dt;
            if (p.effects != null) foreach (var e in p.effects) e?.OnTick(_ctx, dt);

            if (p.exit == null || p.exit.Done(_ctx, _t))
            {
                if (p.effects != null) foreach (var e in p.effects) e?.OnExit(_ctx);
                _phase++;
                _t = 0f;
                _entered = false;
                _ctx.pressedAgain = false;
                _ctx.lastHit = default;
                if (Current == null) return false;   // no quedan más fases
            }
            return true;
        }

        /// Lo llama un carrier (hitbox/proyectil/zona) al impactar algo.
        public void ReportHit(HitInfo hit)
        {
            _ctx.lastHit = hit;
            var p = Current;
            if (p?.effects != null) foreach (var e in p.effects) e?.OnHit(_ctx, hit);
        }
    }
}
