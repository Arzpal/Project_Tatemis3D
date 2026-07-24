# Tatemis3D — El Tamer: menú, acciones y la línea con el monstruo

> **TL;DR:** Hay **dos capas de control** y no deben mezclarse: el **monstruo** (movimiento, kit, básicos, bloqueo, interacción física con el mundo) y el **Tamer** (capturar, objetos, órdenes a aliados, swap). Todo lo del Tamer vive en **un solo menú** — un botón, *slow-mo* contra IA y tiempo real contra jugadores. El **finisher deja de ser un botón**: rematar a un noqueado es **seguir atacándolo**, con una **ventana de gracia invulnerable de 3–5 s** que da margen a decidir en vez de matar por inercia. 🟡 *Dictado por Arzpal el 2026-07-22, sin ratificar con PB/DS.*
> **Mapa:** §1 la línea · §2 el menú · §3 qué queda en el contextual · §4 el finisher · §5 abiertas y riesgos

---

## 1. La línea: monstruo vs. Tamer

El pilar del juego es que **controlas por completo al monstruo**. Por eso todo lo que el *Tamer* hace pertenece a otra capa, y separarlas ordena el esquema de control entero:

| Capa | Qué incluye | Cómo se accede |
|---|---|---|
| **Monstruo** | Mover · saltar · **cadena de básicos** · las 3 habilidades · bloqueo/forma pura · apuntado · **interactuar con el mundo** | Botones directos, siempre en tiempo real |
| **Tamer** | **Capturar** (Sludge) · **objetos** (curas, consumibles de combate — la "mochila" de Pokémon) · **órdenes** a los aliados · **swap** | **Un menú**, §2 |

**Criterio para clasificar cualquier acción futura:** *¿la haría el bicho con su cuerpo?* Si sí, es del monstruo. Si la hace la persona que lo acompaña, va al menú.

> Esto **libera presupuesto de botones**, que era justo el problema que motivó recortar a un kit de 3 ([GDD §6.3](GDD.md)): órdenes, swap, items y captura ocupaban entradas sueltas en el mando y ya no caben más. Ahora es una sola puerta con contenido dentro.

## 2. El menú del Tamer

- **Botón:** `Select`/`-`/`Share` o cruceta · `Tab` en teclado. *(A decidir, §5.)*
- **Tiempo:** ***slow-mo* contra IA**, **tiempo real contra jugadores**.
- **Contenido:** atrapar · objetos · órdenes a aliados · swap.

🔒 **El menú NO es un refugio — modelo Monster Hunter** *(ratificado por Arzpal el 2026-07-22)*. Mientras está abierto, **el mundo sigue vivo y el monstruo queda expuesto**: no se defiende y puede comerse un golpe. Abrirlo es una decisión táctica con coste. Referencia deliberada: **MH** (curarte te deja vendido) y **no** el menú de Pokémon, que existe porque allí no hay tensión en tiempo real. Si abrir el menú sale gratis, el combate de acción —el USP— se vuelve una sucesión de pausas.

> El *slow-mo* contra IA se mantiene **solo como ayuda para navegar**, no como escudo: da tiempo a leer las opciones, no a estar a salvo. *(Interpretación de la decisión; si la idea era ir a MH puro y quitar también el slow-mo, es cambiar esta línea.)*

## 3. Qué queda en el botón contextual

Solo **interactuar con el mundo**, y con eso gana identidad en vez de ser un cajón de sastre: aplastar un hongo de humo, agarrar un objeto y lanzarlo, activar algo del ambiente. Encaja con el USP ambiental y con el kit de Ottorock (agarrar terreno). *Antes cargaba también con finisher y Sludge — ver [GDD §6.3](GDD.md).*

## 4. El finisher: atacar, no pulsar

Cuando el enemigo queda **debilitado** (aturdimiento lleno o vida a 0) se anuncia y se abre la ventana de KO. Rematarlo es **seguir atacándolo**: sin prompt, sin botón dedicado. Mata quien decide seguir pegando.

Encaja con lo ya decidido en [VITALITY-DEATH §1](VITALITY-DEATH.md): *la muerte de un salvaje es una elección del jugador, rematar a un noqueado*. Y el noqueo **ya es automático** en el código (`StaggerReceiver` abre la ventana solo), así que el `TryKnockout()` manual y su enganche al botón contextual **sobran** — retirar al implementar.

### 4.1 · La ventana de gracia 🔒 *(dictado el 2026-07-22)*

**Al quedar noqueado por completo** (aturdimiento lleno **+ vida 0**), el enemigo pasa **3–5 s invulnerable**. Ese margen es la pieza que hace que el finisher-sin-botón funcione:

| | |
|---|---|
| **El problema que resuelve** | Si entra en KO **a mitad de tu combo**, el siguiente golpe lo remataría sin que lo hayas decidido: matarías por inercia justo al capturable que querías. Sin esto, "capturar" sería lo que consigues **solo si reaccionas a tiempo**, y el bucle principal del juego dependería de un reflejo |
| **Durante la gracia** | Indefenso pero **inmune**. Tus golpes en curso no cuentan. Es el hueco para **decidir** |
| **Después** | Sigue noqueado y ya acepta el desenlace: **atacar = rematar** (muerte) · **menú del Tamer = capturar** |
| **Si no haces nada** | Se resuelve solo, como hoy: el salvaje huye / el del tamer se retira |

⚠️ **Requisito de implementación:** la ventana de KO tiene que durar **más** que la gracia, o no quedaría tiempo para rematar. Hoy `StaggerReceiver` la dimensiona con `koDecayPerSec` (~3.3 s), justo en el rango de la propia gracia → **hay que alargarla** al programarlo.

## 5. Abiertas y riesgos

> ✅ **Resueltas el 2026-07-22:** ① *cortar el combo* → **ventana de gracia** (§4.1) · ② *el menú como refugio* → **modelo Monster Hunter** (§2).

1. **Qué anuncia el "debilitado"** — postura + señal visual clara. Sin lectura, el jugador no sabe cuándo puede elegir. Es lo que más urge de lo que queda: el margen de gracia no sirve de nada si no se ve.
2. **El swap de emergencia** ([VITALITY-DEATH §4](VITALITY-DEATH.md), *sin cooldown al noqueo*) probablemente necesite un atajo directo además del menú: abrir un menú para reaccionar a una emergencia es una contradicción. ⏸️ **Aplazado a propósito** — se decide al integrar la mecánica, no antes.
3. **El menú en PvP** — se mantiene el modelo de §2 y punto. 🔮 De hecho **podría no existir en PvP**: contra otro jugador tiene mucho menos sentido que contra IA (no hay captura, y objetos/órdenes son discutibles). Decisión aplazada al multijugador, que ya es post-slice.
4. **Botón concreto** del menú en mando y teclado.
5. **Alcance:** nada de esto es del gate. El menú es material de **Fase 3+** (captura) y las órdenes son **ola 4** ([ROADMAP](ROADMAP.md)). Lo único que toca antes es **no programar** el finisher por botón, y que **T1.7** implemente la gracia (§4.1).
