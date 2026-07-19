using UnityEngine;

namespace Tatemis
{
    /// Controla la criatura activa: movimiento, salto, lock-on, kit de 3 habilidades
    /// (Movilidad / Flex / Signature) + ataque básico universal, bloqueo (forma elemental
    /// pura) y acción contextual (finisher/interactuar). Máquina de estados simple.
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ElementalEnergy))]
    [RequireComponent(typeof(AbilityCaster))]
    public class CreatureController : MonoBehaviour, ICombatant
    {
        public enum State { Locomotion, Ability, Block, Hitstun, KO }

        [Header("Datos")]
        public CreatureInstance data;

        [Header("Movimiento")]
        public float gravity = -25f;
        public float jumpSpeed = 8f;
        public float turnSpeed = 12f;
        public float fallbackMoveSpeed = 5f;

        public State CurrentState { get; private set; } = State.Locomotion;
        public Element Element => data != null ? data.Element : Element.Naturaleza;
        public Transform Transform => transform;

        CharacterController _cc;
        ElementalEnergy _energy;
        AbilityCaster _caster;
        InputReader _input;
        TargetingSystem _targeting;

        Vector3 _velocity;
        float _dashTimer;
        Vector3 _dashVel;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _energy = GetComponent<ElementalEnergy>();
            _caster = GetComponent<AbilityCaster>();
            _input = GetComponent<InputReader>();
            _targeting = GetComponent<TargetingSystem>();
        }

        void Update()
        {
            switch (CurrentState)
            {
                case State.Locomotion: TickLocomotion(); break;
                case State.Block:      TickBlock();      break;
                // Ability / Hitstun / KO: se gestionan con corrutinas/animaciones (TODO Fase 1+).
            }
            ApplyMotion();
        }

        // ---------------- Estados ----------------

        void TickLocomotion()
        {
            if (_input == null) return;

            if (_input.LockOnPressed && _targeting != null) _targeting.ToggleLockOn();

            if (_input.BlockHeld) { EnterBlock(); return; }

            // Movimiento relativo a cámara.
            Vector3 move = CameraRelative(_input.Move);
            float speed = (data?.Form != null) ? data.Form.stats.moveSpeed : fallbackMoveSpeed;
            if (speed <= 0f) speed = fallbackMoveSpeed;

            _velocity.x = move.x * speed;
            _velocity.z = move.z * speed;

            if (move.sqrMagnitude > 0.01f)
                transform.forward = Vector3.Slerp(transform.forward, move.normalized, turnSpeed * Time.deltaTime);

            if (_input.JumpPressed && _cc.isGrounded) _velocity.y = jumpSpeed;

            // Kit de 3 habilidades (con coste) + ataque básico universal (gratis).
            if (data?.Form != null)
            {
                var form = data.Form;
                AbilityData[] kit = { form.movilidad, form.flex, form.signature };
                for (int i = 0; i < kit.Length; i++)
                    if (_input.AbilityPressed(i)) TryCastAbility(kit[i]);

                if (_input.BasicPressed) TryCastAbility(form.basic);
            }

            // Acción contextual (finisher/Sludge/interactuar): no hay botón de noqueo dedicado.
            if (_input.ContextualPressed) TryContextualAction();
        }

        void TickBlock()
        {
            float mult = 2.5f; // fallback (los struct del inspector arrancan en 0)
            if (data?.Form != null && data.Form.block.energyRegenMultiplier > 0f)
                mult = data.Form.block.energyRegenMultiplier;

            _energy.SetRegenMultiplier(mult);
            _velocity.x = 0f;
            _velocity.z = 0f;

            if (_input == null || !_input.BlockHeld) ExitBlock();
        }

        void EnterBlock()
        {
            CurrentState = State.Block;
            // TODO(Fase 2): VFX de forma pura + marcar vulnerabilidad x2 a la debilidad.
        }

        void ExitBlock()
        {
            CurrentState = State.Locomotion;
            _energy.SetRegenMultiplier(1f);
        }

        // ---------------- Acciones ----------------

        void TryCastAbility(AbilityData a)
        {
            if (a == null || _caster.IsBusy) return;
            var ctx = new AbilityCtx
            {
                caster = this,
                casterEnergy = _energy,
                aimPoint = transform.position + transform.forward * 2f,
                lockTarget = _targeting != null ? _targeting.Current : null,
                element = Element,
            };
            _caster.TryStart(a, ctx);
        }

        /// Botón contextual: por ahora solo intenta el finisher sobre el objetivo fijado.
        /// TODO(Fase 1+): compartir con Sludge/interactuar según el contexto (GDD §6.3).
        void TryContextualAction()
        {
            if (_targeting != null && _targeting.Current is StaggerReceiver sr)
                sr.TryKnockout();
        }

        // ---------------- Movimiento ----------------

        Vector3 CameraRelative(Vector2 input)
        {
            var cam = Camera.main;
            if (cam == null) return new Vector3(input.x, 0f, input.y);

            Vector3 f = cam.transform.forward; f.y = 0f; f.Normalize();
            Vector3 r = cam.transform.right;   r.y = 0f; r.Normalize();
            return f * input.y + r * input.x;
        }

        void ApplyMotion()
        {
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                _cc.Move(_dashVel * Time.deltaTime);
                return;
            }

            if (!_cc.isGrounded) _velocity.y += gravity * Time.deltaTime;
            else if (_velocity.y < 0f) _velocity.y = -1f;

            _cc.Move(_velocity * Time.deltaTime);
        }

        /// Lo usan efectos de movilidad (p.ej. Dash).
        public void RequestDash(Vector3 displacement, float time)
        {
            _dashTimer = time;
            _dashVel = displacement / Mathf.Max(0.01f, time);
        }

        // ---------------- ICombatant ----------------

        public void TakeHit(float damage, float stagger)
        {
            // TODO: vida/hitstun de la criatura activa si se decide que recibe daño en combate.
        }
    }
}
