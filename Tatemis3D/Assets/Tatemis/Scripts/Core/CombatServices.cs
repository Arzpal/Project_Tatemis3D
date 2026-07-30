namespace Tatemis
{
    /// Localizador simple de servicios de combate para que los efectos de habilidad
    /// alcancen la tabla de tipos y el sistema de reacciones sin acoplarse a la escena.
    /// Se puede reemplazar por inyección de dependencias más adelante.
    public static class CombatServices
    {
        public static TypeChart TypeChart { get; set; }
        public static ReactionSystem Reactions { get; set; }
        /// El tablero elemental del mundo (ARENA §1). Puede ser null: sin rejilla, el sistema de
        /// reacciones cae al método antiguo de buscar `ElementalSurface` bajo el punto.
        public static ElementalGrid Grid { get; set; }
        /// La zona acotada del encuentro en curso, o null si no hay combate abierto.
        public static CombatZone Zone { get; set; }
    }
}
