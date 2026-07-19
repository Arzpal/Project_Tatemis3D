# Tatemis3D — Sistema de reacciones elementales

> **TL;DR:** El **terreno tiene su elemento y no cambia nunca** — es el tablero. Las habilidades **irradian su elemento al impactar**; si difiere del terreno se produce **una** de tres salidas (**dominancia / combinación / anulación**), que es un **efecto temporal** de duración **proporcional al impacto**. Al terminar se dispersa y **el terreno vuelve a ser lo que era**. Nada se acumula ni se encadena. Modelo ✅ **ratificado por Arzpal (2026-07-18)**; complementa [GDD §8](GDD.md) y [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md).
> **Mapa:** §1 las tres capas · §2 las tres salidas · §3 reglas de resolución (R1–R6) · §4 la tabla (15 parejas con efecto + las 6 anulaciones) · §5 excepciones por especie · §6 legibilidad · §7 implementación · §8 alcance del prototipo

---

## 1. Las tres capas (no confundirlas)

**① Terreno — fijo, es el tablero.**
Al entrar en combate se **acota una zona** dividida en **celdas (hexágonos/cuadrantes)**. Cada celda tiene **o un elemento, o un efecto de ambiente, o nada**, asignado por el diseñador de nivel, y **no cambia jamás**. Los 6 terrenos elementales ✅ *(dictados 2026-07-18)*:

| Elemento | Terreno |
|---|---|
| **Fuego** | roca volcánica |
| **Agua** | río |
| **Naturaleza** | pasto / hierba alta |
| **Mineral** | zona de menas (minerales incrustados en la roca) |
| **Rayo** | piedra electrificada (como la volcánica, pero de electricidad) |
| **Aire** | corrientes de aire (géiseres que expulsan aire en vez de agua) |

**Los 6 elementos tienen terreno propio a propósito:** sin terreno de Aire, la dominancia Rayo › Aire sería inalcanzable. Las celdas **neutras** existen y ahí **no reacciona nada** (salvo la excepción de Ottorock, §5); las de **ambiente** (polvo, nieve, lluvia…) son el otro sistema, sin pasar por la tabla — pendientes de dictar.

> 🟡 **El modelo de rejilla está pendiente de cerrar** en un doc propio (`WORLD.md` o `ARENA.md`): tamaño de celda vs criatura, si la rejilla **se ve o solo se intuye** (riesgo tonal: unos hexágonos visibles leen a juego de mesa, no a acción), cómo se forma la zona en encuentros con **manadas**, y qué pasa con el **encierro** cuando cae el equipo entero y el Tamer tiene que huir ([VITALITY-DEATH](VITALITY-DEATH.md) §5 asume que se puede escapar).
> *Aparte y sin relación con la tabla de tipos:* los **fenómenos físicos y climáticos** (polvo al golpear el suelo, nieve, lluvia, viento) son el **segundo sistema ambiental** del [GDD §8.1](GDD.md) — pura física que las habilidades explotan, sin pasar por elementos. Puede nevar sin que exista un tipo Hielo. Las dos capas conviven en la misma arena sin tocarse.

**② Reacciones — temporales, encima del terreno.**
Lo que produce un impacto elemental sobre terreno de otro elemento. **Duran poco y se van.**

**③ Marcas de especie — independientes de todo lo anterior.**
Cosas que siembra una especie concreta y que **no son terreno ni reacción**: primer caso, el **«combustible»** de Pyrecko. Reglas propias en §5.

## 2. Las tres salidas

| Salida | Cuándo | Cómo se siente |
|---|---|---|
| **Dominancia** | entra el elemento que **supera** al del terreno | **Ruidosa**: fogonazo, AoE, mucho aturdimiento. El pan y la mantequilla. |
| **Combinación** | los dos elementos son de **triángulos distintos** (ninguno supera al otro) | **Suave**: capa visual propia, efecto secundario, sin fogonazo. |
| **Anulación** | entra el elemento **superado** por el del terreno | **No pasa nada.** El terreno manda; el elemento entrante se disipa. *(Antes lo llamábamos "fizzle" — nombre en español desde 2026-07-18.)* |
| *(mismo elemento)* | entrante == terreno | Nada. Fuego sobre lava es fuego. |

> **Toda reacción levanta además corrientes de viento** ✅ *(2026-07-18)* — las mismas que existen como terreno de Aire, pero como **efecto temporal** (el terreno no cambia, §1). Hoy solo las aprovecha **Mantagale**, que se **eleva** con ellas (§5): por eso el terreno de Aire es su mejor zona y el de Rayo —donde su Aire se anula— lo deja sin combustible. *Abierta: ¿puede aprovechar también las corrientes de las reacciones que provoquen sus compañeros de equipo? Sería sinergia de equipo gratis.*

## 3. Reglas de resolución ✅ ratificadas

**R1 · Siempre contra el terreno.** Un impacto resuelve contra el **elemento del terreno**, nunca contra un efecto que siga activo encima. *(Ottorock convierte un trozo de río en barro; dos segundos después Pyrecko dispara ahí: el fuego se encuentra el **río**, no el barro → anulación.)* Esto es lo que impide la explosión combinatoria: la matriz es 6×6 y punto.

**R2 · La duración es proporcional al impacto.** Cuanto más fuerte el golpe que irradia el elemento, más dura la reacción. Premia meter la habilidad pesada en el sitio correcto en vez de escupir habilidades baratas.

**R3 · Un impacto nuevo cancela el efecto activo.** Al impactar en un punto donde ya hay una reacción viva, esta **se cancela** y se resuelve de cero contra el terreno. → **Counterplay explícito:** atacar una zona con reacción activa sirve para **cancelarla** (así se limpia el barro de Ottorock, o cualquier zona que estorbe).

**R4 · Regla direccional.** La dominancia **solo se dispara cuando entra el predador**. Al revés (entra la presa) → **anulación**: no puedes quemar lo mojado, pero sí apagar lo ardiente.

**R5 · Superficie neutra → nada.** Sin elemento de terreno no hay reacción. *(Consecuencia buscada: la arena del gate es gris y sin reacciones — ver §8.)* **Única excepción: la roca sellada de Ottorock** (§5), que lleva su reacción ya resuelta y por eso funciona también sobre neutro.

**R6 · El estallido no golpea a quien lo provoca** (ni a su bando). El **efecto de zona** resultante sí es neutral: afecta a quien entre, de cualquier bando. *(Sin esto, Ottorock se electrocuta a sus propios pies al agarrar sobre Rayo, y un Ottorock salvaje se regalaría el noqueo solo.)*

## 4. La tabla — 15 parejas con efecto (+ las 6 anulaciones, §7)

**Los dos triángulos:** ① **Naturaleza › Agua › Fuego › Naturaleza** · ② **Mineral › Rayo › Aire › Mineral**. Dentro de un triángulo hay **dominancia** (entra el predador) o **anulación** (entra la presa); entre triángulos, siempre **combinación**.

Cada elemento tiene **2 relaciones de triángulo** (a quién supera, quién lo supera) **+ una firma** que explica su fila entera:

| Elemento | Firma | Elemento | Firma |
|---|---|---|---|
| **Fuego** | quema; el aire lo aviva | **Naturaleza** | absorbe y propaga |
| **Agua** | conduce, apaga, embarra | **Mineral** | endurece, cristaliza |
| **Rayo** | electrifica lo que toca | **Aire** | agita, aviva y erosiona |

### Dominancias (6) — ruidosas
| Predador entra sobre terreno de… | Reacción | Efecto temporal |
|---|---|---|
| **Naturaleza › Agua** | las raíces beben el charco → **crecimiento explosivo** | brotes que **atrapan** (root ~1 s, solo a quien esté en el momento del disparo; puede atacar y bloquear) |
| **Agua › Fuego** | apaga → **golpe de vapor** | daño leve + **cortina que rompe el lock-on** a través de ella |
| **Fuego › Naturaleza** | quema → **incendio** | arde; *(rider de propagación por vegetación → Fase 4, §8)* |
| **Mineral › Rayo** | *grounding* → **descarga a tierra** | AoE de daño + aturdimiento alto |
| **Rayo › Aire** | la tormenta domina → **descarga de tormenta** | AoE de daño + aturdimiento alto *(la variante "caen relámpagos uno a uno" queda 🔮: exige scheduler y telegraph por rayo)* |
| **Aire › Mineral** | erosión → **tormenta de esquirlas** | daño cortante + empuje leve |

### Combinaciones (9, cross-triángulo) — suaves
| Pareja | Efecto temporal |
|---|---|
| **Rayo + Agua** ★ | **agua electrificada** — parálisis a quien entre *(la estrella del pitch)* |
| **Rayo + Fuego** | **pequeñas explosiones** en el aire |
| **Rayo + Naturaleza** | **vegetación electrificada** — daña (Rayo) a quien la pise *(la variante "guarda la carga para el siguiente golpe" se descartó: contradice R2/R3 y exigía tocar código)* |
| **Mineral + Agua** | **barro** — ralentiza *(la combinación insignia de Ottorock)* |
| **Mineral + Fuego** | **roca al rojo** — quema al pisar |
| **Mineral + Naturaleza** | **púas cristalinas** — suelo espinoso que daña al pisar *(sin colisión física: no es cobertura; eso vive en `CreateWall/Cover`)* |
| **Aire + Fuego** | **avivado** — el fuego crece y daña más |
| **Aire + Agua** | **niebla** — cortina amplia, sin daño (la versión suave del vapor) |
| **Aire + Naturaleza** | **siembra** — la maleza se agita y el efecto cubre **un área mayor** *(crece en el sitio; no desplaza nada — el terreno no se mueve)* |

> **Aturdimiento ≠ stun.** El "aturdimiento" de las dominancias llena la **barra de Aturdimiento** (la de captura, GDD §8.2); no es un control que te quite el mando. Los CC reales (root, parálisis, empuje) están dichos por su nombre.

## 5. Excepciones por especie

**Ottorock — combinación sellada portátil** ([ficha](CREATURE-OTTOROCK.md)). Es su mecánica insignia en Sujeción: al **agarrar** el terreno se resuelve **una vez** `Mineral + elemento del terreno` y el resultado queda **sellado en el trozo**. A partir de ahí **cada impacto reproduce esa misma reacción**, en cualquier superficie — **incluida la neutra**. Elige su combinación eligiendo **dónde agarra**: agarra en el río → lleva un generador de barro portátil.
- Terreno neutro al agarrar → **roca pura**: el trozo reacciona como **Mineral** contra el terreno donde impacte.
- Si el agarre dispara una **dominancia** (agarrar sobre terreno de Rayo → descarga a tierra), esta se consume ahí y el trozo queda **puro**.
- Al volver a Libre (romper o lanzar la roca) **pierde la combinación**.

**Pyrecko — el «combustible»** ([ficha](CREATURE-PYRECKO.md)). Marca de especie, **no** elemental:
- **Inerte**: solo su Signature la detona. La **explosión sí irradia Fuego** → ese fuego resuelve normalmente contra el terreno donde caiga (sobre terreno de Agua → **anulación**: no prende).
- **Solo desaparece de dos formas: al expirar (10 s) o al detonarla.** ✅ *Las reacciones elementales ya **no** la limpian (regla anterior derogada 2026-07-18).*
- **Pertenece al lugar, no al material:** si Ottorock arranca el trozo de terreno donde hay combustible, se lleva **el terreno, no la marca** — el combustible se queda donde estaba.

**Mantagale — tornados que empujan y detonan por colisión** ([ficha](CREATURE-MANTAGALE.md) §C). ✅ *(2026-07-18: sustituye tanto a "mover/dispersar zonas" como a "dispara reacciones mientras recorre" — ambas derogadas.)*
- Sus **tornados empujan durante todo su recorrido** (negación de área), pero son **inertes en lo elemental**: no tocan el terreno ni disparan reacción al viajar — igual que el combustible de Pyrecko, son una marca de especie con su propia lógica.
- La reacción ocurre **al reventar**, y revientan por **colisión de fuerzas opuestas**: el tornado empuja, su Ciclón **atrae**, y el choque los detona. La reacción resuelve contra el terreno **del punto de colisión**, con el enemigo arrastrado allí dentro.
- El Ciclón **arrastra al enemigo por el suelo** hasta el centro (no lo teletransporta): al volver, **atraviesa los tornados sembrados por el camino**.
- Su fuerza **depende del terreno de la arena** — es el que "juega el tablero". Sobre **Rayo** (anulación) y **neutro** no consigue nada: es su debilidad deliberada.

## 6. Legibilidad
- **Dominancia = ruidosa · combinación = suave · anulación = su propio feedback** (VFX+SFX genérico compartido: un soplo con humillo). Sin ese tercer nivel, una anulación se lee como un bug.
- Familia de volúmenes nublados, que hay que poder distinguir: **vapor** = ráfaga vertical blanca breve · **niebla** = capa baja persistente · **nube/zona de Rayo** = chispas constantes.
- Los efectos **se dispersan solos** (R2) y el terreno vuelve: el mundo nunca queda pintado.

## 7. Implementación (data-driven)
- `ElementalSurface { Element element; }` — **fijo**, asignado en escena. Sin `currentState`: el terreno no muda.
- **`ReactionTable`** (asset) mapea **el par ordenado** `(elementoTerreno, elementoEntrante) → (efecto, duraciónBase)`. Ordenado porque R4 lo exige: `(Agua, Fuego)` = anulación y `(Fuego, Agua)` = vapor.
- **Autorar explícitamente las 6 anulaciones** como entradas de **tipo Anulación**: sin payload de daño ni CC, pero **con el feedback compartido de §6**. El resultado coincide con el de un par sin autorar (no pasa nada), pero la anulación **tiene que verse**: si no, se lee como un bug.
- `ReactionSystem.Impact(pos, radius, elemento, fuerzaImpacto)` → busca superficie, resuelve por tabla, instancia el efecto con `duración = f(fuerzaImpacto)` (R2), cancelando el efecto activo en ese punto (R3) y marcando al instigador (R6).
- Par sin entrada autorada = **sin efecto** (el impacto ocurre, la reacción no).
- Añadir/quitar reacciones = editar la tabla, **sin tocar código**.

## 8. Alcance para el prototipo
- **Base (Fase 2):** `ElementalSurface` + `ReactionSystem` + `ReactionTable` + **un efecto genérico de dominancia parametrizado** (fogonazo + AoE + aturdimiento) → las 6 dominancias entran como **datos**, no como 6 sistemas bespoke. Anulaciones autoradas con su feedback.
- **La arena de Fase 2 necesita parches de terreno elemental** (charco, matorral, veta de roca…): en caja gris son quads etiquetados. **Sin ellos no hay nada que demostrar** (R5).
- **Nodo de cristal junto a un charco** que suelta su carga **al ser golpeado** → única forma de ver **agua electrificada** (★ del pitch) en el slice, porque ninguna de las 3 criaturas genera Rayo ni Agua. Funciona ya con Ottorock-Libre.
- **Herramienta de debug: "spray elemental"** para disparar cualquier pareja sin criatura que la genere.
- **Bespoke por olas, cada uno con su criatura:** barro/roca al rojo/descarga con Ottorock-Sujeción (post-gate) · incendio + propagación con Pyrecko (Fase 4) · esquirlas/niebla/avivado/siembra con Mantagale (post-prototipo).
- *Propagación del incendio sin grafo de vecindad:* la zona ardiente re-lanza `Impact(Fuego)` en radio cada N s; los saltos aplican quemadura **sin** el payload completo de dominancia (evita cadenas de aturdimiento) y **cada punto solo propaga una vez por incendio** — el contador vive en el efecto, **no en el terreno** (que nunca cambia, §1).
