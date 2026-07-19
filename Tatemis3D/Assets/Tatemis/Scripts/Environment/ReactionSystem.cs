using UnityEngine;

namespace Tatemis
{
    /// "Manchado" de terreno: si el elemento que mancha SUPERA al elemento base del suelo,
    /// se dispara una reacción (AoE). Una reacción por tipo de terreno -> legible.
    public class ReactionSystem : MonoBehaviour
    {
        [SerializeField] TypeChart chart;
        public GameObject reactionHazardPrefab;   // AoE placeholder

        void Awake()
        {
            if (chart != null && CombatServices.TypeChart == null)
                CombatServices.TypeChart = chart;
            CombatServices.Reactions = this;
        }

        public void Stain(Vector3 pos, float radius, Element stainElement)
        {
            if (chart == null) return;
            var surface = SurfaceAt(pos);
            if (surface == null) return;

            if (chart.Multiplier(stainElement, surface.baseElement) > 1f)
                SpawnReaction(pos, radius, stainElement);
        }

        ElementalSurface SurfaceAt(Vector3 pos)
        {
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out var hit, 5f))
                return hit.collider.GetComponentInParent<ElementalSurface>();
            return null;
        }

        void SpawnReaction(Vector3 pos, float radius, Element stainElement)
        {
            // TODO(Fase 2): instanciar el AoE y aplicar daño/estado + MUCHO aturdimiento a quien entre.
            Debug.Log($"[Reacción] {stainElement} sobre el terreno en {pos} (r={radius})");
            if (reactionHazardPrefab != null)
                Instantiate(reactionHazardPrefab, pos, Quaternion.identity);
        }
    }
}
