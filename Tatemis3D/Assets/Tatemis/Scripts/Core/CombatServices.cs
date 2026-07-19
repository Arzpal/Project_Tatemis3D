namespace Tatemis
{
    /// Localizador simple de servicios de combate para que los efectos de habilidad
    /// alcancen la tabla de tipos y el sistema de reacciones sin acoplarse a la escena.
    /// Se puede reemplazar por inyección de dependencias más adelante.
    public static class CombatServices
    {
        public static TypeChart TypeChart { get; set; }
        public static ReactionSystem Reactions { get; set; }
    }
}
