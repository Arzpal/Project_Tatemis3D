using UnityEngine;

namespace Tatemis
{
    /// Nodo de cristal cargado (ELEMENTAL-REACTIONS §8): al ser GOLPEADO suelta su carga — un
    /// impacto elemental en su posición que resuelve contra el terreno donde está plantado.
    /// Plantado dentro del borde de un charco es la única forma de ver el ★ agua electrificada del
    /// pitch en el slice: ninguna de las 3 criaturas genera Rayo ni Agua, y funciona ya con
    /// Ottorock-Libre. Es ICombatant solo para que las hitboxes lo reporten: no tiene vida, no cae,
    /// y por eso declara `IsCreature = false` (ni lo fija el lock-on, ni lleva estados encima).
    public class CrystalNode : MonoBehaviour, ICombatant
    {
        public Element charge = Element.Rayo;
        public float dischargeRadius = 3f;
        [Tooltip("Segundos entre descargas: en caja gris no se agota, se recarga")]
        public float rechargeSeconds = 2.5f;

        float _readyAt;
        float _pendingStrength;   // >0 = descarga pedida, sale en el Update siguiente

        public Element Element => charge;
        public Transform Transform => transform;
        public bool IsCreature => false;
        public bool TakesDamage => false;

        /// Golpearlo **pide** la descarga; sale en el Update siguiente, no en la misma pila de
        /// llamadas. Dos razones: por ficción (el cristal suelta la carga al romperse, no a la vez
        /// que le muerden) y porque los efectos de una fase se despachan en cadena — si descargara
        /// aquí mismo, el estallido elemental de la misma habilidad cancelaría por R3 la reacción
        /// que este nodo acaba de crear, y la ★ moriría en el frame en que nace.
        public void TakeHit(float damage, float stagger)
        {
            if (Time.time < _readyAt) return;
            _readyAt = Time.time + rechargeSeconds;
            _pendingStrength = Mathf.Max(10f, damage);
        }

        /// La descarga va SIN dueño (instigator null): un nodo descargado es del terreno, no de
        /// quien lo golpeó — provocarlo de cerca puede costarte comerte la reacción (riesgo buscado,
        /// coherente con el espíritu de R4: el terreno no es aliado de nadie).
        void Update()
        {
            if (_pendingStrength <= 0f) return;
            float strength = _pendingStrength;
            _pendingStrength = 0f;

            HitboxDebug.Show(transform.position, dischargeRadius, 0.25f, HitboxDebug.ColorOf(charge));
            CombatServices.Reactions?.Impact(transform.position, dischargeRadius, charge, strength, null, strength);
        }
    }
}
