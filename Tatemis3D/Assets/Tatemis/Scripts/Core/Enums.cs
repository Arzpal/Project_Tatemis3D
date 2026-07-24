namespace Tatemis
{
    public enum Element { Naturaleza, Agua, Fuego, Mineral, Rayo, Aire }

    public enum Archetype { Guardian, Luchador, Ejecutor, Zonificador, Controlador }

    public enum AbilitySlot { Movilidad, Flex, Signature }

    /// Con qué elemento golpea una habilidad. **Los ataques básicos son Neutro por regla de diseño**
    /// (ver GDD §7): si el básico —la columna vertebral del combate— llevara elemento, pelear contra
    /// alguien que te supera sería imposible en vez de difícil. El elemento es de las habilidades.
    public enum AbilityElement { Neutro, Especie, Secundario }

    public enum StatusType { None, Quemadura, Ralentizado, Vulnerable, Aturdido }
}
