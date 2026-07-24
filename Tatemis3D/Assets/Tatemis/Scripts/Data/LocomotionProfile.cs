using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **Perfil de locomoción intercambiable** (el que pedía ABILITY-EXTRACTION §8.1): no existe "la
    /// locomoción del jugador" — cada especie se mueve distinto y algunas cambian de modo en pleno
    /// combate. Se selecciona por forma/especie y se sobrescribe por stance.
    ///
    /// **La velocidad la manda el stick, no la inercia:** es lineal con la deflexión (0..1). Aparte,
    /// unos **umbrales visuales** dicen qué ANDAR se muestra a cada tramo (paso corto / caminar /
    /// deslizamiento) — cambian la lectura, **no la velocidad**. Eso da carácter sin volver el control
    /// resbaladizo: soltar el stick para *inmediatamente*, innegociable con lock-on y strafe.
    ///
    /// ⚠️ El deslizamiento de Ottorock es un **andar**, no física: parece que patina, pero responde
    /// 1:1. La única inercia real del juego vive dentro de habilidades concretas (`DashMotion.easeOut`).
    [CreateAssetMenu(menuName = "Tatemis/Locomotion Profile", fileName = "Locomotion")]
    public class LocomotionProfile : ScriptableObject
    {
        /// Un andar = un **estado visual** y la deflexión a la que empieza. **No altera la velocidad**:
        /// solo dice qué animación toca. Dentro de un mismo andar la velocidad sigue subiendo con el
        /// stick (de 0.75 a 1 te deslizas cada vez más rápido, pero se ve igual).
        [System.Serializable]
        public struct Gait
        {
            public string name;
            [Range(0f, 1f), Tooltip("Deflexión del stick a partir de la cual se muestra este andar")]
            public float threshold;
        }

        [Tooltip("En orden de deflexión creciente. Son umbrales VISUALES (qué animación se ve), no escalones de velocidad")]
        public List<Gait> gaits = new List<Gait>
        {
            new Gait { name = "Paso corto",    threshold = 0.1f },
            new Gait { name = "Caminar",       threshold = 0.5f },
            new Gait { name = "Deslizamiento", threshold = 0.75f },
        };

        [Tooltip("Deflexión por debajo de la cual se considera parado (zona muerta del stick)")]
        public float deadzone = 0.08f;

        [Tooltip("Tope de velocidad de esta forma respecto al moveSpeed de la criatura. 1 = pleno · Ottorock en Sujeción = 0.75 (con la roca en brazos no puede deslizarse)")]
        public float maxSpeedMultiplier = 1f;

        [Header("Límites por situación")]
        [Tooltip("Deflexión máxima mientras se bloquea: el stance compromete, pero deja reposicionarse a paso corto")]
        [Range(0f, 1f)] public float blockMaxInput = 0.25f;
        [Tooltip("Multiplicador de velocidad mientras se apunta (el coste del modo aim)")]
        public float aimMultiplier = 0.5f;

        [Header("Respuesta del input")]
        [Tooltip("Segundos de 0 a 1. Corto a propósito: con stick es imperceptible (la deflexión ya es analógica) y con TECLADO es el único sucedáneo de esa finura — por eso el mando es el control estrella")]
        public float inputRamp = 0.16f;

        [Header("Cuerpo")]
        public float turnSmoothTime = 0.12f;
        public float gravity = -25f;
        public float jumpSpeed = 8f;
        [Tooltip("Voladores (Mantagale): la altura es un eje propio y no se asume estar pegado al suelo")]
        public bool freeVerticalAxis = false;

        /// Factor de velocidad (0..1): **lineal con la deflexión**. Los andares no lo tocan — mover el
        /// stick un poco más siempre te mueve un poco más rápido, dentro del andar que sea.
        public float SpeedFactor(float input01)
        {
            input01 = Mathf.Clamp01(input01);
            return input01 <= deadzone ? 0f : input01 * maxSpeedMultiplier;
        }

        /// Andar actual — hoy para el HUD de debug; mañana, para elegir el clip de animación.
        public string GaitName(float input01)
        {
            if (input01 <= deadzone || gaits == null || gaits.Count == 0) return "Quieto";
            for (int i = gaits.Count - 1; i >= 0; i--)
                if (input01 >= gaits[i].threshold) return gaits[i].name;
            return "Quieto";
        }
    }
}
