using UnityEngine;

namespace Tatemis
{
    /// Cablea los servicios de combate globales al arrancar la escena (sustituible por DI).
    /// Sin esto, los multiplicadores de tipo caen a 1x (DealDamage null-guardea el TypeChart).
    public class CombatBootstrap : MonoBehaviour
    {
        [SerializeField] TypeChart typeChart;

        void Awake()
        {
            if (typeChart != null) CombatServices.TypeChart = typeChart;
        }
    }
}
