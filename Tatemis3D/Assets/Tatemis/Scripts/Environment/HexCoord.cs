using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Coordenada de celda hexagonal (axial q,r) para la rejilla elemental de [ARENA.md].
    /// **Hexágonos flat-top** (vértice a izquierda y derecha, lados planos arriba y abajo), que es
    /// como los dibuja `HexGridOverlay`.
    ///
    /// ⚠️ La rejilla es un **overlay de lectura, no geometría**: el mundo es continuo y se camina
    /// libremente. Esto solo sirve para responder *"¿qué elemento hay bajo este punto?"*, que es lo
    /// que necesita el sistema de reacciones (ELEMENTAL-REACTIONS R1).
    public readonly struct HexCoord
    {
        public readonly int q, r;

        public HexCoord(int q, int r) { this.q = q; this.r = r; }

        /// Cubo: la tercera coordenada es redundante (x+y+z=0) pero hace trivial el redondeo.
        public int X => q;
        public int Z => r;
        public int Y => -q - r;

        public Vector3 ToWorld(float radius) => new Vector3(
            radius * 1.5f * q,
            0f,
            radius * Mathf.Sqrt(3f) * (r + q * 0.5f));

        /// Punto del mundo → celda que lo contiene. Redondeo en coordenadas de cubo: se redondean
        /// las tres y se corrige la que más se desvió, que es lo único que garantiza caer siempre
        /// dentro del hexágono correcto (redondear q y r por separado da errores en los bordes).
        public static HexCoord FromWorld(Vector3 world, float radius)
        {
            float q = (2f / 3f) * world.x / radius;
            float r = (-1f / 3f * world.x + Mathf.Sqrt(3f) / 3f * world.z) / radius;
            return Round(q, r);
        }

        static HexCoord Round(float q, float r)
        {
            float y = -q - r;
            int rq = Mathf.RoundToInt(q), ry = Mathf.RoundToInt(y), rr = Mathf.RoundToInt(r);
            float dq = Mathf.Abs(rq - q), dy = Mathf.Abs(ry - y), dr = Mathf.Abs(rr - r);

            if (dq > dy && dq > dr) rq = -ry - rr;
            else if (dy <= dr) rr = -rq - ry;
            return new HexCoord(rq, rr);
        }

        /// Ruido determinista por celda: la misma coordenada da siempre el mismo valor 0..1, así que
        /// el mundo se ve igual en cada Play sin guardar nada. (Un `Random` normal haría que la
        /// composición cambiara entre sesiones y no se podría afinar en playtest.)
        public float Hash01(int seed = 0)
        {
            unchecked
            {
                int h = q * 73856093 ^ r * 19349663 ^ seed * 83492791;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0x7FFFFFF) / (float)0x7FFFFFF;
            }
        }

        /// Los 6 vecinos, en orden. Lo usa la **propagación del incendio**: el fuego se extiende de
        /// celda en celda y **solo por terreno de Naturaleza**, así que el propio parche define hasta
        /// dónde llega — sin tope inventado ni grafo de vecindad aparte, la rejilla ya es el grafo.
        static readonly int[,] Dirs = { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 } };

        public HexCoord Neighbor(int i) => new HexCoord(q + Dirs[i % 6, 0], r + Dirs[i % 6, 1]);

        /// Desplazamiento en el MUNDO hacia el vecino `i`. La pared de la zona de combate se coloca a
        /// la mitad de este vector, mirando hacia fuera: así cae **sobre la arista del hexágono** y no
        /// corta ninguna celda por la mitad.
        public static Vector3 NeighborOffset(int i, float radius) =>
            new HexCoord(Dirs[i % 6, 0], Dirs[i % 6, 1]).ToWorld(radius);

        /// Distancia en celdas (métrica hexagonal). Sirve para "todas las celdas a N anillos".
        public int DistanceTo(HexCoord o)
        {
            int dq = q - o.q, dr = r - o.r;
            return Mathf.Max(Mathf.Abs(dq), Mathf.Abs(dr), Mathf.Abs(dq + dr));
        }

        /// Todas las celdas a `rings` anillos o menos de esta. Es **el área de combate**: un conjunto
        /// de celdas COMPLETAS, no un círculo — el cerco tiene que seguir el patrón hexagonal.
        public static IEnumerable<HexCoord> Within(HexCoord center, int rings)
        {
            for (int dq = -rings; dq <= rings; dq++)
                for (int dr = Mathf.Max(-rings, -dq - rings); dr <= Mathf.Min(rings, -dq + rings); dr++)
                    yield return new HexCoord(center.q + dq, center.r + dr);
        }

        public override bool Equals(object o) => o is HexCoord h && h.q == q && h.r == r;
        public override int GetHashCode() => q * 73856093 ^ r * 19349663;
        public override string ToString() => $"({q},{r})";
    }
}
