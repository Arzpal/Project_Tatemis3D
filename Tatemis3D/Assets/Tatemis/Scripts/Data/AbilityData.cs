using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Una habilidad = metadatos + una o varias FASES.
    /// 1 fase = habilidad simple; varias = multi-paso (cargar, agarrar-y-lanzar).
    [CreateAssetMenu(menuName = "Tatemis/Ability", fileName = "Ability")]
    public class AbilityData : ScriptableObject
    {
        public string id;
        public string displayName;
        public AbilitySlot slot = AbilitySlot.Flex;

        [Tooltip("Neutro = sin elemento (siempre x1). Los BÁSICOS van neutros por regla; el kit imbuye el elemento de la especie. Ver Core/Enums.cs")]
        public AbilityElement element = AbilityElement.Especie;

        [Header("Coste y ritmo")]
        public float energyCost = 20f;
        public float cooldown = 1f;

        public List<AbilityPhase> phases = new List<AbilityPhase> { new AbilityPhase() };
    }
}
