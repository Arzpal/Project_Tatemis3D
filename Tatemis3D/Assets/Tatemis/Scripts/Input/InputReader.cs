using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// Input con el new Input System, construido en código (sin depender de un asset .inputactions).
    /// - Teclado/ratón + CUALQUIER gamepad (bindings <Gamepad>/... son agnósticos de marca).
    /// - Rebinding en runtime + guardado/carga de overrides -> listo para un menú de settings.
    ///
    /// Uso desde un menú de settings:
    ///   var jump = inputReader.Actions.FindAction("Jump");
    ///   inputReader.StartRebind(jump, bindingIndex: 0);            // 0 = teclado, 1 = gamepad
    ///   string label = jump.GetBindingDisplayString(0);           // para mostrar en la UI
    ///   inputReader.ResetAllBindings();                           // "restaurar por defecto"
    public class InputReader : MonoBehaviour
    {
        public const int AbilityCount = 3;   // Movilidad / Flex / Signature
        const string PrefsKey = "tatemis.bindings";

        InputActionMap _map;
        InputAction _move, _jump, _lockOn, _contextual, _block, _swap, _basic;
        readonly InputAction[] _abilities = new InputAction[AbilityCount];

        // --- API que consume CreatureController (se lee en el frame actual) ---
        public Vector2 Move => _move.ReadValue<Vector2>();
        public bool JumpPressed => _jump.WasPressedThisFrame();
        public bool LockOnPressed => _lockOn.WasPressedThisFrame();
        public bool BasicPressed => _basic.WasPressedThisFrame();
        public bool ContextualPressed => _contextual.WasPressedThisFrame();
        public bool BlockHeld => _block.IsPressed();
        public bool SwapPressed => _swap.WasPressedThisFrame();
        public bool AbilityPressed(int i) => (uint)i < AbilityCount && _abilities[i].WasPressedThisFrame();

        /// Expuesto para el menú de settings (rebinding, mostrar bindings, etc.).
        public InputActionMap Actions => _map;

        void Awake()
        {
            _map = new InputActionMap("Player");

            // Movimiento: WASD (composite) + stick izquierdo.
            _move = _map.AddAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                 .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                 .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddBinding("<Gamepad>/leftStick").WithGroup("Gamepad");

            // Acciones de botón: cada una con binding de teclado (índice 0) y gamepad (índice 1).
            // Mapeo de mando según GDD §6.3.
            _jump       = Button("Jump",       "<Keyboard>/space",     "<Gamepad>/buttonSouth");       // A
            _lockOn     = Button("LockOn",     "<Keyboard>/q",         "<Gamepad>/rightStickPress");   // R3
            _basic      = Button("Basic",      "<Mouse>/leftButton",   "<Gamepad>/buttonWest");        // X — ataque básico (sin coste)
            _contextual = Button("Contextual", "<Keyboard>/f",         "<Gamepad>/buttonNorth");       // Y — finisher/Sludge/interactuar
            _block      = Button("Block",      "<Keyboard>/leftShift", "<Gamepad>/leftTrigger");       // LT (mantener)
            _swap       = Button("Swap",       "<Keyboard>/tab",       "<Gamepad>/dpad/up");           // D-pad

            _abilities[0] = Button("Ability1", "<Keyboard>/1", "<Gamepad>/buttonEast");     // Movilidad (B)
            _abilities[1] = Button("Ability2", "<Keyboard>/2", "<Gamepad>/rightShoulder"); // Flex (RB)
            _abilities[2] = Button("Ability3", "<Keyboard>/3", "<Gamepad>/rightTrigger");  // Signature (RT)

            LoadBindingOverrides();
        }

        InputAction Button(string name, string keyboardPath, string gamepadPath)
        {
            var a = _map.AddAction(name, InputActionType.Button);
            a.AddBinding(keyboardPath).WithGroup("Keyboard&Mouse");
            a.AddBinding(gamepadPath).WithGroup("Gamepad");
            return a;
        }

        void OnEnable() => _map.Enable();
        void OnDisable() => _map.Disable();

        // ---------------- Rebinding (para el menú de settings) ----------------

        /// Inicia un rebinding interactivo: el siguiente control que pulse el jugador queda asignado.
        public void StartRebind(InputAction action, int bindingIndex, Action onFinished = null)
        {
            if (action == null) return;
            _map.Disable();
            action.PerformInteractiveRebinding(bindingIndex)
                  .WithControlsExcluding("<Mouse>/position")
                  .WithControlsExcluding("<Mouse>/delta")
                  .WithCancelingThrough("<Keyboard>/escape")
                  .OnMatchWaitForAnother(0.1f)
                  .OnCancel(op => { op.Dispose(); _map.Enable(); onFinished?.Invoke(); })
                  .OnComplete(op => { op.Dispose(); _map.Enable(); SaveBindingOverrides(); onFinished?.Invoke(); })
                  .Start();
        }

        public void SaveBindingOverrides()
        {
            PlayerPrefs.SetString(PrefsKey, _map.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }

        public void LoadBindingOverrides()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
                _map.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PrefsKey));
        }

        public void ResetAllBindings()
        {
            _map.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(PrefsKey);
        }
    }
}
