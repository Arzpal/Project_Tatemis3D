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

        [Tooltip("Cómo se mueve ESTA forma/stance: andares, topes y respuesta. Sin él, velocidad lineal simple. Ver ABILITY-EXTRACTION §8.1")]
        public LocomotionProfile locomotion;

        [Header("Kit (3 habilidades con coste)")]
        public AbilityData movilidad;
        public AbilityData flex;
        public AbilityData signature;

        [Header("Acción universal (sin coste)")]
        [Tooltip("Cadena de básicos en orden: cada golpe encadenado avanza un eslabón y al terminarla vuelve al primero. Se rompe sola si dejas de pegar. El último eslabón es el finisher (Ottorock: estalla en su elemento). Ver GDD §6.3.")]
        public List<AbilityData> basicChain = new List<AbilityData>();

        /// Eslabón que toca según cuántos básicos llevas encadenados (cicla al completar la cadena).
        public AbilityData BasicAt(int comboIndex) =>
            basicChain == null || basicChain.Count == 0
                ? null
                : basicChain[((comboIndex % basicChain.Count) + basicChain.Count) % basicChain.Count];

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
