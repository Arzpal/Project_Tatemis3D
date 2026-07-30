# Tatemis3D — Arquetipos: brújula y pentágono

> **TL;DR:** Doctrina de los 5 arquetipos ✅ (**dictada por Arzpal 2026-07-26**; «Coloso» sustituye a «Guardián» en todo el proyecto): cada uno se define por **cómo gana**, **cómo se le gana** y una **pregunta generadora** que clasifica ideas crudas; los counters cierran en un **pentágono 2-2** (ganas a los 2 siguientes del círculo, pierdes ante los 2 anteriores). Validado contra las clases de LoL. Stats/ejes → [CREATURES §4](CREATURES.md) · ideas de especies → [CREATURE-IDEAS](CREATURE-IDEAS.md).
> **Mapa:** §1 la brújula (los 5) **+ la regla verbo/kit** *(2026-07-29: el verbo del elemento gobierna las reacciones, el arquetipo gobierna el kit — conviven)* · §2 pentágono de counters · §3 transversales, validación LoL y abiertas

## 1. La brújula

> Una idea cruda se clasifica viendo **qué pregunta responde con más naturalidad** (el armadillo respondía "motor" → Luchador; el ave respondía "qué compra aguantando" → Coloso). Dentro de una columna, la **dimensión de variedad** no puede repetirse entre especies (complementa la regla de celdas de [CREATURE-IDEAS §1](CREATURE-IDEAS.md)).

| Arquetipo (eje) | Cómo gana | Cómo se le gana | Pregunta generadora | Variedad interna |
|---|---|---|---|---|
| **Coloso** (Vida) | Aguanta **todos los golpes que hagan falta** para efectuar lo suyo — su vida es el coste de su efecto (Topo: el avance que nadie frena · Ave: la sujeción bajo fuego). No es un protector | **Rompiéndole el límite** antes de que efectúe — desgaste que no le da nada a cambio (el Zonificador es su destructor natural) | **¿Qué compra con su aguante?** | la **vía de tanqueo** |
| **Luchador** (Daño) | **Siguiendo su juego, gana siempre**: el 1v1 limpio le pertenece (Ottorock, si alterna sus stances con normalidad · Armadillo, si nadie logra frenarlo) | **Sacándolo de sus casillas**: no darle nunca el 1v1 limpio — romperle el patrón/motor (CC, kiting) | **¿Cuál es su motor — qué lo alimenta y qué lo apaga?** | el **motor** |
| **Ejecutor** (Velocidad) | **Entra, revienta, sale** — su gameplay entero es esa ventana | Negándole el burst, **siguiéndolo** (igualar su movilidad) o **cerrándole la salida** | **¿Desde qué plano entra y qué le abre la ventana?** | el **plano de entrada** |
| **Zonificador** (Alcance) | La guerra de la **distancia**: castigo alto y sostenido desde lejos (la ametralladora de básicos de Pyrecko); cuando lo alcanzan, **escapa** (la siembra-detonación es el único escape de Pyrecko). Pega hasta que se acercan; al acercarse, huye | **Cerrándole la distancia** y negándole la huida | **¿Con qué castiga desde lejos y qué le devuelve la distancia cuando la pierde?** | el **oficio** a distancia |
| **Controlador** (Control) | **Desestabiliza**: genera situaciones incómodas — te mueve, te inmoviliza, te suprime. El que más CC tiene, y lo pone con **fenómenos de su elemento convertidos en kit** — marcas de especie con lógica propia: los tornados/ciclón de Mantagale, la estela del Collie, las olas del castor. ⚠️ *No con reacciones: **un elemento puro solo hace daño** ([ER §0.2](ELEMENTAL-REACTIONS.md), 2026-07-29 — deroga «el Controlador controla con su elemento puro» de [CREATURES §2](CREATURES.md))* | Con quien su CC **no hace palanca**: rango (Pyrecko castiga desde donde sea) o anti-CC (el Topo) | **¿Qué fenómeno de su elemento convierte en kit, y cómo desestabiliza con él?** | el **fenómeno** con el que controla *(antes «el verbo elemental»: término reservado desde [ER §0.1](ELEMENTAL-REACTIONS.md), donde el verbo lo fija el elemento y no la especie)* |

> 🔑 **El verbo gobierna las REACCIONES; el arquetipo gobierna el KIT** ✅ *(2026-07-29)*. Desde que cada elemento tiene **un solo verbo** ([ER §0.1](ELEMENTAL-REACTIONS.md)) y **el elemento puro solo hace daño** ([ER §0.2](ELEMENTAL-REACTIONS.md)), las dos capas dejan de competir por el mismo sitio: el **verbo** solo aparece cuando el elemento se **mezcla** (terreno, choque entre elementos, roca sellada) y el **arquetipo** decide lo que la criatura hace por sí misma. **Conviven**: nadie tiene que elegir entre ser Controlador y ser de Mineral.
>
> ⚠️ **Casillas cuesta arriba de la matriz 6×5.** Cuando el verbo del elemento apunta al mismo sitio que el «cómo gana» del arquetipo —un **Controlador de Mineral**: elemento que *pesa*, arquetipo que *controla*— o justo al contrario, la casilla **no se vuelve ilegal, se vuelve más difícil de diseñar**: hay que decidir **qué aporta el kit que el verbo no aporte ya**. La regla de arriba es la que la desatasca (el verbo no vive en el kit), y el criterio de no-clon de [CREATURE-IDEAS §3](CREATURE-IDEAS.md) es el que la cierra. 🟡 *Sin dictar: si esas casillas piden alguna compensación explícita al llenar la matriz.*

## 2. Pentágono de counters ✅ *(modelo 2-2 pedido por Arzpal; aristas derivadas de la brújula)*

Círculo: **Luchador → Zonificador → Coloso → Controlador → Ejecutor → (Luchador)**. Cada arquetipo **gana a los 2 siguientes y pierde ante los 2 anteriores**.

| Gana | a | Por qué |
|---|---|---|
| Luchador | Zonificador | lo alcanza (aguanta la aproximación) y entonces es 1v1 — su juego |
| Luchador | Coloso | el Coloso sí le da el 1v1 limpio… y el 1v1 limpio muele su límite |
| Zonificador | Coloso | desgaste desde lejos sin darle nada que aguantar-a-cambio |
| Zonificador | Controlador | pega desde donde sea: el CC no le hace palanca |
| Coloso | Controlador | anti-CC — el Topo ni lo siente |
| Coloso | Ejecutor | su burst no rompe el límite ("no logra el burst"), y encima lo atrapa |
| Controlador | Ejecutor | le cierra la salida |
| Controlador | Luchador | lo saca de sus casillas |
| Ejecutor | Luchador | jamás le da el 1v1: entra y sale, no hay "casillas" donde quedarse |
| Ejecutor | Zonificador | le salta la distancia entrando por un plano no vigilado |

- **Auto-validación:** las 2 derrotas de cada arquetipo son exactamente las **2 encarnaciones de su «cómo se le gana»** — el pentágono no añade reglas, cierra la brújula en círculo.
- Es **counter blando** (ventaja, no sentencia) y se **cruza** con el elemental 1.5x: pueden sumarse (Topo vs Halcón: Coloso›Ejecutor **+** Mineral›Rayo) o cancelarse (Ave vs Halcón: Coloso›Ejecutor **pero** Rayo›Aire) — matchups con textura.
- Receta de equipos de 3: **tres arquetipos consecutivos** del círculo se cubren las espaldas en cadena; tres alternos dejan agujeros comunes.

## 3. Transversales, validación y abiertas

- **Transversales a los 5:** autosuficiencia (todo arquetipo prepara su propio daño: el CC es "control + castigo", el aguante compra remate) y la **Sinergia elemental** es el 6º eje de todos, no un arquetipo.
- **Validación externa (espejo LoL):** Coloso ≈ **Sion** · Luchador ≈ **Fiora** · Ejecutor ≈ **Zed** · Zonificador ≈ **Caitlyn** · Controlador ≈ **Taliyah**. Las 13 subclases de Riot caben en los 5 y el pentágono reproduce sus counters canónicos (asesino›ADC, ADC›tanque, tanque›asesino, Taliyah›asesinos-dash, Fiora›tanques). El único rol de LoL **sin** arquetipo aquí es el **Soporte** (Warden/Enchanter) — excluido por autosuficiencia y combate 1v1-céntrico; reevaluar un 6º arquetipo **solo** si algún día hay combate de equipo simultáneo (2v2/multi 🔮).
- ✅ **Regla de graduación (ratificada 2026-07-26): el pico del hexágono = el eje que firma el arquetipo** (empates permitidos). Ratificada de facto con los reajustes del trío: Ottorock Daño 6→8 («el Luchador más tanque») y Mantagale Control 9 pico con Velocidad a promedio ([CREATURES §4](CREATURES.md)). Los 6 conceptos con hexágono la cumplen (Pyrecko→Alcance, Ottorock→Daño, Mantagale→Control, Armadillo→Daño, Ave/Topo→Vida); los 4 sin hexágono (Halcón, Hurón, Collie, Orca) la heredan al diseñarse.
