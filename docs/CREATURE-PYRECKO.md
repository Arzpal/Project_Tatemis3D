# Tatemis3D — Ficha: Pyrecko (Zonificador de Fuego)

> **📌 Nombre y animal cerrados (2026-07-18): «Pyrecko» — *pyre* (pira de material combustible) + ge**cko**.** *Antes se llamaba "Geflame", descartado por estar ya en uso en otro juego indie.* El **gecko** encaja con el kit sin forzar nada: piel **críptica** (su Camuflaje), **cola desprendible** como señuelo (lo escurridizo), **lengua** como entrega a distancia, hábitos nocturnos. Y "pyre" no es fuego genérico: es **una pila de combustible preparada para arder**, que es literalmente su mecánica.

> **TL;DR:** Cañón de cristal a distancia que siembra «combustible» inflamable (a tiros o invisible) y lo detona. Dictada por Arzpal (2026-07-17). **Entra en Fase 4** del prototipo (el gate lo asume Ottorock) y es candidato natural a **starter de Fuego**. Plantilla de los 18 campos → [CREATURES.md](CREATURES.md) §2.

| # | Campo | Valor |
|---|---|---|
| 1 | Nombre + concepto | **Pyrecko** — *cañón de cristal camuflado y escurridizo*: siembra el campo de «combustible» a distancia (o invisible) y lo prende. |
| 2 | Elemento | **Fuego** |
| 3 | Arquetipo | **Zonificador** (rango, proyectiles, negación de área — el "tirador" del equipo; no confundir con Ejecutor = burst/asesino, GDD §6.4) |
| 4 | Silueta y tamaño | 🟡 **Animal propuesto: gecko** *(2026-07-18 — el nombre ya lo encaja: **Ge**cko + **flame**; pendiente de confirmar)*. Cuadrúpedo; **~1/6 de la altura de un Tamer** (pequeño). Rasgos que le sirven al kit: piel **críptica** (su Camuflaje), **cola desprendible** como señuelo (lo "escurridizo"), **lengua** como entrega a distancia, hábitos nocturnos. |
| 5 | Vida / Daño / Velocidad | **15 / 55 / 30** (proporciones relativas, no valores finales) → vida baja · daño alto · velocidad media. Cañón de cristal. |
| 6 | Peso percibido | Ligero. |
| 7 | Locomoción | Neutral en estado normal; **rápida en sigilo**. |
| 8 | **Movilidad** | **Camuflaje** — entra en **sigilo**: invisible para los enemigos y velocidad drásticamente mayor. Dura lo que una **barra** que drena por segundo; el contacto con un tatemis enemigo la **reduce** y atacar lo **corta de golpe**. Mientras dura, **la zona que pisa queda marcada con «combustible»** (rastro). · Energía 7/10 · CD 7/10 · Targeting: self · Mancha: sí, rastro (rango 3/10). |
| 9 | **Flex** (ataque) | **Disparo ígneo** — **proyectil teledirigido** de fuego contra el objetivo (enemigo **u objeto del mundo**). Al impactar: daño + **irradia Fuego** (resuelve contra el terreno: incendio sobre Naturaleza, anulación sobre Agua, nada en neutro) + **quemadura** (daño/s) + **siembra «combustible»** en el punto de impacto. · Energía 5/10 · CD 4/10 · Targeting: objetivo · Siembra: sí (rango 5/10). |
| 10 | **Signature** | **Explosión venenosa** — todas las marcas de «combustible» de los **últimos 10 s** (las anteriores se disuelven solas) **se encienden** (única forma de encenderlas): daño/s a quien las pise + **ralentización 20 %** (no acumulable), durante **5 s**. · Energía 9/10 · CD 10/10 · Targeting: área (todas las marcas vivas). |
| 11 | Ataque básico | Cadena de **6** a alcance **largo** (proyectiles), ritmo rápido (7/10), **sin coste** (básico gratis, decisión ratificada); **cada 6º golpe** mancha la zona de impacto con «combustible» (rango 1/10, pequeño). |
| 12 | Bloqueo / forma elemental | Estándar: pose mínimamente defensiva → **forma de Fuego pura**, **7 s** de duración, **puede ser detenida o interrumpida**. CD 7/10. (En forma pura su debilidad pasa a 2x.) |
| 13 | Plan de juego | Atacar de lejos **sembrando el máximo de combustible posible**; con enemigos cerca, **camuflarse y alejarse dejando un rastro**, y luego **prender el campo**: cada marca resuelve su propio Fuego contra el terreno donde cayó (R1), cubriendo media arena de golpe. Fragilidad intencional: su "defensa" son las zonas encendidas (slow + daño) que impiden que lleguen a él mientras sigue disparando. **Debilidad táctica:** tatemis de velocidad media-alta que lo alcanzan antes de que vuelva a tener el sigilo disponible. |
| 14 | Sinergia ambiental | Arboledas/arbustos: el fuego de su combustible **se propaga por la vegetación** (dominancia Fuego > Naturaleza, [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)). |
| 15 | Como salvaje (IA) | Kitea: se mantiene lo más lejos posible infligiendo daño a distancia; al sentirse amenazado se camufla y se reubica en otra zona; repite el ciclo sembrando el máximo combustible posible y alejándose constantemente. ⚠️ Telegraph legible por definir. |
| 16 | Orden rápida (aliado) 🔮 | Disparo ígneo (tap de LB → dispara al objetivo actual). |
| 17 | Formas evolutivas 🔮 | 3 formas (base + 2). **1ª evolución:** condición = camuflarse y generar mucho «combustible». **2ª (y última):** por decidir. |
| 18 | Rareza 🔮 | Fase inicial **intermedia-difícil**, techo de **calidad starter**. (Fuego es elemento del Triángulo Inicial → candidato natural a starter de Fuego.) |

**Reglas del «combustible» (resueltas 2026-07-17):**
- Antes «veneno»; renombrado **combustible** (no es un elemento ni un tipo de daño).
- **Marca de especie: la tercera capa**, aparte del terreno y de las reacciones ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §1③ y §5): **inerte** — solo la **Signature** la enciende.
- **Rango de mancha** por fuente: básico **1/10** · rastro de Camuflaje **3/10** · Disparo ígneo **5/10**.
- ✅ *(2026-07-18)* Solo desaparece de **2 formas**: **expira a los 10 s** o **se detona**. Las reacciones elementales **ya NO la limpian** (regla anterior derogada: el combustible no es elemental, así que lo elemental no lo toca). El counterplay real es **posicional**: pelear **sobre terreno de Agua** (la detonación irradia Fuego y allí se **anula** → esas marcas no prenden) o simplemente aguantar los 10 s. *(El terreno es fijo: no se puede "mojar" a voluntad — hay que ir a donde ya hay agua.)*
- ✅ *(2026-07-18)* **Pertenece al lugar, no al material:** si Ottorock arranca el trozo de terreno marcado, se lleva el terreno y la marca **se queda donde estaba**.
- Fragilidad de Pyrecko (vida 15, dodge caro): **intencional** — su defensa son las zonas encendidas + la distancia.

**Abiertas:**
1. **Telegraph** legible de la IA salvaje (campo 15).
2. Segunda evolución (campo 17) sin decidir.

> *Cerrada 2026-07-18 — orden de resolución del Disparo ígneo:* ya no aplica. Las reacciones no limpian marcas, así que no hay conflicto entre "limpiar" y "depositar": el disparo deposita su combustible y, en paralelo, su Fuego resuelve contra el terreno.
