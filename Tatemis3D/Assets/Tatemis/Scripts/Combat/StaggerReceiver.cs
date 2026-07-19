using UnityEngine;

namespace Tatemis
{
    /// Va en las criaturas salvajes. Vida + barra de aturdimiento.
    /// Nunca "mueren": al mínimo de vida se agotan y el aturdimiento decide el noqueo.
    public class StaggerReceiver : MonoBehaviour, ICombatant
    {
        public CreatureSpeciesData species;
        public float maxHealth = 100f;
        public float health = 100f;
        public float maxStagger = 100f;
        public float stagger = 0f;

        public bool InKOWindow { get; private set; }

        public Element Element => species != null ? species.element : Element.Naturaleza;
        public Transform Transform => transform;

        public void TakeHit(float damage, float staggerAmount)
        {
            health = Mathf.Max(0f, health - damage);
            stagger = Mathf.Min(maxStagger, stagger + staggerAmount);

            // Las reacciones ambientales llegan con staggerAmount alto -> recompensan el USP.
            if (stagger >= maxStagger || health <= 0f)
                OpenKOWindow();
        }

        /// Intento de noqueo por acción contextual (prompt de finisher, sin botón dedicado).
        /// Prob. sube al bajar la vida y llenar el aturdimiento.
        public bool TryKnockout()
        {
            float byHealth = 1f - health / Mathf.Max(1f, maxHealth);
            float chance = Mathf.Max(Mathf.Lerp(0.15f, 0.95f, byHealth), stagger / maxStagger);
            if (Random.value <= chance) { OpenKOWindow(); return true; }
            return false;
        }

        void OpenKOWindow()
        {
            if (InKOWindow) return;
            InKOWindow = true;
            // TODO(Fase 3): terminar el combate y avisar a CaptureSystem para abrir la ventana temporizada.
            Debug.Log($"[KO] Ventana de captura abierta en {name}");
        }
    }
}
