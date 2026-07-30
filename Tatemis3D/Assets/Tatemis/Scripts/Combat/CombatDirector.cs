using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// Arranca un encuentro y **acota el tablero** ([ARENA.md](../../docs/ARENA.md)): busca al
    /// jugador y a los salvajes cercanos, abre la `CombatZone` a su alrededor y con ella se revelan
    /// las celdas elementales que van a estar en juego.
    ///
    /// Hoy es **automático al dar Play** (`startOnPlay`), que es lo que hace falta para probar el
    /// terreno sin tener todavía IA que provoque encuentros. Cuando exista la IA, este mismo
    /// componente es el sitio donde engancharla: el disparador cambia, el resto no.
    public class CombatDirector : MonoBehaviour
    {
        [Tooltip("Abrir el combate nada más entrar en Play. Sin IA todavía, es la forma de tener " +
                 "tablero desde el primer frame")]
        public bool startOnPlay = true;
        [Tooltip("A qué distancia del jugador se recluta a un salvaje para el encuentro")]
        public float engageRange = 26f;
        [Tooltip("Rehacer la zona con F5 (útil para reabrirla donde estés sin reconstruir la escena)")]
        public bool allowRestartKey = true;

        [SerializeField] CombatZone zone;

        CreatureController _player;

        void Awake()
        {
            if (zone == null) zone = GetComponentInChildren<CombatZone>();
            CombatServices.Zone = zone;
        }

        void Start() { if (startOnPlay) StartEncounter(); }

        /// Abre el encuentro con quien esté cerca. Si no hay nadie, se abre igual alrededor del
        /// jugador: sin enemigos no hay pelea, pero sí hay tablero que mirar.
        public void StartEncounter()
        {
            if (zone == null) { Debug.LogWarning("[Arena] No hay CombatZone: no se puede abrir el encuentro."); return; }

            _player = FindAnyObjectByType<CreatureController>();
            if (_player == null) { Debug.LogWarning("[Arena] No hay jugador en escena."); return; }

            var combatants = new List<Transform> { _player.transform };
            foreach (var sr in FindObjectsByType<StaggerReceiver>(FindObjectsSortMode.None))
            {
                if (sr.Defeated) continue;
                Vector3 d = sr.transform.position - _player.transform.position; d.y = 0f;
                if (d.sqrMagnitude <= engageRange * engageRange) combatants.Add(sr.transform);
            }

            zone.Open(combatants);
            Debug.Log($"[Arena] Encuentro iniciado con {combatants.Count - 1} salvaje(s).");
        }

        void Update()
        {
            // Reabrir la zona en caliente: al afinar la composición de biomas conviene poder
            // caminar a otro bioma y volver a enmarcar allí sin salir de Play.
            if (!allowRestartKey) return;
            var kb = Keyboard.current;
            if (kb != null && kb.f5Key.wasPressedThisFrame) StartEncounter();
        }
    }
}
