using UnityEngine;

namespace Tatemis
{
    /// Cualquier cosa que puede recibir golpes en combate (criatura activa o salvaje).
    public interface ICombatant
    {
        Element Element { get; }
        Transform Transform { get; }
        void TakeHit(float damage, float stagger);
    }
}
