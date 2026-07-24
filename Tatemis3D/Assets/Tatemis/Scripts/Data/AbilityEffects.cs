using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Resolución de UN golpe: multiplicador de tipo, daño/aturdimiento y feedback. Lo comparten
    /// todos los efectos que pegan (contacto directo y estallidos) para que el daño se calcule y se
    /// lea igual venga de donde venga. `element` null = golpe neutro (x1, sin reacción).
    static class DamageResolver
    {
        public static void Apply(AbilityCtx c, ICombatant t, Element? element,
                                 float damage, float stagger, Vector3 point)
        {
            if (t == null) return;

            float mult = element.HasValue && CombatServices.TypeChart != null
                ? CombatServices.TypeChart.Multiplier(element.Value, t.Element)
                : 1f;
            // Vulnerable: el objetivo recibe más daño. Es lo que hace que un debuff se NOTE en caja gris.
            var status = t.Transform != null ? t.Transform.root.GetComponent<StatusController>() : null;
            if (status != null) mult *= status.DamageTakenMultiplier;

            float dealt = damage * mult;
            t.TakeHit(dealt, stagger * mult);   // sube el combo del objetivo ANTES de pesar el golpe

            if (point == Vector3.zero) point = t.Transform.position + Vector3.up * 1.2f;
            float reference = c.caster != null && c.caster.data != null && c.caster.data.Form != null
                ? c.caster.data.Form.stats.damage
                : 0f;
            Vector3 knockDir = c.caster != null ? c.caster.transform.forward : Vector3.zero;
            HitFeedback.Report(t, dealt, mult, point, knockDir, HitFeedback.Weight(dealt, reference, t));

            // Conectar te sostiene un instante en el aire: es lo que deja rematar arriba en vez de
            // desplomarte a media cadena.
            if (c.caster != null) c.caster.NotifyHitLanded();
        }
    }

    // ============ PAYLOAD (causan la consecuencia) ============

    [System.Serializable]
    public class DealDamage : AbilityEffect
    {
        public float amount = 10f;
        public float staggerAmount = 5f;
        [Tooltip("OnCast = al objetivo del lock-on · OnHit = a lo que golpee un carrier (hitbox/proyectil/zona)")]
        public ApplyWhen when = ApplyWhen.OnCast;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Apply(c, c.lockTarget, Vector3.zero); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Apply(c, hit.target, hit.point); }

        void Apply(AbilityCtx c, ICombatant t, Vector3 point) =>
            DamageResolver.Apply(c, t, c.element, amount, staggerAmount, point);
    }

    /// **Estallido elemental.** Revienta en un radio con el elemento de la ESPECIE, pase lo que pase
    /// con el elemento del golpe: así un básico neutro puede rematar la cadena estallando en su
    /// elemento (Ottorock), y el mordisco/la embestida detonan al conectar y al aterrizar.
    /// TODO(Fase 2): al reescribir el sistema elemental, este es el punto donde el estallido irradia
    /// su elemento al TERRENO donde cae y dispara la reacción (ELEMENTAL-REACTIONS §1 ②, R1).
    [System.Serializable]
    public class ElementalBurst : AbilityEffect
    {
        public enum Timing { OnCast, OnHit, OnPhaseEnd }

        public float radius = 2.5f;
        public float damage = 14f;
        public float staggerAmount = 14f;
        [Tooltip("Unidades por delante del caster donde revienta. 0 = a sus pies (embestida: donde cae)")]
        public float forwardOffset = 0f;
        [Tooltip("OnCast = al empezar la fase · OnHit = al conectar el golpe · OnPhaseEnd = al terminarla (fin del dash)")]
        public Timing when = Timing.OnHit;
        [Tooltip("Reportar los alcanzados como impactos, para poder engancharles payloads (ApplyStatus, etc.). ⚠️ NO activarlo si la fase ya tiene un DealDamage de otro actuador: los payloads son de la FASE, no del actuador, y ese daño se cobraría dos veces. Para eso, dale al estallido su propia fase")]
        public bool reportHits = false;
        [Tooltip("Segundos de hitstop del estallido EN SÍ, aparte del de su daño. Un estallido es un evento gordo aunque quite poca vida. 0 = solo el normal")]
        public float hitstop = 0f;

        // El estado va en el blackboard del contexto, NO en un campo: la instancia del efecto vive
        // dentro del AbilityData (un asset) y la comparten todas las activaciones.
        [System.NonSerialized] string _firedKey;
        string FiredKey => _firedKey ??= "burst.fired." + GetHashCode();

        public override void OnEnter(AbilityCtx c)
        {
            c.bb[FiredKey] = false;
            if (when == Timing.OnCast) Burst(c);
        }

        /// OnHit llega UNA VEZ POR ENEMIGO golpeado: sin esta guarda, un golpe a tres maniquíes
        /// provocaría tres estallidos. Un impacto = un estallido.
        public override void OnHit(AbilityCtx c, HitInfo hit)
        {
            if (when != Timing.OnHit) return;
            if (c.bb.TryGetValue(FiredKey, out var o) && o is bool fired && fired) return;
            Burst(c);
        }

        public override void OnExit(AbilityCtx c) { if (when == Timing.OnPhaseEnd) Burst(c); }

        void Burst(AbilityCtx c)
        {
            if (c.caster == null) return;
            c.bb[FiredKey] = true;
            Element element = c.caster.Element;   // el de la especie, no el del golpe

            Vector3 center = c.caster.transform.position
                           + c.caster.transform.forward * forwardOffset
                           + Vector3.up * 0.5f;
            HitboxDebug.Show(center, radius, 0.22f, HitboxDebug.ColorOf(element));
            ImpactVFX.Spawn(center, 1f, 1f);
            // El parón va aquí, no por enemigo alcanzado: el estallido es UN evento, y si no alcanza a
            // nadie también ha ocurrido.
            if (hitstop > 0f) ImpactFeel.Shockwave(hitstop);

            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(center, radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                if (t.Transform.root == c.caster.transform.root) continue;   // R6: no golpea a quien lo provoca
                if (!seen.Add(t.Transform.root)) continue;

                Vector3 point = col.ClosestPoint(center);
                DamageResolver.Apply(c, t, element, damage, staggerAmount, point);
                // Reportar deja engancharle payloads (slow, empuje…) a QUIEN alcanzó el estallido,
                // que es distinto de a quién golpeó la hitbox que lo provocó.
                if (reportHits)
                    c.reportHit?.Invoke(new HitInfo { target = t, point = point, normal = Vector3.up });
            }

            // TODO(Fase 2): CombatServices.Reactions.Irradiate(center, radius, element) contra el terreno.
        }
    }

    /// Empuje + flinch al impactar. Golpe normal = retroceso corto; embestida = empuje sostenido
    /// (speed alto, duration larga) que lleva al enemigo hasta chocar una pared. Requiere HitReactor
    /// en el objetivo. Dirección = hacia donde mira el caster (horizontal).
    [System.Serializable]
    public class Knockback : AbilityEffect
    {
        public float speed = 5f;         // u/seg del empuje
        public float duration = 0.12f;   // cuánto dura el empuje
        public float flinch = 0.12f;     // hitstun breve
        [Tooltip("Arrastrar en la dirección REAL del desplazamiento (3D, con su inclinación) en vez de en el frente horizontal. Es lo que hace que una embestida en diagonal se lleve al enemigo HACIA ARRIBA contigo, en vez de dejarlo en el suelo")]
        public bool useMotionDirection = false;
        [Tooltip("Elevación extra en u/SEG (no en unidades). ⚠️ La altura real es lift × duration: con duration 0.12, un lift de 12 sube ~1.4 u. Valores de un dígito casi no despegan a nadie")]
        public float lift = 0f;
        [Tooltip("Escala SOLO la componente vertical del arrastre. <1 mantiene al enemigo a tu altura en vez de lanzarlo por encima: si sube más que tú, se sale del alcance y el combo se corta")]
        public float verticalScale = 1f;
        [Tooltip("Segundos de suspensión de caída que concede este golpe (juggle). 0 = cae normal")]
        public float hang = 0f;
        public ApplyWhen when = ApplyWhen.OnHit;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Apply(c, c.lockTarget); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Apply(c, hit.target); }

        void Apply(AbilityCtx c, ICombatant t)
        {
            if (t == null || c.caster == null) return;
            if (!t.Transform.TryGetComponent(out HitReactor reactor)) return;

            Vector3 dir;
            if (useMotionDirection && c.caster.LastDashDirection.sqrMagnitude > 0.0001f)
                dir = c.caster.LastDashDirection;          // 3D: conserva la inclinación del dash
            else { dir = c.caster.transform.forward; dir.y = 0f; }
            if (dir.sqrMagnitude < 0.0001f) return;

            Vector3 vel = dir.normalized * speed;
            vel.y = vel.y * verticalScale + lift;   // el empuje horizontal y el vertical se afinan aparte
            reactor.ApplyKnockback(vel, duration);
            if (flinch > 0f) reactor.ApplyFlinch(flinch);
            if (hang > 0f) reactor.ApplyHang(hang);
        }
    }

    /// Mantiene al objetivo por DELANTE del atacante a `distance` (re-centra para encadenar el combo).
    /// `speed` bajo = recolocación sutil (básico); `speed` alto = carry de embestida. Requiere HitReactor.
    [System.Serializable]
    public class CarryToFront : AbilityEffect
    {
        public float distance = 2f;
        public float speed = 8f;
        public float duration = 0.2f;
        public float flinch = 0.12f;
        [Tooltip("Llevarlo también a TU altura. Es el tope que impide que una embestida en diagonal lo mande fuera del alcance del siguiente golpe")]
        public bool matchHeight = false;
        [Tooltip("Altura relativa a la tuya donde se le mantiene (0 = tu misma altura)")]
        public float heightOffset = 0f;
        public ApplyWhen when = ApplyWhen.OnHit;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Apply(c, c.lockTarget); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Apply(c, hit.target); }

        void Apply(AbilityCtx c, ICombatant t)
        {
            if (t == null || c.caster == null) return;
            if (!t.Transform.TryGetComponent(out HitReactor r)) return;
            r.ApplyCarry(c.caster.transform, distance, speed, duration, matchHeight, heightOffset);
            if (flinch > 0f) r.ApplyFlinch(flinch);
        }
    }

    [System.Serializable]
    public class ApplyStatus : AbilityEffect
    {
        public StatusType type = StatusType.Ralentizado;
        public float duration = 3f;
        [Tooltip("Solo Quemadura: daño por pulso (DoT). 0 en los demás estados")]
        public float tickDamage = 0f;
        public ApplyWhen when = ApplyWhen.OnHit;

        public override void OnEnter(AbilityCtx c) { if (when == ApplyWhen.OnCast) Apply(c.lockTarget); }
        public override void OnHit(AbilityCtx c, HitInfo hit) { if (when == ApplyWhen.OnHit) Apply(hit.target); }

        void Apply(ICombatant t)
        {
            if (t == null) return;
            StatusController.Of(t.Transform)?.Apply(type, duration, tickDamage);
        }
    }

    [System.Serializable]
    public class GainEnergy : AbilityEffect
    {
        public float amount = 20f;
        public override void OnEnter(AbilityCtx c)
        {
            if (c.casterEnergy != null)
                c.casterEnergy.current = Mathf.Min(c.casterEnergy.max, c.casterEnergy.current + amount);
        }
    }

    // ============ ACTUADORES (hacen la acción) — versión base; se amplían en la Pasada 2 ============

    /// Desplazamiento propio en la DIRECCIÓN DE APUNTADO (3D). Apuntando puedes lanzarte hacia
    /// arriba o abajo — es lo que permite, p.ej., que un Ottorock se impulse al aire para alcanzar
    /// a un volador. Las habilidades no están atadas al plano del suelo.
    [System.Serializable]
    public class DashMotion : AbilityEffect
    {
        public float distance = 5f;
        public float time = 0.16f;
        [Tooltip("Inclinación máxima del desplazamiento (grados). Impide que apuntar al cielo convierta una embestida en un cohete vertical —que marea— sin quitarle el poder impulsarse hacia un volador. 90 = sin límite")]
        public float maxPitch = 35f;
        [Tooltip("Arranca rápido y frena solo (deslizamiento). Para rematar un desplazamiento sin cortarlo en seco")]
        public bool easeOut = false;
        [Tooltip("Usar la dirección en la que YA se movía en vez de la de apuntado. Para continuar un desplazamiento anterior, no empezar otro")]
        public bool continueForward = false;

        public override void OnEnter(AbilityCtx c)
        {
            if (c.caster == null) return;
            Vector3 flatFwd = c.caster.transform.forward; flatFwd.y = 0f;

            Vector3 dir;
            if (continueForward)
            {
                // Continuar, no redirigir: sigue por donde iba. Si se recalculara contra el aimPoint,
                // el deslizamiento daría un volantazo hacia donde apunte la cámara en ese instante.
                dir = c.caster.LastDashDirection;
                if (dir.sqrMagnitude < 0.0001f) dir = flatFwd;
            }
            else
            {
                Vector3 origin = c.caster.transform.position + Vector3.up * 0.5f;
                dir = c.aimPoint - origin;
                dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : c.caster.transform.forward;
            }

            c.caster.RequestDash(ClampPitch(dir.normalized, maxPitch, flatFwd) * distance, time, easeOut);
        }

        /// Recorta la inclinación conservando el rumbo horizontal. `fallbackFlat` cubre el caso
        /// límite de apuntar al cenit exacto, donde no queda rumbo que conservar.
        static Vector3 ClampPitch(Vector3 dir, float maxDeg, Vector3 fallbackFlat)
        {
            if (maxDeg >= 89f) return dir;

            Vector3 flat = new Vector3(dir.x, 0f, dir.z);
            if (flat.sqrMagnitude < 0.0001f) flat = fallbackFlat;
            if (flat.sqrMagnitude < 0.0001f) return dir;
            flat.Normalize();

            float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
            float clamped = Mathf.Clamp(pitch, -maxDeg, maxDeg) * Mathf.Deg2Rad;
            return (flat * Mathf.Cos(clamped) + Vector3.up * Mathf.Sin(clamped)).normalized;
        }
    }

    /// Delivery melee: al entrar la fase, solapa un volumen frente al caster y reporta
    /// cada ICombatant golpeado -> dispara el OnHit de los payloads de la fase (DealDamage OnHit, etc.).
    [System.Serializable]
    public class MeleeHitbox : AbilityEffect
    {
        public float range = 2f;      // alcance frente al caster
        public float radius = 1.5f;   // grosor del arco

        public override void OnEnter(AbilityCtx c)
        {
            if (c.caster == null || c.reportHit == null) return;
            // Dirección de apuntado en 3D (ctx.aimPoint) → el golpe puede ir arriba/abajo, no solo horizontal.
            Vector3 origin = c.caster.transform.position + Vector3.up * 0.5f;
            Vector3 aimDir = c.aimPoint - origin;
            aimDir = aimDir.sqrMagnitude > 0.0001f ? aimDir.normalized : c.caster.transform.forward;

            Vector3 center = origin + aimDir * range * 0.5f;
            HitboxDebug.Show(center, radius, 0.15f);

            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(center, radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                if (t.Transform.root == c.caster.transform.root) continue;   // no autogolpe
                if (!seen.Add(t.Transform.root)) continue;                   // 1 vez por objetivo
                c.reportHit(new HitInfo { target = t, point = col.ClosestPoint(center), normal = Vector3.up });
            }
        }
    }

    /// Hitbox que acompaña al caster durante la fase y **emite golpes a intervalos** mientras dura
    /// (multi-hit, ref. el Drill Rush de Meta Knight): la embestida *tiene* N golpes repartidos por
    /// el recorrido, y lo que cambia es **cuántos te alcanzan** — rozarla al esquivar cuesta uno,
    /// comerla entera cuesta todos. Cada golpe es un `HitInfo` normal, así que los payloads de
    /// siempre (`DealDamage`, `Knockback`, …) funcionan sin saber nada de esto.
    ///
    /// Dos cosas que **no** dependen del framerate, y son deliberadas:
    ///  · **La cadencia va por reloj de la habilidad**, no por frame (como los frames de animación de
    ///    un juego de lucha) — mañana ese reloj lo sustituye un **anim-event** sin cambiar nada más.
    ///  · **Cada golpe barre el tramo recorrido desde el anterior**, no muestrea un punto suelto: así
    ///    ningún enemigo puede colarse *entre dos muestras*. Cada golpe cubre su trozo del recorrido.
    [System.Serializable]
    public class SweepHitbox : AbilityEffect
    {
        public float radius = 1.2f;
        public float forwardOffset = 0.8f;
        [Tooltip("Segundos entre golpe y golpe. 0 = un único golpe por objetivo en toda la fase")]
        public float hitInterval = 0.09f;

        const string NextKey = "sweepHit.nextAt";
        const string SeenKey = "sweepHit.seen";     // solo se usa con hitInterval = 0
        const string LastPosKey = "sweepHit.lastPos";

        public override void OnEnter(AbilityCtx c)
        {
            c.bb[SeenKey] = new HashSet<Transform>();
            c.bb[NextKey] = 0f;                     // el primer golpe sale en el primer tick
            c.bb.Remove(LastPosKey);
        }

        public override void OnTick(AbilityCtx c, float dt)
        {
            if (c.caster == null || c.reportHit == null) return;

            Vector3 center = c.caster.transform.position
                           + c.caster.transform.forward * forwardOffset
                           + Vector3.up * 0.5f;

            // ¿Toca golpe? Con hitInterval = 0 la hitbox está viva todo el rato y el filtro de
            // "una vez por objetivo" (SeenKey) es lo que evita repetir.
            float nextAt = c.bb.TryGetValue(NextKey, out var n) && n is float f ? f : 0f;
            if (hitInterval > 0.001f && Time.time < nextAt) return;
            c.bb[NextKey] = Time.time + Mathf.Max(0.02f, hitInterval);

            HitboxDebug.Show(center, radius, 0.08f);

            // El tramo va desde el GOLPE anterior (no desde el frame anterior): así los golpes
            // se reparten el recorrido sin dejar huecos entre ellos.
            Vector3 prev = c.bb.TryGetValue(LastPosKey, out var p) && p is Vector3 v ? v : center;
            c.bb[LastPosKey] = center;

            var hitNow = new HashSet<Transform>();   // 1 vez por objetivo EN ESTE GOLPE
            Vector3 step = center - prev;
            float travel = step.magnitude;
            if (travel > 0.001f)
                foreach (var hit in Physics.SphereCastAll(prev, radius, step / travel, travel,
                                                          ~0, QueryTriggerInteraction.Ignore))
                    Consider(c, hit.collider, center, hitNow);

            // Y el solape en la posición actual: el SphereCast no reporta lo que ya estaba dentro
            // al empezar el tramo, ni cubre a quien se acerca a ti mientras tú no te mueves.
            foreach (var col in Physics.OverlapSphere(center, radius))
                Consider(c, col, center, hitNow);
        }

        void Consider(AbilityCtx c, Collider col, Vector3 center, HashSet<Transform> hitNow)
        {
            if (col == null || !col.TryGetComponent(out ICombatant t)) return;
            if (t.Transform.root == c.caster.transform.root) return;   // no autogolpe
            if (!hitNow.Add(t.Transform.root)) return;                 // ya golpeado en este mismo golpe

            if (hitInterval <= 0.001f)                                 // modo golpe único por fase
            {
                if (!(c.bb.TryGetValue(SeenKey, out var o) && o is HashSet<Transform> seen)) return;
                if (!seen.Add(t.Transform.root)) return;
            }

            c.reportHit(new HitInfo { target = t, point = col.ClosestPoint(center), normal = Vector3.up });
        }
    }

    [System.Serializable]
    public class StainTerrain : AbilityEffect
    {
        public float radius = 3f;
        public override void OnEnter(AbilityCtx c)
        {
            if (c.element.HasValue)   // un golpe neutro no tiñe nada
                CombatServices.Reactions?.Stain(c.aimPoint, radius, c.element.Value);
        }
    }

    /// **Cámara propia de una habilidad** (primer caso: la embestida). Mientras dura la fase la cámara
    /// se pone a SUS valores —absolutos, no incrementos— para que la habilidad se vea igual vengas de
    /// libre, de lock o de apuntado; al terminar devuelve el control a la cámara que hubiera, sin
    /// saber cuál era. El encuadre se **renueva cada tick**: si la habilidad muere sin cerrar la fase,
    /// la cámara vuelve sola en cuanto caduca (nada puede dejarla enganchada).
    [System.Serializable]
    public class CameraShot : AbilityEffect
    {
        [Tooltip("Distancia ABSOLUTA de la cámara durante la habilidad (no se suma a la del modo)")]
        public float distance = 9.5f;
        [Tooltip("Picado ABSOLUTO en grados durante la habilidad")]
        public float pitch = 18f;
        [Tooltip("Colocarse DETRÁS del jugador durante la fase (para ver el desplazamiento de frente)")]
        public bool fromBehind = true;
        [Tooltip("Grados/seg a los que se coloca detrás. Alto = seco; bajo = perezoso")]
        public float behindSpeed = 260f;
        [Tooltip("Segundos que la cámara sigue fuera DESPUÉS de terminar la fase, antes de volver. 0 = vuelve al acabar")]
        public float holdAfter = 0.3f;
        [Tooltip("Colocar el encuadre sobre el EJE de la acción (hacia donde apuntabas al lanzarla). Si no, se usa el encarado del cuerpo")]
        public bool alignToAim = true;

        PlayerCameraRig Rig(AbilityCtx c) => c.caster != null ? c.caster.CameraRig : null;

        public override void OnEnter(AbilityCtx c) => Push(c, fromBehind);
        public override void OnTick(AbilityCtx c, float dt) => Push(c, fromBehind);

        // El eje se calcula UNA vez y se guarda en el blackboard: recalcularlo cada frame lo invertiría
        // en cuanto el caster pasa el punto al que apuntaba, y la cámara daría la vuelta a media acción.
        [System.NonSerialized] string _dirKey;
        string DirKey => _dirKey ??= "camshot.dir." + GetHashCode();

        Vector3 Direction(AbilityCtx c)
        {
            if (!alignToAim || c.caster == null) return Vector3.zero;
            if (c.bb.TryGetValue(DirKey, out var o) && o is Vector3 cached) return cached;

            Vector3 d = c.aimPoint - (c.caster.transform.position + Vector3.up * 0.5f);
            Vector3 dir = d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.zero;
            c.bb[DirKey] = dir;
            return dir;
        }

        /// Al acabar la fase el encuadre se queda `holdAfter` y caduca solo. Durante ese resto se
        /// suelta el "colocarse detrás": la habilidad ya terminó y la cámara es otra vez del jugador.
        public override void OnExit(AbilityCtx c)
        {
            if (holdAfter > 0f) Rig(c)?.SetShot(ShotFor(c, false), holdAfter);
            else Rig(c)?.ClearShot();
        }

        void Push(AbilityCtx c, bool behind) => Rig(c)?.SetShot(ShotFor(c, behind));

        PlayerCameraRig.Shot ShotFor(AbilityCtx c, bool behind) => new PlayerCameraRig.Shot
        {
            distance = distance,
            pitch = pitch,
            fromBehind = behind,
            behindSpeed = behindSpeed,
            direction = Direction(c),
        };
    }
}
