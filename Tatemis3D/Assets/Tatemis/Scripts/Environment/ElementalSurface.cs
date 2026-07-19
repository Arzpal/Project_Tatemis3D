using UnityEngine;

namespace Tatemis
{
    /// Marca una zona de terreno con su elemento base (río = Agua, lava = Fuego, etc.).
    public class ElementalSurface : MonoBehaviour
    {
        public Element baseElement = Element.Naturaleza;
    }
}
