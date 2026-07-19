using UnityEngine;

namespace Tatemis
{
    /// Recurso elemental estilo tinta de Splatoon: se gasta con habilidades y regenera por segundo.
    public class ElementalEnergy : MonoBehaviour
    {
        public float max = 100f;
        public float current = 100f;
        public float regenPerSec = 10f;

        float _regenMultiplier = 1f;

        public float Normalized => max > 0f ? current / max : 0f;

        public bool TrySpend(float cost)
        {
            if (current < cost) return false;
            current -= cost;
            return true;
        }

        /// El bloqueo/forma elemental pura sube la regeneración.
        public void SetRegenMultiplier(float m) => _regenMultiplier = Mathf.Max(0f, m);

        void Update()
        {
            current = Mathf.Min(max, current + regenPerSec * _regenMultiplier * Time.deltaTime);
        }
    }
}
