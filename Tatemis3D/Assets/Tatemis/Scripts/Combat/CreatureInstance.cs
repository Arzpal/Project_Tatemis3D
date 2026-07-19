using System;

namespace Tatemis
{
    /// Instancia viva de una criatura del equipo: qué especie, en qué forma, su progreso.
    [Serializable]
    public class CreatureInstance
    {
        public CreatureSpeciesData species;
        public int formIndex;        // forma evolutiva actual
        public int level;            // nivel dentro de la forma (evolución progresiva)
        public float evoProgress;    // avance de mini-misiones -> sube nivel/forma

        public EvolutionForm Form =>
            species != null && formIndex >= 0 && formIndex < species.forms.Count
                ? species.forms[formIndex]
                : null;

        public Element Element => species != null ? species.element : Element.Naturaleza;
    }
}
