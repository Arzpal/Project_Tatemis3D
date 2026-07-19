using UnityEngine;

namespace Tatemis
{
    /// Los 2 triángulos elementales. X supera a Y (1.5x daño; Y resiste 0.5x a X).
    /// El 2x solo existe contra la forma elemental pura, y lo aplica BlockParams.weaknessDamageMultiplier.
    /// Naturaleza > Agua > Fuego > Naturaleza   |   Mineral > Rayo > Aire > Mineral
    /// Cruzar de un triángulo al otro = neutral (x1).
    [CreateAssetMenu(menuName = "Tatemis/Type Chart", fileName = "TypeChart")]
    public class TypeChart : ScriptableObject
    {
        public float Multiplier(Element attacker, Element defender)
        {
            if (Beats(attacker, defender)) return 1.5f;
            if (Beats(defender, attacker)) return 0.5f;
            return 1f;
        }

        public static bool Beats(Element a, Element b) =>
            (a == Element.Naturaleza && b == Element.Agua) ||
            (a == Element.Agua       && b == Element.Fuego) ||
            (a == Element.Fuego      && b == Element.Naturaleza) ||
            (a == Element.Mineral    && b == Element.Rayo) ||
            (a == Element.Rayo       && b == Element.Aire) ||
            (a == Element.Aire       && b == Element.Mineral);
    }
}
