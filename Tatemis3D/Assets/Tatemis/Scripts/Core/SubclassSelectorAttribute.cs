using UnityEngine;

namespace Tatemis
{
    /// Marca un campo [SerializeReference] para que el Inspector muestre un dropdown
    /// con los tipos concretos disponibles. Uso: [SerializeReference, SubclassSelector].
    public class SubclassSelectorAttribute : PropertyAttribute { }
}
