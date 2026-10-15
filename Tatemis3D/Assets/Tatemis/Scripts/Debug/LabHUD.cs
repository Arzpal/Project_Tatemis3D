using UnityEngine;
using UnityEngine.InputSystem;

namespace Tatemis
{
    /// **El instrumental del laboratorio de saturación** ([ER §0.10](../../../../docs/ELEMENTAL-REACTIONS.md)).
    ///
    /// El laboratorio no existe para *ver* el sistema —eso ya se ve en el mundo— sino para **medirlo**.
    /// Quedan ocho números sin afinar (`referenceImpact`, gracia, decaimiento, venteo, reversión y su
    /// ×2 neutro, derrame, velocidad de onda) más el **tamaño de mancha**, y ninguno se afina mirando:
    /// se afinan **leyendo la cifra y repitiendo la prueba rápido**.
    ///
    /// De ahí las tres cosas que hace esto y que no hace ningún otro debug del proyecto:
    ///  · **Lee la batería apuntada** — dueño, %, si está en gracia o ya decayendo, y el reloj del venteo.
    ///  · **Controla el tiempo** — la onda avanza un anillo cada 0,08 s: a velocidad normal se ve *el
    ///    resultado*, no *el mecanismo*. Con pausa y cámara lenta se puede comprobar que arranca donde
    ///    golpeaste y que **cobra una sola vez** por criatura.
    ///  · **Resetea** — borra la carga y devuelve el terreno, para repetir la misma medida desde cero
    ///    sin esperar al decaimiento.
    public class LabHUD : MonoBehaviour
    {
        [Tooltip("Puntos a los que teletransporta cada tecla numérica, y su etiqueta")]
        public Vector3[] benchPoints = new Vector3[0];
        public string[] benchLabels = new string[0];
        [Tooltip("El jugador, para teletransportarlo y para saber a dónde apunta")]
        public Transform player;

        static readonly float[] Speeds = { 1f, 0.25f, 0.1f };
        int _speed;
        bool _paused;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.pKey.wasPressedThisFrame)
            {
                _paused = !_paused;
                Time.timeScale = _paused ? 0f : Speeds[_speed];
            }
            if (kb.oKey.wasPressedThisFrame)
            {
                _speed = (_speed + 1) % Speeds.Length;
                _paused = false;
                Time.timeScale = Speeds[_speed];
            }
            if (kb.rKey.wasPressedThisFrame) ResetBoard();

            for (int i = 0; i < benchPoints.Length && i < 9; i++)
                if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) Teleport(benchPoints[i]);
        }

        /// ⚠️ Devuelve `timeScale` a 1 además de limpiar: quedarse pausado tras un reset y no entender
        /// por qué "no pasa nada" es el fallo más tonto y más frecuente de una escena de pruebas.
        void ResetBoard()
        {
            CombatServices.Reactions?.Saturation?.ClearAll();
            CombatServices.Grid?.RevertAll();
            _paused = false;
            _speed = 0;
            Time.timeScale = 1f;
        }

        void Teleport(Vector3 p)
        {
            if (player == null) return;
            // Desactivar el CharacterController al teletransportar: si no, se come el cambio de posición
            // y el jugador se queda donde estaba sin ninguna señal de por qué.
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = p + Vector3.up * 1.2f;
            if (cc != null) cc.enabled = true;
        }

        static GUIStyle _s, _sTitle;

        void OnGUI()
        {
            var grid = CombatServices.Grid;
            var rs = CombatServices.Reactions;
            var sat = rs != null ? rs.Saturation : null;
            if (grid == null || rs == null || sat == null) return;

            if (_s == null)
            {
                _s = new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true };
                _sTitle = new GUIStyle(GUI.skin.label)
                { fontSize = 13, fontStyle = FontStyle.Bold, richText = true };
            }

            var spray = player != null ? player.GetComponent<ElementalSpray>() : null;
            Vector3 aim = spray != null
                ? player.position + player.forward * spray.reach
                : (player != null ? player.position : Vector3.zero);

            var c = grid.CoordAt(aim);
            var cell = grid.CellAt(c);
            var b = sat.At(c);

            var box = new Rect(12f, 12f, 430f, 232f);
            GUI.Box(box, GUIContent.none);
            float y = box.y + 8f;
            void Line(string t, GUIStyle st = null)
            { GUI.Label(new Rect(box.x + 10f, y, box.width - 20f, 18f), t, st ?? _s); y += 17f; }

            string vel = _paused ? "<color=#ff8080>PAUSA</color>"
                                 : (_speed == 0 ? "×1" : $"<color=#ffd060>×{Speeds[_speed]:0.##}</color>");
            Line($"LABORATORIO DE SATURACIÓN   ·   tiempo {vel}", _sTitle);
            Line($"<color=#a0a0a0>P pausa · O cámara lenta · R reset · 1-{Mathf.Min(9, benchPoints.Length)} bancos</color>");
            y += 4f;

            // ── La escalera, con el valor de y a la vista ──
            float unit = Mathf.Max(1f, rs.ReferenceImpact);
            float shot = spray != null ? spray.impactStrength / unit : 0f;
            Line($"1y = <b>{unit:0.#}</b> de impacto   ·   un disparo del spray = <b>{shot:0.00}y</b>");
            Line($"umbrales:  amplificar <b>{sat.costAmplificacion:0.#}y</b>  ·  neutra <b>{sat.costCargarNeutra:0.#}y</b>" +
                 $"  ·  combinación <b>{sat.costCombinacion:0.#}y</b>  ·  reacción <b>{sat.costReaccion:0.#}y</b>");
            y += 4f;

            // ── La celda apuntada ──
            string terreno = cell.element.HasValue ? cell.element.Value.ToString() : "neutro";
            string mancha = cell.patch != 0
                ? $"mancha #{cell.patch} de <b>{grid.PatchSize(cell.patch)}</b> celdas"
                : "sin mancha (celda suelta)";
            Line($"celda {c}:  <b>{terreno}</b>  ·  {mancha}");

            float rev = grid.RevertRemaining(c);
            Line(rev < 0f
                ? "<color=#a0a0a0>terreno original</color>"
                : $"<color=#80d0ff>TRANSFORMADA</color> · revierte en <b>{rev:0.0}s</b>");
            y += 4f;

            // ── La batería ──
            if (b == null || b.amount <= 0f) Line("<color=#a0a0a0>batería vacía</color>", _sTitle);
            else
            {
                string estado = b.primed
                    ? $"<color=#ffffff>CEBADA</color> · se ventea en <b>{Mathf.Max(0f, sat.primedSeconds - (Time.time - b.primedAt)):0.0}s</b>"
                    : (Time.time - b.lastTouch < sat.holdSeconds
                        ? $"<color=#80ff80>en gracia</color> · empieza a bajar en <b>{sat.holdSeconds - (Time.time - b.lastTouch):0.0}s</b>"
                        : "<color=#ffa060>decayendo</color>");
                Line($"batería <b>{b.owner}</b>  {b.amount:0.00} / {b.threshold:0.#}y   ({b.Fill01 * 100f:0}%)", _sTitle);
                Line(estado);
            }
            y += 4f;

            // ── La receta: qué saldría si disparas ahora ──
            if (spray != null) Line(Recipe(rs, cell, spray.current), _sTitle);
        }

        /// *Terreno X + entrante Y = nombre*. Es lo que convierte el laboratorio en algo que además
        /// **enseña** la tabla, en vez de solo medirla.
        static string Recipe(ReactionSystem rs, ElementalGrid.Cell cell, Element incoming)
        {
            if (!cell.element.HasValue)
                return $"neutro + {incoming} → <b>se vuelve terreno de {incoming}</b>";
            if (cell.element.Value == incoming)
                return $"{incoming} + {incoming} → <b>amplificación</b> (instantánea)";

            var table = rs.Table;
            if (table == null || !table.TryResolve(cell.element.Value, incoming, out var e))
                return $"<color=#a0a0a0>{cell.element.Value} + {incoming} → sin entrada en la tabla</color>";

            string clase = e.style == ReactionStyle.Dominancia ? "reacción" : "combinación";
            string deja = e.TerrainAfter.HasValue ? e.TerrainAfter.Value.ToString() : "neutro";
            string bando = e.NoBando ? " · <color=#ff9090>sin bando</color>" : "";
            return $"{cell.element.Value} + {incoming} → <b>{e.reactionName}</b> ({clase}, deja {deja}){bando}";
        }
    }
}
