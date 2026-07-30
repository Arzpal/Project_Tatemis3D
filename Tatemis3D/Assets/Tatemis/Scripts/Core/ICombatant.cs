using UnityEngine;

namespace Tatemis
{
    /// Cualquier cosa que puede recibir golpes en combate (criatura activa o salvaje).
    public interface ICombatant
    {
        Element Element { get; }
        Transform Transform { get; }
        void TakeHit(float damage, float stagger);

        /// ¿Es un ser vivo, o solo algo golpeable? El nodo de cristal es `ICombatant` únicamente para
        /// que las hitboxes lo reporten y lo puedan romper: no debe fijarlo el lock-on, no debe atraer
        /// el auto-encarado y no debe llevar estados encima ("Paralizado" flotando sobre una piedra).
        bool IsCreature { get; }

        /// ¿`TakeHit` le resta vida de verdad? Sirve para **no mentir en el feedback**: pintar un
        /// número de daño sobre alguien que no lo ha recibido es peor que no pintar nada, porque
        /// confirma en falso lo que se está probando.
        bool TakesDamage { get; }
    }
}
