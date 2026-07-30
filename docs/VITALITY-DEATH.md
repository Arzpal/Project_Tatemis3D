# Tatemis3D — Vitalidad, muerte y derrota

> **TL;DR:** Los tatemis pueden morir de verdad. Salvajes: solo como **elección deliberada** del jugador (seguir golpeando tras el noqueo, con una **ventana de gracia** para decidir → [TAMER.md §4.1](TAMER.md)). Propios: por una barra de **Vitalidad** externa al combate que drena **poco al ser noqueados** y **mucho si les pegan estando caídos**; a 0 → muerte permanente, pero **siempre evitable** jugando bien (swap de emergencia sin cooldown + defender al caído). Derrota total = el Tamer es perseguido y debe escapar.
> **Estado: 🟡 en discusión** — diseñado en sesión con PA; pendiente ratificar con PB/DS. Nada de esto entra en Fase 0–1.
> **Mapa:** §1 principios · §2 vitalidad · §3 muerte de salvajes · §4 noqueo/rescate/swap · §5 derrota total y Tamer · §6 abiertas (IA aliada, manadas) · §7 impacto en docs y roadmap

## 1. Principios
1. **La muerte existe para dar peso al vínculo** (pilar 2 del GDD): el miedo a perder se convierte en conducta protectora, no en castigo.
2. **Regla de oro — siempre evitable:** un tatemis propio muere por **negligencia acumulada, nunca por un solo error**. Toda regla de este doc debe preservar esa promesa; si una situación hace la muerte inevitable, la regla está mal diseñada.
3. **Armonía como elección, no como regla:** el sistema te *permite* matar; el tema te pide no hacerlo (tesis HTTYD). Un tema que puedes traicionar pesa más que uno impuesto.

## 2. Vitalidad (barra externa al combate) ✅ decidido
- Cada tatemis propio tiene **Vitalidad**, separada de la Vida de combate. La Vitalidad **persiste entre combates**.
- **Dos fuentes de drenaje, y solo dos** *(la segunda añadida el 2026-07-22)*. El daño normal de combate **nunca** la toca:

| Qué | Drenaje | Por qué |
|---|---|---|
| **Que lo noqueen** | **bajo** | Dejar que caiga ya es negligencia, aunque el rescate salga perfecto. Sin esto, noquearse infinitas veces sale gratis mientras rescates bien, y la "negligencia acumulada" no acumula nada |
| **Golpes recibidos estando noqueado** (§4) | **alto** | Es la herida grave de verdad: tu compañero indefenso en el suelo mientras siguen pegándole |

> ⚠️ **Guardarraíl de la regla de oro (§1.2):** el drenaje por noqueo tiene que ser **pequeño de verdad**. Quien está aprendiendo se noquea mucho, y si eso solo drena, la muerte deja de ser evitable y pasa a ser una cuenta atrás. La proporción entre ambas fuentes es lo que decide si el sistema castiga *aprender* o castiga *abandonar a tu compañero* — y solo debe castigar lo segundo.
- **Vitalidad 0 → muerte real y permanente.**
- Se recupera **fuera de combate**. *Propuesta:* recuperación por **cuidado activo** (descanso en base, alimentar, ritual), no solo por tiempo — cuidar de tu criatura *es* el vínculo hecho mecánica. Tiempos por definir (ojo: con equipo de 3, una recuperación larga te deja sin roster).
- *Propuesta — sin espiral de muerte:* la Vitalidad baja **no debuffea stats** en combate; solo se comunica visualmente (cojea, vendajes, postura). Vitalidad baja = estar más cerca de la muerte, no ser peor peleando.

## 3. Muerte de criaturas salvajes (elección del jugador) ✅ decidido
- Tras noquear a un salvaje, **seguir golpeándolo lo mata**. Nunca es un accidente: es una decisión deliberada sobre un enemigo ya indefenso.
- El flujo de captura (GDD §8.2) no cambia: noqueo → ventana → Sludge.
- *Gancho macro (propuesta):* matar afecta la **percepción de la aldea / armonía** — conecta con el loop macro existente.

> 🟡 **Sin resolver (2026-07-29) — ¿y si el que remata es el AMBIENTE?** Tras la reformulación elemental, las combinaciones **no tienen bando** y las zonas **duran** ([ELEMENTAL-REACTIONS §0.7](ELEMENTAL-REACTIONS.md)), así que un cuerpo noqueado puede estar comiéndose los pulsos de una zona **que ya estaba puesta antes del KO**. Eso choca a la vez con tres reglas ya decididas: aquí, que matar a un salvaje sea **siempre deliberado** · en [TAMER §4](TAMER.md), que *"mata quien decide seguir pegando"* · y en §4 de este doc, que los golpes al caído sean **heridas graves** que drenan Vitalidad — con lo que un compañero podría morir por una zona del suelo, contra la **regla de oro** (§1.2). Salidas posibles, **ninguna dictada**: que las zonas no toquen a los noqueados · que toquen pero no cuenten como remate · que la gracia invulnerable de [TAMER §4.1](TAMER.md) las ignore también. **Decide Arzpal.**

## 4. Noqueo, swap de emergencia y rescate del caído
- **KO del tatemis activo → swap de emergencia que ignora el cooldown.** El cooldown del swap castiga el swap *táctico*, no el rescate. ✅ decidido
- El **cuerpo del noqueado queda en el suelo** un tiempo; mientras esté expuesto, los golpes que reciba son **heridas graves** (drenan Vitalidad). El tatemis que entra **defiende al caído**. ✅ decidido
- **Recogida:** el cuerpo vuelve a su Tótem al recogerlo (acercarse / canalizar) o por auto-retorno tras X segundos. Método y tiempos por definir.
- *Quién remata (propuesta narrativa):* los salvajes **rara vez se ensañan** con un caído (drenaje bajo en combates normales); los tamers de la **aldea rival** sí ordenan rematar → ellos son la amenaza real y el espejo oscuro del jugador (ellos matan; tú eliges no hacerlo).

## 5. Derrota total y el Tamer ✅ decidido (núcleo)
- Con un tatemis en combate, el Tamer sigue siendo **intocable** (GDD §6.1).
- Si **los 3 caen** (noqueados o heridos): el combate termina y el enemigo **persigue al Tamer** durante un tiempo. Objetivo del jugador: **escapar**. Referencia: Pokémon Leyendas: Arceus.
- *Regla necesaria (propuesta):* al caer el tercero, **todos los cuerpos regresan automáticamente a sus Tótems antes de la persecución** — imprescindible para la regla de oro (§1.2); si quedaran expuestos mientras huyes, habría muertes inevitables.
- El Tamer tiene una **barra de vida invisible** (sin HUD; se comunica con trompicones / efectos de pantalla). Si llega a 0 → Tamer noqueado → es "rescatado" → despierta en la **base**. ✅ decidido
- *Propuestas:* la huida es simple — correr/esquivar, sin combate del Tamer (consistente con GDD §6.1: combate del Tamer = 🔮); el enemigo desiste por tiempo/distancia (leash); la penalización es el viaje/tiempo perdido, sin pérdida de objetos por ahora.

## 6. Preguntas abiertas 🟡

### 6.1 ¿Varios tatemis en campo? (controlas 1, los otros 2 con IA)
Motivación: que Tatemis se sienta más **libre y natural** que Pokémon — tu equipo pelea contigo, no espera en cola.
- **A favor:** el rescate del caído ya pone 2 en campo (es el primer paso natural); refuerza el vínculo porque tus compañeros están *presentes*.
- **Riesgos:** la IA aliada *autónoma* que se siente bien es de lo más caro en juegos de acción (aliados que estorban, comen AoEs o juegan tonto **dañan el pilar de vínculo**); legibilidad de cámara/lock-on con 3 aliados + enemigos; fricción con la regla de oro (si la IA deja que hieran a uno, ¿fue evitable para el jugador? — mitigable si recogerlo siempre está en tu mano).
- **Modelo propuesto — aliados por órdenes** *(ref. Guardianes de la Galaxia)*: los aliados **no pelean "bien" por su cuenta** — su comportamiento autónomo es defensivo/evasivo (chip damage, nunca facetank) y su valor se activa con **órdenes mid-combat** del jugador: provocar, colocar su habilidad firma, preparar o rematar combos. Esto baja el listón de la IA de "jugar bien sola" a "no morir tonta + ejecutar órdenes con precisión", y convierte a los aliados en **extensión del kit del jugador**: habilita **reacciones elementales entre compañeros** (ordenas al de Agua lanzar lo suyo y tú lo cruzas con Rayo **en el aire** → **Corriente**, [ELEMENTAL-REACTIONS §0.2b](ELEMENTAL-REACTIONS.md)) sin gastar el swap. El swap sigue importando: controlar directamente = expresión completa. Coste de diseño pendiente: presupuesto de inputs (posible rueda de órdenes con slow-mo).
- **Coste de sacar más tatemis = exposición, no progreso.** ✅ decidido — se descarta el incentivo de XP (evita que "jugar solo" sea la estrategia dominante y no requiere inventar una moneda de progresión que el GDD no tiene). Sacar a los 3 no cuesta crecimiento; cuesta **riesgo y atención**: controlas del todo a una, las otras 2 quedan expuestas, y no hay reservas en Tótem (si las 3 caen → persecución del Tamer, §5).
- **Comportamiento por defecto de los aliados.** ✅ decidido — sin órdenes, cada aliado pelea contra un enemigo **que no esté atacando al jugador/tatemis principal**: reparte la manada, mantiene tu duelo legible y nadie te roba el objetivo. *Consecuencia a validar:* contra un enemigo único (duelos/jefes) los aliados no atacan por defecto — solo participan vía órdenes.
- **Swap con aliados en campo = posesión.** ✅ decidido — mismo cooldown, pero es solo un cambio de cámara/control entre monstruos ya desplegados (el que sueltas pasa a IA con la regla por defecto). El swap de emergencia por KO sigue ignorando el cooldown (§4). Técnicamente: intercambiar PlayerBrain/AIBrain entre controllers — la interfaz *brain* de T0.3 lo cubre.
- ⚠️ **Requisito — legibilidad del peligro off-screen:** con aliados peleando sus propios duelos, un KO fuera de cámara debe ser **imposible de no notar** (cue de audio + indicador en HUD + ping/slow-mo breve). La regla de oro (§1.2) exige que siempre *sepas* que un compañero cayó y tengas tiempo real de reaccionar.
- **Staging (sin cambios):** el gate de Fase 1 se valida con **1 criatura activa** (como está planeado); las manadas enemigas (§6.2) entran primero; los aliados por órdenes = **experimento post-gate** (decisión de Nivel B). Preparación casi gratis ahora: al cablear **T0.3**, desacoplar `CreatureController` del `InputReader` tras una interfaz *brain* (PlayerBrain hoy, AIBrain después — una orden es solo otra fuente de input al brain).

### 6.2 Manadas de salvajes
- Compensación de dificultad (sobre todo si algún día hay 3 propios en campo): los salvajes aparecen en **manadas** — inicio más difícil **en cantidad**, con individuos más débiles.
- Encaja con el USP: AoEs, Zonificador/Controlador y reacciones ambientales **escalan** contra grupos (1-contra-muchos es la gramática natural del character-action).
- Simetría: **la manada defiende a su caído** igual que tú al tuyo — enseña la mecánica de rescate por espejo, y noquear a uno con la manada viva crea decisiones de captura emergentes.
- No requiere IA aliada: puede probarse desde **Fase 3** (IA mínima) con 1 criatura activa.

## 7. Impacto en docs y roadmap
- **GDD:** §6.1 ("Tamer no es objetivo", "1 criatura activa", swap) y §8.2 ("nunca se mata") quedan **🟡 en lo que este doc toca**; el GDD enlaza aquí.
- **Roadmap/TASKS:** nada entra en Fase 0–1. Primeros puntos de contacto: **Fase 3** (rematar/heridas al noqueado, manadas con IA mínima) y **Fase 4** (swap de emergencia). Única acción presente: sugerencia de interfaz *brain* en T0.3.
