# Tatemis3D — Sistema de reacciones elementales

> **TL;DR:** El **terreno tiene su elemento y no cambia nunca** — es el tablero, y su papel es ser **un tercer jugador** que convierte zonas en ventaja tuya o en problema del rival. Un **golpe elemental puro solo hace daño**; los *efectos* ambientales nacen únicamente al mezclarse con el terreno, en **cinco salidas** (§0.2). Toda reacción es un **efecto temporal** que se dispersa, y **el terreno vuelve a ser lo que era**. Modelo ✅ **ratificado por Arzpal (2026-07-18 · rev. 2026-07-26 · reformulación de base 2026-07-29)**; complementa [GDD §8](GDD.md) y [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md).
> **Mapa:** **§0 la capa base (verbos, salidas, bandos, persistencia) ← empezar aquí** · §1 las tres capas · §2 *(pendiente de reescritura contra §0)* · §3 reglas de resolución (R1–R6) · **§4 las 15: 6 reacciones + 9 combinaciones ✅ cerradas** · §5 excepciones por especie · §6 legibilidad · §7 implementación *(⚠️ el código es anterior a §0)* · §8 alcance del prototipo

---

## 0. La capa base ✅ *(dictada por Arzpal el 2026-07-29 — de aquí cuelga todo lo demás)*

> ⚠️ **§2 y §4 son anteriores a esta sección y hay que reescribirlos contra ella.** Las 15 parejas de §4 se están reformulando una a una partiendo del verbo de cada elemento; hasta que eso se cierre, **manda esta sección**.

### 0.1 · Un verbo por elemento

Cada elemento tiene **una personalidad insignia**: no una lista de efectos, sino **un verbo** del que se deriva todo lo que provoca. Los seis ocupan **ejes distintos** y ninguno se solapa con otro.

| Elemento | Verbo | Qué hace | De dónde sale |
|---|---|---|---|
| **Fuego** | **Propagar** | Arde de forma continua, **se extiende poco a poco** y termina apagándose solo | el fuego quema *y se propaga*; tiene ciclo de vida propio |
| **Agua** | **Corriente** | **Arrastra** mientras estés dentro; al salir, la inercia te sigue llevando | el agua es una fuerza difícil de detener, no un empujón |
| **Naturaleza** | **Entrelazar** | **Bloquea el movimiento pero deja atacar y bloquear** | raíces que crecen enredadas |
| **Mineral** | **Peso** | **Ralentiza** (horizontal) y **ancla al suelo** (vertical: corta el juggle) | roca y cristal son lo denso |
| **Rayo** | **Paralizar** | **Ticks por reloj** que interrumpen: no impiden moverse, impiden **encadenar** | electrocución intermitente |
| **Aire** | **Nubelizar** | Convierte un elemento del suelo en **nube**: cubre más área y **tapa la visión** | el aire lleva y suspende lo que toca |

> 🔑 **El dominante pone el VERBO, el dominado pone la MATERIA** ✅ *(2026-07-29)*. Una reacción lleva
> el verbo **del elemento que manda**, y del perdedor solo hereda **de qué está hecha**. Por eso la
> tormenta de arena (Aire › Mineral) tapa la visión —verbo de Aire— y araña porque es roca
> pulverizada —materia de Mineral—, pero **no ralentiza**: ralentizar es el verbo de Mineral, y ahí
> Mineral no manda. Esta regla es la que impide que los seis verbos se contaminen entre sí.

**El espejo Naturaleza ↔ Rayo** es deliberado: uno te quita moverte y te deja pegar; el otro te quita encadenar y te deja moverte. Están en triángulos distintos, así que **nunca se dominan** — solo se combinan. **El espejo Mineral ↔ Aire** también: uno ancla y pesa, el otro suspende y dispersa; y esos dos **sí** son pareja de dominancia (Aire › Mineral).

> 🔑 **El verbo de Rayo es el más sistémico:** el bucle del juego es *combo → daño continuo → aturdimiento → captura*, y el aturdimiento decae si dejas de golpear ([GDD §8.2](GDD.md)). Un efecto cuyo trabajo es **impedir que encadenes** no te quita vida: te quita la captura.
>
> ⚠️ **Los ticks van por reloj, no por azar.** La referencia es la parálisis de Pokémon, pero en tiempo real una interrupción aleatoria no se lee como castigo sino como *input perdido*. Con ticks cronometrados el jugador **aprende el ritmo y cuela el combo entre ellos**: misma sensación, y premia habilidad. Es la misma regla de oro del muestreo de [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md).

### 0.2 · Un elemento puro solo hace daño 🔑

**Los verbos NO son propiedades del elemento suelto.** Fuego solo no propaga; Rayo solo no paraliza. **Los efectos ambientales nacen exclusivamente de la mezcla con el terreno** — como en el mundo real. Un golpe elemental que no toca terreno elemental es *daño de su tipo y nada más*, y eso es una opción válida y deliberada: es **segura e inmediata**, y en un duelo de dominancia el ×1.5 contra el rival ya es su recompensa.

De ahí salen **las cinco salidas**, todas distintas y todas explicables en una frase:

| Dónde cae el golpe | Qué pasa | Daño | Efecto |
|---|---|---|---|
| **Suelo neutro** | nada, solo el golpe | de tu tipo | ninguno |
| **Tu mismo elemento** → **amplificación** | tu explosión **se repite** tras un delay corto (19 de fuego → otro 19 de fuego) | de tu tipo, **dos veces** | ninguno *(no hay mezcla que lo produzca)* |
| **Un elemento al que superas** → **reacción** | estalla la reacción, y es **tuya** (R6: no te toca) | **de tu elemento** | **tu verbo** |
| **Un elemento que te supera** → **reacción del terreno** | estalla la misma reacción, pero es **suya** y **sin bando** | **del elemento del terreno** | **su verbo** |
| **Cruzando triángulos** → **combinación** | se transforma la zona | **neutro** *(de ningún elemento)* | **los dos verbos mezclados** |

> **Reacción = explosión · combinación = transformación.** Las dos —y también la amplificación— **tardan un poco en aparecer**: son explosiones y transformaciones, necesitan su tiempo. Ese delay es lo que hace que **elegir sea real**: el golpe directo es seguro e inmediato; apuntar al suelo pega más y añade efecto, pero el enemigo tiene ese instante para salirse. Y funciona en los dos sentidos — si provocas una reacción del terreno bajo tus pies, ese mismo instante es tu ventana para apartarte.
>
> ⚠️ *Dos verbos crean zona por su propia naturaleza —**propagar** (Fuego) y **nubelizar** (Aire)—, así que sus reacciones duran en vez de estallar y punto. Es deliberado: **manda el verbo**, no la simetría.*

### 0.2b · Los elementos también chocan entre sí ✅ *(2026-07-29)*

Las mezclas **no necesitan terreno**: dos entregas elementales que se encuentran en el aire resuelven
su pareja **en el punto del choque**. Ottorock lanza su roca, Mantagale le responde con un tornado,
las dos colisionan y ahí nace la reacción. Vale para **cualquier entrega elemental**, no solo
proyectiles — un estallido melee contra un proyectil entrante también cuenta; la regla es *dos
elementos que se tocan reaccionan*, sin enumerar casos.

- **Es imparcial**: la mezcla proviene de los dos, así que **afecta a los dos** (mismo principio que
  la reacción del terreno — nadie es dueño de lo que provocaron entre ambos).
- **Mismo elemento → amplificación**: los proyectiles se anulan, pero la explosión suena dos veces.
- **El suelo de debajo no cuenta**: ya resolvió una vez. Las reacciones **nunca se encadenan** (R1).
- Le da **uso defensivo** a los ataques elementales: un proyectil se puede *contestar*, no solo
  esquivar. Es la interacción que faltaba entre criaturas a distancia.
- ⚠️ *Consecuencia para el melee:* pegar de cerca te deja dentro del radio de una mezcla imparcial.
  Lo que te protege es **el delay** (golpeas y sales), y ayuda que los estallidos melee nazcan **por
  delante del cuerpo** (`ElementalBurst.forwardOffset`, que ya se usa así). Comerse más ambiente es
  el peaje justo del cuerpo a cuerpo, y parte de por qué existe el rango.
>
> **Objetivo de peso ✅:** una reacción debe valer **casi lo que una habilidad promedio**. En un duelo completo, el reparto del daño total debe ser **30% básicos / 40% habilidades / 30% reacciones y combinaciones**. Es decir: el terreno **no es un extra**, pesa lo mismo que tus básicos, y *un combo bueno consiste en encadenar básicos y habilidades apuntando a zonas elementales*.

### 0.3 · El efecto se pega ✅ *(2026-07-29)*

Los efectos ocurren en la zona, **pero se quedan pegados a quien alcanzan**. Dentro de la zona la duración es **eterna** (hasta que la zona muera); al salir, el efecto **decrece** hasta apagarse. Una quemadura sigue quemando fuera del incendio, cada vez menos.

**Modelo propuesto — una carga, no seis reglas:** cada efecto lleva una **carga que sube dentro y baja fuera**, y su intensidad *es* esa carga. De ahí salen las tres propiedades con un solo número: dentro se mantiene, fuera decae, y volver a entrar recarga. Y regala una cuarta gratis: **cuanto más tiempo estuvo dentro, más le dura fuera**, así que forzar la permanencia sigue siendo la jugada premium.

Esto es lo que hace que **las zonas no necesiten CC para rentar**: empujar al enemigo un instante ya paga. 🟡 *Excepción a resolver: **Aire**, cuyo verbo modifica la zona y no el cuerpo — no hay nada que llevarse puesto al salir, salvo quedar desorientado un momento.* ⚠️ *A vigilar en playtest: con la mitad del suelo elemental y efectos que se pegan, el estado por defecto puede pasar a ser «siempre bajo algún efecto», y entonces dejan de leerse como acontecimientos.*

### 0.4 · Periodo de gracia ✅ *(2026-07-29 — sustituye al «cancelar al instante» de R3)*

Un efecto elemental vivo **no se puede convertir durante un periodo de gracia**. Eso impide cambiar de efecto al instante y spamear efectos sobre el mismo punto. La gracia es **menor que la duración del efecto**, así que **sí se puede interrumpir con otra habilidad** — solo que hay que esperar. R3 sigue existiendo (atacar una zona *para* limpiarla es counterplay buscado), pero con esa espera de por medio.

> **Repetir la MISMA reacción la refresca, no se bloquea** ✅. La gracia existe para impedir *cambiar* de efecto, no para castigar la insistencia. Sin esta precisión, la mecánica insignia de Ottorock se rompe: su cadena de 3 en Sujeción reproduce la combinación sellada en cada golpe, y los golpes 2 y 3 caerían sobre su propia zona en gracia y no harían nada.

### 0.5 · Composición de la arena ✅ *(2026-07-29)*

Proporción objetivo de celdas: **5 elementales / 2 neutras (nada) / 3 de efecto** *(neutras + algún efecto de environment — el 2º sistema ambiental, aún sin dictar)*. Cada **bioma** sesga esa mezcla hacia ciertos elementos: en una laguna la mayoría de celdas son de Agua. **Eso es lo que recompensa la versatilidad del equipo** — el jugador elige entre especializarse en un grupo de elementos, cubrir un triángulo entero, o variar.

> Y provocar reacciones con un elemento al que eres débil **no siempre es malo**: sigue siendo daño al enemigo. Solo cambia el gameplay — tus ataques pasan a llevar riesgo para ti.

### 0.6 · Anatomía universal del ataque ✅ *(dictada 2026-07-29 — vive aquí de momento, su sitio es [ABILITY-SYSTEM](ABILITY-SYSTEM.md))*

Todo ataque del juego (básicos, habilidades) tendrá **tres tramos**, y de ahí sale qué se puede parar:

1. **Arranque — interrumpible.** Lo cortan los ticks de Rayo, los golpes y cualquier otra cosa. **Este tramo *es* el "te corté a media animación".**
2. **El golpe — comprometido.** Movimiento del golpe + hitbox. A partir de aquí **entra sí o sí**.
3. **Recuperación — cancelable.** El jugador puede **cortar la cola con otra habilidad** para encadenar más seguido.

> Tres consecuencias: le da a Rayo un **contrajuego real** (castiga a quien *empieza* ataques, no a quien ya está comprometido → la respuesta es entrar ya atacando y no dejar huecos) · la recuperación cancelable **es un sistema de combos que alimenta la captura**, porque encadenar más seguido sube el multiplicador de combo y con él el aturdimiento · y **el andamiaje existe a medias**: `AbilityPhase` ya tiene `interruptible` y `superArmor`, y la embestida de Ottorock ya tiene esta forma exacta (carga → embestida → deslizamiento). **Lo que falta es cancelar**: `AbilityCaster` no deja cortar una habilidad con otra.


### 0.7 · De quién es cada efecto ✅ *(2026-07-29)*

**Una sola frase genera los cinco casos: el efecto pertenece al elemento que MANDA; si no manda nadie, no es de nadie.**

| Situación | ¿Quién manda? | ¿Te afecta a ti? |
|---|---|---|
| Reacción sobre un elemento que superas | **tú** | **No** — R6 te respeta |
| Amplificación (tu propio elemento) | **tú** | **No** — es tu explosión otra vez |
| Reacción del terreno (te supera) | el terreno | **Sí**, sin bando |
| **Combinación** (cruzando triángulos) | **nadie** | **Sí** |
| Choque de elementos (§0.2b) | **nadie** | **Sí** |

**La diferencia entre las dos reacciones no es *dónde* pasan —las dos son sobre el terreno— sino *quién gana*, y eso lo decide el triángulo.** Ottorock (Mineral) golpea un parche de Rayo: Mineral › Rayo, manda lo suyo, la reacción es de Mineral y le respeta. El mismo Ottorock golpea un parche de Aire: Aire › Mineral, manda el suelo, sale la reacción de Aire y no le respeta. **Si tu elemento manda, lo controlas; si manda el del suelo, lo has despertado.**

> 🔑 **R6 protege del ESTALLIDO, no de la ZONA.** Tu reacción no te golpea al nacer, pero si deja una zona que dura (incendio, nube de arena, gravedad), **esa zona no distingue a nadie** y te afecta si entras. Por eso las reacciones con zona son más peligrosas de usar que las instantáneas, aunque sean tuyas.
>
> **Por qué las combinaciones no tienen bando:** si te respetaran, ponerlas sería *siempre* bueno y no habría decisión — las escupirías sin pensar. Es el riesgo propio lo que convierte **dónde** la pones en una jugada, y el delay de §0.2 es lo que te da la ventana para colocarte. Sale además del principio que ya está dictado: el terreno es **un tercer jugador**, y un tercer jugador no toma partido. Regalo: puedes **montarte en tu propia corriente** o pelear dentro de tus propias mini explosiones.

### 0.8 · Las tres fuentes de efecto elemental ✅

Fuera de estas tres **no hay efectos**, solo daño de tipo:
1. **El terreno** — 5 de cada 10 celdas (§0.5).
2. **El choque entre elementos** (§0.2b) — en el aire, sin suelo de por medio.
3. **Lo que una criatura lleva encima** — hoy solo la **roca sellada de Ottorock**, única excepción a R5.

> Consecuencia de balance: sobre suelo neutro el juego se queda en daño + kit, **sin verbos**. Eso convierte la **composición de la arena en una palanca de balance de primer orden**, no en decoración — y hace que el nodo de cristal y la roca sellada valgan mucho más de lo que parecen, porque son las dos formas de tener efectos donde el suelo no los da.

### 0.9 · Techo de efectos y esquivabilidad ✅ *(2026-07-29)*

**Máximo 2 efectos pegados a la vez**, y el techo **no es el objetivo**: el juego apunta a **uno o ninguno** — si el jugador se posiciona bien, sale limpio. De ahí se sigue lo importante: **todo tiene que ser esquivable**, y el 30% del daño que aporta el terreno sale de **acertar** reacciones sobre el rival, no de desgaste ambiental inevitable. Si estar bajo un efecto es el estado normal, los efectos dejan de leerse como acontecimientos.

> 📐 **Regla de documento:** *si un efecto no se ve en el cuerpo del afectado, no entra en §4.* En caja gris eso es tinte y escala de cápsula — barato y suficiente. Es lo que separa "el barro me está castigando" de "la esquiva de este juego a veces falla".


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

> ✅ **El modelo de rejilla se dictó el 2026-07-26 → [ARENA.md](ARENA.md):** mundo continuo (la rejilla es **overlay**, no geometría), hexágonos **leves** marcando el elemento predominante de cada sección, **zona de combate enmarcada** alrededor del límite del terreno, celda ≈ 2× un monstruo grande. Quedan allí las abiertas de **manadas** y **encierro/huida del Tamer**.
> *Aparte y sin relación con la tabla de tipos:* los **fenómenos físicos y climáticos** (polvo al golpear el suelo, nieve, lluvia, viento) son el **segundo sistema ambiental** del [GDD §8.1](GDD.md) — pura física que las habilidades explotan, sin pasar por elementos. Puede nevar sin que exista un tipo Hielo. Las dos capas conviven en la misma arena sin tocarse.

**② Reacciones — temporales, encima del terreno.**
Lo que produce un impacto elemental sobre terreno de otro elemento. **Duran poco y se van.**

**③ Marcas de especie — independientes de todo lo anterior.**
Cosas que siembra una especie concreta y que **no son terreno ni reacción**: primer caso, el **«combustible»** de Pyrecko. Reglas propias en §5.

## 2. Las salidas → **han pasado de tres a CINCO: viven en [§0.2](#02--un-elemento-puro-solo-hace-daño-)**

> ✏️ *Sección derogada el 2026-07-29 y reemplazada por §0.2.* La versión vieja tenía **tres** salidas
> (dominancia / combinación / respuesta del terreno) y decía que sobre **tu mismo elemento no pasaba
> nada**. El modelo vigente tiene **cinco** —se añaden el **suelo neutro** y la **amplificación**, que
> es lo que ahora pasa sobre tu propio elemento— y la diferencia entre ellas ya no es solo *cómo se
> siente* sino **de qué tipo es el daño**: la reacción pega con el elemento que manda, la combinación
> pega **neutro**. Ver §0.2 y §0.7.

> ✅ **«Toda reacción levanta corrientes de viento»** *(dictado 2026-07-18)* — **DEROGADA el 2026-07-30**.
> Con **Aire = nubelizar**, las corrientes son lo que hace el propio Aire, no un subproducto de toda
> reacción; y ningún verbo de §0.1 las produce. Lo único que dependía de ella era el **mecanismo de
> altura de Mantagale**, que pasa a los **peces espíritu** (§5, marca de especie). **La capa base no
> genera corrientes por ninguna vía, y ya no necesita hacerlo.**

## 3. Reglas de resolución ✅ ratificadas

**R1 · Siempre contra el terreno.** Un impacto resuelve contra el **elemento del terreno**, nunca contra un efecto que siga activo encima. *(Ottorock convierte un trozo de río en **arenas movedizas**; dos segundos después Pyrecko dispara ahí: el fuego se encuentra el **río**, no las arenas → el **géiser** del terreno, que le eleva a él también.)* Esto es lo que impide la explosión combinatoria: la matriz es 6×6 y punto. **También vale para los choques en el aire** (§0.2b): resuelven una vez y el suelo de debajo no vuelve a contar.

**R2 · La duración es proporcional al impacto.** Cuanto más fuerte el golpe que irradia el elemento, más dura la reacción. Premia meter la habilidad pesada en el sitio correcto en vez de escupir habilidades baratas.

**R3 · Un impacto nuevo convierte el efecto activo — pasado su periodo de gracia** *(✏️ rev. 2026-07-29, ver [§0.4](#04--periodo-de-gracia--2026-07-29--sustituye-al-cancelar-al-instante-de-r3))*. Al impactar **dentro** de una reacción viva —dentro, no rozándola: el propósito es puntería, no proximidad— esta se cancela y se resuelve de cero contra el terreno. → **Counterplay explícito:** atacar una zona activa sirve para **limpiarla** si estorba. **Tres precisiones que trajo el rediseño:** durante la **gracia** el efecto aguanta y el impacto nuevo no cuaja · repetir **la misma** reacción la **refresca** en vez de bloquearse · y dos efectos de **la misma habilidad** no se pisan entre sí (el daño crea y el estallido borraría).

**R4 · Regla direccional (rev. ✅ 2026-07-26).** La misma pareja resuelve distinto según quién entra. Entra el **predador** → **dominancia** estándar (R6 protege a su bando). Entra la **presa** → **respuesta del terreno**: se dispara la **misma reacción** de la fila del predador, plena pero **sin bando** (excepción de R6). No puedes quemar lo mojado — **lo mojado te vaporiza**; apagar lo ardiente sigue siendo la dominancia normal del Agua. *(Consecuencia buscada: en espejos del mismo elemento, el terreno se vuelve el tercer jugador — ambos pueden provocarlo y ambos deben respetarlo.)*

**R5 · Superficie neutra → nada.** Sin elemento de terreno no hay reacción. *(Consecuencia buscada: la arena del gate es gris y sin reacciones — ver §8.)* **Única excepción: la roca sellada de Ottorock** (§5), que lleva su reacción ya resuelta y por eso funciona también sobre neutro.

**R6 · El estallido no golpea a quien lo provoca** (ni a su bando). El **efecto de zona** resultante sí es neutral: afecta a quien entre, de cualquier bando. *(Sin esto, Ottorock se electrocuta a sus propios pies al agarrar sobre Rayo, y un Ottorock salvaje se regalaría el noqueo solo.)* **Dos casos SIN bando ni en el estallido:** la **respuesta del terreno** *(✅ 2026-07-26)* y **todas las combinaciones y choques** *(✅ 2026-07-29)* — el terreno no es aliado de nadie, y nadie es dueño de una mezcla en la que ningún elemento se impone. **El cuadro completo está en [§0.7](#07--de-quién-es-cada-efecto--2026-07-29)**, que es la versión vigente de esta regla.

## 4. Las 15 parejas ✅ **CERRADAS (2026-07-29)** — 6 reacciones + 9 combinaciones

> ✅ **REFORMULACIÓN COMPLETA (2026-07-29): las 15 están cerradas y derivadas de los verbos de §0.1.** Las 6 reacciones y las 9 combinaciones son las tablas de abajo. Se resolvió el problema de fondo del diseño anterior — **cuatro combinaciones que hacían lo mismo** ("zona que daña a quien está dentro": pequeñas explosiones, vegetación electrificada, roca al rojo, púas cristalinas) — dándole a cada una una función que ninguna otra de las quince tiene.
>
> **Nombres que murieron y por qué:** *barro* → **arenas movedizas** (ralentizar era redundante con el verbo Peso) · *roca al rojo* → **colada** (de clon a la única zona que avanza; el "quema al pisar" sobrevive como su rastro) · *púas cristalinas* → **cristalización** (el cristal se queda, el "daña al pisar" no) · *agua electrificada* → **corriente** (muere "parálisis a quien entre") · *pequeñas explosiones* → **mini explosiones** (ahora amplifican en vez de solo dañar) · *vegetación electrificada* → **amarra** (aquella hacía daño de Rayo, contra la regla de que las combinaciones son neutras) · *avivado* → **mecha** · *siembra* → **velo de esporas** (cubrir más área es el rasgo de familia de Aire, no una función) · *niebla* conserva nombre y gana el romper-lock-on que antes tenía el vapor.


**Los dos triángulos:** ① **Naturaleza › Agua › Fuego › Naturaleza** · ② **Mineral › Rayo › Aire › Mineral**. Dentro de un triángulo siempre hay **reacción** — es *tuya* si entra el predador y *del terreno, sin bando*, si entra la presa (R4). Entre triángulos, siempre **combinación**. Y sobre tu **mismo** elemento, **amplificación** (§0.2).

Cada elemento tiene **2 relaciones de triángulo** (a quién supera, quién lo supera) **+ una firma** que explica su fila entera:

| Elemento | Firma | Elemento | Firma |
|---|---|---|---|
| **Fuego** | quema; el aire lo aviva | **Naturaleza** | absorbe y propaga |
| **Agua** | conduce, apaga, embarra | **Mineral** | endurece, cristaliza |
| **Rayo** | electrifica lo que toca | **Aire** | agita, aviva y erosiona |

### Las 6 reacciones ✅ **CERRADAS (2026-07-29)** — derivadas del verbo del dominante

Cada una hace **daño del elemento que manda** (≈ una habilidad promedio) **+ su verbo**. Se disparan
en los dos sentidos (R4): si entra el predador es *suya* y no le toca (R6); si entra la presa, es
**del terreno** y no tiene bando.

| Pareja | Reacción | Qué hace = el verbo del dominante |
|---|---|---|
| **Naturaleza › Agua** | **Crecimiento explosivo** — las raíces beben el charco | **Atrapa**: quien pilla no se mueve, pero **puede atacar y bloquear**. **Solo suelo** — no alcanza a quien vuela, que es la salida natural. Es la mejor herramienta de **setup** del sistema: lo único que fija a alguien dentro de la zona de otro. |
| **Agua › Fuego** | **Géiser** — el agua apaga el fuego | **Eleva**: agua caliente y humo que te suben. Es la reacción que **abre el juego aéreo**, y lo que permite que un melee de Agua o de Fuego alcance a un volador. Al caer, la inercia de la corriente te sigue arrastrando. |
| **Fuego › Naturaleza** | **Incendio** | **Se propaga**: prende y **se extiende poco a poco, solo por terreno de Naturaleza** — el parche define hasta dónde llega, sin necesidad de un tope inventado — hasta apagarse solo. Quien lo toca se lleva la quemadura puesta. |
| **Mineral › Rayo** | **Energía geo** — la roca hace de toma de tierra | **Pesa**: zona de **más gravedad** que imanta hacia abajo. No impide despegar: hace que despegar **valga menos**, acortando la suspensión. Ralentiza y arranca del aire lo que vuele → es la reacción **anti-aérea** del juego. |
| **Rayo › Aire** | **Descarga de tormenta** | **Corta el combo**: ticks por reloj que solo muerden en el **arranque** de un ataque (§0.6). No te quita vida: te quita la captura, porque sin cadena el aturdimiento decae. |
| **Aire › Mineral** | **Tormenta de arena** — el viento erosiona la roca | **Nubeliza**: levanta la roca en una nube que cubre bastante más área de la que golpeó y **tapa la visión**; araña porque está hecha de roca pulverizada. **No ralentiza** (eso es el verbo de Mineral, y aquí no manda). Es la que **se come Ottorock**: sobre corrientes de aire, sus propios ataques le dejan ciego. |

> *La variante de la tormenta "caen relámpagos uno a uno" queda 🔮: exige scheduler y telegraph por rayo.*

> **Gatillo inverso (rev. ✅ 2026-07-26):** estas 6 filas también se disparan **al revés** — entra la presa sobre el terreno de su predador y el terreno responde con la **misma reacción, sin bando** (R4/R6). *Fuego sobre río → **géiser** con daño de Agua, que eleva también al que disparó; Mineral sobre corrientes de aire → **tormenta de arena**, que ciega también a Ottorock.*

### Las 9 combinaciones ✅ **CERRADAS (2026-07-29)** — derivadas de los DOS verbos

Cross-triángulo: **ninguno domina**, así que el daño es **neutro** y **no tienen bando** (te afectan a
ti también, §0.7). Son **transformaciones**: zonas que duran, no estallidos. Cada una tiene **una
función distinta de las otras ocho y de las 6 reacciones** — que es lo que faltaba en la versión vieja,
donde cuatro eran la misma cosa.

| Pareja | Nombre | Función única | Qué hace |
|---|---|---|---|
| **Mineral + Agua** | **Arenas movedizas** | **Anti-fuga** | Peso sobre corriente = fluido denso que **te traga**. Dentro, el dash y el salto salen **enteros y a distancia normal** —recortar la distancia se leería como un bug, no como barro— pero **al aterrizar te hundes y la recuperación se alarga**. Escapar funciona; escapar te deja vendido. *(Muere «barro = ralentiza»: era redundante con el propio verbo Peso.)* |
| **Mineral + Fuego** | **Colada** | **La única zona que AVANZA** | El fuego con peso encima no salta de mata en mata: **se vierte**. Nace en el impacto y repta despacio en la dirección del golpe, comiéndose el suelo jugable, hasta solidificar y morir. Quien la toca sale con roca fundida que sigue quemando mientras se enfría. **Al llegar a una celda de Agua se apaga** — no es reacción encadenada, es su muerte, y es la respuesta del rival. Es lo único del sistema que **devuelve distancia**. ⚠️ Sin bando + avanza = **te persigue a ti también**: la más exigente de usar. |
| **Mineral + Naturaleza** | **Cristalización** | **Te quita el CANCEL** | Entrelazar deja actuar; Peso dice que lo que arranca no se detiene. Peleas, bloqueas y te mueves **a velocidad normal**, pero **pagas cada acción entera**: no puedes cortar la recuperación para encadenar (§0.6). No te quita daño, **te quita captura** (sin cadena apretada no sube el combo). Espejo exacto de Rayo: *él te corta el golpe, esta no te deja cortarlo a ti*. 🟡 **Aparcada**: el cancel aún no existe en `AbilityCaster`. |
| **Rayo + Agua** ★ | **Corriente** *(agua electrificada)* | **El único arrastre** | Cinta transportadora eléctrica: lámina **direccional** —la dirección la fija el vector de tu golpe aplanado al suelo, así que **tú la apuntas**— que lleva a lo que pise, con inercia decreciente al salir. Encima, el tick de Rayo mordiendo el **arranque de tus ataques**, nunca tu movimiento. Puedes salir siempre, pero gastas la huida en irte en vez de en contraatacar. *(Muere «parálisis a quien entre».)* **Pide dato nuevo: la zona necesita vector de dirección** (lo comparte con la colada, se paga una vez). |
| **Rayo + Fuego** | **Mini explosiones** | **Amplifica** *(la única que sube el daño ajeno)* | Un tick de más (Rayo) + el daño extendido en el tiempo (Fuego): **cada impacto dentro de la zona lo sigue una mini explosión** que repite un porcentaje del golpe original. Junto a la niebla, **una de las dos zonas que QUIERES pisar**. Sin bando, así que quien gane el intercambio gana más. ⚠️ **El eco no dispara reacciones**: ya *es* la combinación, y encadenar rompería R1. |
| **Rayo + Naturaleza** | **Amarra** | **Anti-kiting** | Rayo pone su **metrónomo** y Naturaleza el **anclaje**: cada tantos ticks, un **tirón** te devuelve al punto donde te alcanzó — cada víctima con su propia estaca. Entre tirón y tirón corres normal, y tu ofensiva no se toca; es *luchar por soltarte*. Discreto y no continuo **a propósito**: una fuerza constante contra tu input se lee como mandos rotos, un tirón se lee como acontecimiento. Cuanto más aguantes fuera, más débil el siguiente, hasta que la correa rompe. |
| **Aire + Agua** | **Niebla** | **Desinforma** | Capa baja, ancha y opaca: **a través de ella no se puede FIJAR objetivo**, y esconde igual a quien está dentro que a quien mira desde fuera. **La otra zona que quieres pisar** — hasta aquí el ambiente solo sabía castigar. Los proyectiles ya guiados no se pierden: se corta la **adquisición**, no el guiado. *(Casi gratis: `TargetingSystem` ya distingue fijar de mantener.)* |
| **Aire + Fuego** | **Mecha** | **Temporizador** *(la única que no hace nada mientras dura)* | El fuego nubelizado no arde: **se acumula**. La nube crece y se satura toda su vida sin dañar, y **deflagra de golpe en todo el volumen al final**, aire incluido — por eso es **la única que alcanza a un volador**, pero como evento puntual del que puedes irte. **Detona solo por su reloj**, nunca por golpes, o Pyrecko tendría un detonador gratis sobre su kit de marcas. **Invierte el delay de §0.2**: aquí lo peligroso es el final. |
| **Aire + Naturaleza** | **Velo de esporas** | **Interdicta lo lanzado** | La maraña deja el suelo y queda **colgando**. Se atraviesa andando, saltando o dasheando sin efecto: lo que se come es **el proyectil elemental que la cruza**, que **detona ahí**. No es mecánica nueva — el velo se comporta como un elemento estacionario para la regla de choques (§0.2b). Es el **contrajuego natural contra un Zonificador**. Solo detona **lo elemental**: disparar sin cargar elemento la atraviesa. |

> ⚠️ **El menú de Ottorock.** Las tres de Mineral son las que él sella en Sujeción, así que **el jugador elige entre ellas**. Para que la elección sea real necesitan **radio y duración muy distintos**: cristalización pequeña y larga · colada estrecha y direccional · arenas amplia y corta. Si no, siempre elegirá la misma y su mecánica insignia es decorativa.
>
> **Aturdimiento ≠ stun.** El "aturdimiento" llena la **barra de captura** (GDD §8.2); no es un control que te quite el mando. Los CC reales (atrapar, arrastrar, tirón) están dichos por su nombre.


## 5. Excepciones por especie

**Ottorock — combinación sellada portátil** ([ficha](CREATURE-OTTOROCK.md)). Es su mecánica insignia en Sujeción: al **agarrar** el terreno se resuelve **una vez** `Mineral + elemento del terreno` y el resultado queda **sellado en el trozo**. A partir de ahí **cada impacto reproduce esa misma mezcla**, en cualquier superficie — **incluida la neutra**. Es la **única fuente portátil de efecto** del juego (§0.8). Elige su combinación eligiendo **dónde agarra**: río → **arenas movedizas** · roca volcánica → **colada** · pasto → **cristalización**.
- Terreno neutro al agarrar → **roca pura**: el trozo reacciona como **Mineral** contra el terreno donde impacte.
- **Solo se sella lo que es COMBINACIÓN.** Si el agarre dispara una **reacción** (sobre Rayo, que él domina; sobre Aire, que le domina) o una **amplificación** (sobre sus propias menas), esta se consume ahí y el trozo queda **puro**.
- Al volver a Libre (romper o lanzar la roca) **pierde la combinación**.
- 🟡 **Sin resolver — el bando.** §0.7 dice que las combinaciones **no son de nadie**, y él reproduce la suya **en cada impacto y a bocajarro**: su mecánica insignia le salpica cada vez que la usa. Puede que basten el **delay** y que su estallido nazca **por delante del cuerpo**, o puede que la roca sellada tenga que ser la excepción. **Decide Arzpal.**

**Pyrecko — el «combustible»** ([ficha](CREATURE-PYRECKO.md)). Marca de especie, **no** elemental:
- **Inerte**: solo su Signature la **enciende** — y encenderla **no la detona: la arma**. ✅ *(2026-07-30)* Una marca encendida **estalla en Fuego al más mínimo contacto**: **daño de Fuego** a quien la toca, y **el estallido irradia Fuego contra su celda** → ese fuego resuelve normalmente contra el terreno (sobre terreno de Agua → **respuesta del terreno**: el **géiser**, sin bando y con daño **de Agua**, que le eleva a él si está dentro — es el ejemplo canónico del modelo). El campo encendido es un **campo de minas**: *no pisarlo* es el counterplay.
- ✅ **Su kit no aplica NINGÚN efecto por sí solo** *(2026-07-30)* — se le quitaron la **quemadura** del Disparo ígneo y la **ralentización del 20 %** de la Signature (§0.2 y §0.8; el slow era además el verbo de **Mineral**, §0.1). Todo lo que produce es **daño de Fuego + entrega de Fuego**; los efectos siguen naciendo solo de la mezcla.
- **Solo desaparece de dos formas: al expirar (10 s) o al estallar** — y estalla **una sola vez**: ✅ *(2026-07-30)* encendida es una **mina de un uso, que se consume al contacto**, así que el campo se gasta según lo pisan. *Las reacciones elementales ya **no** la limpian (regla anterior derogada 2026-07-18).*
- **Pertenece al lugar, no al material:** si Ottorock arranca el trozo de terreno donde hay combustible, se lleva **el terreno, no la marca** — el combustible se queda donde estaba.

**Mantagale — tornados que empujan y detonan por colisión** ([ficha](CREATURE-MANTAGALE.md) §C). ✅ *(2026-07-18: sustituye tanto a "mover/dispersar zonas" como a "dispara reacciones mientras recorre" — ambas derogadas.)*
- Sus **tornados empujan durante todo su recorrido** (negación de área), pero son **inertes en lo elemental**: no tocan el terreno ni disparan reacción al viajar — igual que el combustible de Pyrecko, son una marca de especie con su propia lógica.
- La reacción ocurre **al reventar**, y revientan por **colisión de fuerzas opuestas**: el tornado empuja, su Ciclón **atrae**, y el choque los detona. La reacción resuelve contra el terreno **del punto de colisión**, con el enemigo arrastrado allí dentro.
- El Ciclón **arrastra al enemigo por el suelo** hasta el centro (no lo teletransporta): al volver, **atraviesa los tornados sembrados por el camino**.
- Su fuerza **depende del terreno de la arena** — es el que "juega el tablero". Sobre **neutro** no consigue nada. Sobre **Rayo**: sus tornados al reventar provocan la **descarga de tormenta del terreno, sin bando**, que además le corta el **arranque** de sus propios ataques. Es su tablero de **alto riesgo**.
- ✅ **Peces espíritu — su motor de altura** *(dictado 2026-07-30; **deroga** «montar las corrientes que levanta toda reacción», §2)*. Cada **mezcla elemental que provoca** hace nacer un **pez espíritu**, hasta **5**; el pez **nace en la mezcla y viaja hasta él** para unirse al séquito (la altura llega **con un viaje de por medio**, no al instante), y los peces **lo elevan** —más peces, más altura— y **cada uno tiene su duración**: al morir, baja un escalón. Sin peces, planea hasta el suelo. Es **marca de especie** (§1③), como el combustible y la roca sellada, así que **la capa base no necesita ninguna excepción**: ningún verbo genera corrientes, y ya no hace falta que lo haga. ✅ **Los peces cuentan como PROYECTILES** *(2026-07-30)*: un **ataque elemental** los destruye —en tránsito o escoltándola—, los neutros no. Su altura es **un recurso atacable**, y el **velo de esporas** (que detona el proyectil elemental que lo cruza) se los mata, incluido el suyo propio. ✅ **El choque que revienta un pez NO le repone otro** *(confirmado 2026-07-30)*: la mezcla que nace ahí (§0.2b) **no es de nadie** (§0.7), así que no cuenta como provocada por él — si contara, matarle peces se los devolvería y no habría contrajuego. El que dispara paga un fogonazo **imparcial** por cada pez. ✅ **Solo le dan pez las mezclas que provoca ELLA** *(dictado 2026-07-30)* — las de sus compañeros no: **sin sinergia de equipo** (deroga la decisión del 26/07). El principio general vive en [CREATURES §2](CREATURES.md).
- ✅ **Sus ráfagas (básico) son NEUTRAS sin potenciar** *(2026-07-30)* — solo son **entrega de Aire** cuando la Flex las potencia. Si no, el **velo de esporas** que él mismo genera sobre pasto le detonaría sus propios básicos encima (§0.2b).

## 6. Legibilidad
- **Reacción = ruidosa (explosión) · combinación = suave (transformación) · respuesta del terreno = el VFX de su reacción espejo con matiz de "provocada"** (arranca desde el terreno hacia el atacante, no desde el impacto hacia fuera). En caja gris eso se resuelve **tiñendo de rojo** el fogonazo y la etiqueta. Los únicos "no pasa nada" que quedan son el **suelo neutro** y —solo en cuanto a efecto— la **amplificación**, que sí pega.
- **Una reacción tiene que separarse en luminancia del terreno donde nace.** No es acabado, es restricción dura: crítico en las combinaciones, que no tienen fogonazo y cuyo disco es lo único que aparece.
- **Si un efecto no se ve en el cuerpo del afectado, no entra en §4** (§0.9). En caja gris: tinte y escala de cápsula.
- **Toda reacción avisa antes de existir** (el delay de §0.2 se dibuja como un disco que late). Es lo que la hace esquivable, y sin ello §0.9 no se cumple.
- Los efectos **se dispersan solos** (R2) y el terreno vuelve: el mundo nunca queda pintado.

## 7. Implementación (data-driven)
> ⚠️ **El código va por detrás del diseño (2026-07-29).** Están implementados **el mundo, el tablero y el motor**; lo que **NO** está es el **contenido**: la `ReactionTable` sigue autorada con las 15 parejas VIEJAS. Ver el bloque Estado de [TASKS](TASKS.md) para la lista ordenada de lo que falta.
>
> ✅ **Ya en código:** `Environment/ElementalGrid` + `HexCoord` (el tablero: celdas horneadas desde biomas, composición 5/2/3 determinista) · `Environment/CombatZone` (encierra el encuentro y revela sus celdas) · `Combat/CombatDirector` (abre el combate) · `Data/ReactionTable` + `Editor/ReactionTableContent` (la matriz como datos) · `Environment/ReactionSystem` (R1–R6, delay, gracia con refresco) · `Environment/ReactionEffect` (aviso que late, fogonazo, zona por pulsos) · `Environment/Echo` (amplificación) · `RadiateElement` y el estallido irradiando en `Data/AbilityEffects.cs` · `Editor/WorldBuilder` (mundo libre de 6 biomas).
>
> ❌ **Falta:** reautorar las 30 entradas · el **modelo de carga** de §0.3 · el **techo de 2** de §0.9 · **vector de dirección** en la zona (corriente y colada) · **zona que avanza** (colada) · **romper la adquisición de lock-on** (niebla) · y dos bloqueadas por sistemas inexistentes: **cancel de recuperación** (cristalización) y **esquiva** (arenas movedizas).

- **El elemento de un punto lo dice la REJILLA DEL MUNDO** (`ElementalGrid`), no un collider: el mundo es continuo y la celda traduce posición → elemento (ARENA §1). `ElementalSurface` queda como respaldo para parches puestos a mano. Ni una ni otro mudan jamás: el terreno es fijo.
- **`ReactionTable`** (asset) mapea **el par ordenado** `(elementoTerreno, elementoEntrante) → payload`. Ordenado porque R4 lo exige: `(Agua, Fuego)` = géiser **sin bando** (respuesta del terreno) y `(Fuego, Agua)` = el mismo géiser como reacción del Agua entrante — mismo efecto, distinto gatillo y bandera.
- Cada entrada lleva además **quién manda** (`dominantElement`): su daño va con **ese tipo** y pasa por la tabla de tipos. Las **combinaciones** no lo usan — pegan **neutro** (§0.2).
- **`NoBando`** sale del dato, no del código: lo son las 6 respuestas del terreno y **las 18 entradas de combinación**.
- `ReactionSystem.Impact(pos, radius, elemento, fuerzaImpacto, instigador, dañoDelEco)` → resuelve el terreno por rejilla; si coincide con el entrante lanza la **amplificación** (`Echo`); si no, busca en la tabla, aplica la **gracia** (§0.4) e instancia el efecto con `duración = f(fuerzaImpacto)` (R2) y el **delay** de §0.2.
- Par sin entrada autorada = **sin efecto** (el impacto ocurre, la reacción no).
- Añadir/quitar/afinar reacciones = editar la tabla, **sin tocar código**.

## 8. Alcance para el prototipo
- ✅ *(2026-07-29)* **El tablero, el mundo y el motor** están montados: `ElementalGrid` + `CombatZone` + `ReactionSystem` cubren las cinco salidas con **un efecto genérico parametrizado**, así que las 15 parejas entran como **datos** y no como 15 sistemas bespoke.
- ✅ *(2026-07-29)* **Mundo libre con los 6 biomas juntos** (`Tatemis ▸ Mundo ▸ Construir mundo libre`): se camina de un bioma a otro y el combate se abre donde estés. **Sin terreno elemental no hay nada que demostrar** (R5), y ahora lo hay en todas partes.
- ✅ *(2026-07-29)* **Nodos de cristal** en la frontera Laguna–Campo de tormenta: sueltan Rayo sobre celdas de Agua, que es la única forma de ver la ★ hasta que entre una especie de Rayo o de Agua en el slice.
- ✅ *(2026-07-29: F2 cicla · G dispara)* **Spray elemental de debug** para provocar cualquier pareja sin criatura que la genere.
- **Bespoke por olas, cada uno con su criatura:** las 3 combinaciones de Mineral con Ottorock-Sujeción *(que ahora entra en el slice, antes que los otros dos)* · el incendio y su propagación con Pyrecko · las 3 de Aire con Mantagale.
- *Propagación del incendio:* se extiende **solo por terreno de Naturaleza**, así que el propio parche define hasta dónde llega — no hace falta ningún tope inventado ni grafo de vecindad: la rejilla ya sabe qué celdas son de Naturaleza.
