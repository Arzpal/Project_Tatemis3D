using UnityEngine;
using UnityEngine.Serialization;

namespace Tatemis
{
    /// Marca una zona de terreno con su elemento — FIJO, asignado en escena (río = Agua, lava =
    /// Fuego…). El terreno es el tablero y NO MUDA JAMÁS (ELEMENTAL-REACTIONS §1): sin
    /// `currentState`, sin excepciones. Todo lo que pasa encima son efectos temporales aparte.
    public class ElementalSurface : MonoBehaviour
    {
        [FormerlySerializedAs("baseElement")]
        public Element element = Element.Naturaleza;
    }
}
