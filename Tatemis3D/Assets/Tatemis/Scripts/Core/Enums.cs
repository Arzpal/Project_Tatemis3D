namespace Tatemis
{
    public enum Element { Naturaleza, Agua, Fuego, Mineral, Rayo, Aire }

    public enum Archetype { Coloso, Luchador, Ejecutor, Zonificador, Controlador }

    public enum AbilitySlot { Movilidad, Flex, Signature }

    /// Con qué elemento golpea una habilidad. **Los ataques básicos son Neutro por regla de diseño**
    /// (ver GDD §7): si el básico —la columna vertebral del combate— llevara elemento, pelear contra
    /// alguien que te supera sería imposible en vez de difícil. El elemento es de las habilidades.
    public enum AbilityElement { Neutro, Especie, Secundario }

    /// Estados temporales. Los seis primeros **son los verbos de los elementos**
    /// ([ELEMENTAL-REACTIONS §0.1](../../../../docs/ELEMENTAL-REACTIONS.md)) — uno por elemento, sin
    /// solaparse, y esa correspondencia 1:1 es lo que hace que el jugador aprenda el sistema:
    /// si sabe qué le hace el Rayo, sabe qué le hace *cualquier* cosa de Rayo.
    ///
    ///   · **Quemadura**  → Fuego     · *propaga*: arde por pulsos y se extiende
    ///   · **Arrastrado** → Agua      · *corriente*: te lleva mientras estés dentro
    ///   · **Enraizado**  → Naturaleza· *entrelaza*: no te mueves, pero SÍ atacas y bloqueas
    ///   · **Pesado**     → Mineral   · *pesa*: ralentiza y te ancla al suelo (más gravedad)
    ///   · **Interferido**→ Rayo      · *paraliza*: ticks que cortan el ARRANQUE de tus ataques
    ///   · **Cegado**     → Aire      · *nubeliza*: te tapa la visión y no puedes fijar objetivo
    ///
    /// Los tres últimos no son de ningún elemento: son utilidades del kit.
    public enum StatusType
    {
        None,
        Quemadura, Arrastrado, Enraizado, Pesado, Interferido, Cegado,
        Ralentizado, Vulnerable, Aturdido,
    }
}
