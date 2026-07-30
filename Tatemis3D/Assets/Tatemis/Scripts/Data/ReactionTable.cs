using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Cómo se SIENTE una reacción (ELEMENTAL-REACTIONS §2): la dominancia es ruidosa (fogonazo),
    /// la combinación es suave (zona persistente). La respuesta del terreno no es un estilo aparte:
    /// es la MISMA entrada de su dominancia espejo con `friendlyFire` — la excepción vive en el dato.
    public enum ReactionStyle { Dominancia, Combinacion }

    /// Una casilla de la matriz 6×6, con su payload de caja gris. El par es ORDENADO (R4): la misma
    /// pareja resuelve distinto según quién es terreno y quién entra — (Agua, Fuego) = golpe de vapor
    /// sin bando (respuesta del terreno) y (Fuego, Agua) = el mismo vapor como dominancia estándar.
    [System.Serializable]
    public class ReactionEntry
    {
        [Header("Par ordenado (R4)")]
        public Element terrain;
        public Element incoming;
        public string reactionName = "Reacción";
        public ReactionStyle style = ReactionStyle.Dominancia;
        [Tooltip("Respuesta del terreno: la reacción es plena y SIN BANDO — puede golpear a quien la " +
                 "provoca (excepción a R6). Solo en las 6 entradas de gatillo inverso")]
        public bool friendlyFire;
        [Tooltip("El elemento que MANDA en esta reacción. Su daño va con este tipo y pasa por la tabla " +
                 "(ER §0.2). Solo lo usan las dominancias: las combinaciones hacen daño NEUTRO porque " +
                 "ahí no manda ninguno de los dos")]
        public Element dominantElement;

        /// **Sin bando** = el estallido golpea a todos, incluido quien lo provocó (ER §0.7).
        /// Lo son la **respuesta del terreno** (`friendlyFire`) y **todas las combinaciones**: nadie es
        /// dueño de una mezcla donde ningún elemento se impone.
        public bool NoBando => friendlyFire || style == ReactionStyle.Combinacion;

        /// El tipo con el que pega: el del dominante si manda alguien, **neutro** si es combinación.
        public Element? DamageElement => style == ReactionStyle.Dominancia ? dominantElement : (Element?)null;

        [Header("Fogonazo (reacciones): AoE instantáneo")]
        public float burstDamage;
        [Tooltip("0 = igual al daño. **El aturdimiento es proporcional al daño** por dictado (GDD §8.2), " +
                 "así que dejarlo en 0 es lo normal: solo se autora aparte si una pareja necesita " +
                 "romper esa proporción a propósito")]
        public float burstStagger;
        /// Aturdimiento efectivo: proporcional al daño salvo que se autore aparte.
        public float Stagger => burstStagger > 0f ? burstStagger : burstDamage;
        [Tooltip("Empuje radial. 0 = sin empuje")]
        public float burstKnockback;
        [Tooltip("Elevación vertical del fogonazo (u/s). Es el GÉISER: abre el juego aéreo y deja que " +
                 "un melee alcance a un volador. 0 = no eleva")]
        public float burstLift;
        [Tooltip("Estado que aplica el fogonazo. `burstStatusTail` es lo que tarda en apagarse fuera")]
        public StatusType burstStatus = StatusType.None;
        public float burstStatusSeconds = 1f;

        [Header("Zona temporal (la duración base escala con el impacto, R2)")]
        [Tooltip("0 = solo fogonazo, no deja zona")]
        public float baseDuration;
        [Tooltip("Estado a quien esté DENTRO, re-aplicado por pulsos: al salir DECAE solo (§0.3)")]
        public StatusType zoneStatus = StatusType.None;
        [Tooltip("Segundo estado, para las parejas que aplican los DOS verbos: la corriente arrastra " +
                 "(Agua) y encima interfiere (Rayo). None = solo uno")]
        public StatusType zoneStatus2 = StatusType.None;
        [Tooltip("Aplicar el estado UNA sola vez por objetivo, en vez de re-aplicarlo cada pulso. " +
                 "Obligatorio para los CC duros: re-aplicar parálisis cada 0,5 s deja clavado a quien " +
                 "esté dentro y ER §4 dice 'a quien ENTRE'. Los estados blandos (slow, quemadura) van sin él")]
        public bool zoneStatusOnce;
        [Tooltip("Cuánto dura cada re-aplicación del estado (algo mayor que el pulso, para que no parpadee)")]
        public float statusSeconds = 1.2f;
        [Tooltip("DoT del estado aplicado (solo lo usa Quemadura)")]
        public float statusTickDamage;
        [Tooltip("Daño directo por pulso a quien esté dentro (roca al rojo, púas…). Sin estado de por medio")]
        public float zoneTickDamage;
        [Tooltip("La zona cubre un área mayor que el impacto que la creó (rasgo de familia de Aire)")]
        public float radiusScale = 1f;

        [Header("Mecánicas propias de una sola pareja (ER §4)")]
        [Tooltip("CORRIENTE / COLADA: la zona tiene DIRECCIÓN, tomada del vector del golpe que la creó. " +
                 "En la corriente es a dónde arrastra; en la colada, hacia dónde repta")]
        public bool directional;
        [Tooltip("CORRIENTE: velocidad a la que arrastra a lo que pise dentro (u/s)")]
        public float dragSpeed;
        [Tooltip("COLADA: velocidad a la que la zona AVANZA (u/s). 0 = zona quieta. Se implementa como " +
                 "cadena de discos: cada pulso nace uno más adelante y muere el más viejo")]
        public float advanceSpeed;
        [Tooltip("INCENDIO: se propaga a las celdas vecinas que sean de este elemento. None = no se " +
                 "propaga. El propio parche define hasta dónde llega, sin tope inventado")]
        public Element propagateThrough = Element.Naturaleza;
        public bool propagates;
        [Tooltip("MINI EXPLOSIONES: cada impacto dentro de la zona lo sigue un eco que repite este % " +
                 "del daño original. 0 = sin eco. ⚠️ El eco NO irradia al terreno (R1: no se encadena)")]
        [Range(0f, 1f)] public float echoPercent;
        [Tooltip("AMARRA: fuerza del tirón que devuelve al punto donde te alcanzó (u/s). 0 = no ata")]
        public float tetherPull;
        [Tooltip("MECHA: no hace nada mientras dura y DEFLAGRA al final, en todo el volumen y aire " +
                 "incluido. Invierte el delay: aquí lo peligroso es el final")]
        public bool deflagrates;
        [Tooltip("VELO DE ESPORAS: detona ahí el proyectil elemental que la cruce (se atraviesa andando)")]
        public bool blocksProjectiles;
        [Tooltip("NIEBLA: a través de ella no se puede FIJAR objetivo (mantener el que ya tenías, sí)")]
        public bool breaksTargeting;

        [Header("Caja gris")]
        public Color debugColor = Color.white;
    }

    /// La matriz 6×6 como DATOS (ELEMENTAL-REACTIONS §7): 6 dominancias + 6 respuestas del terreno
    /// (mismo efecto, gatillo inverso, `friendlyFire`) + 9 combinaciones × 2 direcciones = 30 entradas.
    /// Los únicos pares sin entrada son mismo-elemento (nada) y el terreno neutro (R5, ni llega aquí).
    /// Añadir/quitar/afinar reacciones = editar la tabla, sin tocar código.
    [CreateAssetMenu(menuName = "Tatemis/Reaction Table", fileName = "ReactionTable")]
    public class ReactionTable : ScriptableObject
    {
        public List<ReactionEntry> entries = new List<ReactionEntry>();

        /// Resuelve el par ordenado (elemento del TERRENO, elemento ENTRANTE). Par sin entrada =
        /// sin reacción (el impacto ocurre igual). Búsqueda lineal: son 30 entradas.
        public bool TryResolve(Element terrain, Element incoming, out ReactionEntry entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e != null && e.terrain == terrain && e.incoming == incoming) { entry = e; return true; }
            }
            entry = null;
            return false;
        }
    }
}
