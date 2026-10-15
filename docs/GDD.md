# Tatemis3D · Game Design Document

> **Nombre "Tatemis3D" es placeholder.** · **Hecho por Arzpal** · Solo (Arzpal: programación + diseño)
> **Qué es este documento:** solo las decisiones de diseño **cerradas**. Lo que está en discusión, lo que es a futuro, el alcance del slice, la producción y el historial de cambios viven fuera: alcance y fases en [ROADMAP.md](ROADMAP.md), estado en [TASKS.md](TASKS.md), y la versión completa con su historial en [archive/GDD-completo-2026-10-14.md](archive/GDD-completo-2026-10-14.md).
> La numeración de secciones se conserva a propósito: otros documentos citan estas secciones por número.
>
> **Mapa:** §1 visión/pilares · §2 público y plataforma · §4 narrativa/lore · §5 core loop · §6 combate · §7 elementos · §8 ambiente/reacciones/captura · §9 criaturas/progresión · §10 Tamer · §11 mundo · §13 dirección de arte · §17 glosario.

---

## 1. Visión y pilares

### 1.1 Pitch (una frase)
Un **creature-collector de acción en tiempo real** con tono *Cómo entrenar a tu dragón*: no coleccionas cientos de monstruos, **te vinculas con unos pocos** y los controlas por completo en combate, combinando sus habilidades únicas con el **elemento y el ambiente** para dominar y capturar bestias salvajes.

### 1.2 Los 3 pilares de diseño
1. **Combate expresivo, no repetitivo.** El corazón no son ataques que se repiten, sino **combinar las habilidades únicas de cada criatura + su elemento + el ambiente** (golpear el suelo para levantar polvo, usar la nieve, generar tornados con viento, **irradiar tu elemento sobre el terreno adecuado** para provocar reacciones).
2. **Vínculo por encima de colección.** Equipo máximo pequeño (3). Elegir a tus compañeros es una decisión con peso narrativo y estratégico. *Conectar, no coleccionar.*
3. **El mundo cuenta la historia.** Narrativa ligera y accesible en la superficie (tono Zelda/HTTYD); el lore profundo (los Tomos, su destrucción) queda oculto en pistas sutiles para quien explore.

### 1.3 Referencias de gameplay
Temtem · Pokémon Leyendas Z-A · con combate **soulslike + character-action con lock-on**. La captura y el vínculo beben de Pokémon; el combate, de los juegos de acción.

---

## 2. Público y plataforma

| Aspecto | Decisión |
|---|---|
| Plataforma | **PC / Consola** (mando y teclado+ratón) |
| Público | Fans de creature-collectors que quieren **experiencia de gameplay activa**; jugadores de acción |
| Tono | Zelda / *Cómo entrenar a tu dragón*: cálido y accesible, con lore oscuro opcional |
| Estilo visual | **Estilizado / cell-shaded** |

---

## 4. Narrativa y ambientación

### 4.1 Premisa (capa superficial, tono HTTYD)
El protagonista vive en una aldea que **teme a los monstruos**. Una aldea rival intenta destruir su pueblo usando bestias. Para defender a los suyos, el protagonista aprende a **conectar con los monstruos** en lugar de temerlos, demostrando que humanos y bestias pueden **vivir en armonía**. El arco emocional es un cambio de perspectiva: del miedo al vínculo.

### 4.2 Lore profundo (capa oculta, solo pistas sutiles)
En el pasado, la civilización se regía por **Ocho Grandes Tomos** que otorgaban control elemental. Un Gran Líder, temiendo que la humanidad no debía blandir tal poder, **destruyó los Tomos**. Su esencia se fundió con la naturaleza ("polución mágica"), dando origen a las **bestias elementales sentientes** y colapsando la civilización en una distopía de ruinas (**"Ancient Spots"**). La materia pura de los Tomos se convirtió en **Polvo sagrado (Pulvis)**, disperso por el mundo.
> **El juego no explica esto directamente.** Solo pistas ambientales sutiles (ruinas, murales, objetos).

### 4.3 El Polvo, el Sludge y los Tótems (justificación de la captura)
Los supervivientes ("Scavengers" / "Tamers") recogen **Polvo elemental** y lo mezclan con **agua** para crear un **sludge mágico**. Al lanzarlo sobre una criatura debilitada, reacciona con su esencia, **se endurece y cristaliza en un Tótem** con la forma simplificada de la bestia. El Tótem es su **vasija** hasta que se la invoca de nuevo.

---

## 5. Loop de juego (core loop)

```mermaid
graph TD
  A[Explorar bioma semiabierto] --> B[Encontrar criatura salvaje]
  B --> C[Combate en tiempo real]
  C --> D[Llenar Aturdimiento con combos + reacciones ambientales]
  D --> E[Noquear -> fin del combate]
  E --> F[Ventana de captura -> Sludge -> Totem]
  F --> G[Vincular / decidir equipo de 3]
  G --> H[Mini-misiones -> evolucion progresiva]
  H --> A
```

- **Micro (segundo a segundo):** moverse, apuntar (lock-on), encadenar ataque básico + 3 habilidades + salto + bloqueo, gestionar energía elemental, explotar el ambiente.
- **Meso (sesión):** explorar el bioma, encontrar y capturar criaturas, avanzar mini-misiones de evolución, superar la mazmorra y su jefe.
- **Macro (campaña):** cambiar la percepción de la aldea, defenderse de la aldea rival, formar el vínculo con tu equipo de 3.

---

## 6. Sistema de combate (núcleo del USP)

### 6.1 Control y cámara
- **1 criatura activa a la vez.** La controlas por completo (tercera persona).
- **Swap rápido** entre las del equipo, con **cooldown**.
- **Lock-on** al objetivo (cámara estilo action/soulslike).
- El **Tamer NO es objetivo** mientras tenga un tatemis en combate: narrativamente es "intocable" (esquiva cualquier intento). Solo explora; los monstruos luchan.

### 6.2 Energía elemental (recurso)
- Modelo tipo **tinta de Splatoon**: las 3 habilidades consumen **energía elemental**, que se **regenera lentamente por segundo**. El **ataque básico no gasta energía**.
- El **bloqueo** acelera la regeneración.

### 6.3 Kit de habilidades (plantilla por criatura)
Cada criatura tiene **3 habilidades** (cuestan energía), con estos roles de plantilla:
1. **Movilidad**: desplazamiento o sigilo; hace las veces de "dodge".
2. **Flex**: ataque de daño directo **o** efecto (CC, buff, AoE, debuff), según la especie.
3. **Característica (signature)**: única de la especie; cualquier categoría o algo completamente nuevo.

Más las **acciones universales** (todas las criaturas):
- **Ataque básico:** cadena sin coste de energía; columna vertebral del combate y generador base de Aturdimiento. **El básico NO lleva elemento** (daño neutro, siempre x1) salvo que una especie diga lo contrario. *Razón: si la acción que más repites llevara elemento, pelear contra quien te supera pasaría de difícil a imposible. El elemento debe ser una **palanca** que manejas con el kit, no un impuesto sobre cada golpe.* La excepción es el **último eslabón de la cadena**, que puede **estallar en el elemento de la especie**: la recompensa por sostener la cadena entera. *(Ottorock: 4 garras neutras + remate que estalla en Mineral.)*
- **Salto.**
- **Noqueo:** sin botón dedicado y **sin prompt**. Al llenarse el Aturdimiento (o con vida muy baja) el enemigo queda **debilitado** y rematarlo es **seguir atacándolo**. La única ruta al noqueo es jugar bien (combos + reacciones), no spamear un botón (§8.2).
- **Contextual:** solo para **interactuar con el mundo** (aplastar un hongo, agarrar y lanzar un objeto, activar algo del ambiente).
- **Bloqueo / Recarga elemental:** la criatura **adopta su forma elemental pura**:
  - **Defiende** y **recarga energía elemental más rápido**.
  - **No puede atacar mientras bloquea** (stance comprometido: la decisión pesa).
  - **Contra:** en forma pura la debilidad elemental se **amplifica de 1.5x a 2x** (§7.1). Bloquear contra tu counter es un riesgo real.
  - Los detalles (duración, mitigación) varían ligeramente por criatura.
- **Apuntado (mantener):** tercer estado de cámara. La cámara pasa **al hombro** con retícula y el monstruo encara hacia ella; puedes usar básico y habilidades apuntando, **a cambio de movilidad reducida**. Es lo que permite apuntar a un **punto del mundo** (no solo a un enemigo) y lanzar la Movilidad en **cualquier dirección, incluida la vertical**.

**Los tres estados de cámara, todos elegidos por el jugador**, y una sola regla: *el ataque va siempre hacia donde encara el cuerpo*; lo único que cambia es qué lo decide: **libre** → tu movimiento · **lock** (toggle) → el enemigo · **apuntado** (mantener) → la cámara.

**Mapeo de referencia (mando):** LS/RS/R3 mover · cámara · lock-on · A salto · X ataque básico · B Movilidad · Y contextual (interactuar con el mundo) · RB Flex · RT Signature · **LT apuntado (mantener)** · **LB bloqueo (mantener)**. En **ratón+teclado**: apuntar = clic derecho, lock = Q, bloqueo = Shift.

### 6.4 Stats y arquetipos
Híbrido **stats + habilidad/acción**. Los stats principales (**Daño, Vida, Velocidad**) definen el **arquetipo** de la criatura, modulado por su **naturaleza**:

| Arquetipo | Rol en combate de acción |
|---|---|
| **Coloso** | Aguanta todos los golpes que hagan falta para **efectuar lo suyo**: su vida es el coste de su efecto. No es un protector |
| **Luchador** | Dueño del **1v1 limpio**: siguiendo su juego, gana siempre. DPS sostenido de contacto |
| **Ejecutor** | **Entra, revienta, sale**: burst + movilidad, alto riesgo (glass cannon) |
| **Zonificador** | La guerra de la **distancia**: castigo sostenido a rango; sus zonas le compran la distancia o la huida |
| **Controlador** | **Desestabiliza** (mover, inmovilizar, suprimir) con **fenómenos de su elemento convertidos en kit** (marcas de especie con lógica propia, como los tornados de Mantagale) y castiga él mismo. No con reacciones: **un elemento puro solo hace daño** (§8.1). El **verbo** gobierna las reacciones; el **arquetipo** gobierna el kit ([ARCHETYPES](ARCHETYPES.md)) |

> Descripciones operativas (cómo gana · cómo se le gana · pregunta generadora) + pentágono de counters → [ARCHETYPES.md](ARCHETYPES.md).

---

## 7. Sistema elemental

### 7.1 Los 6 elementos y las dos capas
Seis elementos, repartidos en **dos triángulos de piedra-papel-tijera**. La flecha significa **"supera a"**: su golpe hace **1.5x** de daño, que sube a **2x si el defensor está en su forma elemental pura** (§6.3), y el que recibe **resiste 0.5x** al que supera.

> **Quién lleva elemento y quién no.** El multiplicador **solo se aplica a lo que imbuye elemento**: las **3 habilidades del kit** y los **estallidos elementales**. Los **ataques básicos van neutros** (§6.3), así que siempre puedes hacer daño aunque el rival te supere: el elemento decide *cuánto premia jugar bien*, no *si puedes jugar*. Corolario para el terreno: un básico neutro **no dispara reacciones**; quien irradia elemento al suelo es la habilidad o el estallido.

```mermaid
graph LR
  subgraph Triangulo_Inicial_Starters
    N[Naturaleza] -->|1.5x| A[Agua]
    A -->|1.5x| F[Fuego]
    F -->|1.5x| N
  end
  subgraph Segundo_Triangulo
    M[Mineral] -->|1.5x| R[Rayo]
    R -->|1.5x| Ai[Aire]
    Ai -->|1.5x| M
  end
```

- **Dentro de un triángulo:** relación piedra-papel-tijera (**1.5x / 0.5x**; la debilidad sube a **2x** contra forma elemental pura). **Una sola debilidad por elemento** → legibilidad total.
- **Entre triángulos:** neutral (1x).
- **Starters:** uno de cada elemento del Triángulo Inicial (**Naturaleza / Agua / Fuego**), icónico y "vendible".

**Lógica de los matchups (para que se lean "solos"):**

| Triángulo Inicial | | Segundo Triángulo | |
|---|---|---|---|
| Naturaleza › Agua | las raíces beben el agua | Mineral › Rayo | la roca hace tierra y dispersa la descarga (*grounding*) |
| Agua › Fuego | lo apaga | Rayo › Aire | la tormenta domina los cielos; el rayo rompe el aire |
| Fuego › Naturaleza | la quema | Aire › Mineral | el viento erosiona la piedra |

> **El elemento es dos cosas a la vez:** un **multiplicador** (esta sección) y un **verbo** que solo aparece **al mezclarse con el terreno** (§8.1). Suelto, **un elemento solo hace daño**, y esa es una opción perfectamente válida: es la segura e inmediata, y el ×1.5 contra quien superas ya es su recompensa. Los seis verbos, **Fuego propaga · Agua arrastra · Naturaleza entrelaza · Mineral pesa · Rayo paraliza · Aire nubeliza**, ocupan **ejes distintos y no se solapan**: son la identidad de cada elemento sobre el tablero.

> **Identidad de "Mineral" = roca / piedra / cristal** (NO metal refinado). Esto resuelve la contradicción eléctrica: la roca es aislante y por eso hace tierra al rayo; un metal conductor sería lo contrario.

### 7.2 Mapeo con el lore (Tomos)
Naturaleza → Tomo de Naturaleza · Agua → Tomo de Agua · Fuego → Tomo de Fuego · Mineral → Tomo de Tierra (roca/cristal) · Rayo → Tomo de Electricidad · Aire → Tomo de Viento.

### 7.3 Elemento secundario (opcional)
La mayoría de criaturas son **mono-elemento**. Algunas, en su **forma evolutiva final**, adquieren un **elemento secundario** que **NO altera debilidades/resistencias**: solo les permite usar ese elemento en alguna habilidad (utilidad/sabor). **No es obligatorio:** muchas criaturas **nunca lo desbloquean** y se quedan con **un solo elemento**.

---

## 8. Ambiente, reacciones y captura (USP)

### 8.1 Dos sistemas ambientales complementarios
1. **Reacciones elementales (el "círculo continuo").** El terreno tiene **su elemento** (roca volcánica → Fuego · río → Agua · **pasto / hierba alta** → Naturaleza · **zona de menas** → Mineral · piedra electrificada → Rayo · corrientes de aire → Aire): es el tablero, y su papel es ser **un tercer jugador con historia**. **Los 6 elementos tienen terreno propio a propósito:** sin terreno de Aire, la pareja Rayo › Aire sería inalcanzable. Las habilidades **irradian su elemento al impactar**, y de ahí salen **cinco salidas**:
   - **Suelo neutro** → nada: solo el golpe.
   - **Tu mismo elemento** → **amplificación**: tu explosión **se repite** tras un instante.
   - **Un elemento al que superas** → **reacción tuya**: estalla con daño de tu elemento y **a ti no te alcanza**.
   - **Un elemento que te supera** → **reacción del terreno**: la misma explosión, pero es **suya y sin bando**; también te alcanza a ti.
   - **Cruzando triángulos** → **combinación**: no manda nadie, así que el daño es **neutro**, tampoco tiene bando, y en vez de estallar **transforma la zona** un rato.

   **Un elemento puro solo hace daño.** El fuego suelto no incendia y el rayo suelto no paraliza: **los efectos ambientales nacen únicamente de la mezcla**, como en el mundo real. Lo que cada elemento aporta a esa mezcla es **un verbo** (**Fuego propaga · Agua arrastra · Naturaleza entrelaza · Mineral pesa · Rayo paraliza · Aire nubeliza**) y **manda el verbo del que domina**; el otro solo pone de qué está hecha la mezcla.

   **El terreno se CARGA antes de reaccionar, y cambia al hacerlo** ([ELEMENTAL-REACTIONS §0.10](ELEMENTAL-REACTIONS.md)). Un impacto no dispara nada: **deposita saturación** en la mancha que pisa. Cuesta llenarla (**1 unidad** amplificar tu propio elemento, **2** convertir suelo neutro, **3** una combinación, **5** una reacción) y al llenarse la mancha queda **cebada**: **tú eliges cuándo la revientas**. Entonces estalla con una **onda que la recorre desde donde golpeaste**, y deja **terreno de otro elemento** (el vapor deja Aire, el incendio deja ceniza/Mineral…) que **vuelve solo** al cabo de un rato, salvo que se siga peleando encima. Así hay **menos acontecimientos y mucho más grandes** (≈ uno cada 15 s, y cada uno pega como 4-5 habilidades), **el mundo cambia con lo que hacen los monstruos**, y apuntar al suelo pasa de reflejo a **inversión**. El efecto queda **pegado a quien alcanza**: dentro de la zona dura, fuera decrece.

   **El aviso es el propio tablero.** La mancha **se ve llenarse** y late cuando está lista, así que la explosión puede permitirse ser enorme sin ser injusta: te da tiempo a salirte, y la onda tarda en llegar porque **viaja**.

   **Cuánto debe pesar:** el reparto del daño de un duelo completo es **30% básicos / 40% habilidades / 30% terreno**. El ambiente no es decoración ni un extra: pesa lo mismo que tus básicos, y *un combo bueno es encadenar apuntando a zonas elementales*.

   *Ejemplos:* charco de Agua + Rayo → **corriente**, una cinta que arrastra a lo que pise *(combinación)* · roca volcánica/Fuego + Agua → **géiser** que **eleva** y abre el juego aéreo *(manda el Agua)* · veta de Mineral + Fuego → **colada**, la única zona que **avanza** *(combinación)*.

   **Las 15 parejas (6 reacciones + 9 combinaciones), reglas y excepciones por especie → [ELEMENTAL-REACTIONS.md](ELEMENTAL-REACTIONS.md).**
2. **Interacciones físicas / climáticas.** *Independientes de la tabla de tipos.* Golpear el suelo → nube de polvo; nieve → esquirlas de hielo; vientos fuertes → mini-tornado; corrientes de agua, etc. **Puede nevar o haber viento sin que exista un "tipo Hielo/Viento"**: el clima es física que las habilidades explotan. Esto mantiene la tabla simple **y** el ambiente riquísimo.

### 8.2 Captura (armonía como elección)
El protagonista busca **armonía**: el flujo central **subyuga, no mata**.

- El enemigo tiene **Vida** + una **barra de Aturdimiento visible**, y el tamaño de esa barra es **proporcional a su vida máxima** (los tanques aguantan más aturdimiento).
- **El aturdimiento que suma cada golpe es proporcional a su daño**, multiplicado por el **combo**: la llena pegar fuerte y seguido. Por eso las reacciones ambientales pesan tanto en la captura: hacen **daño de verdad** (§8.1, el 30% del total), no un bonus aparte.
- **Decae si dejas de golpear**, y ahí entra el control: **el CC no llena la barra, la MANTIENE**. Mientras el enemigo está controlado **no puede reducir lo que ya lleva acumulado**. Atrapar, arrastrar o amarrar es lo que impide que se vacíe entre golpe y golpe. Y al revés: **el CC que te comes tú te quita la captura más que la vida**, porque te rompe la cadena. → La captura **recompensa el USP** (habilidad + ambiente), no el spam.
- Puedes intentar capturar en cualquier momento, pero las probabilidades altas están al **noquear**.
- Al llenar el Aturdimiento (o con vida muy baja) el enemigo queda **debilitado**, **sin prompt ni botón** (§6.3): hay un **margen invulnerable** para decidir, y pasado ese margen seguir atacándolo es rematarlo.
- Se abre una **breve ventana de captura**: lanzas el **Sludge**; hay una baja probabilidad de que la criatura **se resista**. Si se resiste o la ventana termina, **huye y desaparece**.

### 8.3 Método de captura
**Sludge**: es el más característico e identitario.

### 8.4 Tótems (objeto físico)
El Tótem es un **objeto físico** que el jugador **lleva puesto** como accesorio (brazaletes, collares, aretes, al estilo de las Mega Piedras de Pokémon), con la **forma simplificada** de la criatura que contiene.

---

## 9. Criaturas y progresión

### 9.0 El trío
Tres especies diseñadas a fondo, **deliberadamente complejas** para descubrir los límites del sistema de habilidades antes de estandarizarlo. Cada una usa el mismo tablero elemental de una forma **opuesta**:

| Especie | Animal | Elemento · Arquetipo | Su verbo | Cómo usa el tablero |
|---|---|---|---|---|
| **[Pyrecko](CREATURE-PYRECKO.md)** | gecko | Fuego · Zonificador | **acumular y detonar** | siembra combustible invisible y lo **prende** |
| **[Ottorock](CREATURE-OTTOROCK.md)** | nutria | Mineral · Luchador | **comprometerse y repetir** | **lleva el terreno al enemigo** (roca sellada portátil) |
| **[Mantagale](CREATURE-MANTAGALE.md)** | mantarraya aérea | Aire · Controlador | **improvisar** | **lleva al enemigo al terreno** (lo arrastra por el mapa) |

- **Triángulo estratégico** (nace de los kits, no de la tabla elemental): Pyrecko › Mantagale › Ottorock › Pyrecko.
- Cubren 3 de los 5 arquetipos; **Coloso y Ejecutor** quedan para las especies siguientes.

### 9.1 Roster
> Fichas de diseño por especie → [CREATURES.md](CREATURES.md).

- **Juego final:** la **matriz tipo × arquetipo completa = 6×5 = 30 especies** (una por celda; mapa e ideas en [CREATURE-IDEAS.md](CREATURE-IDEAS.md)).
- Cada especie: **2–4 formas** (contando la forma base "bebé").
- **Rareza sutil:** las criaturas con stats finales más altos tienen una **fase inicial más difícil** y **más formas evolutivas** (estilo pseudolegendario).

### 9.2 Evolución progresiva (= sus "niveles")
- Cada especie tiene **mini-misiones mid-gameplay** para evolucionar.
- La evolución es **progresiva**: pequeños cambios visuales acumulativos hasta completar la forma. Estos cambios **son sus "niveles"**: suben stats de forma regular.
- Entre forma y forma hay **2–3 niveles**.
- Al evolucionar, algunas habilidades base **mejoran** o son **reemplazadas** por otras.

---

## 10. El Tamer

- **Rol:** explora el mundo, dirige y potencia a las criaturas. **No combate directamente** y es **intocable** en los combates entre monstruos.
- **Progresión:** subir la **capacidad de vínculo** eleva el **límite de equipo activo** (1 → 2 → 3). El jugador empieza solo con el inicial (de regalo) y debe **ver otras criaturas antes de poder sumar la segunda** → onboarding natural. También mejora el **aprovechamiento de las habilidades en el entorno** (estilo Leyendas Arceus).

### 10.1 Filosofía de equipo: "conectar, no coleccionar"
- **Equipo máximo: 3.**
- Para reclutar una nueva criatura hay que **liberar** una del equipo. Existe una forma de **rebuscar a las últimas 3 liberadas**.
- **Justificación narrativa:** las criaturas potencian sus habilidades gracias al **vínculo con el Tamer**; tener demasiadas **diluiría** ese vínculo.

---

## 11. Mundo y estructura

- **Estructura:** **semiabierto de tamaño mediano-pequeño**, por **biomas** ligados a elementos (cada bioma con su elemento base y su clima). La **arena de combate y su rejilla elemental** (overlay hexagonal leve + zona enmarcada) → [ARENA.md](ARENA.md).
- **Composición de celdas: 5 elementales / 5 neutras**, sesgada por el bioma. Lo ambiental no es una clase de celda sino un **modificador** encima de cualquiera, y las neutras **se cargan** hasta volverse elementales. Es lo que **recompensa la versatilidad del equipo** (especializarse en un grupo de elementos, cubrir un triángulo entero o variar) y convierte la mezcla de la arena en una **palanca de balance de primer orden**, no en decoración.
- **Ancient Spots:** mazmorras / puntos de interés con **jefes de contexto narrativo**.
- **Economía:** el **Polvo (Pulvis)** + agua = **Sludge** (recurso de captura).

---

## 13. Dirección de arte

- **Estilo:** estilizado / **cell-shaded**, tono cálido HTTYD/Zelda.

---

## 17. Glosario

- **Pulvis / Polvo:** materia elemental pura de los Tomos destruidos; recurso de captura.
- **Sludge:** mezcla de Polvo + agua; se lanza para capturar.
- **Tótem:** cristalización del sludge con forma de la criatura; su vasija; se lleva como accesorio.
- **Tamer / Scavenger:** el jugador; recolecta Polvo y se vincula con criaturas.
- **Ancient Spots:** ruinas de la civilización antigua; mazmorras.
- **Aturdimiento:** barra que, al llenarse, permite noquear y capturar.
- **Tomos:** los Ocho Grandes libros elementales originales (lore oculto).
