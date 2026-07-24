using UnityEngine;

namespace Tatemis
{
    /// Punto único donde un golpe se convierte en FEEDBACK (T1.5): destello en el cuerpo golpeado
    /// + número de daño. Lo llama `DealDamage`, el único sitio donde nace el daño.
    ///
    /// La pieza clave es el **peso** del golpe (0..1), que mezcla dos cosas y alimenta a los dos canales:
    ///  ① cuánto pega ESTE golpe para lo que suele pegar ESE monstruo (daño ÷ daño base de su forma),
    ///  ② lo avanzado que va el combo sobre el objetivo — el mismo multiplicador que sube el aturdimiento.
    /// Así un jab suelto es pequeño y seco, y un mordisco al final de una cadena es grande y destella largo.
    /// T1.6 debería escalar el hitstop con este mismo peso: una sola noción de "contundencia".
    public static class HitFeedback
    {
        /// Pegar este múltiplo del daño base de la criatura = peso 1 (el golpe más gordo de un kit).
        const float HeavyRatio = 5f;
        /// Cuánto manda el daño frente al combo en la mezcla.
        const float DamageShare = 0.6f;

        public static float Weight(float damage, float reference, ICombatant target)
        {
            float dmg01 = reference > 0.01f
                ? Mathf.Clamp01((damage / reference - 1f) / (HeavyRatio - 1f))
                : 0f;

            float combo01 = 0f;
            if (target is StaggerReceiver sr && sr.comboMax > 1f)
                combo01 = Mathf.Clamp01((sr.ComboMultiplier - 1f) / (sr.comboMax - 1f));

            // Sin daño de referencia (un objetivo sin forma) el combo manda solo.
            float share = reference > 0.01f ? DamageShare : 0f;
            return Mathf.Clamp01(dmg01 * share + combo01 * (1f - share));
        }

        /// `knockDir` = hacia dónde sale despedido el objetivo (el forward del atacante, la misma
        /// fuente que usa el efecto Knockback) → el número viaja con él en vez de subir recto.
        public static void Report(ICombatant target, float damage, float multiplier,
                                  Vector3 point, Vector3 knockDir, float weight01)
        {
            if (target == null) return;
            HitFlash.Play(target.Transform, multiplier, weight01);
            DamagePopups.Spawn(point, damage, multiplier, knockDir, weight01);
            ImpactVFX.Spawn(point, multiplier, weight01);   // pop en el punto de impacto (T1.6)
            ImpactFeel.Impact(weight01);   // hitstop + shake, con el MISMO peso (T1.6)
        }
    }
}
