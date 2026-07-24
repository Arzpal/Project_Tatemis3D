using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// Cámara 3ª persona estilo BotW/TOTK — ver docs/CAMERA-TARGETING.md.
    /// - LIBRE: orbita por detrás del jugador con ratón/stick y, si te mueves sin tocar el look,
    ///   se **recentra sola** detrás de él (trail-behind).
    /// - LOCK: se coloca sobre el **eje jugador→enemigo**, por detrás del jugador, encuadrando a
    ///   AMBOS; puedes girar H/V dentro de unos **límites** y vuelve al encuadre al soltar.
    /// Va en el JUGADOR y mueve la Main Camera en LateUpdate (control total para clavar el feel;
    /// portable a Cinemachine cuando los parámetros estén cerrados).
    public class PlayerCameraRig : MonoBehaviour
    {
        [Header("Referencias")]
        public Transform follow;              // por defecto, este mismo transform (el jugador)
        public TargetingSystem targeting;
        public InputReader input;             // para leer el apuntado sostenido
        public Camera cam;                    // por defecto Camera.main

        [Header("Aim (apuntado sostenido)")]
        public float aimDistance = 3.2f;      // cámara cerca, al hombro
        public float aimShoulder = 0.8f;      // desplazamiento lateral
        public float aimMinDistance = 3f;     // convergencia: no apuntar a algo pegado a la cara
        public float aimMaxDistance = 200f;
        [Tooltip("Grados/seg a los que el picado vuelve a su sitio AL SOLTAR el apuntado. Se cancela en cuanto tocas la cámara")]
        public float aimExitPitchSpeed = 120f;

        /// true mientras se mantiene el apuntado (manda por encima del lock).
        public bool Aiming { get; private set; }
        /// Punto del mundo bajo la retícula (centro de pantalla) → ahí caen los ataques al apuntar.
        public Vector3 AimPoint { get; private set; }

        [Header("Pivote")]
        public float pivotHeight = 1.4f;

        [Header("Sensibilidad / suavizado")]
        public float mouseSensitivity = 0.12f;
        public float stickSensitivity = 180f;
        [Tooltip("Suavizado del giro (mata el jitter del ratón). Más alto = más directo")]
        public float lookResponse = 30f;
        [Tooltip("Suavizado del giro APUNTANDO: más directo, porque ahí el giro ES la puntería")]
        public float aimLookResponse = 45f;
        [Tooltip("Muelle de posición: más bajo = más pegada, más alto = más suave/perezosa")]
        public float positionSmoothTime = 0.08f;
        [Tooltip("Muelle de posición APUNTANDO: casi rígido A PROPÓSITO. Con muelle, girar rápido deja la posición atrás mientras la mirada va al día, y la cámara se columpia — inaceptable en una cámara de precisión")]
        public float aimPositionSmoothTime = 0.015f;

        [Header("Libre")]
        public float freeDistance = 6f;
        public float freeDefaultPitch = 15f;
        public Vector2 pitchClamp = new Vector2(-30f, 60f);
        [Tooltip("Grados/seg a los que la cámara se recoloca detrás al MOVERSE sin tocar el look (trail-behind de BotW). ⚠️ 0 = desactivado, que es como está: en combate cambias de dirección a cada segundo y la cámara persiguiéndote marea. El recentrado manual (lock-on sin enemigo) cubre la necesidad sin pelear contigo")]
        public float autoRecenterSpeed = 0f;
        public float autoRecenterDelay = 1f;
        [Tooltip("Segundos que tarda el recentrado en alcanzar su velocidad máxima (lo hace perezoso)")]
        public float autoRecenterRamp = 0.6f;

        [Header("Cambio de objetivo (flick)")]
        public float switchStickThreshold = 0.7f;   // deflexión del stick derecho
        public float switchMouseThreshold = 45f;    // píxeles de delta en un frame
        public float switchCooldown = 0.35f;

        [Header("Lock")]
        public float lockDistanceBase = 5f;
        [Tooltip("+ distancia por metro de separación jugador↔enemigo (para que quepan los dos)")]
        public float lockDistanceFactor = 0.7f;
        public float lockDistanceMax = 12f;
        [Tooltip("Picado base en lock (grados) para ver a ambos")]
        public float lockBasePitch = 12f;
        [Tooltip("0 = mira al jugador · 1 = mira al enemigo. ~0.45 deja a los dos encuadrados")]
        public float lockLookBias = 0.45f;
        public float lockYawLimit = 35f;      // giro manual permitido sin romper el encuadre
        public float lockPitchLimit = 25f;
        public float lockRecenterSpeed = 45f; // vuelta al encuadre al soltar el look

        [Header("Colisión")]
        public LayerMask collisionMask = ~0;
        public float collisionRadius = 0.25f;

        [Header("Encuadre por habilidad (ver CAMERA-TARGETING §7)")]
        [Tooltip("Respuesta al ENTRAR en el encuadre de una habilidad (alto = seco)")]
        public float shotBlendIn = 14f;
        [Tooltip("Respuesta al VOLVER a la cámara normal (más bajo = regreso suave)")]
        public float shotBlendOut = 5f;

        /// Encuadre que pide una habilidad. Sus valores son **absolutos, no incrementos**: mientras
        /// está activo MANDA él y la habilidad se ve igual vengas de libre, de lock o de apuntado.
        /// (Sumarlo al modo se probó y no vale: partiendo del apuntado —cámara al hombro y cerca— el
        /// mismo encuadre seguía sintiéndose de apuntado.) La transición sí se mezcla, en los dos sentidos.
        public struct Shot
        {
            public float distance;      // distancia absoluta a la que se pone la cámara
            public float pitch;         // picado absoluto (grados), medido sobre el eje de la acción
            public bool fromBehind;
            public float behindSpeed;
            [Tooltip("Eje de la acción (hacia dónde va el dash). Vector3.zero = usar el encarado del cuerpo")]
            public Vector3 direction;
        }

        Shot _shot;
        float _shotTtl;                 // el encuadre caduca si nadie lo renueva → nada lo deja pegado
        float _shotWeight;              // 0 = cámara del modo · 1 = cámara de la habilidad
        const float ShotTtl = 0.25f;

        /// La pide una habilidad cada tick mientras dura su fase (efecto `CameraShot`). El `ttl` sirve
        /// para dejarlo colgado un poco más al acabar: la cámara se queda fuera y vuelve sola.
        public void SetShot(Shot shot, float ttl = ShotTtl) { _shot = shot; _shotTtl = Mathf.Max(0f, ttl); }
        public void ClearShot() => _shotTtl = 0f;

        /// Base horizontal para el movimiento del jugador (siempre relativo a la cámara).
        public Vector3 LookForward => Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
        public Vector3 LookRight   => Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;
        /// Dirección de apuntado 3D (incluye el pitch) → permite atacar/apuntar en vertical.
        public Vector3 AimForward  => Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.forward;

        float _yaw, _pitch;                     // suavizados (los que usa la cámara)
        float _targetYaw, _targetPitch;         // objetivo
        float _lockYawOffset, _lockPitchOffset; // desvío manual dentro del lock
        float _noLookTimer;
        bool _recenterPitch;                    // devolviendo el picado tras soltar el apuntado
        Vector3 _lastFollowPos;
        Vector3 _camVel;                        // estado del muelle de posición
        float _lateral;                         // desplazamiento al hombro (0 salvo apuntando)
        float _switchTimer;
        bool _stickArmed = true;

        /// Recoloca la cámara detrás del jugador (el ZL de Zelda cuando no hay enemigo delante).
        public void RecenterBehind()
        {
            if (follow == null) return;
            _targetYaw = follow.eulerAngles.y;
            _targetPitch = freeDefaultPitch;
            _noLookTimer = 0f;
        }

        void Start()
        {
            if (follow == null) follow = transform;
            if (cam == null) cam = Camera.main;
            _yaw = _targetYaw = follow.eulerAngles.y;
            _pitch = _targetPitch = freeDefaultPitch;
            _lastFollowPos = follow.position;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (follow == null) return;
            if (cam == null) { cam = Camera.main; if (cam == null) return; }

            float dt = Mathf.Max(0.0001f, Time.deltaTime);

            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                bool freeing = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = freeing ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = freeing;
            }

            float playerSpeed = (follow.position - _lastFollowPos).magnitude / dt;
            _lastFollowPos = follow.position;

            Vector2 look = ReadLook();
            bool hasLook = look.sqrMagnitude > 0.000001f;

            var target = targeting != null ? targeting.Current : null;
            bool wasAiming = Aiming;
            Aiming = input != null && input.AimHeld;
            if (wasAiming && !Aiming) _recenterPitch = true;      // soltaste la mira → devuelve el picado
            if (Aiming || hasLook) _recenterPitch = false;        // ...salvo que vuelvas a mandar tú
            // El apuntado MANDA mientras se mantiene; el lock se conserva y vuelve al soltar.
            bool locked = target != null && !Aiming;

            // Flick del stick derecho / ratón → cambiar de objetivo (solo con lock activo).
            if (locked)
            {
                HandleTargetSwitch(dt);
                target = targeting.Current;
                locked = target != null;
            }

            Vector3 pivot = follow.position + Vector3.up * pivotHeight;
            float desiredDistance;
            float lateral = 0f;
            bool lookAlongAim = false;
            Vector3 lookAtPoint;

            if (Aiming)
            {
                // Hombro: giras libre como en modo libre, pero cerca y con retícula.
                _lockYawOffset = 0f; _lockPitchOffset = 0f;
                _targetYaw += look.x;
                _targetPitch = Mathf.Clamp(_targetPitch - look.y, pitchClamp.x, pitchClamp.y);

                desiredDistance = aimDistance;
                lateral = aimShoulder;
                lookAlongAim = true;
                lookAtPoint = pivot;
            }
            else if (locked)
            {
                Vector3 tpos = target.Transform.position + Vector3.up * (pivotHeight * 0.6f);
                Vector3 axis = tpos - pivot; axis.y = 0f;
                float sep = axis.magnitude;

                // Giro manual dentro de límites; vuelve al encuadre al soltar.
                _lockYawOffset   = Mathf.Clamp(_lockYawOffset + look.x, -lockYawLimit, lockYawLimit);
                _lockPitchOffset = Mathf.Clamp(_lockPitchOffset - look.y, -lockPitchLimit, lockPitchLimit);
                if (!hasLook)
                {
                    _lockYawOffset   = Mathf.MoveTowards(_lockYawOffset, 0f, lockRecenterSpeed * dt);
                    _lockPitchOffset = Mathf.MoveTowards(_lockPitchOffset, 0f, lockRecenterSpeed * dt);
                }

                // La cámara se alinea con el eje jugador→enemigo (por eso moverte de lado lo rodea).
                float axisYaw = sep > 0.01f ? Quaternion.LookRotation(axis).eulerAngles.y : _targetYaw;
                // Picado: base + diferencia de altura REAL (mismo suelo → no se dispara hacia arriba).
                float heightDiff = target.Transform.position.y - follow.position.y;
                float basePitch = lockBasePitch - Mathf.Atan2(heightDiff, Mathf.Max(1f, sep)) * Mathf.Rad2Deg;

                _targetYaw = axisYaw + _lockYawOffset;
                _targetPitch = Mathf.Clamp(basePitch + _lockPitchOffset, pitchClamp.x, pitchClamp.y);

                desiredDistance = Mathf.Clamp(lockDistanceBase + sep * lockDistanceFactor,
                                              lockDistanceBase, lockDistanceMax);
                lookAtPoint = Vector3.Lerp(pivot, tpos, lockLookBias);
            }
            else
            {
                _lockYawOffset = 0f; _lockPitchOffset = 0f;

                _targetYaw += look.x;
                _targetPitch = Mathf.Clamp(_targetPitch - look.y, pitchClamp.x, pitchClamp.y);

                // Al salir del apuntado, el picado vuelve solo a su sitio: ahí el pitch era la MIRA,
                // no la cámara, y heredarlo te deja mirando al cielo o al suelo sin haberlo pedido.
                // Solo el picado — el yaw ya viene bien encarado (apuntando, el cuerpo mira a cámara).
                if (_recenterPitch)
                {
                    _targetPitch = Mathf.MoveTowards(_targetPitch, freeDefaultPitch, aimExitPitchSpeed * dt);
                    if (Mathf.Abs(Mathf.DeltaAngle(_targetPitch, freeDefaultPitch)) < 0.1f) _recenterPitch = false;
                }

                // Trail-behind: al moverte sin tocar el look, la cámara vuelve detrás del jugador.
                _noLookTimer = hasLook ? 0f : _noLookTimer + dt;
                if (autoRecenterSpeed > 0f && _noLookTimer > autoRecenterDelay && playerSpeed > 0.5f)
                {
                    // Perezoso: la velocidad entra en rampa y crece con lo desalineada que esté.
                    float ramp = Mathf.Clamp01((_noLookTimer - autoRecenterDelay) / Mathf.Max(0.01f, autoRecenterRamp));
                    float err = Mathf.Abs(Mathf.DeltaAngle(_targetYaw, follow.eulerAngles.y));
                    float speed = autoRecenterSpeed * ramp * Mathf.Clamp01(0.25f + err / 60f);
                    _targetYaw = Mathf.MoveTowardsAngle(_targetYaw, follow.eulerAngles.y, speed * dt);
                }

                desiredDistance = freeDistance;
                lookAtPoint = pivot;
            }

            // Encuadre de habilidad: mientras está activo MANDA él, con valores absolutos, y el peso
            // mezcla la ida y la vuelta. Caduca solo (ShotTtl) si la habilidad deja de renovarlo,
            // así nada puede dejar la cámara enganchada.
            _shotTtl = Mathf.Max(0f, _shotTtl - dt);
            bool shotActive = _shotTtl > 0f;
            float ks = Mathf.Clamp01((shotActive ? shotBlendIn : shotBlendOut) * dt);
            _shotWeight = Mathf.Lerp(_shotWeight, shotActive ? 1f : 0f, ks);

            // El encuadre se coloca sobre el EJE DE LA ACCIÓN (la dirección del dash, fija desde que
            // se lanzó), no sobre el encarado del cuerpo. Dos motivos: así la cámara mira por donde
            // vas de verdad, y se evita el bucle cámara→cuerpo→cámara que hacía temblar el apuntado
            // (ahí el cuerpo persigue a la cámara, así que perseguirlo a él realimenta el temblor).
            Vector3 shotFlat = new Vector3(_shot.direction.x, 0f, _shot.direction.z);
            bool shotHasDir = shotFlat.sqrMagnitude > 0.0001f;

            if (shotActive && _shot.fromBehind)
            {
                if (shotHasDir)
                    _targetYaw = Mathf.MoveTowardsAngle(_targetYaw,
                        Quaternion.LookRotation(shotFlat.normalized).eulerAngles.y, _shot.behindSpeed * dt);
                else if (!Aiming)   // sin eje que seguir, el cuerpo vale — salvo apuntando (bucle)
                    _targetYaw = Mathf.MoveTowardsAngle(_targetYaw, follow.eulerAngles.y, _shot.behindSpeed * dt);
            }

            // Si la acción va inclinada, el picado la acompaña: subir 30° pone la cámara por debajo
            // mirando hacia arriba, en el mismo eje del viaje.
            float shotPitch = _shot.pitch;
            if (shotHasDir)
                shotPitch -= Mathf.Asin(Mathf.Clamp(_shot.direction.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;
            shotPitch = Mathf.Clamp(shotPitch, pitchClamp.x, pitchClamp.y);

            if (_shotWeight > 0.001f)
            {
                desiredDistance = Mathf.Lerp(desiredDistance, _shot.distance, _shotWeight);
                lateral = Mathf.Lerp(lateral, 0f, _shotWeight);   // sin hombro: apuntando también se ve la habilidad
            }

            // Suavizado del giro (jitter del ratón). Apuntando responde más directo: ahí el giro es la mira.
            float k = Mathf.Clamp01((Aiming ? aimLookResponse : lookResponse) * dt);
            _yaw = Mathf.LerpAngle(_yaw, _targetYaw, k);
            _pitch = Mathf.Lerp(_pitch, _targetPitch, k);

            // Posición + colisión con el entorno (ignorando al propio jugador). El picado del encuadre
            // se mezcla AQUÍ, no en _targetPitch: ese es estado persistente del modo y quedaría movido
            // al terminar la habilidad.
            float pitch = Mathf.Lerp(_pitch, shotPitch, _shotWeight);
            Quaternion rot = Quaternion.Euler(pitch, _yaw, 0f);
            Vector3 dir = rot * Vector3.back;
            _lateral = Mathf.Lerp(_lateral, lateral, k);          // desplazamiento al hombro, suavizado
            Vector3 origin = pivot + rot * Vector3.right * _lateral;

            float dist = desiredDistance;
            foreach (var hit in Physics.SphereCastAll(origin, collisionRadius, dir, desiredDistance,
                                                      collisionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root == follow.root) continue;
                if (hit.distance < dist) dist = hit.distance;
            }

            // Apuntando el muelle se anula casi por completo: la cámara va CLAVADA al rig. Con muelle,
            // al girar rápido la posición se queda atrás mientras la mirada (que aquí es rígida, no
            // recalculada hacia un punto) va al día → el personaje se columpia por el encuadre y se
            // pierde la distancia. En libre/lock no pasa porque allí la mirada se recalcula hacia
            // `lookAtPoint` y corrige sola el desfase.
            Vector3 desiredPos = origin + dir * dist;
            float posSmooth = Aiming ? aimPositionSmoothTime : positionSmoothTime;
            cam.transform.position = Vector3.SmoothDamp(cam.transform.position, desiredPos, ref _camVel, posSmooth);

            // Apuntando la cámara mira RECTA a lo largo de la mira (la retícula es el centro);
            // en libre/lock encuadra su punto de interés.
            if (lookAlongAim)
            {
                cam.transform.rotation = rot;
                UpdateAimPoint();
                return;
            }

            Vector3 fwd = lookAtPoint - cam.transform.position;
            if (fwd.sqrMagnitude > 0.0001f)
                cam.transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
        }

        /// Convergencia: lo que hay bajo la RETÍCULA (centro de pantalla). Como la cámara va al hombro,
        /// disparar "recto" no coincidiría con la mira — por eso los ataques apuntan a este punto.
        void UpdateAimPoint()
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out var hit, aimMaxDistance, ~0, QueryTriggerInteraction.Ignore)
                && hit.transform.root != follow.root)
                AimPoint = hit.distance < aimMinDistance ? ray.GetPoint(aimMinDistance) : hit.point;
            else
                AimPoint = ray.GetPoint(aimMaxDistance);
        }

        /// Detecta un "flick" lateral (stick derecho o ratón) para saltar al objetivo de ese lado.
        void HandleTargetSwitch(float dt)
        {
            if (_switchTimer > 0f) { _switchTimer -= dt; return; }

            float flick = 0f;
            var pad = Gamepad.current;
            if (pad != null)
            {
                float sx = pad.rightStick.ReadValue().x;
                if (Mathf.Abs(sx) < 0.35f) _stickArmed = true;                      // rearmar al soltar
                else if (_stickArmed && Mathf.Abs(sx) >= switchStickThreshold)
                { flick = Mathf.Sign(sx); _stickArmed = false; }
            }
            if (flick == 0f)
            {
                var mouse = Mouse.current;
                if (mouse != null && Cursor.lockState == CursorLockMode.Locked)   // con el cursor libre, mover el ratón no debe cambiar de objetivo
                {
                    float mx = mouse.delta.ReadValue().x;
                    if (Mathf.Abs(mx) >= switchMouseThreshold) flick = Mathf.Sign(mx);
                }
            }

            if (flick != 0f && targeting != null && targeting.SwitchTarget(flick))
                _switchTimer = switchCooldown;
        }

        /// El cursor capturado es condición del RATÓN, no del stick: antes se leía el look entero solo
        /// con el cursor bloqueado, así que soltarlo (Esc, o que el editor perdiera el foco) dejaba al
        /// MANDO sin cámara. Con mando el cursor no tiene por qué estar capturado.
        Vector2 ReadLook()
        {
            Vector2 v = Vector2.zero;
            var mouse = Mouse.current;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                v += mouse.delta.ReadValue() * mouseSensitivity;
            var pad = Gamepad.current;
            if (pad != null) v += pad.rightStick.ReadValue() * (stickSensitivity * Time.deltaTime);
            return v;
        }
    }
}
