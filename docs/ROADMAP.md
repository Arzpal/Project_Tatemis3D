# Tatemis3D — Roadmap de producción

> Documento vivo. Estrategia y cronograma para construir el proyecto a ritmo **part-time**. Complementa el diseño de [GDD.md](GDD.md).

## Estrategia: prototipo de *feel* primero

El USP del juego es **cómo se siente** combinar habilidades + elemento + ambiente en combate de acción. Eso es lo más difícil de ejecutar y lo que decide todo lo demás. Por eso **no** construimos el vertical slice completo de golpe: primero probamos el corazón en **caja gris** (placeholders), y solo si se siente increíble invertimos en el slice con arte.

Esto encaja con el equipo: prog/diseño sólidos + sin artista dedicado + part-time → un prototipo grey-box es **99% programación y diseño** (nuestra zona fuerte), y el arte se difiere hasta validar.

## Niveles de scope

| Nivel | Qué es | Estimación |
|---|---|---|
| **A — Prototipo de feel** | Grey-box, 1–2 criaturas, combate core + 1 reacción ambiental + loop de captura. Sin arte final. | **~12–18 semanas part-time** (~3–6 meses calendario) |
| **B — Vertical slice pulido** | 1 bioma, 3–5 criaturas, 1 cadena evolutiva, captura completa, mazmorra + jefe, onboarding. Con arte. | Solo **tras validar A** |
| **C — Juego completo** | 10–20 especies, varios biomas, PvP, postgame. | Multi-año / requiere equipo y/o financiación mayores |

## Nivel A — Plan por fases

Asunción: **~8–12 h/semana efectivas por persona**. Duraciones en **semanas de trabajo part-time**. Cada fase termina en un **build jugable**.

| Fase | Qué construyes | Dur. | Criterio de salida |
|---|---|---|---|
| **0 · Fundaciones** | Unity + URP, Input System, cámara 3ª persona + lock-on, controller de la criatura (mover/saltar), maniquí dummy, arena grey-box | 1–2 sem | Te mueves y haces lock-on a un maniquí en una caja gris |
| **1 · Core feel** 🚩 | Ataque básico + noqueo, energía elemental (barra tipo tinta), 2–3 habilidades, *game feel* (hitstop, hit-reacts, feedback) | 3–4 sem | **EL GATE:** "¿es divertido pegar y encadenar habilidades, aunque no haya arte?" Si no → itera aquí antes de seguir |
| **2 · Elemento + ambiente** | Bloqueo / forma elemental pura (stance, recarga, 2× debilidad), **1 reacción ambiental** (ej. dominancia Mineral › Rayo → AoE + aturdimiento, o la ★ Rayo + Agua → agua electrificada) resuelta contra terreno fijo | 2–3 sem | Combinar habilidad + ambiente para una reacción se siente satisfactorio y legible |
| **3 · Loop de captura** | Barra de aturdimiento visible, noqueo → fin → ventana → Sludge → Tótem (con resistencia), enemigo con IA mínima (persigue/ataca/telegrafía) | 2–3 sem | El loop "debilitar → aturdir → noquear → capturar" corre de principio a fin |
| **4 · 2ª criatura + swap** | Añadir una criatura de **arquetipo distinto** (ej. Luchador vs Zonificador), swap con cooldown | 2–3 sem | Dos criaturas se sienten **claramente diferentes** con el mismo framework |
| **5 · Pulido de pitch** | Game feel, cámara, HUD (energía/aturdimiento), meter placeholders "detallados" del concept art, grabar gameplay limpio | 2–3 sem | Build de 3–5 min que demuestra el USP + dossier de concept art |

### El gate que importa
El hito crítico es la **salida de la Fase 1** (~semana 4–6): *"¿es divertido pegar y encadenar habilidades sin arte?"*. Si la respuesta es no, se itera ahí **antes** de continuar. Es el milestone más valioso: en ~mes y medio ya sabes si tienes algo.

### Concept art (en paralelo)
El concept artist trabaja concepts de las criaturas héroe + el bioma **en paralelo** al grey-box — **no está en el camino crítico**. El paquete de pitch al studio = **build jugable de 3–5 min + dossier de concept art**.

## Consejos para ritmo part-time
- **Milestones cortos con build jugable al final de cada fase.** El enemigo real del nights-and-weekends no es la dificultad técnica, es **perder el momentum**.
- **Sistema de habilidades data-driven desde la Fase 1** (ver [ARCHITECTURE.md](ARCHITECTURE.md)): cada criatura nueva pasa a ser **datos, no código** → las Fases 4+ vuelan.
- **No sobre-ingenierizar arte/mundo.** Caja gris hasta validar el feel.

## Recalibración de alcance — valor × viabilidad

> Añadida tras las decisiones de muerte/vitalidad/aliados ([VITALITY-DEATH.md](VITALITY-DEATH.md)). Conclusión: **la escala creció en el post-gate, no en el camino crítico** — el gate y las Fases 0–2 quedan intactos; lo nuevo se ordena en olas condicionadas.

Ejes (0–100, estimación de diseño): **Valor** = aporte al gate + USP + pitch · **Viabilidad** = inverso de esfuerzo/riesgo para este equipo (3 personas, part-time, sin artista).

| Sistema | Valor | Viab. | Ola |
|---|---|---|---|
| Game feel (hitstop, reacts) | 100 | 68 | **1 · Gate** (F0–2) |
| Habilidades data-driven + energía | 95 | 80 | 1 · Gate |
| Reacción ambiental + terreno | 90 | 55 | 1 · Gate |
| Locomoción + cámara + lock-on | 85 | 88 | 1 · Gate |
| TypeChart | 68 | 95 | 1 · Gate |
| Captura completa | 85 | 65 | **2 · Loop** (F3–5) |
| 2ª criatura + swap | 80 | 75 | 2 · Loop |
| Pulido de pitch | 72 | 68 | 2 · Loop |
| IA enemiga 1v1 | 70 | 72 | 2 · Loop |
| Muerte de salvajes (elección) | 62 | 92 | 2 · Loop — quick win, se cuela en F3 |
| Rescate del caído + swap de emergencia | 82 | 60 | **3 · Vínculo y riesgo** (Nivel A+) |
| Vitalidad + heridas graves | 76 | 58 | 3 · A+ |
| Manadas | 74 | 46 | 3 · A+ |
| Cuidado/base (recuperación) | 55 | 42 | 3 · A+ |
| Aliados por órdenes + posesión | 85 | 26 | **4 · Equipo en campo** (Nivel B) |
| Persecución del Tamer + escape | 45 | 34 | 4 · Nivel B |

**Lectura:**
- **Olas 1–2 = Nivel A tal cual** (~12–18 sem, sin cambios). Única adición: muerte de salvajes en Fase 3 (coste ~0: es daño sobre una entidad ya noqueada + un estado).
- **Ola 3 = "Nivel A+"** — entra **solo si el gate pasa**; ~+4–6 sem part-time. Orden interno sugerido: rescate → vitalidad → manadas → cuidado.
- **Ola 4 = Nivel B** — los aliados por órdenes son la pieza más cara del juego (única viabilidad <30); la persecución del Tamer es mucho sistema para un estado que el jugador ve poco. Ambas condicionadas a validar A y A+.
- **Eje de diseño en 3 capas:** ① *feel* de combate (en curso, sin cambios) → ② *vínculo con riesgo real* (la nueva identidad del meta) → ③ *equipo en campo* (la apuesta diferencial, condicionada).

## Evaluación honesta (para tener presente)

| Eje | Nota |
|---|---|
| Originalidad del concepto | 8.5/10 |
| Coherencia de diseño (tema ↔ mecánica) | 9/10 |
| Marketability del concepto | 8/10 |
| Realismo del scope (equipo de 3, part-time) | 4.5/10 |
| Riesgo de ejecución | **Alto** (concentrado en *combat feel* + arte de criaturas) |

**Resumen:** el concepto es fuerte, diferenciado y con un instinto de scope inteligente ("conectar, no coleccionar"). El riesgo no está en la idea, está en la **ejecución** del combate de acción y el arte con un equipo pequeño y part-time. El plan de arriba (prototipo-primero) está diseñado para neutralizar exactamente ese riesgo.
