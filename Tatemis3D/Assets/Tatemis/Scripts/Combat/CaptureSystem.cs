using UnityEngine;

namespace Tatemis
{
    /// Al abrirse la ventana de KO: lanzar Sludge -> probabilidad de resistir -> Tótem.
    public class CaptureSystem : MonoBehaviour
    {
        public float windowSeconds = 4f;

        /// Devuelve true si la captura tiene éxito.
        public bool TryCapture(StaggerReceiver target)
        {
            if (target == null || !target.InKOWindow) return false;

            // Cuanta menos vida, menos probabilidad de resistir.
            float resistChance = Mathf.Clamp01(target.health / Mathf.Max(1f, target.maxHealth)) * 0.2f;
            bool captured = Random.value > resistChance;

            // TODO(Fase 3): crear el Tótem (CreatureInstance) y añadirlo al TeamManager si captured.
            Debug.Log($"[Captura] {(captured ? "Éxito" : "Se resistió")} sobre {target.name}");
            return captured;
        }
    }
}
