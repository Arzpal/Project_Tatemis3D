# Tatemis3D — Ficha: Mantagale (Controlador de Aire, volador)

> **📌 Nombre y animal cerrados (2026-07-18): «Mantagale» — manta + *gale* (vendaval).** Es una **mantarraya aérea**, no un búho. *Antes se llamaba "Cyclowl".* Motivo del cambio: mecánicamente el búho es un **emboscador silencioso** (fantasía de Ejecutor, y pisaba el sigilo de Pyrecko), mientras que la manta **planea con aleteos lentos** y —lo decisivo— **se alimenta generando vórtices que embudonan el plancton hacia su centro**: el Ciclón no es una metáfora, es cómo come el animal. Además su silueta plana y ancha se distingue de un vistazo de Pyrecko y Ottorock.
> ✅ **Ficha de 18 campos reescrita (2026-07-18)** al kit nuevo (§C) + los stats fijados ([CREATURES.md](CREATURES.md) §4). El **detalle profundo** del diseño vive en **§C** y el **brief de rediseño** en §B, conservados como registro. Números de habilidad aún abiertos → §C.6.

> **TL;DR:** Controlador aéreo **autosuficiente** (ref. dictada: **Aurelion Sol**, LoL): complica la movilidad del rival con mini-tornados —que además provocan reacciones elementales— y lo castiga a distancia con **ráfagas de aire** (su básico, su herramienta de daño). **Semilla dictada por Arzpal (2026-07-17):** volador · Aire · Controlador · mini-tornados · básico de ráfagas · verticalidad como ventaja y riesgo de balance · **principio: todo tatemis debe poder ganar solo**. **El resto es propuesta** (marcado *[P]*) — ratificar/editar campo a campo. Propósito doble: 3ª especie del slice + cubrir los puntos ciegos de [ABILITY-EXTRACTION.md](ABILITY-EXTRACTION.md) §7 (vuelo, desplazamiento de enemigos, cargas/canalización). Se construye post-prototipo; la ficha existe **ya** para que las Fases 0–2 no asuman "solo suelo". Plantilla → [CREATURES.md](CREATURES.md) §2.

> 🔴 **AUDITORÍA DEL CICLO (2026-07-18) — el kit NO cierra como bucle.** 5 análisis independientes coincidieron: es problema de **diseño**, no de redacción. Causas: ① **no tiene motor de recurso** — vuelo, Flex y Signature beben de la misma barra y nada la llena salvo bloquear quieto en el suelo (su peor estado), mientras que el básico gratis de Ottorock y Pyrecko **sí** alimenta su mecánica y el de Mantagale no alimenta nada; ② **las cuentas no caben** (despegue 10 + tornado 40 + Ciclón 80 = 130 sobre 100, antes del drenaje), y el juego óptimo resulta ser **no volar**; ③ **Flex y Signature hacen el mismo verbo** (empujar + disparar reacciones) → no hay setup→payoff, y el tornado **aleja** al enemigo de las reacciones que provoca; ④ la referencia **Aurelion Sol está invertida** (su CC es centrípeto: mete al enemigo *dentro* del daño; el de Mantagale es centrífugo). **Arzpal rediseña el kit** (2026-07-18) — brief de requisitos en §B, abajo. **→ Resuelto (§C):** kit nuevo cerrado y la tabla de 18 campos de arriba ya está **reescrita a ese kit** (2026-07-18).

| # | Campo | Valor |
|---|---|---|
| 1 | Nombre + concepto | **Mantagale** — *ser el viento: nadie te toca y tú decides dónde está cada uno; te arrastro a mi terreno y lo detono encima de ti*. CC **centrípeto** (patrón Aurelion Sol de verdad: su control mete al enemigo **dentro** de su propio daño). Tríada del slice: *Ottorock lleva el terreno al enemigo · Pyrecko prepara el terreno y lo enciende · **Mantagale lleva al enemigo al terreno**.* |
| 2 | Elemento | **Aire** — débil a **Rayo** (1.5x, 2x en forma pura); fuerte contra **Mineral**. Su terreno propio son las **corrientes de aire**. |
| 3 | Arquetipo | **Controlador** — CC y manipulación del espacio, pero **autosuficiente** (debe poder ganar solo, [CREATURES.md](CREATURES.md) §2). Hexágono completo → [CREATURES.md](CREATURES.md) §4. |
| 4 | Silueta y tamaño | **Mantarraya aérea** (manta + *gale* = vendaval). Silueta **plana y ancha**, se distingue de un vistazo de Pyrecko/Ottorock; planea con aleteos lentos. Se alimenta generando vórtices que embudonan el plancton a su centro — el Ciclón es literalmente cómo come. **Escala base (forma bebé, dictada 2026-07-22):** ~**55 cm** de largo (cabeza a cola) pero ~**70 cm** de aleta a aleta → **1/5 de un Tamer**; el **más ancho** del trío (su medida grande es horizontal). *(Base no restrictiva; sube con la evolución.)* |
| 5 | Vida / Daño / Velocidad | **20 / 30 / 50** (proporciones relativas) → frágil · daño bajo pero **sostenido** · muy rápido. *Mata lento, controla siempre.* Hexágono de 6 ejes (Vida 3 · Daño 3 · Velocidad 9 · Alcance 6 · Control 8 · Sinergia 8) → [CREATURES.md](CREATURES.md) §4. |
| 6 | Peso percibido | Muy ligero. |
| 7 | Locomoción | **Siempre vuela**, por defecto **a ras de suelo**. Volar **no es habilidad ni gasta energía**: la **altura es una pasiva** que dan las **corrientes generadas por cada reacción elemental**. Sin reacciones, **planea y baja**. Terreno de **Aire = su mejor zona** (ya es corriente pura); **Rayo y neutro = sus zonas malas** (no combina → no sube) — desventaja deliberada. Estar arriba **siempre conviene** (más difícil de golpear). |
| 8 | **Movilidad** — Elevarse | Se eleva de golpe y **suelta 1 tornado**. Es también su **dodge**: la vía **rápida y segura** de sembrar (*setup barato*). · Energía/CD **por balancear** · Targeting: self. |
| 9 | **Flex** — Potenciar básicos | Los **siguientes 3 ataques básicos** pasan a **golpear en zona**, **empujar con daño** y **generar 1 tornado cada uno** (al golpear enemigo **y** al golpear el suelo). Vía **cara y arriesgada** de sembrar (hay que conectarlos) y **su ventana de daño real** (*setup masivo + daño*). ⚠️ **Vigilar sobrecarga:** AoE + empuje + daño + tornado, ×3 golpes — lo compensa su daño base bajo, pero es el primer sitio a mirar si sale roto. · Energía/CD **por balancear**. |
| 10 | **Signature** — Ciclón | **Atrae** hacia su centro **a los enemigos y a los tornados**, y hace daño (*payoff*). El **choque de las dos fuerzas** —el tornado que empuja contra el ciclón que atrae— provoca la **explosión** → **reacción contra el terreno bajo el centro** → corrientes → **sube**. El arrastre trae al enemigo **por el suelo**, atravesando los tornados sembrados en el trayecto (*empujas fuera → siembras el camino → lo traes de vuelta por tu campo de minas*). Como eliges dónde flotas, **eliges el elemento eligiendo dónde poner el centro**. · Energía/CD **por balancear** · máx. **4 tornados** (FIFO). |
| 11 | Ataque básico (sin potenciar) | **Pequeñas ráfagas de aire**: cadena de **4** proyectiles, alcance medio, ritmo rápido, daño bajo pero sostenido; en vuelo dispara hacia donde mira. **Sin coste.** Normal salvo cuando la Flex lo potencia (campo 9) — ahí se vuelve su motor de siembra. |
| 12 | Bloqueo / forma elemental | **Su rasgo diferencial: puede bloquear y recargar EN EL AIRE** → es su **motor de energía** (la barra de tinta se rellena sobre todo así). Coste: se queda **quieto**, y si lo golpean **se interrumpe y pierde algo de altura**. En forma pura su debilidad —**Rayo**— pasa a 2x ("la tormenta domina los cielos"). *(Sin AoE al bloquear: el bloqueo es defensivo puro, GDD §6.3.)* |
| 13 | Plan de juego | Sembrar tornados (Elevarse barato / Flex arriesgado) → **Ciclón** arrastra enemigo y tornados al mismo punto → estallan **encima del enemigo** (CC centrípeto) → reacción → corrientes → **sube**. Autosuficiente pero de **matar lento**: gana por **desgaste**. **La verticalidad se paga:** golpe en el aire = pérdida de altura/derribo; daño bajo; en arena **neutra o de Rayo no gana altura**. **Ventaja única del roster:** volando **no pisa sus propias reacciones** — el único que puede envenenar el suelo sin sufrirlo. **Debilidad táctica:** tiradores (esp. **Rayo**, su predador) y ejecutores con burst que lo borran antes de que el desgaste cuente. |
| 14 | Sinergia ambiental | **Es el que más juega el tablero:** su valor sube con la **variedad de terreno elemental** (un Ciclón que revienta sobre charco, o sobre veta, da reacciones distintas). **Elige el elemento** colocando el centro del Ciclón sobre la celda que quiere. Clima ventoso (GDD §8.1): tornados duran/crecen más. ⚠️ **Contrapartida (R5):** en arena **neutra** su plan cae casi a cero → su poder oscila mucho por escenario. |
| 15 | Como salvaje (IA) | Vuela hostigando con **ráfagas** constantes; siembra tornados para cortar el paso y provocar reacciones; **al agotar la energía baja a bloquear/recuperar** → ventana de castigo. **Telegraph:** antes del Ciclón se cierne estático aleteando con fuerza. ⚠️ Afinar. |
| 16 | Orden rápida (aliado) 🔮 | **Apartar:** desplaza a los enemigos respecto a un punto marcado (p. ej. defender a un caído — [VITALITY-DEATH](VITALITY-DEATH.md) §4). |
| 17 | Formas evolutivas 🔮 | Mini-misión: acumular **tiempo en altura** + **disparar N reacciones distintas** con sus tornados. |
| 18 | Rareza 🔮 | Difícil de **capturar** (hay que derribarlo); techo medio-alto. |

> **Detalle profundo del diseño → §C** (premisa de la altura pasiva, triángulo estratégico, validación de 5 lentes). **Números de habilidad aún abiertos → §C.6** (altura máxima, escalón por reacción, planeo, costes/CD, coste en altura de un golpe recibido, y las dos microdecisiones: tornados atraídos en secuencia vs a la vez · FIFO). **§B** es el brief de rediseño, conservado como registro.

---

# §C · Kit nuevo (dictado por Arzpal, 2026-07-18) — 🟢 los 3 slots cerrados

## C.1 · La premisa
**La recompensa de jugar bien es mantenerse en el aire.** Mantagale **siempre vuela**, por defecto **a ras de suelo**. Volar **no es una habilidad ni gasta energía**: la altura es una **pasiva**, una reacción inmediata que el jugador no controla directamente.

**Cómo se gana altura (mecanismo unificado):** toda **reacción elemental genera corrientes de viento** —las mismas que existen como terreno de Aire— y **son esas corrientes las que lo elevan**. De ahí que el terreno de **Aire sea su mejor zona** (ya es corriente pura) y que **Rayo y neutro sean sus zonas malas**: donde no puede combinar, no sube. Es su desventaja deliberada.

Sin reacciones, **planea** y baja. Estar arriba **siempre conviene** (más difícil de golpear): no hay razón para bajar voluntariamente — *"planificar cuándo caer"* aplica solo cuando las cosas salen mal.

## C.2 · El kit completo ✅ *(los 3 slots, cerrados 2026-07-18)*
- **Movilidad — Elevarse:** se eleva y suelta **1 tornado**. Es también su dodge: la vía **rápida y segura** de sembrar. *(Setup barato.)*
- **Flex — Potenciar básicos:** los **siguientes 3 ataques básicos** pasan a **golpear en zona**, **empujar con daño** y **generar 1 tornado cada uno**. Es la vía **cara y arriesgada** de sembrar (hay que conectarlos) y **su ventana de daño real**. *(Setup masivo + daño.)*
- **Signature — Ciclón:** **atrae** hacia su centro **a los enemigos y a los tornados**, y **hace daño**. El choque de las dos fuerzas aéreas opuestas —el tornado que empuja contra el ciclón que atrae— provoca la **explosión** → **reacción elemental contra el terreno del centro** → corrientes → **sube**. *(Payoff.)*
- **Bloqueo — su rasgo diferencial: puede bloquear y recargar EN EL AIRE.** Coste: se queda **quieto**, y si lo golpean **se interrumpe y pierde algo de altura**. Es su **motor de energía**.
- **Ataque básico (sin potenciar):** normal. Lo interesante del personaje está en acertar o fallar las habilidades, que es lo que lo mantiene arriba.

**Dos rutas para sembrar, con riesgo distinto** — la decisión táctica que le faltaba: Movilidad (1 tornado, instantáneo, es tu esquiva) vs Flex (hasta 3, pero hay que conectar los golpes y gastas tu ventana de daño).

## C.2b · Triángulo estratégico entre las 3 especies (dictado)
- **Pyrecko > Mantagale** — no necesita acercarse para dañarlo, y si Mantagale cae, le llena el suelo de combustible y lo detona.
- **Mantagale > Ottorock** — es lento, le cuesta alcanzarlo, el Ciclón lo aleja y su lentitud lo hace blanco fácil (más Aire › Mineral).
- **Ottorock > Pyrecko** — tankea a distancia, se acerca al melee; si Pyrecko huye, vuelve a modo tanque y repite.

> Nota: el triángulo nace de los **kits**, no de la tabla elemental (Fuego es neutral con Mineral y con Aire). Solo la arista Mantagale›Ottorock está reforzada por el elemento — lo que **confirma** el riesgo de balance ya anotado (§B.6: se countean dentro del propio equipo).

## C.3 · Veredicto de la validación (5 lentes, 2026-07-18)
**✅ Cierra los 2 agujeros que hundieron el kit anterior:**
- **Cadena setup→payoff real** (agujero §B.2③): Elevarse siembra, el agujero negro cobra. Verbos distintos, ya no hay dos slots haciendo lo mismo.
- **CC centrípeto** (agujero §B.2④): arrastra al enemigo **dentro** de la reacción. Repara la inversión de Aurelion Sol que denunció la auditoría; se puede cerrar §B.6.
- **Bonus:** quitar el stance de vuelo mata de raíz la línea óptima "no volar" del kit viejo. Y la latencia es inmediata: pulsas y **subes**.

**⚠️ Problemas abiertos (por orden de gravedad):**
1. **El motor recarga el recurso equivocado.** Recarga la **altura**, pero la **energía elemental** que paga Elevarse y el agujero negro sigue sin fuente salvo bloquear — y bloquear ahora te obliga a estar abajo. Simulación con números plausibles (barra 100, regen 8/s, costes 30/45): se seca a los **~15 s / 2,5 ciclos** y **la altura máxima es inalcanzable**. *Arreglo propuesto: que cada reacción formada devuelva también ~20 de energía → el acierto se autofinancia y el fallo se paga entero.*
2. **Arena neutra = premisa a cero.** El Aire solo reacciona en **4 de 7** tipos de celda (Mineral→esquirlas · Fuego→avivado · Agua→niebla · Naturaleza→siembra; **Rayo→anulación**, **Aire→nada**, **neutro→nada**). En arena gris no gana **ni un metro** de altura → falla el test 5 (autosuficiencia), y **la arena del gate es gris** (ELEMENTAL-REACTIONS §8). *Curiosidad: no puede cargar sobre su propio terreno de Aire (absurdo), y sí se queda sin combustible sobre el de Rayo, su counter (perfecto).*
3. **El azar decide su recurso.** Con tornados a ~3 m/s y radio ~5 m: ~1,1 reacciones esperadas por ciclo, varianza enorme. Contradice la premisa ("la recompensa de **jugar bien**"). *Arreglo: ver C.4.*
4. **La altura solo sirve para no ser golpeado** — premio puramente defensivo → óptimo pasivo (subir y no bajar), y "planificar cuándo caer" no es decisión si caer es pérdida pura.
5. **Contradice [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §5**, que dice que el tornado dispara reacciones *mientras recorre*. Con el kit nuevo el tornado debe ser **inerte al viajar** (como el combustible de Pyrecko) y reaccionar **solo al reventar**; si no, el bucle se rompe por arriba. Hay que reescribir esa entrada.
6. **Se puede farmear altura sin pelear:** reventar en el vacío recarga igual → la línea óptima es alejarse, subir y luego acercarse (falla tests 4 y 8).
7. **Bloquear le cuesta la identidad** y, si vuela siempre, no está claro que tenga bloqueo — le faltaría una acción universal del GDD.
8. **Riesgo en el test 6:** "siembro dos marcas y las detono" es *literalmente* el verbo de Pyrecko. Lo que lo salva es el arrastre centrípeto (Pyrecko detona lejos y huye; Mantagale detona encima y te mete dentro) y el eje vertical — pero hay que **escribirlo explícito** en la ficha o no se distingue.

## C.4 · El cambio que arregla tres cosas de golpe
**Que el agujero negro arrastre también los tornados hacia su centro**, no solo a los enemigos. Con eso:
- el azar pasa a ser **decisión** (colocas el centro, todo converge ahí) → arregla el problema 3;
- el tornado detona **encima del enemigo**, no en el borde → cierra de verdad el agujero §B.2④;
- y como la reacción resuelve contra el terreno **bajo el centro**, y el centro lo eliges tú flotando: **Mantagale elige el elemento eligiendo dónde ponerse** → el framing *"Mantagale te lleva a un elemento"* se vuelve literal.

## C.5 · Las 4 preguntas bloqueantes — ✅ RESUELTAS (2026-07-18)
1. **Aire de alas y energía = dos cosas distintas.** La altura es **pasiva** (la dan las corrientes que generan las reacciones); la energía solo paga habilidades. **Volar no cuesta energía.** → *El problema del motor se resuelve por otra vía: el **bloqueo en el aire** es la fuente de energía.*
2. **Basta con que la reacción se forme** — no hace falta alcanzar a un enemigo. *(Coherente con el mecanismo de corrientes: la corriente nace de la reacción, no del impacto.)*
3. **Sí puede bloquear en vuelo.** Es su rasgo diferencial. Coste: quieto, e interrumpible con pérdida de altura.
4. **Ni "empuja" ni "atrae": las dos.** El tornado **empuja**, el Ciclón **atrae**, y **el choque de ambas fuerzas es lo que detona** la reacción. *(Mejor que la propuesta de C.4: los tornados sirven antes de estallar, el Ciclón reúne enemigo y tornado en el mismo punto, y hay una lógica física legible.)*

## C.6 · Lo que queda abierto
1. ✅ **Kit cerrado.** Reglas de tornados ratificadas (2026-07-18):
   - **Arrastre confirmado:** el Ciclón trae al enemigo **por el suelo** hasta el centro, atravesando los tornados sembrados en el trayecto. Empujas fuera → siembras el camino → lo traes de vuelta por tu campo de minas.
   - **El básico potenciado genera tornado al golpear enemigo Y al golpear el suelo** (ambos).
   - **Duración de los tornados: 6/10** (intermedia).
   - **Máximo 4 tornados** simultáneos; al crear uno por encima del tope desaparece **el más antiguo** (FIFO) → siempre puedes refrescar el campo.
   - ⚠️ **Vigilar sobrecarga del Flex:** da AoE + empuje + daño + tornado, ×3 golpes. Es mucho valor por botón frente a las Flex de Ottorock y Pyrecko; compensa que su daño base sea el más bajo, pero es el primer sitio donde mirar si sale roto.
   - 🟡 **Dos microdecisiones pendientes:** ¿los tornados atraídos revientan **en secuencia** (más legible, se lee cada reacción) o **a la vez** (más espectacular, riesgo de papilla visual con 3 reacciones distintas)? · Con el tope de 4, ¿desaparece **el más antiguo** (permite refrescar el campo) o el recién creado (el input se desperdicia)?
2. ✅ **[ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §5 alineado** (2026-07-18): el tornado **empuja durante todo el recorrido** pero es **inerte en lo elemental** (no dispara reacción al viajar); solo reacciona **al reventar** por choque con el Ciclón.
3. **Documentar las corrientes generadas por reacción** en ELEMENTAL-REACTIONS: son un **efecto temporal** más (no cambian el terreno, R1). *Pregunta abierta: ¿puede aprovechar las corrientes de reacciones provocadas por sus compañeros de equipo? Sería sinergia de equipo gratis.*
4. **Coste en altura de un golpe recibido** fuera del bloqueo (¿escalón fijo, proporcional al impacto, derribo desde altura máxima?).
5. **Tope y duración de los tornados**; si el enemigo puede reventarlos y quién contaría como instigador (R6).
6. **Riesgo de tiempo de combate:** su único daño son los básicos (bajo y constante) y a Ottorock lo gana kiteando — con 30 de vida + la barra extra de la roca, ese duelo puede eternizarse. Vigilar en playtest que "ganar por desgaste" **termine**.
7. **Grado de la debilidad en arena neutra:** aceptada como diseño, pero es casi total (sin altura, sin identidad, solo básicos). *Posible suelo mínimo si en playtest resulta injugable: que el estallido del tornado —que es aire— genere una corriente **pequeña** aunque no haya reacción.*
8. **Cámara/rejilla** para leer el tablero, y **números** (altura máxima, escalón por reacción, velocidad de planeo, costes, CD).

---

# §B · Brief de rediseño (para Arzpal)

> Qué tiene que cumplir el kit nuevo. **No dice qué habilidades hacer** — dice qué agujeros tapar y qué tests pasar.

## B.1 · Restricciones fijas (vienen de decisiones ya cerradas, no se tocan)
- **3 slots**: Movilidad · Flex · Signature. Más las universales: **ataque básico gratis**, salto, **bloqueo/forma elemental pura**, noqueo contextual (GDD §6.3).
- **Elemento Aire**: débil a Rayo (1.5x, y 2x en forma pura), fuerte contra Mineral. Su terreno propio son las **corrientes de aire**.
- **Arquetipo Controlador** · **volador** · **frágil y rápido**.
- **Autosuficiente**: tiene que poder ganar un combate solo ([CREATURES](CREATURES.md) §2).
- **Energía tipo tinta**: se gasta al castear, regenera por segundo, el bloqueo acelera la regen (GDD §6.2).
- **Sistema elemental**: terreno fijo por celdas, reacciones = efectos temporales, reglas R1–R6 ([ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md)).
- **Tiene que ser el tercer extremo de diseño**: Ottorock **elige una combinación y la repite** de cerca (compromiso) · Pyrecko **acumula y detona** de lejos (paciencia) · Mantagale tiene que ser **la tercera cosa**, no una variante de esas dos.

## B.2 · Los 4 agujeros que el diseño DEBE tapar
Son los que hundieron el kit anterior. Cada uno es una pregunta que el diseño nuevo tiene que responder:

1. **¿De dónde sale la energía?** Necesita un **motor**: algo que devuelva recurso (o que abarate el gasto) **jugando bien**, no dejando de jugar. Referencias internas: a Ottorock la energía nunca lo limita (mandan los cooldowns); Pyrecko siembra gratis con el básico. Hoy Mantagale solo recupera bloqueando quieto en el suelo, que es su peor estado.
2. **¿Qué hace su ataque básico POR su mecánica?** El de Ottorock reproduce la reacción sellada; el de Pyrecko siembra combustible. El de Mantagale solo hacía daño — por eso el juego óptimo era volar y spamearlo sin usar el kit.
3. **¿Qué slot prepara y qué slot cobra?** Hace falta una **cadena**. Ottorock: Signature sella → Flex y básico cobran. Pyrecko: Movilidad, Flex y básico siembran → Signature detona. El kit viejo tenía dos slots haciendo el mismo verbo, así que no encadenaba nada.
4. **¿Cómo hace que el enemigo se coma la reacción?** Las reacciones ocurren en el terreno y solo dañan a quien entra (R6). Nada obligaba al enemigo a entrar — de hecho el tornado lo **alejaba**. Sin este mecanismo, los fogonazos son decorativos.

## B.3 · Decisiones sueltas que hay que dictar
- ¿El **vuelo** cuesta energía de la barra común, tiene medidor propio, o es gratis con otro límite?
- ¿Se puede **bloquear en vuelo**? *(Determina si existe una fase de recuperación viable — decidir esto antes que los números.)*
- **Derribo**: ¿binario (un golpe y caes) o gradual (barra de estabilidad que se llena)? El binario encadena castigos y se lee como stunlock.
- Si hay habilidad **canalizada**: ¿puede moverse mientras carga? ¿Se puede soltar antes con efecto parcial? *(Canalizar quieto, volando y con 20 de vida es inviable en un juego con esquiva.)*
- **Cómo lee el jugador el tablero**: con lock-on la cámara mira al enemigo en horizontal y las celdas se ven de canto. ¿Volar sube e inclina la cámara? ¿Lock-on y vista de tablero son excluyentes? *(Mantagale es la única criatura que necesita leer el tablero como mapa.)*
- **¿La rejilla se ve o solo se intuye?** Mantagale fuerza esta decisión de arte que [ELEMENTAL-REACTIONS](ELEMENTAL-REACTIONS.md) §1 dejó abierta. *(Compromiso posible: no se ve nunca, salvo al apuntar la habilidad que la necesita.)*
- **Números del vuelo**, y **nombre/silueta** definitivos (Mantagale/búho es placeholder; alternativa: mantarraya aérea).

## B.4 · Los 8 tests que tiene que pasar
Cuando tengas el kit, pásale esta lista. Si falla uno, no está listo:

1. **Bucle** — escribe 30 s de combate segundo a segundo. ¿Hay fase de gasto **y** fase de recuperación, y la recuperación consiste en jugar?
2. **Básico** — ¿qué hace por tu mecánica, además de daño?
3. **Cadena** — ¿qué slot prepara y cuál cobra? Si dos hacen lo mismo, sobra uno.
4. **Impacto** — ¿el enemigo se come las reacciones, o pasan en el vacío?
5. **Arena neutra** — ¿puede ganar donde no hay terreno elemental? Si no, viola el principio de autosuficiencia.
6. **Tercer extremo** — ¿en qué se parece a Ottorock o a Pyrecko? Si acumula y suelta, es Pyrecko con otro traje.
7. **Latencia** — ¿cuánto pasa entre pulsar y ver algo? Ottorock: 0 s con pago garantizado. Un personaje de desgaste puede pegar flojo, pero no puede dejarte con la duda de si tu acción hizo algo.
8. **Óptimo** — ¿la forma más eficiente de jugarlo **usa** sus habilidades? Si la línea óptima las ignora, está roto.

## B.5 · Lo que conviene conservar
- La **fantasía**: *"nadie me toca; tú decides dónde está cada uno"*.
- El **vuelo** como identidad, y su ventaja estructural única: **volando no pisa sus propias reacciones** — es el único del roster que puede envenenar el suelo sin sufrirlo. Justifica su verticalidad y no estaba escrito en ninguna parte.
- Que su valor salga del **tablero** (jugar con el terreno, no añadirlo).
- La idea de la tríada: *Ottorock lleva el terreno al enemigo · Pyrecko prepara el terreno y lo enciende · **Mantagale lleva al enemigo al terreno***.
- Los **tornados** como objeto con presencia física — funcionaban como fantasía, el problema era el verbo que ejecutaban.

## B.6 · Otras abiertas que siguen vivas
- **Equipo:** Aire › Mineral → countea a Ottorock dentro del propio equipo (aceptado por ahora; revisar en balance).
- **Bloqueo ofensivo:** el campo 12 le daba un AoE al bloqueo, que es el botón *defensivo* (GDD §6.3 dice que no se puede atacar bloqueando). Si el rediseño lo mantiene, es una cuarta fuente de reacciones compitiendo con Flex y Signature.
- **Referencia Aurelion Sol:** o el kit nuevo reproduce el patrón de verdad (CC y daño **en el mismo objeto**, centrípeto: mete al enemigo dentro del daño), o se quita la referencia de la ficha para no confundir.
