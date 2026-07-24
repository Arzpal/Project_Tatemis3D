#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Verificación de la tabla elemental (T1.1). No es Unity Test Framework porque el proyecto no
    /// tiene .asmdef y montarlos es un cambio estructural aparte (ver nota al pie); es un validador
    /// por menú que corre los mismos asserts y falla ruidoso. Menú: Tatemis ▸ Tests ▸ Validar TypeChart.
    ///
    /// Cubre el AC del ticket + un barrido exhaustivo de las 36 combinaciones contra los dos triángulos:
    ///   Naturaleza › Agua › Fuego › Naturaleza   |   Mineral › Rayo › Aire › Mineral
    /// (cruzar de un triángulo al otro = neutral).
    public static class TypeChartValidator
    {
        // Fuente de verdad del diseño, independiente del código de TypeChart: si alguien rompe
        // TypeChart.Beats, esta tabla NO cambia y el test lo caza.
        static readonly (Element, Element)[] ExpectedBeats =
        {
            (Element.Naturaleza, Element.Agua),
            (Element.Agua,       Element.Fuego),
            (Element.Fuego,      Element.Naturaleza),
            (Element.Mineral,    Element.Rayo),
            (Element.Rayo,       Element.Aire),
            (Element.Aire,       Element.Mineral),
        };

        [MenuItem("Tatemis/Tests/Validar TypeChart")]
        public static void Run()
        {
            var chart = ScriptableObject.CreateInstance<TypeChart>();
            var fails = new List<string>();
            int checks = 0;

            bool ExpectBeats(Element a, Element b)
            {
                foreach (var (x, y) in ExpectedBeats) if (x == a && y == b) return true;
                return false;
            }

            void Check(bool cond, string msg) { checks++; if (!cond) fails.Add(msg); }

            // --- Casos explícitos del AC del ticket ---
            Check(Mathf.Approximately(chart.Multiplier(Element.Fuego, Element.Naturaleza), 1.5f), "Fuego→Naturaleza debe ser 1.5");
            Check(Mathf.Approximately(chart.Multiplier(Element.Agua, Element.Fuego), 1.5f), "Agua→Fuego debe ser 1.5");
            Check(Mathf.Approximately(chart.Multiplier(Element.Naturaleza, Element.Fuego), 0.5f), "Naturaleza→Fuego debe ser 0.5 (resiste)");
            Check(Mathf.Approximately(chart.Multiplier(Element.Fuego, Element.Mineral), 1f), "Fuego→Mineral debe ser 1 (cruce neutral)");

            // --- Barrido exhaustivo: las 36 combinaciones ---
            var all = (Element[])System.Enum.GetValues(typeof(Element));
            foreach (var a in all)
                foreach (var b in all)
                {
                    float got = chart.Multiplier(a, b);
                    float want = a == b ? 1f : ExpectBeats(a, b) ? 1.5f : ExpectBeats(b, a) ? 0.5f : 1f;
                    Check(Mathf.Approximately(got, want), $"Multiplier({a},{b}) = {got}, esperado {want}");
                }

            // --- Estructura de los triángulos: cada elemento supera a 1 y es superado por 1 ---
            foreach (var a in all)
            {
                int beatsN = 0, beatenN = 0;
                foreach (var b in all)
                {
                    if (a == b) continue;
                    if (TypeChart.Beats(a, b)) beatsN++;
                    if (TypeChart.Beats(b, a)) beatenN++;
                }
                Check(beatsN == 1, $"{a} debería superar a EXACTAMENTE 1 elemento, supera a {beatsN}");
                Check(beatenN == 1, $"{a} debería ser superado por EXACTAMENTE 1 elemento, lo superan {beatenN}");
            }

            Object.DestroyImmediate(chart);

            var sb = new StringBuilder();
            sb.Append($"[TypeChart] {checks - fails.Count}/{checks} comprobaciones OK.");
            if (fails.Count == 0)
                Debug.Log("✅ " + sb + " La tabla elemental es correcta.");
            else
            {
                sb.Append($"\n❌ {fails.Count} FALLOS:");
                foreach (var f in fails) sb.Append("\n  · " + f);
                Debug.LogError(sb.ToString());
            }
        }
    }
}
#endif
