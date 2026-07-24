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
        [Tooltip("Suavizado del giro del cuerpo (SmoothDamp): da aceleración y frenada, no un giro plano")]
        public float turnSmoothTime = 0.12f;
        [Tooltip("Multiplicador de velocidad mientras apuntas (el coste del modo aim)")]
        public float aimMoveMultiplier = 0.5f;

        [Header("Asistencia de melee")]
        public float assistRange = 4f;
        public float assistCone = 120f;
        public float fallbackMoveSpeed = 5f;

        [Header("Suspensión en el aire (juggle)")]
        [Tooltip("Segundos que dejas de caer al CONECTAR un golpe. ⚠️ Debe cubrir lo que tarda el SIGUIENTE golpe en poder salir — y eso incluye las fases de recuperación: tras una embestida pasan ~0.7 s antes de poder encadenar. Si dura menos, caes antes de poder rematar")]
        public float hangOnHitLanded = 1f;
        [Tooltip("Segundos que dejas de caer al RECIBIR un golpe")]
        public float hangOnHitTaken = 0.8f;
        [Tooltip("Tope acumulable sin tocar suelo. Guardarraíl contra quedarse flotando a base de golpear; se recarga al aterrizar. ~3 s ≈ una cadena aérea completa, no más")]
        public float maxHang = 3f;
        [Tooltip("Margen para encadenar tras TERMINAR una habilidad. Pasado esto, caes aunque te quedara hang: el hang largo es para cubrir las fases de una habilidad, no para flotar cuando ya has dejado de pegar")]
        public float hangTailAfterAbility = 0.45f;
        [Tooltip("A qué ritmo se frena la caída al suspender (u/s²). ALTO a propósito: debe frenar mucho más rápido de lo que dura el hang, o el golpe solo 'frena un poco' la caída en vez de sostenerte, y no se siente")]
        public float hangDamp = 160f;

        public State CurrentState { get; private set; } = State.Locomotion;
        public Element Element => data != null ? data.Element : Element.Naturaleza;
        public Transform Transform => transform;
        /// Para que una habilidad pueda pedir su propio encuadre (efecto `CameraShot`).
        public PlayerCameraRig CameraRig => _camRig;

        CharacterController _cc;
        ElementalEnergy _energy;
        AbilityCaster _caster;
        InputReader _input;
        TargetingSystem _targeting;
        PlayerCameraRig _camRig;
        ActionColor _color;
        StatusController _status;

        Vector3 _velocity;
        float _dashTimer, _dashTime;
        bool _dashEaseOut;
        Vector3 _dashVel;
        float _turnVel;   // estado del SmoothDamp del giro

        // Contador de básicos (combo), se reinicia tras un rato sin golpear.
        public int BasicCombo { get; private set; }
        public float basicComboReset = 1.2f;
        float _lastBasicTime = -999f;

        // Colores de debug por acción (los aplica ActionColor).
        static readonly Color ColMovilidad = new Color(0f, 0.7f, 1f);     // azul cian
        static readonly Color ColFlex      = new Color(1f, 0.45f, 0f);    // naranja fuerte
        static readonly Color ColSignature = new Color(1f, 0f, 0.85f);    // magenta
        static readonly Color ColBasic     = new Color(1f, 0.9f, 0.1f);   // amarillo
        static readonly Color ColBlock     = new Color(0.2f, 1f, 0.35f);  // verde

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _energy = GetComponent<ElementalEnergy>();
            _caster = GetComponent<AbilityCaster>();
            _input = GetComponent<InputReader>();
            _targeting = GetComponent<TargetingSystem>();
            _camRig = GetComponent<PlayerCameraRig>();
            _color = GetComponent<ActionColor>();
            _status = GetComponent<StatusController>();
        }

        void Update()
        {
            if (Time.time - _lastBasicTime > basicComboReset) BasicCombo = 0;

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

            // Lock-on: si ya hay objetivo lo suelta; si no, intenta fijar el mejor en pantalla y,
            // si no hay ninguno, **recentra la cámara** detrás (el ZL de Zelda sin enemigo delante).
            if (_input.LockOnPressed && _targeting != null)
            {
                if (_targeting.Current != null) _targeting.ClearTarget();
                else if (!_targeting.TryAcquire() && _camRig != null) _camRig.RecenterBehind();
            }

            if (_input.BlockHeld) { EnterBlock(); return; }

            // Movimiento siempre relativo a la CÁMARA. En libre avanzas hacia donde miras; en lock,
            // como la cámara se alinea al eje jugador→enemigo, moverte de lado lo ROdea (strafe/orbitar).
            Vector3 camFwd = _camRig != null ? _camRig.LookForward : transform.forward;
            Vector3 camRight = _camRig != null ? _camRig.LookRight : transform.right;
            Vector3 move = camRight * _input.Move.x + camFwd * _input.Move.y;

            bool aiming = _camRig != null && _camRig.Aiming;
            float speed = ResolveSpeed(_input.Move.magnitude, aiming);

            if (move.sqrMagnitude > 0.0001f) move.Normalize();   // la magnitud ya la resolvió el andar
            _velocity.x = move.x * speed;
            _velocity.z = move.z * speed;

            // Encarado — una regla, tres fuentes: apuntando → la cámara · con lock → el enemigo ·
            // en libre → su dirección de avance. El ataque siempre sale hacia donde encara el cuerpo.
            Vector3 faceDir;
            if (aiming) faceDir = _camRig.LookForward;
            else if (_targeting != null && _targeting.Current != null)
            {
                faceDir = _targeting.Current.Transform.position - transform.position;
                faceDir.y = 0f;
            }
            else faceDir = move;

            if (faceDir.sqrMagnitude > 0.01f && !IsRooted)   // anclado tampoco gira: la carga se telegrafía quieta
            {
                float want = Quaternion.LookRotation(faceDir.normalized, Vector3.up).eulerAngles.y;
                float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, want, ref _turnVel, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            if (_input.JumpPressed && _cc.isGrounded) _velocity.y = jumpSpeed;

            // Kit de 3 habilidades (con coste) + ataque básico universal (gratis).
            if (data?.Form != null)
            {
                var form = data.Form;
                AbilityData[] kit = { form.movilidad, form.flex, form.signature };
                for (int i = 0; i < kit.Length; i++)
                    if (_input.AbilityPressed(i) && TryCastAbility(kit[i]))
                        Flash(i == 0 ? ColMovilidad : i == 1 ? ColFlex : ColSignature);

                // Cadena de básicos: cada golpe encadenado avanza un eslabón y vuelve al primero al
                // completarla. Si dejas de pegar (basicComboReset) la cadena se rompe sola arriba.
                if (_input.BasicPressed)
                {
                    if (Time.time - _lastBasicTime > basicComboReset) BasicCombo = 0;
                    if (TryCastAbility(form.BasicAt(BasicCombo)))
                    {
                        BasicCombo = form.basicChain.Count > 0 ? (BasicCombo + 1) % form.basicChain.Count : 0;
                        _lastBasicTime = Time.time;
                        Flash(ColBasic);
                    }
                }
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

            // Bloquear ya no te clava: puedes reposicionarte a PASO CORTO (`blockMaxInput`). El stance
            // sigue comprometiendo —no puedes atacar— pero mantener la guardia mientras te recolocas
            // es parte del juego; quedarte clavado solo hacía que bloquear se sintiera un castigo.
            if (_input != null)
            {
                Vector3 camFwd = _camRig != null ? _camRig.LookForward : transform.forward;
                Vector3 camRight = _camRig != null ? _camRig.LookRight : transform.right;
                Vector3 move = camRight * _input.Move.x + camFwd * _input.Move.y;
                float cap = Profile != null ? Profile.blockMaxInput : 0.25f;
                float speed = ResolveSpeed(_input.Move.magnitude, false, cap);
                if (move.sqrMagnitude > 0.0001f) move.Normalize();
                _velocity.x = move.x * speed;
                _velocity.z = move.z * speed;
            }
            else { _velocity.x = 0f; _velocity.z = 0f; }

            Flash(ColBlock);   // se mantiene verde mientras se bloquea

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

        // ---------------- Velocidad y andares ----------------

        /// Perfil de la forma activa (null = sin perfil: se cae a velocidad lineal simple).
        LocomotionProfile Profile => data?.Form?.locomotion;

        /// Andar actual, para el HUD y —mañana— para elegir el clip de animación.
        public string CurrentGait { get; private set; } = "Quieto";
        /// Deflexión efectiva ya rampada (0..1), por si alguien la quiere leer.
        public float MoveInput01 { get; private set; }

        /// **La velocidad la manda el stick.** La deflexión pasa por una rampa corta —imperceptible con
        /// stick, y el único sucedáneo de finura con teclado— y luego por la curva de andares del perfil.
        /// `maxInput` recorta la deflexión sin recortar el recorrido del stick: bloquear te deja a paso
        /// corto usando todo el rango, en vez de convertir el stick en un interruptor.
        float ResolveSpeed(float rawInput, bool aiming, float maxInput = 1f)
        {
            var p = Profile;
            float ramp = p != null ? p.inputRamp : 0.16f;
            float target = Mathf.Clamp01(rawInput) * Mathf.Clamp01(maxInput);
            MoveInput01 = Mathf.MoveTowards(MoveInput01, target, Time.deltaTime / Mathf.Max(0.01f, ramp));

            float baseSpeed = (data?.Form != null && data.Form.stats.moveSpeed > 0f)
                ? data.Form.stats.moveSpeed : fallbackMoveSpeed;

            // Sin perfil, la deflexión es el factor (comportamiento lineal de siempre).
            float factor = p != null ? p.SpeedFactor(MoveInput01) : MoveInput01;
            CurrentGait = p != null ? p.GaitName(MoveInput01) : (MoveInput01 > 0.05f ? "Caminar" : "Quieto");

            float speed = baseSpeed * factor;
            if (aiming) speed *= p != null ? p.aimMultiplier : aimMoveMultiplier;   // apuntar cuesta movilidad
            if (_status != null) speed *= _status.SpeedMultiplier;                  // ralentizado/aturdido
            return speed;
        }

        // ---------------- Acciones ----------------

        bool TryCastAbility(AbilityData a)
        {
            if (a == null || _caster.IsBusy) return false;
            AlignForAttack();   // el cuerpo encara el golpe ANTES de resolver la hitbox
            var ctx = new AbilityCtx
            {
                caster = this,
                casterEnergy = _energy,
                aimPoint = ResolveAimPoint(),
                lockTarget = _targeting != null ? _targeting.Current : null,
                element = ResolveElement(a),
            };
            return _caster.TryStart(a, ctx);
        }

        /// Elemento con el que golpea esta habilidad. **null = neutro**, que es como van los básicos
        /// por regla de diseño (GDD §7): si la columna vertebral del combate llevara elemento, pelear
        /// contra quien te supera sería imposible en vez de difícil.
        Element? ResolveElement(AbilityData a)
        {
            if (a == null) return null;
            switch (a.element)
            {
                case AbilityElement.Neutro: return null;
                case AbilityElement.Secundario: return data?.Form?.SecondaryElement ?? Element;
                default: return Element;
            }
        }

        void Flash(Color c) { if (_color != null) _color.Flash(c, 0.3f); }

        /// Punto al que apuntan las habilidades:
        /// **apuntando** → lo que hay bajo la retícula (convergencia, 3D — permite lanzarse en cualquier
        /// dirección) · **con lock** → el objetivo (3D, sirve para voladores) · **libre** → la horizontal
        /// del cuerpo (nunca por la espalda) con la inclinación de la cámara.
        Vector3 ResolveAimPoint()
        {
            if (_camRig != null && _camRig.Aiming) return _camRig.AimPoint;

            if (_targeting != null && _targeting.Current != null)
                return _targeting.Current.Transform.position + Vector3.up * 0.5f;

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 dir = transform.forward; dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return origin + transform.forward * 3f;
            dir.Normalize();

            if (_camRig != null) { dir.y = _camRig.AimForward.y; dir.Normalize(); }
            return origin + dir * 3f;
        }

        /// Asistencia de melee: si no apuntas ni tienes lock, orienta hacia un enemigo cercano dentro
        /// de un cono. El "abalanzarse" lo aporta el step-in que ya lleva cada golpe.
        void AlignForAttack()
        {
            if (_camRig != null && _camRig.Aiming) return;                 // apuntando mandas tú
            if (_targeting != null && _targeting.Current != null) return;  // el lock ya encara

            var t = FindAssistTarget();
            if (t == null) return;
            Vector3 d = t.Transform.position - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 0.01f) return;

            transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
            _turnVel = 0f;
        }

        ICombatant FindAssistTarget()
        {
            ICombatant best = null;
            float bestScore = float.MaxValue;
            foreach (var col in Physics.OverlapSphere(transform.position, assistRange))
            {
                if (!col.TryGetComponent(out ICombatant c)) continue;
                if (c.Transform.root == transform.root) continue;
                if (c is StaggerReceiver s && s.Defeated) continue;

                Vector3 d = c.Transform.position - transform.position; d.y = 0f;
                float dist = d.magnitude;
                if (dist < 0.01f || dist > assistRange) continue;

                float ang = Vector3.Angle(transform.forward, d / dist);
                if (ang > assistCone * 0.5f) continue;

                float score = ang + dist * 4f;
                if (score < bestScore) { bestScore = score; best = c; }
            }
            return best;
        }

        /// Botón contextual = **interactuar con el mundo** y nada más (ver TAMER.md §3): aplastar un
        /// hongo, agarrar y lanzar algo, activar el ambiente. El finisher ya no vive aquí —rematar es
        /// seguir atacando— y capturar/objetos/órdenes/swap se van al menú del Tamer.
        /// TODO(Fase 2+): resolver el interactuable más cercano y ejecutarlo.
        void TryContextualAction()
        {
        }

        // ---------------- Movimiento ----------------

        /// Fase de habilidad marcada `rooted`: el cuerpo queda CLAVADO donde está — ni control, ni
        /// inercia, ni gravedad. Es lo que da el momento de carga antes de una embestida (y la frenada
        /// después), y funciona igual en el aire: se queda suspendido.
        public bool IsRooted => _caster != null && _caster.CurrentPhase != null && _caster.CurrentPhase.rooted;

        void ApplyMotion()
        {
            if (IsRooted)
            {
                _velocity = Vector3.zero;
                _dashTimer = 0f;
                return;                      // sin Move() → se queda exactamente donde está
            }

            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                float k = _dashEaseOut ? Mathf.Clamp01(_dashTimer / _dashTime) : 1f;   // 1→0 = frena solo
                _cc.Move(_dashVel * k * Time.deltaTime);
                return;
            }

            float grav = (data?.Form?.locomotion != null) ? data.Form.locomotion.gravity : gravity;

            // El hang se corta al dejar de encadenar. Mientras hay una habilidad en curso el reloj de
            // la cola no avanza, así que las fases largas (la embestida son ~0.7 s) quedan cubiertas;
            // en cuanto terminas y no lanzas otra, te quedan `hangTailAfterAbility` y caes. Sin esto,
            // el segundo sobrante del último golpe se sentía como flotar sin motivo.
            if (_caster != null && _caster.IsBusy) _idleSince = Time.time;
            float hangUntil = Mathf.Min(_hangUntil, _idleSince + hangTailAfterAbility);

            if (_cc.isGrounded)
            {
                if (_velocity.y < 0f) _velocity.y = -1f;
                _hangUsed = 0f;                                  // aterrizar recarga la suspensión
                _idleSince = Time.time;
            }
            else if (Time.time < hangUntil)
                _velocity.y = Mathf.MoveTowards(_velocity.y, 0f, hangDamp * Time.deltaTime);
            else
                _velocity.y += grav * Time.deltaTime;

            _cc.Move(_velocity * Time.deltaTime);
        }

        /// Lo usan efectos de movilidad (p.ej. Dash). `easeOut` = arranca rápido y **frena solo**
        /// (deslizamiento): la velocidad decae de v0 a 0, y por eso v0 se dobla — el área del
        /// triángulo tiene que dar la misma distancia que daría el rectángulo a velocidad constante.
        public void RequestDash(Vector3 displacement, float time, bool easeOut = false)
        {
            _dashTime = _dashTimer = Mathf.Max(0.01f, time);
            _dashEaseOut = easeOut;
            _dashVel = displacement / _dashTime * (easeOut ? 2f : 1f);
            if (displacement.sqrMagnitude > 0.0001f) LastDashDirection = displacement.normalized;
        }

        /// Dirección del último desplazamiento pedido — para que una fase posterior pueda CONTINUARLO
        /// en vez de recalcular contra la mira (ver `DashMotion.continueForward`), y para que el
        /// knockback pueda arrastrar al enemigo en esa misma dirección 3D.
        public Vector3 LastDashDirection { get; private set; }

        float _hangUntil, _hangUsed, _idleSince;

        /// Suspende la caída un instante. Golpear y ser golpeado te mantienen en el aire, que es lo que
        /// hace posible encadenar allí arriba en vez de desplomarte a mitad de combo. El tope
        /// (`maxHang`) impide sostenerse indefinidamente a base de pegar; se recarga al aterrizar.
        public void GrantHang(float seconds)
        {
            // Ojo: NO se descarta por estar en el suelo. Una embestida arranca pisando tierra, así que
            // sus primeros golpes llegan con `isGrounded` aún true — descartarlos dejaba al jugador sin
            // suspensión mientras el enemigo sí la recibía, y por eso él caía y el otro se quedaba
            // flotando. En el suelo el hang no hace nada igualmente (manda el branch de grounded).
            if (seconds <= 0f || _cc == null || _hangUsed >= maxHang) return;
            // El presupuesto mide TIEMPO SUSPENDIDO, no número de golpes: solo se cobra lo que este
            // golpe añade sobre lo que ya había (si no, una cadena lo agota en cuatro impactos).
            float from = Mathf.Max(_hangUntil, Time.time);
            float extension = Mathf.Min(Time.time + seconds - from, maxHang - _hangUsed);
            if (extension <= 0f) return;
            _hangUsed += extension;
            _hangUntil = from + extension;
        }

        // ---------------- ICombatant ----------------

        public void TakeHit(float damage, float stagger)
        {
            GrantHang(hangOnHitTaken);   // que te peguen en el aire tampoco te desploma de golpe
            // TODO: vida/hitstun de la criatura activa si se decide que recibe daño en combate.
        }

        /// Lo llama el resolutor de daño cuando ESTE caster conecta un golpe.
        public void NotifyHitLanded() => GrantHang(hangOnHitLanded);
    }
}
