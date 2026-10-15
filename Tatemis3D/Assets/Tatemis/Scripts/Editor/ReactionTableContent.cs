#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Autoría de la ReactionTable según **[ELEMENTAL-REACTIONS §4](../../../../docs/ELEMENTAL-REACTIONS.md)**,
    /// reformulada el **2026-07-29**: las 15 parejas **derivadas del verbo de cada elemento** (§0.1),
    /// no de ficción suelta. 30 entradas = 6 reacciones × 2 gatillos (R4) + 9 combinaciones × 2 sentidos.
    ///
    /// Reglas que esta autoría implementa por construcción:
    ///  · **Reacción** → daño **del elemento que manda** (pasa por la tabla de tipos) + **su verbo**.
    ///  · **Combinación** → daño **NEUTRO** + **los dos verbos**, y **sin bando** (§0.7).
    ///  · El **aturdimiento no se autora**: es proporcional al daño (GDD §8.2) — ver `ReactionEntry.Stagger`.
    ///  · Cada pareja tiene **una función que ninguna otra de las quince tiene**: eso es lo que arregla
    ///    el problema del diseño anterior, donde cuatro combinaciones eran la misma cosa.
    ///
    /// Números = **caja gris**, se afinan en playtest. El ancla: una reacción debe valer *casi lo que
    /// una habilidad promedio* (el mordisco de Ottorock pega 50+15), y el reparto objetivo del daño
    /// total es **30% básicos / 40% habilidades / 30% terreno**.
    public static class ReactionTableContent
    {
        const string Path = OttorockContent.Dir + "/ReactionTable.asset";

        /// Transparencia de TODA reacción, forzada en `Make` para que no se pueda olvidar en una
        /// entrada suelta: el terreno es el tablero y tiene que verse por debajo (§0.1).
        const float ZoneAlpha = 0.55f;

        /// Daño de referencia de una reacción (≈ una habilidad promedio). Las que además dejan zona
        /// pegan menos de golpe, porque su aporte se acumula en el tiempo.
        const float R = 45f;

        [MenuItem("Tatemis/Contenido/Generar ReactionTable")]
        static void GenerateMenu() => Generate();

        public static ReactionTable Generate()
        {
            if (!AssetDatabase.IsValidFolder(OttorockContent.Dir))
                AssetDatabase.CreateFolder("Assets/Tatemis", "Examples");

            var table = AssetDatabase.LoadAssetAtPath<ReactionTable>(Path);
            if (table == null)
            {
                table = ScriptObj();
                AssetDatabase.CreateAsset(table, Path);
            }

            var e = new List<ReactionEntry>();

            // ═══════════ LAS 6 REACCIONES ═══════════
            // Explosiones. Daño DEL ELEMENTO QUE MANDA + su verbo. Cada Fila() autora sus dos gatillos:
            // entra el predador → es suya (R6 le respeta) · entra la presa → es del terreno, SIN BANDO.

            // Naturaleza › Agua — las raíces beben el charco. Verbo: ENTRELAZAR.
            // Atrapa: no te mueves, pero sí atacas y bloqueas. Solo suelo (quien vuela se libra).
            // Es la mejor herramienta de SETUP del sistema: lo único que fija a alguien dentro de la
            // zona de otro. Sin zona residual — es un evento, no un sitio.
            // SUBPRODUCTO: Naturaleza — las raíces se beben el charco y se quedan. Es el ÚNICO caso en
            // que el subproducto coincide con el elemento del predador, y por eso en la dirección
            // "entra Agua sobre pasto" la mancha no cambia… pero **el anillo sí**: ahí su función es
            // EXPANDIR la mancha. Que la única reacción llamada *crecimiento* sea la única que hace
            // crecer terreno no es casualidad. Lectura preciosa: una criatura de Agua peleando sobre
            // hierba **riega el prado del rival**, justo en la pareja donde ella pierde.
            Fila(e, presa: Element.Agua, predador: Element.Naturaleza, "Crecimiento explosivo",
                 x => { x.burstDamage = R; x.subproduct = Element.Naturaleza;
                        x.burstStatus = StatusType.Enraizado; x.burstStatusSeconds = 1.3f;
                        x.debugColor = new Color(0.35f, 0.95f, 0.35f); });

            // Agua › Fuego — el agua apaga el fuego. Verbo: CORRIENTE, en vertical.
            // ELEVA: es la reacción que abre el juego aéreo, y lo que permite que un melee de Agua o de
            // Fuego alcance a un volador. El blanco puro es su firma exclusiva (§6: hay que poder
            // distinguirla de la niebla).
            // SUBPRODUCTO: Aire — el vapor.
            Fila(e, presa: Element.Fuego, predador: Element.Agua, "Géiser",
                 x => { x.burstDamage = R; x.burstLift = 13f; x.subproduct = Element.Aire;
                        x.debugColor = new Color(0.95f, 0.98f, 1f); });

            // Fuego › Naturaleza — arde. Verbo: PROPAGAR.
            // Se extiende poco a poco y SOLO por terreno de Naturaleza: el propio parche define hasta
            // dónde llega, sin ningún tope inventado. Fogonazo menor porque su aporte está en el tiempo.
            // SUBPRODUCTO: Mineral — la ceniza. Es el ejemplo canónico de que el tablero genera
            // parejas nuevas a mitad de combate: un incendio en un bioma de Naturaleza deja Mineral
            // donde no lo había, y con él parejas que la arena no ofrecía al empezar.
            Fila(e, presa: Element.Naturaleza, predador: Element.Fuego, "Incendio",
                 x => { x.burstDamage = R * 0.6f; x.subproduct = Element.Mineral;
                        x.baseDuration = 5f; x.zoneStatus = StatusType.Quemadura;
                        x.statusSeconds = 2.5f; x.statusTickDamage = 7f;
                        x.propagates = true; x.propagateThrough = Element.Naturaleza;
                        x.debugColor = new Color(1f, 0.45f, 0.12f); });

            // Mineral › Rayo — la roca hace de toma de tierra ("energía geo"). Verbo: PESO.
            // Zona de MÁS GRAVEDAD que imanta hacia abajo: ralentiza y arranca del aire lo que vuele.
            // Es la reacción ANTI-AÉREA del juego. Violeta y no amarillo, porque nace sobre la piedra
            // electrificada y el amarillo se fundía con ella (§6).
            // SUBPRODUCTO: Fuego — la roca se sobrecalienta con la carga que acaba de absorber.
            Fila(e, presa: Element.Rayo, predador: Element.Mineral, "Energía geo",
                 x => { x.burstDamage = R * 1.1f; x.subproduct = Element.Fuego;
                        x.baseDuration = 4f; x.zoneStatus = StatusType.Pesado; x.statusSeconds = 2f;
                        x.debugColor = new Color(0.60f, 0.30f, 0.95f); });

            // Rayo › Aire — la tormenta domina el cielo abierto. Verbo: PARALIZAR.
            // CORTA EL COMBO: ticks por reloj que muerden el arranque de tus ataques. No te quita vida,
            // te quita la captura — sin cadena, el aturdimiento decae. Ya no es gemela de la anterior.
            // SUBPRODUCTO: Agua — la lluvia que viene después de la tormenta.
            Fila(e, presa: Element.Aire, predador: Element.Rayo, "Descarga de tormenta",
                 x => { x.burstDamage = R; x.subproduct = Element.Agua;
                        x.baseDuration = 4f; x.zoneStatus = StatusType.Interferido; x.statusSeconds = 2.5f;
                        x.debugColor = new Color(0.75f, 0.55f, 1f); });

            // Aire › Mineral — el viento erosiona la roca. Verbo: NUBELIZAR.
            // Tormenta de ARENA: tapa la visión (verbo de Aire) y araña porque está hecha de roca
            // pulverizada (materia de Mineral). **NO ralentiza** — ralentizar es el verbo de Mineral y
            // aquí Mineral no manda (§0.1, la regla que impide que los verbos se contaminen).
            // Es la que se come Ottorock: sobre corrientes de aire sus propios ataques le dejan ciego.
            // SUBPRODUCTO: Rayo — la arena en suspensión **se carga de estática**. No es una excusa:
            // las tormentas de polvo generan campos eléctricos fuertes por fricción entre partículas.
            // Mismo método que produjo los verbos de §0.1 — sacar la mecánica del fenómeno real.
            Fila(e, presa: Element.Mineral, predador: Element.Aire, "Tormenta de arena",
                 x => { x.burstDamage = R * 0.85f; x.burstKnockback = 6f; x.subproduct = Element.Rayo;
                        x.baseDuration = 5f; x.zoneStatus = StatusType.Cegado; x.statusSeconds = 2f;
                        x.zoneTickDamage = 4f; x.radiusScale = 1.4f;
                        x.debugColor = new Color(0.78f, 0.72f, 0.60f); });

            // ═══════════ LAS 9 COMBINACIONES ═══════════
            // Transformaciones. Daño NEUTRO (no manda ninguno) y SIN BANDO: te afectan a ti también,
            // que es lo que convierte *dónde* las pones en una jugada.

            // ── Las tres de Mineral: EL MENÚ DE OTTOROCK ──
            // Él elige cuál se lleva sellada según dónde agarre, así que necesitan radio y duración
            // MUY distintos o siempre elegiría la misma: arenas amplia y corta · colada estrecha y
            // direccional · cristalización pequeña y larga.

            // Mineral + Agua — peso sobre corriente = fluido denso que te traga. ANTI-FUGA.
            // 🟡 APROXIMACIÓN: el diseño dice "el dash sale entero pero la recuperación al aterrizar se
            // alarga", y el juego **no tiene esquiva** todavía. Hasta que la tenga se aproxima con
            // `Pesado`, que es el verbo correcto (Mineral) aunque no la mecánica exacta.
            Combo(e, Element.Agua, Element.Mineral, "Arenas movedizas",
                  x => { x.baseDuration = 3f; x.radiusScale = 1.45f;
                         x.zoneStatus = StatusType.Pesado; x.statusSeconds = 2.5f;
                         x.zoneTickDamage = 4f;
                         x.debugColor = new Color(0.50f, 0.36f, 0.20f); });

            // Mineral + Fuego — el fuego con peso encima no salta de mata en mata: SE VIERTE.
            // La ÚNICA zona que avanza: repta en la dirección del golpe comiéndose el suelo jugable, y
            // **se apaga al llegar a una celda de Agua** (no es reacción encadenada, es su muerte).
            // Es lo único del sistema que devuelve distancia a quien se le echan encima.
            Combo(e, Element.Fuego, Element.Mineral, "Colada",
                  x => { x.baseDuration = 6f; x.radiusScale = 0.7f;
                         x.directional = true; x.advanceSpeed = 2.4f;
                         x.zoneStatus = StatusType.Quemadura; x.statusSeconds = 2f; x.statusTickDamage = 8f;
                         x.debugColor = new Color(1f, 0.62f, 0.15f); });

            // Mineral + Naturaleza — entrelazar deja actuar; peso dice que lo que arranca no se detiene.
            // TE QUITA EL CANCEL: sigues peleando a velocidad normal, pero pagas cada acción entera.
            // Espejo exacto de Rayo (él te corta el golpe, esta no te deja cortarlo a ti).
            // 🚧 APARCADA: necesita el **cancel de recuperación**, que `AbilityCaster` no tiene. Se
            // autora ya para que exista como dato y como zona visible; su efecto real llega con él.
            Combo(e, Element.Naturaleza, Element.Mineral, "Cristalización",
                  x => { x.baseDuration = 7f; x.radiusScale = 0.6f;
                         x.zoneTickDamage = 5f;
                         x.debugColor = new Color(0.62f, 0.78f, 0.72f); });

            // ── Las tres de Rayo ──

            // Rayo + Agua ★ — LA ESTRELLA DEL PITCH. Cinta transportadora eléctrica.
            // El ÚNICO arrastre del sistema: lámina direccional —la apuntas con el vector de tu golpe—
            // que lleva a lo que pise, con inercia decreciente al salir; y encima el tick de Rayo
            // mordiéndote el arranque de los ataques, nunca el movimiento. Puedes salir siempre, pero
            // gastas la huida en irte en vez de en contraatacar.
            Combo(e, Element.Agua, Element.Rayo, "Corriente",
                  x => { x.baseDuration = 5f;
                         x.directional = true; x.dragSpeed = 5.5f;
                         x.zoneStatus = StatusType.Arrastrado; x.zoneStatus2 = StatusType.Interferido;
                         x.statusSeconds = 1.5f;
                         x.debugColor = new Color(0.45f, 0.85f, 1f); });

            // Rayo + Fuego — un tick de más (Rayo) + el daño extendido en el tiempo (Fuego).
            // AMPLIFICA: cada impacto dentro de la zona lo sigue un eco que repite un % del golpe.
            // Junto a la niebla, **una de las dos zonas que QUIERES pisar** — hasta aquí el ambiente
            // solo sabía castigar. Sin bando, así que quien gane el intercambio gana más.
            // ⚠️ El eco NO irradia al terreno: ya es la combinación, y encadenar rompería R1.
            Combo(e, Element.Fuego, Element.Rayo, "Mini explosiones",
                  x => { x.baseDuration = 4.5f; x.echoPercent = 0.35f;
                         x.debugColor = new Color(1f, 0.72f, 0.20f); });

            // Rayo + Naturaleza — Rayo pone su metrónomo, Naturaleza el anclaje. ANTI-KITING.
            // Tirones a intervalos que te devuelven al punto donde te alcanzó — cada víctima con su
            // propia estaca. Discreto y no continuo A PROPÓSITO: una fuerza constante contra tu input
            // se lee como mandos rotos; un tirón se lee como acontecimiento. Es un ring, no un charco.
            Combo(e, Element.Naturaleza, Element.Rayo, "Amarra",
                  x => { x.baseDuration = 5f; x.tetherPull = 10f; x.zoneTickDamage = 3f;
                         x.debugColor = new Color(0.65f, 0.95f, 0.35f); });

            // ── Las tres de Aire: el elemento que NUBELIZA a los demás ──

            // Aire + Agua — DESINFORMA. Capa baja, ancha y opaca: a través de ella **no se puede FIJAR
            // objetivo**, y esconde igual a quien está dentro que a quien mira desde fuera. La otra
            // zona que quieres pisar. Los proyectiles ya guiados no se pierden: se corta la
            // adquisición, no el guiado. Gris azulado para que el blanco puro sea firma del géiser.
            Combo(e, Element.Agua, Element.Aire, "Niebla",
                  x => { x.baseDuration = 6f; x.radiusScale = 1.5f;
                         x.breaksTargeting = true; x.zoneTickDamage = 2f;
                         x.debugColor = new Color(0.62f, 0.68f, 0.78f); });

            // Aire + Fuego — el fuego nubelizado no arde: SE ACUMULA. TEMPORIZADOR.
            // La única zona que no hace nada mientras dura y **deflagra de golpe al final**, en todo el
            // volumen y aire incluido — por eso es la única que alcanza a un volador, pero como evento
            // puntual del que puedes irte. Detona solo por su reloj, nunca por golpes, o Pyrecko
            // tendría un detonador gratis encima de su kit de marcas. **Invierte el delay de §0.2.**
            Combo(e, Element.Fuego, Element.Aire, "Mecha",
                  x => { x.baseDuration = 4f; x.radiusScale = 1.3f;
                         x.deflagrates = true; x.burstDamage = 60f;
                         x.debugColor = new Color(1f, 0.45f, 0f); });

            // Aire + Naturaleza — la maraña deja el suelo y queda COLGANDO. INTERDICTA LO LANZADO.
            // Se atraviesa andando, saltando y dasheando sin efecto: lo que se come es el **proyectil
            // elemental que la cruza**, que detona ahí. No es mecánica nueva — el velo se comporta como
            // un elemento estacionario para la regla de choques (§0.2b). Contrajuego natural contra un
            // Zonificador. 🚧 Hoy **no hay proyectiles** en el slice, así que su efecto no se puede
            // probar todavía; llega con Ottorock-Sujeción (lanzar la roca) y con Pyrecko.
            Combo(e, Element.Naturaleza, Element.Aire, "Velo de esporas",
                  x => { x.baseDuration = 6f; x.radiusScale = 1.8f;
                         x.blocksProjectiles = true; x.zoneTickDamage = 2f;
                         x.debugColor = new Color(0.60f, 0.95f, 0.50f); });

            table.entries = e;
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ReactionTable] {e.Count} entradas autoradas con las 15 parejas NUEVAS " +
                      "(6 reacciones × 2 gatillos + 9 combinaciones × 2 sentidos). " +
                      "Aparcadas por falta de sistema: Cristalización (no hay cancel) y " +
                      "Velo de esporas (no hay proyectiles). Aproximada: Arenas movedizas (no hay esquiva).");
            return table;
        }

        static ReactionTable ScriptObj() => ScriptableObject.CreateInstance<ReactionTable>();

        /// Una FILA de reacción autora sus **dos gatillos** (R4). En las dos direcciones **manda el
        /// predador**: si entra él la reacción es suya y R6 le respeta; si entra la presa, la reacción
        /// sigue siendo la del predador (que aquí es el terreno) pero **sin bando**. Por eso el daño
        /// lleva SU tipo en los dos casos (§0.2).
        static void Fila(List<ReactionEntry> list, Element presa, Element predador, string name,
                         System.Action<ReactionEntry> payload)
        {
            list.Add(Make(terrain: presa, incoming: predador, name, ReactionStyle.Dominancia, false, predador, payload));
            list.Add(Make(terrain: predador, incoming: presa, name + " (provocada)", ReactionStyle.Dominancia, true, predador, payload));
        }

        /// Una combinación cross-triángulo aplica igual en los dos sentidos (nadie supera a nadie), y
        /// por eso su daño es **neutro** y **no tiene bando**: te afecta a ti también (§0.7).
        static void Combo(List<ReactionEntry> list, Element a, Element b, string name,
                          System.Action<ReactionEntry> payload)
        {
            list.Add(Make(terrain: a, incoming: b, name, ReactionStyle.Combinacion, false, a, payload));
            list.Add(Make(terrain: b, incoming: a, name, ReactionStyle.Combinacion, false, b, payload));
        }

        static ReactionEntry Make(Element terrain, Element incoming, string name, ReactionStyle style,
                                  bool friendlyFire, Element dominant, System.Action<ReactionEntry> payload)
        {
            var x = new ReactionEntry
            {
                terrain = terrain, incoming = incoming, reactionName = name,
                style = style, friendlyFire = friendlyFire, dominantElement = dominant,
            };
            payload(x);
            // La transparencia se impone aquí y no entrada a entrada: los colores se escriben con
            // `new Color(r,g,b)`, que deja alpha 1 = opaco, y una sola que se escape tapa el tablero.
            x.debugColor = new Color(x.debugColor.r, x.debugColor.g, x.debugColor.b, ZoneAlpha);
            return x;
        }
    }
}
#endif
