# Tatemis3D — Criaturas (índice + plantilla)

> **TL;DR:** Una ficha por especie, **cada una en su propio doc** (`CREATURE-*.md`; se dividió al superar el tope de ~12 KB de [INDEX.md](INDEX.md)). El kit es de **3 habilidades** (Movilidad · Flex · Signature) + universales (ataque básico, salto, bloqueo/forma elemental, noqueo contextual) — ver GDD §6.3. De las fichas reales se **extrae** el sistema de habilidades ([ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md); estrategia *bespoke*-primero de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)).
> **Mapa:** §1 especies (índice) · §2 plantilla (los 18 campos) · §3 por qué este trío · §4 báscula de arquetipos (hexágono de stats)

## 1. Especies

| Especie | Ficha | Rol | Estado |
|---|---|---|---|
| **Pyrecko** — Zonificador de Fuego | [CREATURE-PYRECKO.md](CREATURE-PYRECKO.md) | Fase 4 del prototipo · candidato a starter de Fuego | 🟢 dictada (2 abiertas menores) |
| **Ottorock** — Luchador de Mineral | [CREATURE-OTTOROCK.md](CREATURE-OTTOROCK.md) | **Criatura del gate (T1.4)** — empezar por su modo Libre · ✅ **(2026-07-29) Ottorock COMPLETO (con Sujeción) entra en el slice, y va ANTES que Pyrecko y Mantagale** | 🟢 dictada (abiertas menores) |
| **Mantagale** — Controlador de Aire (mantarraya aérea) | [CREATURE-MANTAGALE.md](CREATURE-MANTAGALE.md) | Post-prototipo (Nivel B) · informa la arquitectura de vuelo | 🟢 kit cerrado (§C) · ficha reescrita · stats fijados (§4) · 🆕 **pasiva de peces espíritu ✅ (30/07)** = vuelo resuelto · quedan números de habilidad (§C.6) |

> **Cantera de ideas sin ficha:** las ideas de monstruos aún sin dictar viven en [CREATURE-IDEAS.md](CREATURE-IDEAS.md); cuando una madura se le hace su `CREATURE-*.md` y entra a esta tabla.
> **Modelo de roster ✅ (2026-07-26):** roster completo = **matriz tipo × arquetipo = 6×5 = 30 especies**, una por celda ([CREATURE-IDEAS §1](CREATURE-IDEAS.md)). **Slice ✅: 6 especies, una por elemento** — la de Agua (olas) entra al slice; componer el sexteto (elegir el Rayo · qué pasa con el Coloso) → cola de [TASKS](TASKS.md).

## 2. Plantilla de ficha (los 18 campos)

> **Principio de kits (2026-07-17): toda especie es autosuficiente** — cualquier tatemis debe poder ganar un combate por sí solo; el CC/soporte prepara su **propio** daño además del del equipo (ref. Aurelion Sol para el patrón "control + castigo" de Mantagale). Aplica también a Controladores y a los futuros Colosos.

> **Principio de Controladores** *(2026-07-26 · ✏️ **reescrito el 2026-07-29**)*: **el Controlador controla con fenómenos de su elemento convertidos en KIT** — marcas de especie con lógica propia: los tornados/Ciclón de Mantagale, la estela del Collie, las olas y la resaca del castor ([CREATURE-IDEAS](CREATURE-IDEAS.md)). Agarres, placajes y estampes físicos pueden existir como detalle en cualquier kit (el Ave agarra, el Topo estampa), pero no ser el medio del arquetipo.
>
> ⚠️ **Ni «con el elemento puro» ni «con reacciones».** La redacción anterior —*«el Controlador controla con su elemento puro»*— queda **derogada** por [ELEMENTAL-REACTIONS §0.2](ELEMENTAL-REACTIONS.md): **un elemento puro solo hace daño**, y todo el CC elemental (atrapar, arrastrar, tirón, peso…) nace **únicamente de la mezcla**. Pero el CC del arquetipo **tampoco sale de las reacciones**: 🔑 **el verbo gobierna las REACCIONES, el arquetipo gobierna el KIT** ([ARCHETYPES §1](ARCHETYPES.md), donde vive la versión larga de este principio). Las dos capas conviven y por eso **el Controlador sigue controlando en arena neutra**, que es lo que exige el principio de autosuficiencia de arriba.

> **Principio de sinergia de equipo** 🆕 ✅ *(dictado 2026-07-30)*: **las sinergias apoyan COMBOS GENERALES, no las habilidades únicas de cada monstruo.** Lo que un compañero aporta se cruza con el tablero y con las reglas comunes (el terreno, las reacciones, el choque de elementos), **nunca alimenta la mecánica insignia de otra especie**. Caso que lo estrenó: la pasiva de vuelo de **Mantagale** solo cobra por **las mezclas que provoca ella** — las de sus aliados no le dan peces ([ficha](CREATURE-MANTAGALE.md) campo 7; deroga la decisión del 26/07 de que «montaba también las corrientes de sus compañeros»). Es lo que protege el **principio de kits** de arriba: si el kit propio se alimentase del compañero, la autosuficiencia pasaría a depender de la composición del equipo.

**Identidad**
1. **Nombre** (placeholder) + **concepto en 1 frase** — la fantasía: qué se siente pelear *siendo* él.
2. **Elemento** — Naturaleza / Agua / Fuego / Mineral / Rayo / Aire. 🔑 **Cada elemento tiene UN verbo** ✅ *(2026-07-29, [ER §0.1](ELEMENTAL-REACTIONS.md))*, y de él se deriva todo lo que provoca al mezclarse: **Fuego = propagar** · **Agua = corriente (arrastra)** · **Naturaleza = entrelazar (bloquea el movimiento, deja atacar y bloquear)** · **Mineral = peso (ralentiza y ancla al suelo)** · **Rayo = paralizar (ticks por reloj que cortan el arranque, no el movimiento)** · **Aire = nubelizar (vuelve nube el otro elemento: más área y tapa la visión)**. ⚠️ **El verbo NO es del kit**: un elemento puro **solo hace daño** ([ER §0.2](ELEMENTAL-REACTIONS.md)) y el verbo solo aparece **al mezclarse** (terreno · choque entre elementos · roca sellada). Al llenar los campos 8–11 hay que **comprobar que el kit no contradiga el verbo** de su elemento, aunque no lo imite (ARCHETYPES §1).
3. **Arquetipo** — Coloso / Luchador / Ejecutor / Zonificador / Controlador.
4. **Silueta y tamaño** — bípedo/cuadrúpedo/serpentino…, escala vs el maniquí (sirve para el grey-box y para el concept artist).

**Stats y feel** *(relativo: bajo / medio / alto)*

5. **Vida / Daño / Velocidad** — definen el arquetipo.
6. **Peso percibido** — afecta hit-reacts, salto y el "sonido" del personaje.
7. **Locomoción** — ágil o pesada, cómo salta, rasgo propio de movimiento (opcional).

**Kit — las 3 habilidades**
> Para cada una: qué hace en 1–2 frases · **coste** de energía relativo · **targeting** (self / dirección / proyectil / área / objetivo) · **qué elemento irradia** al impactar (→ reacción contra el terreno, [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)) y si además **siembra marca de especie**.

8. **Movilidad** — además hace de "dodge".
9. **Flex** — decidir: ¿ataque directo **o** efecto (CC / buff / AoE / debuff)?
10. **Signature** — la única de la especie; aquí vive la personalidad.

**Universales con sabor propio**

11. **Ataque básico** — nº de golpes de la cadena, alcance (corto/medio), ritmo.
12. **Bloqueo / forma elemental** — aspecto de la forma pura + su detalle propio (duración / mitigación). Recordar: en forma pura su debilidad pasa de **1.5x a 2x** (GDD §7.1).

**Rol en combate**

13. **Plan de juego** — cómo gana y su debilidad táctica (más allá del elemento).
14. **Sinergia ambiental** — 1 ejemplo concreto con el terreno/clima de la arena de pruebas.
15. **Como salvaje (enemigo)** — 2–3 comportamientos de IA + su telegraph legible.

**Futuro** *(1 frase cada uno; opcional por ahora)*

16. **Orden rápida como aliado** — qué hace al tap de LB ([VITALITY-DEATH.md](VITALITY-DEATH.md) §6.1).
17. **Formas evolutivas** — nombres/idea + idea de mini-misión (GDD §9.2).
18. **Rareza** — fase inicial difícil ↔ techo de stats.

## 3. Por qué este trío

- **Complejos a propósito** (decisión 2026-07-17): la pareja del prototipo estresa el futuro sistema de habilidades por lados distintos, para descubrir sus límites antes de estandarizar (estrategia *bespoke*-primero de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)). **Pyrecko:** manchas con TTL, DoT, sigilo, homing. **Ottorock:** stances con variantes por modo, vida extra temporal, combinación sellada portátil. **Mantagale:** vuelo, desplazamiento de enemigos, cargas/canalización, hazard móvil que **estalla por choque de fuerzas** (los 3 puntos ciegos de [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) §7). *(✏️ 2026-07-29: era «hazard móvil que dispara reacciones en cadena» — derogado ya el 18/07, el tornado es **inerte mientras viaja**, y R1 impide encadenar reacciones.)*
- **✅ Gate (T1.4) = Ottorock en modo Libre** (melee puro; sin reacciones **porque la arena del gate es neutra — regla R5**, no porque Libre no irradie); Sujeción post-gate; Pyrecko en Fase 4; Mantagale post-prototipo (en papel desde ya para que cámara/lock-on/locomoción **no asuman "solo suelo"**).
- **Contraste de arquetipos:** zonificador a distancia vs luchador melee vs controlador aéreo → el criterio de Fase 4 y del slice ("criaturas claramente diferentes, mismo framework") se valida al máximo.
- **Elementos:** Fuego (1er triángulo) + Mineral y Aire (2º). Fuego es neutral con ambos ✓. ⚠️ **Aire › Mineral:** Mantagale countea a Ottorock dentro del propio equipo (el caso que se evitó con Agua/Fuego — aceptado por ahora, revisar en balance).
- **Cruces intra-equipo** *(actualizado 2026-07-18 con el modelo de terreno fijo — [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)):* la anti-sinergia Mineral↔combustible **desapareció** (las reacciones ya no limpian marcas) y el combo "Mantagale empuja el fuego de Pyrecko" **también** (no hay zonas que mover). Lo que queda es más limpio: los tres **irradian elementos distintos sobre el mismo tablero**, así que el equipo cubre más filas de la tabla en la misma arena. ⚠️ **Nuevo cruce a vigilar (2026-07-29):** con [ER §0.2b](ELEMENTAL-REACTIONS.md) **los elementos también chocan en el aire**, y esa mezcla **no es de nadie** (afecta a los dos). Como los tres irradian elementos distintos, sus entregas se cruzan en vuelo y generan combinaciones sin bando **sobre su propio equipo**: Fuego×Mineral → **colada**, Fuego×Aire → **mecha**, Mineral×Aire → **tormenta de arena** (aquí manda el Aire, así que es reacción y castiga al Ottorock). Es fuego amigo estructural, no un bug.
- **El USP de reacciones lo venden los tres, cada uno por una vía distinta:** Pyrecko (marcas de especie + detonación que irradia Fuego), Ottorock (**combinación sellada portátil**: elige su combinación eligiendo dónde agarra), Mantagale (**lleva al enemigo al terreno** y le detona el tornado encima con el Ciclón). *(✏️ 2026-07-29: la vía de Mantagale ya no es «tornado que encadena reacciones conforme recorre el terreno» — derogado.)*
- *Nota:* los tres cubren solo 3 de los 5 arquetipos; Coloso y Ejecutor quedan para las especies 4–5 del slice (diseño en papel, sin fecha).

## 4. Báscula de arquetipos (hexágono de stats)

> **Fijado 2026-07-18.** Los 6 ejes en los que se mide **toda** especie del roster (en papel). Cada arquetipo tiene un eje que "firma"; el 6º (Sinergia elemental) es el **USP ambiental**, transversal. Escala **0–10**, relativa al roster con headroom (nadie llega a 10 salvo un extremo deliberado).

| Eje | Qué mide | Arquetipo que lo firma |
|---|---|---|
| **Vida** | Aguante / supervivencia | Coloso |
| **Daño** | Output ofensivo (sostenido + burst) | Luchador |
| **Velocidad** | Movilidad / reposicionamiento | Ejecutor |
| **Alcance** | Rango efectivo / negación de área | Zonificador |
| **Control** | CC **del kit**: empujes, agarres, anclajes, cortes de acción… | Controlador |
| **Sinergia elemental** | Cuánto depende su plan del terreno + reacciones | *(USP, transversal)* |

> *Efectos/status (buffs/debuffs) se contabilizan dentro de **Control** + **Sinergia**, no como eje propio (decisión de mantener 6 ejes limpios; revisable).*
>
> ✏️ **(2026-07-29) El eje Control mide el CC que la especie pone POR SÍ MISMA** (su kit), no el que provoca el terreno — eso lo mide **Sinergia elemental**. Es la misma frontera de ARCHETYPES §1: *el verbo gobierna las reacciones, el arquetipo gobierna el kit*. La definición decía «empujes, roots, stuns, slows»; se quitó **«slows»** porque **ralentizar es ahora el verbo exclusivo de Mineral** ([ER §0.1](ELEMENTAL-REACTIONS.md)) y leerlo como CC genérico de kit reintroduce justo la contaminación que §0.1 existe para evitar. 🟡 **Sin dictar:** si el vocabulario de CC del proyecto se unifica del todo al del modelo vigente (**atrapar · arrastrar · tirón · pesar/anclar · cortar el arranque · tapar visión**) o el kit conserva vocabulario propio. **Decide Arzpal.**

**Regla del presupuesto:** cada especie reparte **Σ = 37 puntos** entre los 6 ejes. Es la báscula que mantiene justo el hexágono: quien sube un eje, baja otro. **✅ (2026-07-26): Σ = 37 para TODAS las rarezas** — la rareza no compra puntos, compra **extremos** (distribuciones más polarizadas; los 10 "deliberados" viven ahí). Así todo tatemis es viable competitivamente.

| Eje | Pyrecko | Ottorock | Mantagale |
|---|---|---|---|
| **Vida** | 2 | 7 | 4 |
| **Daño** | **9** | **8** | 4 |
| **Velocidad** | 4 | 5 | 5 |
| **Alcance** | **10** | 4 | 7 |
| **Control** | 4 | 6 | **9** |
| **Sinergia elemental** | 8 | 7 | 8 |
| **Σ** | 37 | 37 | 37 |

✏️ *Reajuste 2026-07-26 (aplica la regla «pico = eje firmado», [ARCHETYPES §3](ARCHETYPES.md)): **Ottorock** +2 Daño / −1 Vida / −1 Control — su tanqueo real vive en la vida temporal de Sujeción («el Luchador más tanque» compensa un Daño menor que el del Armadillo). **Mantagale** Control 9 como pico y Velocidad a promedio (lo único "rápido" es la facilidad con la que se eleva y la recurrencia de sus básicos de poco daño — rasgo de locomoción, no el eje) + 1 punto a Alcance/Daño/Vida = Controlador que deriva muy levemente a Zonificador. ⚠️ Sincronizar el campo 5 de [CREATURE-OTTOROCK](CREATURE-OTTOROCK.md) y [CREATURE-MANTAGALE](CREATURE-MANTAGALE.md) al abrirlos, y **re-medir el dps del kit de Ottorock** (su Daño subió 6→8 → revisar el ancla de tiempos del maniquí, ver abajo).*

Picos sin solaparse — **Alcance → Pyrecko (10)** · **Daño → Ottorock (8)** · **Control → Mantagale (9)**: ahora **los tres firman el eje de su arquetipo**. Ottorock sigue siendo el de silueta más redonda (4–8) = el generalista. Empates: Sinergia 8 (Pyrecko = Mantagale), coherente porque ambos viven del terreno.

### Escala de tamaños 🆕 ✅ *(dictada 2026-07-29)*

**Hay 5 niveles de tamaño de especie, de pequeño a grande.** **Ottorock es pequeño-mediano** (nivel 2 de 5), y lo que hoy se ve en caja gris es su **versión adulta**. La escala importa por dos cosas que ya están dictadas: el **tamaño de la celda** de la arena se mide contra un monstruo *grande* ([ARENA §1](ARENA.md): celda ≈ 2× uno grande, 3× un bebé), y las **escalas base de forma bebé** del campo 4 de cada ficha (Pyrecko ~50 cm · Ottorock ~65 cm · Mantagale ~55×70 cm) son el suelo de esa escala, no su medio.

> 🟡 Sin dictar: qué especie ocupa cada uno de los 5 niveles, y si el nivel de tamaño **cuesta puntos** del presupuesto Σ=37 o es independiente de él (hoy no aparece en el hexágono).

### Mapeo hexágono → HP 🔒 *(fijado el 2026-07-22)*

**1 punto de Vida = 500 HP.** El ancla no es el número, es el **tiempo de combate**: un monstruo de *early game* debe caer en **1–2 min** y uno de combate importante en **4–6 min**.

| Vida (hex) | HP | Quién |
|---|---|---|
| 2 | 1 000 | Pyrecko |
| 4 | 2 000 | Mantagale *(reajuste 2026-07-26)* |
| **5** | **2 500** | **referencia estándar** (el maniquí del sandbox) |
| 7 | 3 500 | Ottorock *(reajuste 2026-07-26)* |

> **Cómo se calculó:** el kit de Ottorock rinde ~65 dps contra un saco quieto, pero contra un enemigo que se mueve y te obliga a defenderte el *uptime* real ronda **un tercio** → ~23 dps efectivos. 2 500 HP ≈ **1,8 min**, dentro del objetivo de early game. *(Antes eran 6 000 = ~4-5 min reales: el combate se hacía largo y ni siquiera había enemigo que se defendiera.)*
>
> **Combates importantes ✅ (2026-07-26) = múltiples enemigos** (2–3 tamers, cada uno con sus 3 tatemis), **no HP inflado ni fases** — así el balance individual no se rompe: los 4–6 min salen del número de oponentes. ⚠️ Recalcular esta escala **cada vez que cambie el daño de un kit**: el ancla es el tiempo, no los HP.
>
> ✏️ **Medición del kit de Ottorock (2026-07-29, en papel).** Rotación completa (2 embestidas + 2 mordiscos + básicos de relleno / 6 s, sostenible en energía al límite): **~65 dps contra saco quieto** (la cadena de 5 sola: 116 de daño en 2,22 s ≈ 52). Con uptime ⅓ → ~22 efectivos → 2 500 HP ≈ 1,9 min. Esta cifra es **aritmética del kit tal como está**, y no ha cambiado con el reajuste.
>
> 🟡 **ABIERTA — falta la conversión del eje Daño (pendiente de dictar por Arzpal).** Vida tiene su conversión dictada (500 HP/punto) y por eso el reajuste 8→7 se aplicó solo. **Daño no tiene ninguna**, así que el 6→8 del 26/07 —que fue una decisión de diseño ("pico = eje firmado"), no una medición— **no dice si el kit debe pegar más**. ⚠️ *Ojo al razonamiento circular: el kit rendía esos mismos ~65 dps cuando el eje decía 6, luego la medición no distingue un valor del otro.* Lo que hace falta es **elegir el ancla**, igual que se hizo con los 500 HP: mientras Ottorock sea la única especie implementada, lo que él haga **define** la escala para las otras 29. Propuesta a ratificar: **1 punto de Daño ≈ 8 dps de saco** (→ 8 = 64, deja el kit de Ottorock intacto y ya afinado por feel; 4 = 32 para Mantagale, 9 = 72 para Pyrecko, 10 = 80 de tope del roster). Alternativa: subir el kit ~33% para que el salto 6→8 se note, lo que llevaría el maniquí a ~1,4 min — sigue dentro del ancla de 1-2 min, así que **las dos opciones son viables**: es una decisión de diseño, no un cálculo.

> Los stats duros (Vida/Daño/Velocidad) salen de las proporciones dictadas en el campo 5 de cada ficha (Pyrecko 15/55/30 · Ottorock 30/35/35 · Mantagale 20/30/50); los otros 3 ejes se derivan del kit. **Huecos libres del hexágono** para especies futuras: un Coloso de Vida ~10, un Ejecutor de Daño+Velocidad glass, un Zonificador aún más extremo.
