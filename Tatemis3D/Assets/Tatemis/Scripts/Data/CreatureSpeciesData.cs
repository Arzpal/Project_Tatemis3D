using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Definición de una especie: elemento, arquetipo y su cadena de formas evolutivas.
    [CreateAssetMenu(menuName = "Tatemis/Creature Species", fileName = "Creature")]
    public class CreatureSpeciesData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Element element;
        public Archetype archetype;

        [Tooltip("[0] = forma base (bebé) … [n] = forma final")]
        public List<EvolutionForm> forms = new List<EvolutionForm>();
    }

    [System.Serializable]
    public class EvolutionForm
    {
        public string formName;
        public GameObject modelPrefab;          // placeholder por ahora
        public BaseStats stats;                 // define el arquetipo

        [Header("Kit (3 habilidades con coste)")]
        public AbilityData movilidad;
        public AbilityData flex;
        public AbilityData signature;

        [Header("Acción universal (sin coste)")]
        [Tooltip("Ataque básico: cadena gratuita, columna vertebral del combate. Ver GDD §6.3.")]
        public AbilityData basic;

        [Header("Forma elemental pura (bloqueo)")]
        public BlockParams block;

        [Header("Elemento secundario (opcional)")]
        [Tooltip("Muchas criaturas NUNCA lo desbloquean: se quedan en su elemento puro. Déjalo desmarcado en ese caso.")]
        public bool hasSecondaryElement;

        [Tooltip("Solo aplica si 'Has Secondary Element'. Utilidad: usar ese elemento en alguna habilidad. NO cambia debilidades/resistencias.")]
        public Element secondaryElement;

        [Tooltip("2–3 niveles de evolución progresiva dentro de esta forma")]
        public int levelsInForm = 2;

        /// Elemento secundario efectivo, o null si la criatura se queda en su elemento puro.
        public Element? SecondaryElement => hasSecondaryElement ? secondaryElement : (Element?)null;
    }

    [System.Serializable]
    public struct BaseStats
    {
        public float maxHealth;
        public float damage;
        public float moveSpeed;
    }

    /// Ojo: los struct añadidos en el inspector arrancan en default (0). Validar en runtime.
    [System.Serializable]
    public struct BlockParams
    {
        public float energyRegenMultiplier;     // sugerido ~2.5
        public float weaknessDamageMultiplier;  // sugerido 2 (recibe x2 de su debilidad)
    }
}
