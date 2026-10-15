using System.Collections.Generic;
using UnityEngine;

namespace Tatemis
{
    /// Qué clase de salida está cargando una batería. Decide **cuánto cuesta** llenarla y **qué pasa
    /// al llenarse** ([ELEMENTAL-REACTIONS §0.10](../../../../docs/ELEMENTAL-REACTIONS.md)).
    public enum ChargeKind
    {
        /// Tu mismo elemento. **1y, instantánea y sin cebado** — el carril rápido del sistema.
        Amplificacion,
        /// Suelo neutro. **2y** y la celda se vuelve elemental: es el plan B cuando no hay tablero.
        CargarNeutra,
        /// Cruzando triángulos. **3y** → zona que dura, y deja el suelo neutro.
        Combinacion,
        /// Dentro de tu triángulo. **5y** → el estallido grande, y deja el subproducto.
        Reaccion,
    }

    /// Qué acaba de pasar al depositar. Lo interpreta `ReactionSystem`, que es quien sabe disparar
    /// cada cosa; esta capa solo lleva la cuenta.
    public enum SaturationEvent
    {
        /// Depositó, pero falta. El impacto solo deja el **destello mudo** (§0.10).
        Cargando,
        /// Otro elemento le quitó la batería. Es el momento dramático del tira y afloja.
        CambiaDueno,
        /// Se acaba de llenar. Amplificación y carga de neutra **disparan ya**; reacción y combinación
        /// **quedan cebadas**.
        Lleno,
        /// Estaba cebada y este impacto la revienta.
        Detona,
    }

    /// **La capa que gobierna CUÁNDO ocurre una reacción** (ER §0.10). Un impacto elemental ya no
    /// dispara nada: **deposita saturación** en la batería del punto. Cuando se llena, la mancha queda
    /// **cebada** y el siguiente impacto la revienta.
    ///
    /// 🔑 **La batería es la MANCHA, no la celda.** Acertar es fácil, la explosión hereda la forma de
    /// ameba del terreno, y no hace falta agrandar las celdas. Las **neutras** no pertenecen a ninguna
    /// mancha, así que esas sí acumulan **por celda** — y por eso el terreno que te fabricas nace
    /// pequeño y débil frente al natural, que es el incentivo correcto.
    ///
    /// Es una clase plana, no un `MonoBehaviour`: la posee `ReactionSystem`, que la hace avanzar. Así
    /// no hay que tocar ningún builder de escena para que exista.
    public class TerrainSaturation
    {
        /// El medidor de una mancha (o de una celda neutra suelta).
        public class Battery
        {
            /// El elemento que la está cargando. **Un elemento distinto la BAJA en vez de subirla**, y
            /// al llegar a 0 se la queda.
            public Element owner;
            public ChargeKind kind;
            public float amount;        // en unidades `y`
            public float threshold;
            public bool primed;
            public float primedAt;
            public float lastTouch;
            /// Dónde nació el último depósito: la onda arranca **ahí**, no en el centro de la mancha,
            /// que es lo que deja al jugador apuntar la explosión.
            public HexCoord lastHit;
            public ICombatant instigator;
            public Vector3 direction;

            public float Fill01 => threshold <= 0f ? 0f : Mathf.Clamp01(amount / threshold);
        }

        readonly Dictionary<int, Battery> _byPatch = new Dictionary<int, Battery>();
        readonly Dictionary<HexCoord, Battery> _byCell = new Dictionary<HexCoord, Battery>();
        readonly List<int> _deadPatches = new List<int>();
        readonly List<HexCoord> _deadCells = new List<HexCoord>();

        ElementalGrid _grid;

        // ───────────────── Afinado (ER §0.10) ─────────────────

        /// La escalera dictada. **El ratio 1/2/3/5 está cerrado**; lo que se afina en playtest es el
        /// valor absoluto de `y` (ver `ReactionSystem.referenceImpact`).
        public float costAmplificacion = 1f;
        public float costCargarNeutra = 2f;
        public float costCombinacion = 3f;
        public float costReaccion = 5f;

        /// **Gracia antes de que empiece a bajar** ✅ *(playtest 2026-07-31)*. Sin esto la carga se
        /// evapora desde el frame siguiente al golpe, y **cargar se sentía como llenar un cubo agujereado**:
        /// el jugador no puede reposicionarse, ni encadenar, ni elegir cuándo seguir. Durante la gracia
        /// el medidor **se queda donde está** — solo se mueve si alguien lo sube o lo baja a propósito.
        public float holdSeconds = 4.5f;

        /// Lo que tarda una batería llena en vaciarse **una vez pasada la gracia**. **Hace falta**: sin
        /// decaimiento, al minuto 2 el tablero entero está cebado y vuelve el ruido que §0.10 vino a
        /// curar. Lento a propósito — el goteo disperso no debe llegar nunca a detonar.
        public float decaySeconds = 16f;

        /// Lo que aguanta cebada antes de **ventearse** (fogonazo débil, sin efecto). Es el guardarraíl
        /// que impide que el tablero se llene de bombas dormidas.
        public float primedSeconds = 5f;

        /// Se llama al ventearse una batería: `ReactionSystem` lo usa para el fogonazo flojo.
        public System.Action<Vector3, Element> Vented;

        public TerrainSaturation(ElementalGrid grid)
        {
            _grid = grid;
            if (_grid != null) _grid.CellsChanged += OnCellsChanged;
        }

        public void Dispose()
        {
            if (_grid != null) _grid.CellsChanged -= OnCellsChanged;
            _grid = null;
        }

        public float CostOf(ChargeKind kind)
        {
            switch (kind)
            {
                case ChargeKind.Amplificacion: return costAmplificacion;
                case ChargeKind.CargarNeutra:  return costCargarNeutra;
                case ChargeKind.Combinacion:   return costCombinacion;
                default:                       return costReaccion;
            }
        }

        // ───────────────── Depósito ─────────────────

        /// Deposita `y` unidades en la batería de este punto y dice qué ha pasado.
        ///
        /// **Tira y afloja:** si el elemento entrante no es el dueño, **resta**; al cruzar el cero se
        /// queda la batería con el sobrante. Eso hace que **defender salga mucho más barato que
        /// robar** (al que va por 4y de 5 le falta 1y; al rival le cuestan 4y vaciar más los suyos para
        /// llenar), y es deliberado: negar terreno es el último recurso, no el reflejo.
        public SaturationEvent Deposit(HexCoord cell, int patchId, Element incoming, ChargeKind kind,
                                       float y, ICombatant instigator, Vector3 direction,
                                       out Battery battery)
        {
            battery = Get(cell, patchId, incoming, kind);
            battery.lastTouch = Time.time;
            battery.lastHit = cell;
            battery.instigator = instigator;
            battery.direction = direction;

            // Cebada: cualquier impacto elemental la revienta, sea de quien sea. La reacción que sale
            // es **la que se cargó** — así montar la bomba sigue siendo mérito de quien la montó, pero
            // el rival puede hacerla estallar antes de tiempo y en sus términos.
            if (battery.primed) return SaturationEvent.Detona;

            bool robada = false;
            if (battery.owner != incoming && battery.amount > 0f)
            {
                battery.amount -= y;
                if (battery.amount > 0f) return SaturationEvent.Cargando;   // solo la ha frenado
                y = -battery.amount;                                        // el sobrante ya carga la suya
                robada = true;
            }

            if (battery.owner != incoming || battery.amount <= 0f)
            {
                battery.owner = incoming;
                battery.kind = kind;
                battery.threshold = CostOf(kind);
                if (battery.amount < 0f) battery.amount = 0f;
            }

            battery.amount += y;
            if (battery.amount < battery.threshold)
                return robada ? SaturationEvent.CambiaDueno : SaturationEvent.Cargando;

            // Llena. Amplificación y carga de neutra se consumen aquí mismo (no tienen cebado); las
            // otras dos quedan armadas y esperan a que el jugador elija el momento.
            if (kind == ChargeKind.Amplificacion || kind == ChargeKind.CargarNeutra)
                battery.amount -= battery.threshold;   // el sobrante se guarda: nada se desperdicia
            else
            {
                battery.primed = true;
                battery.primedAt = Time.time;
            }
            return SaturationEvent.Lleno;
        }

        /// Vacía la batería tras detonar. **La detonación no deposita nada** (ER §0.10): es el cobro,
        /// no una entrada — sin esto, el anillo que acaba de crear se cargaría con el propio estallido
        /// y tendrías dominó infinito.
        public void Consume(Battery b)
        {
            if (b == null) return;
            b.amount = 0f;
            b.primed = false;
        }

        Battery Get(HexCoord cell, int patchId, Element incoming, ChargeKind kind)
        {
            Battery b;
            if (patchId != 0)
            {
                if (_byPatch.TryGetValue(patchId, out b)) return b;
                b = New(incoming, kind);
                _byPatch[patchId] = b;
                return b;
            }
            if (_byCell.TryGetValue(cell, out b)) return b;
            b = New(incoming, kind);
            _byCell[cell] = b;
            return b;
        }

        Battery New(Element incoming, ChargeKind kind) => new Battery
        {
            owner = incoming, kind = kind, threshold = CostOf(kind), lastTouch = Time.time,
        };

        // ───────────────── Reloj ─────────────────

        /// Decaimiento y venteo. Lo llama `ReactionSystem.Update`.
        public void Tick()
        {
            float dt = Time.deltaTime;
            _deadPatches.Clear();
            _deadCells.Clear();

            foreach (var kv in _byPatch) if (Age(kv.Value, dt, PatchCenter(kv.Key))) _deadPatches.Add(kv.Key);
            foreach (var kv in _byCell) if (Age(kv.Value, dt, CellCenter(kv.Key))) _deadCells.Add(kv.Key);

            for (int i = 0; i < _deadPatches.Count; i++) _byPatch.Remove(_deadPatches[i]);
            for (int i = 0; i < _deadCells.Count; i++) _byCell.Remove(_deadCells[i]);
        }

        /// Devuelve true si la batería se ha vaciado del todo y puede olvidarse.
        bool Age(Battery b, float dt, Vector3 where)
        {
            if (b.primed)
            {
                if (Time.time - b.primedAt < primedSeconds) return false;
                b.primed = false;
                b.amount = 0f;
                Vented?.Invoke(where, b.owner);
                return true;
            }

            if (b.amount <= 0f) return true;
            // Periodo de gracia: la carga **se sostiene**, no se escurre. Solo la mueve alguien.
            if (Time.time - b.lastTouch < holdSeconds) return false;
            b.amount -= (b.threshold / Mathf.Max(0.5f, decaySeconds)) * dt;
            return b.amount <= 0f;
        }

        // ───────────────── Consultas para el overlay ─────────────────
        // Sin esto el sistema es INVISIBLE: el jugador no percibe carga, percibe que golpear el suelo
        // dejó de hacer nada (ER §0.10 y §6). No es pulido, es la mitad de la mecánica.

        public Battery At(HexCoord cell)
        {
            int patch = _grid != null ? _grid.PatchAt(cell) : 0;
            if (patch != 0) return _byPatch.TryGetValue(patch, out var b) ? b : null;
            return _byCell.TryGetValue(cell, out var c) ? c : null;
        }

        /// Las baterías vivas, para pintarlas. `SaturationVisual` recorre esto cada frame.
        public IReadOnlyDictionary<int, Battery> LivePatches => _byPatch;
        public IReadOnlyDictionary<HexCoord, Battery> LiveCells => _byCell;

        /// Borra toda la carga del tablero. Es la otra mitad del *reset* del laboratorio: medir un
        /// número exige poder repetir la misma prueba desde cero sin esperar al decaimiento.
        public void ClearAll() { _byPatch.Clear(); _byCell.Clear(); }

        public float Fill01(HexCoord cell) { var b = At(cell); return b != null ? b.Fill01 : 0f; }
        public bool IsPrimed(HexCoord cell) { var b = At(cell); return b != null && b.primed; }
        public Element? OwnerAt(HexCoord cell) { var b = At(cell); return b != null && b.amount > 0f ? b.owner : (Element?)null; }

        Vector3 PatchCenter(int patchId)
        {
            if (_grid == null) return Vector3.zero;
            var cells = _grid.PatchCells(patchId);
            if (cells.Count == 0) return Vector3.zero;
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < cells.Count; i++) sum += cells[i].ToWorld(_grid.cellRadius);
            return sum / cells.Count;
        }

        Vector3 CellCenter(HexCoord c) => _grid != null ? c.ToWorld(_grid.cellRadius) : Vector3.zero;

        /// Cuando el terreno se transforma, las manchas se renumeran: las baterías cuyo id ya no existe
        /// quedan huérfanas. Se tiran — y es lo correcto además de lo barato: **una mancha que acaba de
        /// reventar y cambiar de elemento no debe conservar la carga de lo que era antes**.
        void OnCellsChanged(IReadOnlyList<HexCoord> touched)
        {
            _deadPatches.Clear();
            foreach (var kv in _byPatch)
                if (_grid == null || _grid.PatchSize(kv.Key) == 0) _deadPatches.Add(kv.Key);
            for (int i = 0; i < _deadPatches.Count; i++) _byPatch.Remove(_deadPatches[i]);

            // Una celda neutra que acaba de volverse elemental ya pertenece a una mancha: su medidor
            // suelto sobra.
            for (int i = 0; i < touched.Count; i++)
                if (_grid != null && _grid.PatchAt(touched[i]) != 0) _byCell.Remove(touched[i]);
        }
    }
}
