# Tatemis3D — Criaturas (índice + plantilla)

> **TL;DR:** Una ficha por especie, **cada una en su propio doc** (`CREATURE-*.md`; se dividió al superar el tope de ~12 KB de [INDEX.md](INDEX.md)). El kit es de **3 habilidades** (Movilidad · Flex · Signature) + universales (ataque básico, salto, bloqueo/forma elemental, noqueo contextual) — ver GDD §6.3. De las fichas reales se **extrae** el sistema de habilidades ([ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md); estrategia *bespoke*-primero de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)).
> **Mapa:** §1 especies (índice) · §2 plantilla (los 18 campos) · §3 por qué este trío · §4 báscula de arquetipos (hexágono de stats)

## 1. Especies

| Especie | Ficha | Rol | Estado |
|---|---|---|---|
| **Pyrecko** — Zonificador de Fuego | [CREATURE-PYRECKO.md](CREATURE-PYRECKO.md) | Fase 4 del prototipo · candidato a starter de Fuego | 🟢 dictada (2 abiertas menores) |
| **Ottorock** — Luchador de Mineral | [CREATURE-OTTOROCK.md](CREATURE-OTTOROCK.md) | **Criatura del gate (T1.4)** — empezar por su modo Libre | 🟢 dictada (abiertas menores) |
| **Mantagale** — Controlador de Aire (mantarraya aérea) | [CREATURE-MANTAGALE.md](CREATURE-MANTAGALE.md) | Post-prototipo (Nivel B) · informa la arquitectura de vuelo | 🟢 kit cerrado (§C) · ficha reescrita · stats fijados (§4) · quedan números de habilidad (§C.6) |

## 2. Plantilla de ficha (los 18 campos)

> **Principio de kits (2026-07-17): toda especie es autosuficiente** — cualquier tatemis debe poder ganar un combate por sí solo; el CC/soporte prepara su **propio** daño además del del equipo (ref. Aurelion Sol para el patrón "control + castigo" de Mantagale). Aplica también a Controladores y a los futuros Guardianes.

**Identidad**
1. **Nombre** (placeholder) + **concepto en 1 frase** — la fantasía: qué se siente pelear *siendo* él.
2. **Elemento** — Naturaleza / Agua / Fuego / Mineral / Rayo / Aire.
3. **Arquetipo** — Guardián / Luchador / Ejecutor / Zonificador / Controlador.
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

- **Complejos a propósito** (decisión 2026-07-17): la pareja del prototipo estresa el futuro sistema de habilidades por lados distintos, para descubrir sus límites antes de estandarizar (estrategia *bespoke*-primero de [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md)). **Pyrecko:** manchas con TTL, DoT, sigilo, homing. **Ottorock:** stances con variantes por modo, vida extra temporal, combinación sellada portátil. **Mantagale:** vuelo, desplazamiento de enemigos, cargas/canalización, hazard móvil que dispara reacciones en cadena (los 3 puntos ciegos de [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) §7).
- **✅ Gate (T1.4) = Ottorock en modo Libre** (melee puro; sin reacciones **porque la arena del gate es neutra — regla R5**, no porque Libre no irradie); Sujeción post-gate; Pyrecko en Fase 4; Mantagale post-prototipo (en papel desde ya para que cámara/lock-on/locomoción **no asuman "solo suelo"**).
- **Contraste de arquetipos:** zonificador a distancia vs luchador melee vs controlador aéreo → el criterio de Fase 4 y del slice ("criaturas claramente diferentes, mismo framework") se valida al máximo.
- **Elementos:** Fuego (1er triángulo) + Mineral y Aire (2º). Fuego es neutral con ambos ✓. ⚠️ **Aire › Mineral:** Mantagale countea a Ottorock dentro del propio equipo (el caso que se evitó con Agua/Fuego — aceptado por ahora, revisar en balance).
- **Cruces intra-equipo** *(actualizado 2026-07-18 con el modelo de terreno fijo — [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)):* la anti-sinergia Mineral↔combustible **desapareció** (las reacciones ya no limpian marcas) y el combo "Mantagale empuja el fuego de Pyrecko" **también** (no hay zonas que mover). Lo que queda es más limpio: los tres **irradian elementos distintos sobre el mismo tablero**, así que el equipo cubre más filas de la tabla en la misma arena.
- **El USP de reacciones lo venden los tres, cada uno por una vía distinta:** Pyrecko (marcas de especie + detonación que irradia Fuego), Ottorock (**combinación sellada portátil**: elige su reacción eligiendo dónde agarra), Mantagale (**tornado que encadena reacciones** conforme recorre el terreno).
- *Nota:* los tres cubren solo 3 de los 5 arquetipos; Guardián y Ejecutor quedan para las especies 4–5 del slice (diseño en papel, sin fecha).

## 4. Báscula de arquetipos (hexágono de stats)

> **Fijado 2026-07-18.** Los 6 ejes en los que se mide **toda** especie del roster (en papel). Cada arquetipo tiene un eje que "firma"; el 6º (Sinergia elemental) es el **USP ambiental**, transversal. Escala **0–10**, relativa al roster con headroom (nadie llega a 10 salvo un extremo deliberado).

| Eje | Qué mide | Arquetipo que lo firma |
|---|---|---|
| **Vida** | Aguante / supervivencia | Guardián |
| **Daño** | Output ofensivo (sostenido + burst) | Luchador |
| **Velocidad** | Movilidad / reposicionamiento | Ejecutor |
| **Alcance** | Rango efectivo / negación de área | Zonificador |
| **Control** | CC: empujes, roots, stuns, slows | Controlador |
| **Sinergia elemental** | Cuánto depende su plan del terreno + reacciones | *(USP, transversal)* |

> *Efectos/status (buffs/debuffs) se contabilizan dentro de **Control** + **Sinergia**, no como eje propio (decisión de mantener 6 ejes limpios; revisable).*

**Regla del presupuesto:** cada especie reparte **Σ = 37 puntos** entre los 6 ejes. Es la báscula que mantiene justo el hexágono: quien sube un eje, baja otro. *(Las rarezas más altas podrían justificar 38–40; sin decidir.)*

| Eje | Pyrecko | Ottorock | Mantagale |
|---|---|---|---|
| **Vida** | 2 | **8** | 3 |
| **Daño** | **9** | 6 | 3 |
| **Velocidad** | 4 | 5 | **9** |
| **Alcance** | **10** | 4 | 6 |
| **Control** | 4 | 7 | **8** |
| **Sinergia elemental** | 8 | 7 | **8** |
| **Σ** | 37 | 37 | 37 |

Picos sin solaparse — **Alcance → Pyrecko (10)** · **Vida → Ottorock (8)** · **Velocidad → Mantagale (9)** — cada uno ocupa una esquina distinta del hexágono. Ottorock es el de silueta más redonda (rango 4–8, sin valles profundos) = el generalista. Único empate: Sinergia 8 (Pyrecko = Mantagale), coherente porque ambos viven del terreno; Mantagale se despega por Velocidad + Control.

> Los stats duros (Vida/Daño/Velocidad) salen de las proporciones dictadas en el campo 5 de cada ficha (Pyrecko 15/55/30 · Ottorock 30/35/35 · Mantagale 20/30/50); los otros 3 ejes se derivan del kit. **Huecos libres del hexágono** para especies futuras: un Guardián de Vida ~10, un Ejecutor de Daño+Velocidad glass, un Zonificador aún más extremo.
