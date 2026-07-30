using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// **Amplificación** (ELEMENTAL-REACTIONS §0.2): golpear tu propio elemento no produce ninguna
    /// mezcla —y por tanto ningún efecto— pero **reaviva el elemento**: tu explosión vuelve a sonar
    /// tras un delay corto, con el mismo daño. *19 de fuego → otro 19 de fuego.*
    ///
    /// No es un ×2 encubierto: son **dos eventos separados en el tiempo**, así que el segundo **se
    /// puede esquivar**. Eso es lo que impide que jugar en tu bioma casa sea daño gratis, y es la
    /// razón de que el multiplicador de tipo no se "apile" — cada explosión es un golpe normal.
    ///
    /// Es tuya (la provocó tu elemento imponiéndose), así que **respeta a quien la causó** (R6).
    public class Echo : MonoBehaviour
    {
        Element? _element;
        float _damage, _radius, _at;
        Transform _instigatorRoot;
        bool _done;

        /// `element = null` → daño **neutro**. Lo usan las **mini explosiones** (Rayo + Fuego), que son
        /// una combinación y por tanto no llevan el tipo de nadie; la amplificación sí lleva el suyo.
        public static void Spawn(Vector3 groundPos, float radius, Element? element, float damage,
                                 float delay, ICombatant instigator)
        {
            var go = new GameObject(element.HasValue ? "Amplificacion_" + element.Value : "Eco");
            go.transform.position = groundPos;
            var e = go.AddComponent<Echo>();
            e._element = element;
            e._radius = radius;
            e._damage = damage;
            e._at = Time.time + Mathf.Max(0f, delay);
            e._instigatorRoot = instigator != null && instigator.Transform != null
                ? instigator.Transform.root : null;
        }

        void Update()
        {
            if (_done || Time.time < _at) return;
            _done = true;
            Fire();
            Destroy(gameObject, 0.4f);
        }

        void Fire()
        {
            Vector3 origin = transform.position + Vector3.up * 0.4f;
            HitboxDebug.Show(origin, _radius, 0.3f,
                _element.HasValue ? HitboxDebug.ColorOf(_element.Value) : new Color(1f, 0.75f, 0.3f, 0.45f));
            ImpactVFX.Spawn(origin, 1f, 0.8f);

            var seen = new HashSet<Transform>();
            foreach (var col in Physics.OverlapSphere(origin, _radius))
            {
                if (!col.TryGetComponent(out ICombatant t)) continue;
                var root = t.Transform.root;
                if (!seen.Add(root)) continue;
                if (_instigatorRoot != null && root == _instigatorRoot) continue;   // R6: es tuya

                float mult = _element.HasValue && CombatServices.TypeChart != null
                    ? CombatServices.TypeChart.Multiplier(_element.Value, t.Element) : 1f;
                float dealt = _damage * mult;
                t.TakeHit(dealt, dealt);   // el aturdimiento va con el daño (GDD §8.2)

                if (t.TakesDamage)
                    HitFeedback.Report(t, dealt, mult, col.ClosestPoint(origin),
                                       t.Transform.position - transform.position, 0.45f);
            }

            // ⚠️ El eco NO irradia al terreno: ya es la consecuencia de un impacto elemental, y
            // encadenar reacciones es justo lo que R1 existe para impedir.
        }
    }
}
