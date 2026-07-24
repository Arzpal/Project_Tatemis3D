using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// HUD de debug con IMGUI (sin montar Canvas): barra de energía del jugador +
    /// vida/aturdimiento del objetivo con lock-on. Añádelo a cualquier GameObject de la escena.
    /// **F1** abre el panel de diagnóstico de input (T0.6): qué acción llega y desde qué dispositivo.
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] ElementalEnergy energy;      // del jugador
        [SerializeField] TargetingSystem targeting;   // del jugador
        [SerializeField] AbilityCaster caster;        // del jugador (cooldowns)
        [SerializeField] CreatureController controller; // del jugador (kit + combo de básicos)
        [SerializeField] PlayerCameraRig camRig;        // para saber si está apuntando
        [SerializeField] InputReader input;             // del jugador (diagnóstico de input)

        void Reset() => AutoWire();
        void Awake() => AutoWire();

        /// Rellena en runtime lo que no esté cableado. Un HUD de debug no debe depender de que alguien
        /// lo conecte: si no, una escena guardada antes de añadir un campo se queda muda y parece roto.
        void AutoWire()
        {
            if (energy == null) energy = Object.FindFirstObjectByType<ElementalEnergy>();
            if (targeting == null) targeting = Object.FindFirstObjectByType<TargetingSystem>();
            if (caster == null) caster = Object.FindFirstObjectByType<AbilityCaster>();
            if (controller == null) controller = Object.FindFirstObjectByType<CreatureController>();
            if (camRig == null) camRig = Object.FindFirstObjectByType<PlayerCameraRig>();
            if (input == null) input = Object.FindFirstObjectByType<InputReader>();
        }

        /// Retícula en el centro de pantalla — solo existe mientras apuntas.
        void DrawReticle()
        {
            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
            const float len = 9f, thick = 2f, gap = 5f;
            Color old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.85f);
            GUI.DrawTexture(new Rect(cx - gap - len, cy - thick * 0.5f, len, thick), Px);
            GUI.DrawTexture(new Rect(cx + gap, cy - thick * 0.5f, len, thick), Px);
            GUI.DrawTexture(new Rect(cx - thick * 0.5f, cy - gap - len, thick, len), Px);
            GUI.DrawTexture(new Rect(cx - thick * 0.5f, cy + gap, thick, len), Px);
            GUI.color = old;
        }

        void OnGUI()
        {
            const float w = 280f, h = 18f, pad = 10f, gap = 4f;
            float y = pad;

            if (energy != null)
            {
                Bar(new Rect(pad, y, w, h), energy.Normalized,
                    $"Energía  {Mathf.RoundToInt(energy.current)}/{Mathf.RoundToInt(energy.max)}",
                    new Color(0.3f, 0.8f, 0.95f));
                y += h + gap;
            }

            if (targeting != null && targeting.Current is StaggerReceiver sr)
            {
                Bar(new Rect(pad, y, w, h), sr.health / Mathf.Max(1f, sr.maxHealth),
                    $"Vida  {Mathf.RoundToInt(sr.health)}", new Color(0.85f, 0.25f, 0.25f));
                y += h + gap;

                Bar(new Rect(pad, y, w, h), sr.stagger / Mathf.Max(1f, sr.maxStagger),
                    $"Aturdimiento  {Mathf.RoundToInt(sr.stagger)}", new Color(0.95f, 0.75f, 0.2f));
                y += h + gap;

                if (sr.Downed)
                {
                    // Lo que decide el jugador ahora: durante la gracia no puede rematar (es el margen
                    // para elegir); después, seguir atacando mata. Ver TAMER.md §4.1.
                    Bar(new Rect(pad, y, w, h), 1f - sr.DownedProgress,
                        sr.InGrace ? "DEBILITADO — decide (invulnerable)" : "DEBILITADO — atacar REMATA",
                        sr.InGrace ? new Color(0.9f, 0.9f, 1f) : new Color(0.8f, 0.35f, 0.35f));
                }
                else if (sr.InKOWindow)
                    GUI.Label(new Rect(pad, y, w, h), "★ Aturdido — ventana abierta");
                // Los estados (Ralentizado, etc.) NO se listan aquí: van como etiqueta anclada sobre
                // el propio enemigo (`StatusController.OnGUI`), así se ven en quien los sufre —tenga
                // lock-on o no— en vez de solo en el objetivo fijado.
            }
            else
            {
                GUI.Label(new Rect(pad, y, w, h), "Sin objetivo — haz lock-on (Q / R3)");
            }

            DrawCooldowns(h, pad, gap);
            if (camRig != null && camRig.Aiming) DrawReticle();
            if (_showInput) DrawInputDiagnostics();
        }

        bool _showInput;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame) _showInput = !_showInput;
        }

        /// Diagnóstico de input (T0.6): para cada acción, si está activa y **de qué dispositivo viene**.
        /// Con esto, verificar el mando es conectar y pulsar todo una vez, no ir adivinando.
        /// El stick derecho aparece aparte porque la cámara lo lee directamente del `Gamepad`, sin
        /// pasar por el `InputReader` — es la única entrada que hoy NO es rebindeable.
        void DrawInputDiagnostics()
        {
            const float w = 320f, rowH = 17f, pad = 10f;
            var map = input != null ? input.Actions : null;
            int rows = (map != null ? map.actions.Count : 0) + 3;

            float x = Screen.width * 0.5f - w * 0.5f;
            float y = pad;
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(x, y, w, rows * rowH + 12f), Px);
            GUI.color = Color.white;
            y += 6f;

            var pad0 = Gamepad.current;
            GUI.Label(new Rect(x + 8f, y, w, rowH),
                pad0 != null ? $"MANDO: {pad0.displayName}" : "MANDO: ninguno detectado"); y += rowH;
            GUI.Label(new Rect(x + 8f, y, w, rowH),
                _rebinding != null ? "► PULSA la tecla/botón nuevo (Esc cancela)"
                                   : "F1 cierra · pulsa todo para verificar · [tec]/[pad] rebindea"); y += rowH + 2f;

            if (map == null) { GUI.Label(new Rect(x + 8f, y, w, rowH), "Sin InputReader cableado"); return; }

            foreach (var a in map.actions)
            {
                bool on = a.IsPressed();
                // De qué control viene AHORA (null si nadie lo toca) → así se ve si llega del mando.
                string src = a.activeControl != null ? a.activeControl.device.shortDisplayName ?? a.activeControl.device.name : "—";
                string val = a.expectedControlType == "Vector2" ? a.ReadValue<Vector2>().ToString("0.00") : (on ? "●" : "·");

                GUI.color = on ? new Color(0.4f, 1f, 0.5f) : new Color(0.75f, 0.75f, 0.75f);
                GUI.Label(new Rect(x + 8f, y, 110f, rowH), a.name);
                GUI.Label(new Rect(x + 118f, y, 90f, rowH), val);
                GUI.Label(new Rect(x + 200f, y, 55f, rowH), src);
                GUI.color = Color.white;

                // Rebinding: el índice 0 es el binding de teclado/ratón y el 1 el de mando (los crea así
                // InputReader.Button). Move es un composite → no se rebindea desde aquí.
                if (_rebinding == null && a.bindings.Count >= 2 && !a.bindings[0].isComposite)
                {
                    if (GUI.Button(new Rect(x + 256f, y, 30f, rowH - 1f), "tec")) BeginRebind(a, 0);
                    if (GUI.Button(new Rect(x + 288f, y, 30f, rowH - 1f), "pad")) BeginRebind(a, 1);
                }
                y += rowH;
            }

            GUI.color = pad0 != null && pad0.rightStick.ReadValue().sqrMagnitude > 0.01f
                ? new Color(0.4f, 1f, 0.5f) : new Color(0.75f, 0.75f, 0.75f);
            GUI.Label(new Rect(x + 8f, y, 110f, rowH), "Cámara (RS)");
            GUI.Label(new Rect(x + 118f, y, 90f, rowH),
                pad0 != null ? pad0.rightStick.ReadValue().ToString("0.00") : "—");
            GUI.Label(new Rect(x + 200f, y, 55f, rowH), "directo");
            GUI.color = Color.white;
            y += rowH;

            if (GUI.Button(new Rect(x + 8f, y, 150f, rowH), "Restaurar por defecto"))
                input.ResetAllBindings();
        }

        InputAction _rebinding;

        /// El rebinding en sí ya vivía en `InputReader` (interactivo + guardado en PlayerPrefs);
        /// esto es solo la UI que faltaba. El menú de settings de verdad es tarea de UI-UX.
        void BeginRebind(InputAction action, int bindingIndex)
        {
            _rebinding = action;
            input.StartRebind(action, bindingIndex, () => _rebinding = null);
        }

        /// Zona inferior-derecha: cooldowns del kit + contador de básicos.
        void DrawCooldowns(float h, float pad, float gap)
        {
            var form = controller?.data?.Form;
            if (caster == null || form == null) return;

            const float w = 220f;
            float x = Screen.width - w - pad;
            float y = Screen.height - pad - (h + gap) * 5f;   // 3 cooldowns + andar + cadena

            CooldownRow(new Rect(x, y, w, h), "Movilidad", form.movilidad); y += h + gap;
            CooldownRow(new Rect(x, y, w, h), "Flex", form.flex);           y += h + gap;
            CooldownRow(new Rect(x, y, w, h), "Signature", form.signature); y += h + gap;

            // Andar actual + deflexión: para verificar las bandas del perfil de locomoción con el stick.
            Bar(new Rect(x, y, w, h), controller.MoveInput01,
                $"{controller.CurrentGait}   {controller.MoveInput01:0.00}",
                new Color(0.45f, 0.75f, 0.6f));
            y += h + gap;

            // Cadena de básicos: qué eslabón toca. El último es el finisher (estalla en su elemento).
            int total = form.basicChain != null ? form.basicChain.Count : 0;
            if (total > 0)
            {
                var next = form.BasicAt(controller.BasicCombo);
                bool finisher = controller.BasicCombo == total - 1;
                Bar(new Rect(x, y, w, h), (controller.BasicCombo + 1f) / total,
                    $"Cadena {controller.BasicCombo + 1}/{total}  {next?.displayName}",
                    finisher ? new Color(0.85f, 0.7f, 0.35f) : new Color(0.75f, 0.75f, 0.45f));
            }
        }

        void CooldownRow(Rect r, string label, AbilityData a)
        {
            if (a == null) { GUI.Label(r, $"{label}: —"); return; }
            float cd = caster.CooldownRemaining(a);
            float frac = a.cooldown > 0.01f ? Mathf.Clamp01(cd / a.cooldown) : 0f;   // 1 = recién lanzada
            bool ready = cd <= 0.01f;
            Bar(r, 1f - frac,
                ready ? $"{label}  listo" : $"{label}  {cd:0.0}s",
                ready ? new Color(0.35f, 0.8f, 0.45f) : new Color(0.5f, 0.55f, 0.9f));
        }

        static Texture2D _px;
        static Texture2D Px
        {
            get
            {
                if (_px == null)
                {
                    _px = new Texture2D(1, 1);
                    _px.SetPixel(0, 0, Color.white);
                    _px.Apply();
                }
                return _px;
            }
        }

        static void Bar(Rect r, float fill01, string label, Color color)
        {
            fill01 = Mathf.Clamp01(fill01);
            Color old = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);            // fondo
            GUI.DrawTexture(r, Px);

            GUI.color = color;                                   // relleno
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * fill01, r.height), Px);

            GUI.color = old;
            GUI.Label(new Rect(r.x + 6f, r.y, r.width, r.height), label);
        }
    }
}
