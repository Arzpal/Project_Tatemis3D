> 🗄️ **ARCHIVO.** Copia íntegra de `docs/ELEMENTAL-REACTIONS.md` tal como estaba el 2026-10-14, antes de limpiarlo. Aquí vive todo lo que salió de la versión limpia: historial de cambios (✏️, fechas, playtests), lo que estaba 🟡 en discusión o 🔮 a futuro, el alcance de producción y las notas de implementación. **El documento vigente es `../ELEMENTAL-REACTIONS.md`**; esto es referencia, no fuente de verdad.

# Tatemis3D — Sistema de reacciones elementales

> **TL;DR:** El terreno es **un tercer jugador con historia**. Un **golpe elemental puro solo hace daño**; los *efectos* nacen únicamente al mezclarse con el terreno, en **cinco salidas** (§0.2). Pero un impacto **ya no dispara nada: deposita saturación** (§0.10) — la **mancha** se carga, queda **cebada**, y el siguiente golpe la revienta con una **onda que la recorre**, **transforma el suelo** y ese suelo **vuelve solo** al cabo de un rato. Menos acontecimientos, mucho más grandes, y un tablero que cambia con lo que hacen los monstruos. Modelo ✅ **ratificado por Arzpal (2026-07-18 · rev. 2026-07-26 · reformulación de base 2026-07-29 · saturación 2026-07-31)**; complementa [GDD §8](GDD.md) y [ABILITY-SYSTEM.md](ABILITY-SYSTEM.md).
> **Mapa:** **§0 la capa base (verbos, salidas, bandos, persistencia) ← empezar aquí** · **§0.10 saturación del terreno ✅ 2026-07-31 — la capa que gobierna CUÁNDO ocurre todo lo demás** · §1 las tres capas · §2 *(derogada → §0.2)* · §3 reglas de resolución (R1–R6) · **§4 las 15: 6 reacciones + 9 combinaciones ✅ cerradas, con su subproducto** · §5 excepciones por especie · §6 legibilidad · §7 implementación *(⚠️ el código es anterior a §0.10)* · §8 alcance del prototipo
>
> ⚠️ **Premisa muerta el 2026-07-31:** este doc decía *«el terreno tiene su elemento y no cambia nunca»*. **Ya no.** Lo que **sí** sobrevive intacto es **R1** —se resuelve contra *el terreno*, nunca contra un efecto vivo encima—, así que la matriz sigue siendo 6×6 y la explosión combinatoria sigue muerta. Lo único que cambia es que el terreno de ahora puede no ser el de hace veinte segundos.

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

> ✏️ **Revisada contra [§0.10](#010--saturación-del-terreno--dictado-por-arzpal-el-2026-07-31--gobierna-cuándo-ocurren-las-cinco-salidas-de-02) el 2026-07-31.** Esta tabla dice **QUÉ** sale de cada pareja y sigue vigente entera. Lo que cambia es **CUÁNDO**: ninguna de estas salidas ocurre ya con un impacto — **hay que llenar la mancha** (1y amplificación · 3y combinación · 5y reacción), y el suelo neutro deja de ser un "no pasa nada" para ser una batería a 2y. **Los tres primeros pares de columnas se leen igual; la frecuencia es otra.**

| Dónde cae el golpe | Qué pasa | Daño | Efecto |
|---|---|---|---|
| **Suelo neutro** | nada, solo el golpe | de tu tipo | ninguno |
| **Tu mismo elemento** → **amplificación** | tu explosión **se repite** tras un delay corto (19 de fuego → otro 19 de fuego) | de tu tipo, **dos veces** | ninguno *(no hay mezcla que lo produzca)* |
| **Un elemento al que superas** → **reacción** | estalla la reacción, y es **tuya** (R6: no te toca) | **de tu elemento** | **tu verbo** |
| **Un elemento que te supera** → **reacción del terreno** | estalla la misma reacción, pero es **suya** y **sin bando** | **del elemento del terreno** | **su verbo** |
| **Cruzando triángulos** → **combinación** | se transforma la zona | **neutro** *(de ningún elemento)* | **los dos verbos mezclados** |

> **Reacción = explosión · combinación = transformación.** Elegir entre pegar directo y apuntar al suelo sigue siendo real, pero **la ventana de esquiva ya no es un delay: es la carga y la onda** (§0.10). El golpe directo es seguro e inmediato; apuntar al suelo **invierte** en una mancha que el rival **ve llenarse**, y cuando revienta pega mucho más pero **tarda en llegar hasta él porque viaja**. Funciona en los dos sentidos: si cebas una mancha bajo tus pies, ese aviso es también el tuyo. ✏️ *2026-07-31: el `appearDelay` que cumplía esta función se funde con la propagación — un solo mecanismo, y encima visible.*
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

Esto es lo que hace que **las zonas no necesiten CC para rentar**: empujar al enemigo un instante ya paga. 🟡 *Excepción a resolver: **Aire**, cuyo verbo modifica la zona y no el cuerpo — no hay nada que llevarse puesto al salir, salvo quedar desorientado un momento.*

> ⚠️ **El aviso de esta sección SE CUMPLIÓ** *(playtest 2026-07-30)*. Decía: *«con la mitad del suelo elemental y efectos que se pegan, el estado por defecto puede pasar a ser «siempre bajo algún efecto», y entonces dejan de leerse como acontecimientos»*. Pasó exactamente eso, y de ahí nace **[§0.10](#010--saturación-del-terreno--dictado-por-arzpal-el-2026-07-31--gobierna-cuándo-ocurren-las-cinco-salidas-de-02)**. El modelo de carga de aquí **no cambia** — sigue gobernando el efecto pegado al cuerpo.
>
> 📛 **Dos cosas distintas, dos nombres.** **«Carga»** = la intensidad 0..1 **pegada al cuerpo** de esta sección. **«Saturación»** = lo que se acumula **en el terreno** (§0.10). No mezclarlos en docs ni en código: son sistemas separados y solo comparten la palabra «acumular».

### 0.4 · Periodo de gracia ✅ *(2026-07-29 — sustituye al «cancelar al instante» de R3)*

Un efecto elemental vivo **no se puede convertir durante un periodo de gracia**. Eso impide cambiar de efecto al instante y spamear efectos sobre el mismo punto. La gracia es **menor que la duración del efecto**, así que **sí se puede interrumpir con otra habilidad** — solo que hay que esperar. R3 sigue existiendo (atacar una zona *para* limpiarla es counterplay buscado), pero con esa espera de por medio.

> ✏️ **Reconciliada con §0.10 el 2026-07-31 — la gracia pierde su trabajo principal y conserva uno pequeño.** El *anti-spam* ya no lo hace ella: lo hacen **los umbrales**, porque no se puede escupir efectos sobre un punto si cada uno cuesta 3y o 5y de saturación. Lo que la gracia sigue cubriendo es el caso corto: **dos efectos del mismo golpe pisándose** y **el rebote inmediato** dentro de una misma detonación. Y la parte de R3 que sí sigue viva —*atacar una zona para limpiarla*— pasa a resolverse por **drenaje**, no por cancelación: ver R3 en §3.

> **Repetir la MISMA reacción la refresca, no se bloquea** ✅. La gracia existe para impedir *cambiar* de efecto, no para castigar la insistencia. Sin esta precisión, la mecánica insignia de Ottorock se rompe: su cadena de 3 en Sujeción reproduce la combinación sellada en cada golpe, y los golpes 2 y 3 caerían sobre su propia zona en gracia y no harían nada.

### 0.5 · Composición de la arena ✅ *(2026-07-29)*

Proporción objetivo de celdas: **5 elementales / 5 neutras** ✏️ *(revisado 2026-07-31 — antes era 5/2/3)*. Cada **bioma** sesga esa mezcla hacia ciertos elementos: en una laguna la mayoría de celdas son de Agua. **Eso es lo que recompensa la versatilidad del equipo** — el jugador elige entre especializarse en un grupo de elementos, cubrir un triángulo entero, o variar.

> ✅ **Lo ambiental deja de ser una CLASE de celda y pasa a ser un MODIFICADOR** *(dictado 2026-07-31)*. Antes, las 3 celdas de efecto ambiental eran una tercera categoría que competía por espacio de tablero con el elemento; ahora el 2º sistema ambiental (ceniza, bruma, nieve — [GDD §8.1](GDD.md)) **se pinta encima de cualquier celda**, sea elemental o neutra. Tres ganancias: el tablero se lee con **dos** categorías en vez de tres *(pasa algo aquí, o no)* · **los dos sistemas ambientales pueden ocupar el 100 % de la arena** sin quitarse sitio · y desaparece la ambigüedad de tener **dos grises** con significados distintos.
>
> 🔑 **Y las neutras dejan de ser terreno muerto:** con §0.10 se **cargan a 2y**, así que ahora son **la mitad del tablero en potencia**. Eso resuelve la abierta de [ARENA §2](ARENA.md) (*«¿se acepta una zona pobre en elementos si el jugador pelea en un descampado?»*): ya no existe la arena que te deja sin sistema — **te la fabricas**, solo que cuesta el doble que jugar terreno natural.

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

### 0.10 · Saturación del terreno ✅ *(dictado por Arzpal el 2026-07-31 — gobierna CUÁNDO ocurren las cinco salidas de §0.2)*

> 🔑 **El terreno acumula, no reacciona.** Un impacto elemental ya **no dispara nada**: deposita **saturación** en la mancha que pisa. Cuando la mancha se llena queda **cebada**, y el siguiente impacto la revienta —mucho más fuerte y mucho más lejos que antes—, **transforma el suelo**, y ese suelo **vuelve solo** al cabo de un rato.

**Por qué existe.** El playtest del 2026-07-30 confirmó el riesgo que el propio §0.3 ya avisaba: con la mitad del suelo elemental y **una reacción por impacto**, el estado por defecto pasó a ser *«siempre está pasando algo»* — tantos estímulos a la vez que se volvían difusos, mareaban y **dejaban de leerse como acontecimientos**. Con el 30 % del daño colgando de aquí, eso no es un problema de pulido. La cura no es quitar terreno: es **concentrar el mismo daño en menos eventos y más grandes**.

#### La escalera de umbrales

**`y` = una unidad de impacto** = el `impactStrength` de un estallido de habilidad medio (`referenceImpact` en `ReactionSystem`). Un básico aporta ≈ **0,3-0,5y**.

| Dónde cae | Umbral | Qué sale | Qué deja |
|---|---|---|---|
| **Tu mismo elemento** | **1y** | **amplificación** — instantánea, **sin cebado ni delay**: es el carril rápido | nada, pero su radio **se sale de la celda** y deposita saturación en las manchas vecinas |
| **Suelo neutro** | **2y** | la celda **se vuelve elemental** | una mancha de **una sola celda** |
| **Cruzando triángulos** | **3y** | **combinación** — rango y efecto potenciados | **suelo neutro** |
| **Dentro de tu triángulo** | **5y** | **reacción** — mucho más daño y mucho más rango | **el elemento subproducto** (§4) |

La escalera **ordena por frecuencia y por potencia en el mismo sentido**: lo que más sale es lo que menos hace. Y por fin la frecuencia dice lo mismo que el nombre — las combinaciones son *transformaciones que duran* (§4) y ahora son la **textura**; las reacciones son *explosiones* y ahora son el **acontecimiento**.

🔑 **Se mide en `impactStrength`, no en daño infligido.** Medido en daño, la velocidad de carga dependería del stat de daño de la especie y el terreno sería un chequeo de estadística. Medido en `impactStrength` —campo que ya existe y se autora por habilidad— **cargar rápido es una decisión de diseño**, no un efecto lateral.

⚠️ **El único número de playtest es el valor absoluto de `y`; el ratio 1/2/3/5 está dictado.** Estimación de papel con `y = referenceImpact` y el dps de Ottorock: **~6 acontecimientos de terreno en 90 s** (uno cada ~15 s), cada uno ≈ **4-5 veces una habilidad media**. Se confirma o se tira en el laboratorio.

#### La batería es la MANCHA

Acumula la **mancha entera** —las 3-6 celdas conectadas del mismo elemento de [ARENA §1b](ARENA.md)—, no la celda suelta.
- **Acertar es fácil**, así que **no hace falta agrandar las celdas**: `cellRadius` se queda en 3,5.
- **La explosión ES la mancha**, así que el «mucho más rango» sale gratis y **con forma de ameba**, no de círculo: la geografía del tablero pasa de estética a táctica.
- 🟡 **Las neutras no tienen mancha, así que acumulan por CELDA** y al llenarse nacen como mancha de una. Buscado: **el terreno fabricado es pequeño y débil frente al natural** — fabricarte tablero es el plan B, no el plan A.
- 🔑 **El cerco de la zona de combate CORTA las manchas** ✅ *(playtest 2026-07-31)*. Dos celdas vecinas del mismo elemento **no son la misma mancha si el muro pasa entre ellas**. En una frase: *si no puedes ir andando de una celda a la otra sin salir de la arena, no son la misma mancha.* Arregla dos cosas que resultaron ser la misma:
  - Al reventar una mancha cortada por el muro, **reaccionaba también la mitad de fuera** — pagabas una detonación entera y media se perdía en terreno al que no puedes llegar.
  - Una mancha **partida en dos por el muro** seguía siendo una sola batería, así que cargar un lado llenaba el otro **por un puente que no existe para el jugador**.

  Y trae una consecuencia de diseño que conviene tener presente: **dónde cae el cerco cambia el tablero, no solo lo delimita.** El mismo terreno da baterías distintas según dónde se abra el encuentro — una mancha grande partida en dos son **dos bombas pequeñas en vez de una enorme**. Al dictar cómo se coloca la zona ([ARENA §1](ARENA.md)), eso cuenta como decisión de balance.

#### El medidor es uno, y se disputa

**Un medidor por mancha**, con dueño = el elemento que la está cargando. Un elemento distinto **la baja** en vez de subirla; al llegar a 0 **cambia de dueño**, y con él el umbral. Una sola barra: legible, y convierte el terreno en **objetivo disputado** — que es el «tercer jugador» con cara.

⚠️ **Consecuencia buscada: defender es mucho más barato que robar.** Al que va por 4y de 5 le falta 1y; al rival le cuestan 4y vaciar **más** 3y llenar. Negar terreno es el último recurso, no el reflejo.

**Gracia y luego decaimiento** ✅ *(corregido en el playtest del 2026-07-31)*. Tras el último golpe el medidor **se sostiene** durante un periodo de gracia —no se mueve salvo que alguien lo suba o lo baje— y **solo después** empieza a bajar. Sin la gracia, la carga se escurría desde el frame siguiente y **cargar se sentía como llenar un cubo agujereado**: no daba tiempo a reposicionarse, ni a encadenar, ni a elegir cuándo seguir. El decaimiento posterior sigue haciendo falta: sin él, al minuto 2 el tablero entero está cebado y vuelve el ruido que esto vino a curar.

#### Cebada, no automática

Al llenarse, la mancha **no estalla: queda cebada**, y detona con **el siguiente impacto elemental**. El jugador elige el momento y el terreno pasa a ser **remate de combo** — que es exactamente lo que pide §0.2 (*«un combo bueno consiste en encadenar básicos y habilidades apuntando a zonas elementales»*) y lo que le da función real al *crecimiento explosivo*, lo único del sistema que fija a alguien dentro de una zona. **Guardarraíl:** si nadie la detona en **~5 s**, **se ventea** — fogonazo débil, sin efecto, medidor a cero.

#### La detonación se ENCADENA ✅

La mancha **no revienta de golpe**: revienta **la celda que recibió el impacto** y la **onda avanza por vecindad** al resto de la mancha, dejando terreno transformado detrás. No es mecanismo nuevo — es **el del incendio de §4, generalizado a las seis**; la rejilla ya es el grafo de vecindad y el código ya existe.

- 🔑 **El jugador APUNTA la onda.** Nace donde cae el golpe: detonar por el borde cercano la manda lejos de ti; por el lejano te la trae encima, pero alcanza al que huye. Decisión nueva **sin input nuevo**.
- 🔑 **La onda ES la ventana de esquiva.** El `appearDelay` de §0.2 existía solo para dar tiempo a apartarse; ahora ese tiempo lo da **el viaje**. Dos mecanismos se funden en uno, y el que queda **se ve**.
- ⚠️ **Golpea a cada criatura UNA sola vez.** Sin esto, quedarse en el centro de una mancha de 6 son 6 impactos y la reacción deja de ser acontecimiento para ser bomba. **Se ve recorriendo, cobra una vez** — es la regla del 22/07 con otro nombre (*la detección se muestrea, las consecuencias se cronometran*, [ABILITY-SYSTEM §4.1](ABILITY-SYSTEM.md)).
- ⚠️ **La onda va más rápido que correr**, o se escapa hacia delante. Debe esquivarse **de lado o hacia atrás**: con decisión, no con velocidad.
- **Se encadenan las REACCIONES; las combinaciones se EXTIENDEN.** Una reacción es explosión: recorre. Una combinación es transformación: no revienta en cadena, **ocupa la mancha**. Mismo código de propagación, dos verbos. La amplificación no encadena nada.

#### Qué deja detrás

🔑 **Toda detonación cambia el tablero: la mancha adopta el subproducto y el ANILLO de celdas que la rodea se mancha con él.** Cuando subproducto y mancha coinciden, lo que ocurre es que **la mancha se expande** — no existe ningún caso nulo.

- **Reacciones** → el subproducto de §4. **Combinaciones** → **neutro** (nadie ganó; el suelo se gastó).
- ⚠️ **Una detonación NO deposita saturación.** Es el cobro, no una entrada. Sin esta regla, el anillo que acaba de crear se carga con el propio estallido y tienes **dominó infinito**. Es la tercera aplicación de algo que el doc ya dice dos veces (R1 *«las reacciones nunca se encadenan»* · *«el eco no dispara reacciones»*, §4).
- **Una detonación borra los efectos vivos en su alcance**: el suelo que los sostenía dejó de ser ese elemento.

#### Reversión con refresco

La celda transformada **vuelve a su estado horneado** pasado un cooldown — pero **si durante ese cooldown se pelea encima, el cooldown se refresca**. Consecuencia emergente y buscada: **el suelo que se disputa se queda cambiado; el que se abandona, se cura.** El combate deja cicatriz donde de verdad ocurrió, y la arena **no se agota nunca**. Mismo patrón que §0.4 ya usa para los efectos (*repetir la misma reacción la refresca*), aplicado al terreno.

✅ **Dos precisiones del playtest del 2026-07-31:**
- 🔑 **Refresca CARGAR, no solo detonar.** Basta con **mover el medidor** —subir el tuyo o bajar el del rival, las dos cosas son disputa— para reiniciar el reloj. Antes solo lo refrescaba una detonación, y una mancha que estabas cargando activamente podía revertirse **a mitad de la carga**: veías deshacerse justo lo que estabas construyendo.
- 🔑 **El doble de tiempo en las celdas que nacieron NEUTRAS.** Fabricarte tablero es un proceso de **dos pasos** (convertir la neutra a 2y y *después* cargarla a 3y o 5y). Con el mismo reloj que el terreno natural se te deshacía a mitad del proceso y el plan B no era viable nunca. El terreno natural sigue siendo mejor —sale más barato y su mancha es más grande—, pero el fabricado ahora **te da tiempo a usarlo**.

#### El golpe que no llena habla, pero no hace

Cada impacto que no completa la mancha produce **un destello mudo de la mezcla correcta** —el vapor sisea, la hierba chisporrotea— **sin daño, sin estado y sin zona**, y **crece con la saturación**: casi imperceptible al principio, escandaloso cuando falta poco. Hace dos trabajos que el sistema necesita:
1. **Enseña las 15 parejas.** Con las mezclas hechas raras, sin esto el jugador no aprendería nunca qué produce cada combinación.
2. **Es el telegraph.** Una mancha a punto de reventar **se ve y se oye desde lejos**, y por eso la detonación puede permitirse ser enorme sin ser injusta.

⚠️ **Sin realimentación visual de la celda este sistema no existe.** Si el hexágono no se ve llenarse, el jugador no percibe carga: percibe que **golpear el suelo dejó de hacer nada**. **La saturación visible no es pulido, es la mitad de la mecánica.** ✅ *Confirmado por las malas en el playtest del 2026-07-31: con la carga solo en texto de debug, el sistema entero se leía como roto.* Resuelto con **dos señales para dos distancias** (`Environment/SaturationVisual`):
- **De cerca — el hexágono se llena:** crece desde el centro y se enciende con el color del elemento que lo carga. **Crecer** es lo que se lee como medidor; solo cambiar el color parecería otro terreno.
- **De lejos — la columna:** una barra vertical en el centro de la mancha cuya **altura es el medidor**. Es lo que permite decidir *«voy a esa zona»* sin estar encima.
- **Cebada, las dos laten en blanco.** Ese latido es lo que hace justa una explosión enorme.

> Se dibuja **al margen de `CombatZone`** a propósito: la zona solo pinta celdas mientras hay un encuentro abierto, y la carga tiene que verse **siempre y en cualquier escena** — mundo, sandbox y el futuro laboratorio.

#### Las tres excepciones instantáneas ✅

No pasan por saturación y resuelven en el acto (más flojas que una detonación, a cambio de ser inmediatas):
1. **Los choques en el aire** (§0.2b) — no hay suelo donde acumular: o son instantáneos, o desaparecen del juego y con ellos el único carril rápido a distancia.
2. **La roca sellada de Ottorock** (§5) — es su mecánica insignia y la única fuente portátil de efecto (§0.8); con carga se apagaría.
3. **Las minas encendidas de Pyrecko** (§5) — el campo de minas sigue siendo un campo de minas.

> 💾 **Semilla guardada (Arzpal, 2026-07-31):** *cargar terreno a propósito para detonarlo después* suena a **arquetipo entero**, no a habilidad — es la pregunta generadora del **Controlador** ([ARCHETYPES](ARCHETYPES.md)). Anotada en la cantera ([CREATURE-IDEAS §2](CREATURE-IDEAS.md)); fuera del slice.


## 1. Las tres capas (no confundirlas)

**① Terreno — el tablero, y tiene historia** ✏️ *(rev. 2026-07-31: era «fijo, no cambia jamás»)*.
Al entrar en combate se **acota una zona** dividida en **celdas hexagonales**. Cada celda tiene **o un elemento, o nada** *(lo ambiental es un modificador encima, §0.5)*, horneado desde el bioma. **El terreno arranca en un estado y ese estado se mantiene mientras nadie lo trabaje** — pero una detonación lo transforma, y la reversión con refresco lo devuelve (§0.10). Los 6 terrenos elementales ✅ *(dictados 2026-07-18)*:

| Elemento | Terreno |
|---|---|
| **Fuego** | roca volcánica |
| **Agua** | río |
| **Naturaleza** | pasto / hierba alta |
| **Mineral** | zona de menas (minerales incrustados en la roca) |
| **Rayo** | piedra electrificada (como la volcánica, pero de electricidad) |
| **Aire** | corrientes de aire (géiseres que expulsan aire en vez de agua) |

**Los 6 elementos tienen terreno propio a propósito:** sin terreno de Aire, la dominancia Rayo › Aire sería inalcanzable. Las celdas **neutras** ya no son terreno muerto: no reaccionan, pero **se cargan a 2y y se vuelven elementales** (§0.10) — R5 sigue siendo cierta *en el momento del golpe*, y ahora además es **el punto de partida de un tablero que te construyes**. Lo **ambiental** (polvo, nieve, lluvia…) es el otro sistema y se pinta **encima** de cualquier celda, sin pasar por la tabla — pendiente de dictar.

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
> ✅ **R1 sobrevive intacto a §0.10** — es la regla que hace que el terreno mutable no rompa nada. Con la saturación, «el terreno» pasa a significar **el terreno de ahora mismo**: si una detonación convirtió el pasto en ceniza, el siguiente impacto resuelve contra **Mineral**, no contra el pasto que hubo. La matriz sigue siendo 6×6 porque **lo que cambia es la casilla, no la tabla**.

**R2 · La duración es proporcional al impacto.** Cuanto más fuerte el golpe que irradia el elemento, más dura la reacción. Premia meter la habilidad pesada en el sitio correcto en vez de escupir habilidades baratas.

**R3 · Un impacto nuevo convierte el efecto activo — pasado su periodo de gracia** *(✏️ rev. 2026-07-29, ver [§0.4](#04--periodo-de-gracia--2026-07-29--sustituye-al-cancelar-al-instante-de-r3))*. Al impactar **dentro** de una reacción viva —dentro, no rozándola: el propósito es puntería, no proximidad— esta se cancela y se resuelve de cero contra el terreno. → **Counterplay explícito:** atacar una zona activa sirve para **limpiarla** si estorba. **Tres precisiones que trajo el rediseño:** durante la **gracia** el efecto aguanta y el impacto nuevo no cuaja · repetir **la misma** reacción la **refresca** en vez de bloquearse · y dos efectos de **la misma habilidad** no se pisan entre sí (el daño crea y el estallido borraría).

> ✏️ **REESCRITA el 2026-07-31 contra §0.10 — la cancelación pasa a DRENAJE.** Con la saturación, un impacto ya no *resuelve*: **deposita**. Si R3 se quedara como estaba, **dejaría de existir la forma de quitar una zona** — y las zonas ahora son más grandes y duran más, así que no poder limpiarlas es peor que antes. La regla vigente:
> - Golpear **dentro** de una zona viva con **otro elemento** le **drena duración** (mismo tira y afloja del medidor de §0.10 — cero conceptos nuevos).
> - **Ese impacto no deposita saturación**: gastas el golpe en limpiar en vez de en cargar. **El counterplay conserva su coste**, que es lo que R3 siempre quiso.
> - Sigue exigiendo caer **dentro**, no rozar: el propósito es puntería, no proximidad.
> - **Una detonación borra los efectos vivos en su alcance** sin pagar nada — el suelo que los sostenía dejó de ser ese elemento (§0.10).

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

#### Qué terreno deja cada reacción ✅ **CERRADO (2026-07-31)**

Al detonar (§0.10), la mancha adopta el **subproducto** y el **anillo** que la rodea se mancha con él. Cada resultado sale del **fenómeno físico**, igual que salieron los verbos de §0.1:

| Reacción | Deja terreno de | Por qué |
|---|---|---|
| **Naturaleza › Agua** — crecimiento explosivo | **Naturaleza** | las raíces se beben el charco y se quedan |
| **Agua › Fuego** — géiser | **Aire** | el vapor |
| **Fuego › Naturaleza** — incendio | **Mineral** | la ceniza |
| **Mineral › Rayo** — energía geo | **Fuego** | la roca se sobrecalienta con la carga |
| **Rayo › Aire** — descarga de tormenta | **Agua** | la lluvia después de la tormenta |
| **Aire › Mineral** — tormenta de arena | **Rayo** | la arena en suspensión **se carga de estática** *(real: las tormentas de polvo generan campos eléctricos por fricción)* |

> 🔑 **La paridad es una regla de balance, no una simetría bonita.** Los seis elementos aparecen **exactamente una vez** como resultado, así que la tabla es una **permutación** — y de ahí sale que **ningún elemento pueda extinguirse del tablero ni comérselo**. Si dos reacciones dieran Mineral y ninguna diera Fuego, una arena larga tendería a mineralizarse sola. Con la paridad, **el tablero cambia sin derivar**.
>
> **Las 9 combinaciones dejan NEUTRO**, todas. No manda nadie, así que nadie reclama el suelo: se gastó.
>
> 🔑 **Ningún caso es nulo, gracias al anillo.** *Crecimiento explosivo* deja Naturaleza, y una de sus dos direcciones (R4: entra Agua sobre terreno de Naturaleza) ocurre sobre suelo **que ya es Naturaleza** — la mancha no cambia, pero **el anillo sí**, así que su función ahí es **expandir la mancha**. Que la única reacción llamada *crecimiento* sea la única que hace crecer terreno no es casualidad: es señal de que el modelo de verbos está bien tirado. Y tiene un lector precioso: **una criatura de Agua peleando sobre hierba riega el prado del rival** — justo la pareja donde ella pierde. Es el mejor ejemplo que tendrá el juego de *«provocar reacciones con un elemento al que eres débil no siempre es malo»* (§0.5), pero con un coste que **se queda en el suelo**.
>
> 🟡 **A vigilar en el laboratorio:** Naturaleza es **el único elemento que puede crecer sin consumirse a sí mismo**, así que en una pelea larga sobre hierba puede reptar. Tiene dos frenos ya dentro del modelo —el cooldown de reversión, y que Fuego › Naturaleza la convierte en ceniza— pero es el único sitio donde el tablero puede derivar.

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
- ✅ **INSTANTÁNEA — no pasa por saturación** *(dictado 2026-07-31, §0.10)*. Cada impacto reproduce su combinación **en el acto**, sin cargar ninguna mancha. Es más floja que una detonación, y a cambio es inmediata. Sin esta excepción su mecánica insignia se apagaría: no puede ser la *«única fuente portátil de efecto»* (§0.8) si además hay que llenar un medidor para usarla.
- Terreno neutro al agarrar → **roca pura**: el trozo reacciona como **Mineral** contra el terreno donde impacte.
- **Solo se sella lo que es COMBINACIÓN.** Si el agarre dispara una **reacción** (sobre Rayo, que él domina; sobre Aire, que le domina) o una **amplificación** (sobre sus propias menas), esta se consume ahí y el trozo queda **puro**.
- Al volver a Libre (romper o lanzar la roca) **pierde la combinación**.
- 🟡 **Sin resolver — el bando.** §0.7 dice que las combinaciones **no son de nadie**, y él reproduce la suya **en cada impacto y a bocajarro**: su mecánica insignia le salpica cada vez que la usa. Puede que basten el **delay** y que su estallido nazca **por delante del cuerpo**, o puede que la roca sellada tenga que ser la excepción. **Decide Arzpal.**

**Pyrecko — el «combustible»** ([ficha](CREATURE-PYRECKO.md)). Marca de especie, **no** elemental:
- **Inerte**: solo su Signature la **enciende** — y encenderla **no la detona: la arma**. ✅ *(2026-07-30)* Una marca encendida **estalla en Fuego al más mínimo contacto**: **daño de Fuego** a quien la toca, y **el estallido irradia Fuego contra su celda** → ese fuego resuelve normalmente contra el terreno (sobre terreno de Agua → **respuesta del terreno**: el **géiser**, sin bando y con daño **de Agua**, que le eleva a él si está dentro — es el ejemplo canónico del modelo). El campo encendido es un **campo de minas**: *no pisarlo* es el counterplay.
- ✅ **Su kit no aplica NINGÚN efecto por sí solo** *(2026-07-30)* — se le quitaron la **quemadura** del Disparo ígneo y la **ralentización del 20 %** de la Signature (§0.2 y §0.8; el slow era además el verbo de **Mineral**, §0.1). Todo lo que produce es **daño de Fuego + entrega de Fuego**; los efectos siguen naciendo solo de la mezcla.
- **Solo desaparece de dos formas: al expirar (10 s) o al estallar** — y estalla **una sola vez**: ✅ *(2026-07-30)* encendida es una **mina de un uso, que se consume al contacto**, así que el campo se gasta según lo pisan. *Las reacciones elementales ya **no** la limpian (regla anterior derogada 2026-07-18).*
- ✅ **La mina encendida es INSTANTÁNEA — no pasa por saturación** *(dictado 2026-07-31, §0.10)*. Al pisarla, el estallido irradia Fuego contra su celda y **resuelve ahí mismo** contra el terreno. El campo de minas sigue siendo un campo de minas: si depositara saturación, dejaría de ser una amenaza para convertirse en una máquina de cargar tablero *(idea guardada como semilla de **Controlador** en la cantera, §0.10)*.
- **Pertenece al lugar, no al material:** si Ottorock arranca el trozo de terreno donde hay combustible, se lleva **el terreno, no la marca** — el combustible se queda donde estaba.

**Mantagale — tornados que empujan y detonan por colisión** ([ficha](CREATURE-MANTAGALE.md) §C). ✅ *(2026-07-18: sustituye tanto a "mover/dispersar zonas" como a "dispara reacciones mientras recorre" — ambas derogadas.)*
- Sus **tornados empujan durante todo su recorrido** (negación de área), pero son **inertes en lo elemental**: no tocan el terreno ni disparan reacción al viajar — igual que el combustible de Pyrecko, son una marca de especie con su propia lógica.
- La reacción ocurre **al reventar**, y revientan por **colisión de fuerzas opuestas**: el tornado empuja, su Ciclón **atrae**, y el choque los detona. La reacción resuelve contra el terreno **del punto de colisión**, con el enemigo arrastrado allí dentro.
- El Ciclón **arrastra al enemigo por el suelo** hasta el centro (no lo teletransporta): al volver, **atraviesa los tornados sembrados por el camino**.
- Su fuerza **depende del terreno de la arena** — es el que "juega el tablero". Sobre **neutro** no consigue nada. Sobre **Rayo**: sus tornados al reventar provocan la **descarga de tormenta del terreno, sin bando**, que además le corta el **arranque** de sus propios ataques. Es su tablero de **alto riesgo**.
- ✅ **Peces espíritu — su motor de altura** *(dictado 2026-07-30; **deroga** «montar las corrientes que levanta toda reacción», §2)*. Cada **mezcla elemental que provoca** hace nacer un **pez espíritu**, hasta **5**; el pez **nace en la mezcla y viaja hasta él** para unirse al séquito (la altura llega **con un viaje de por medio**, no al instante), y los peces **lo elevan** —más peces, más altura— y **cada uno tiene su duración**: al morir, baja un escalón. Sin peces, planea hasta el suelo. Es **marca de especie** (§1③), como el combustible y la roca sellada, así que **la capa base no necesita ninguna excepción**: ningún verbo genera corrientes, y ya no hace falta que lo haga. ✅ **Los peces cuentan como PROYECTILES** *(2026-07-30)*: un **ataque elemental** los destruye **en tránsito**, los neutros no ✏️ *(31/07: sin fase de escolta — se consumen al llegar)*. Su altura es **un recurso atacable**, y el **velo de esporas** (que detona el proyectil elemental que lo cruza) se los mata, incluido el suyo propio. ✅ **El choque que revienta un pez NO le repone otro** *(confirmado 2026-07-30)*: la mezcla que nace ahí (§0.2b) **no es de nadie** (§0.7), así que no cuenta como provocada por él — si contara, matarle peces se los devolvería y no habría contrajuego. El que dispara paga un fogonazo **imparcial** por cada pez. ✅ **Solo le dan pez las mezclas que provoca ELLA** *(dictado 2026-07-30)* — las de sus compañeros no: **sin sinergia de equipo** (deroga la decisión del 26/07). El principio general vive en [CREATURES §2](CREATURES.md).
- ✅ **REDISEÑADA POR §0.10 *(2026-07-31)*: los peces pasan de PORTADORES a COMBUSTIBLE.** Cada detonación que ella provoca hace nacer **5 peces de golpe**; viajan hasta ella y **se consumen uno a uno, rápido**. 🔑 **El pez no es altura: es sustentación** — Mantagale **planea siempre**, y los peces **invierten el signo del planeo**: con peces asciende, sin peces desciende. En el techo no se desperdician: compran **permanencia**, así que **acertarlo todo la mantiene arriba**. Con las mezclas hechas raras (~6 por combate), *«un pez por mezcla, cada uno con su reloj»* la dejaba **sin volar nunca**; así, **una detonación = un vuelo**, y su altura pasa de goteo a **arco**: sube de golpe y baja preparando la siguiente. 🔑 **Los 5 viajan por curvas distintas**, así que el tirador elemental **no puede reventarlos de una pasada**: matarle 2 = sube 3 escalones. **El contrajuego pasa de binario a gradiente**, y su ventana es **el viaje** (consumido = intocable). Detalle completo en la [ficha](CREATURE-MANTAGALE.md).
- ✅ **Sus ráfagas (básico) son NEUTRAS sin potenciar** *(2026-07-30)* — solo son **entrega de Aire** cuando la Flex las potencia. Si no, el **velo de esporas** que él mismo genera sobre pasto le detonaría sus propios básicos encima (§0.2b).

## 6. Legibilidad
- **Reacción = ruidosa (explosión) · combinación = suave (transformación) · respuesta del terreno = el VFX de su reacción espejo con matiz de "provocada"** (arranca desde el terreno hacia el atacante, no desde el impacto hacia fuera). En caja gris eso se resuelve **tiñendo de rojo** el fogonazo y la etiqueta. Los únicos "no pasa nada" que quedan son el **suelo neutro** y —solo en cuanto a efecto— la **amplificación**, que sí pega.
- **Una reacción tiene que separarse en luminancia del terreno donde nace.** No es acabado, es restricción dura: crítico en las combinaciones, que no tienen fogonazo y cuyo disco es lo único que aparece.
- **Si un efecto no se ve en el cuerpo del afectado, no entra en §4** (§0.9). En caja gris: tinte y escala de cápsula.
- **Toda reacción avisa antes de existir** — ✏️ *(rev. 2026-07-31)* el aviso ya no es un disco que late durante 0,45 s: es **la mancha llenándose a la vista** y luego **latiendo cebada** (§0.10). El aviso pasa de durar medio segundo a durar todo lo que tarde el combate en cargarla, y por eso la detonación puede permitirse ser enorme sin ser injusta.
- 🔑 **La saturación de la celda ES interfaz, no decoración.** El hexágono tiene que **verse llenar** y la mancha cebada tiene que **verse y oírse desde lejos**. Sin esto el jugador no percibe carga: percibe que golpear el suelo dejó de hacer nada, y el sistema entero se vuelve invisible (§0.10).
- **El destello mudo crece con la carga** (§0.10): casi imperceptible al principio, escandaloso cuando falta poco. Es a la vez **el tutorial permanente de las 15 parejas** y **el telegraph**.
- **La onda de la detonación se lee como frente que viaja**, no como fogonazo: es lo que la hace esquivable **de lado**, y lo que comunica que nació donde cayó el golpe.
- **El terreno transformado tiene que distinguirse del horneado**, o el jugador no sabrá que la ceniza de esa mancha la hizo él. En caja gris basta un borde o un tono ligeramente distinto que **se apaga al revertir**.
- Los efectos **se dispersan solos** (R2) y el terreno vuelve — ✏️ *ahora literalmente*: la reversión con refresco de §0.10 devuelve la celda a su estado horneado. **El mundo cambia, pero nunca queda pintado.**

## 7. Implementación (data-driven)
> ✅ **§0.10 IMPLEMENTADA (2026-07-31).** `ReactionSystem.Impact()` ya **deposita en vez de resolver**. Piezas nuevas y cambiadas:
> - **`Environment/TerrainSaturation`** *(nuevo)* — la capa de medidores: batería **por mancha** (las neutras, **por celda**), **tira y afloja** con dueño, decaimiento lento, **cebado** y **venteo**. Es una clase plana que posee `ReactionSystem`, así que **no hace falta tocar ningún builder de escena**.
> - **`Environment/DetonationWave`** *(nuevo)* — la **onda encadenada**: agrupa la mancha por distancia hexagonal a la celda impactada y avanza anillo a anillo (`StepSeconds = 0.08` ≈ 75 u/s, muy por encima de la carrera). Un `HashSet` compartido garantiza **un golpe por criatura**. Su `Settle()` es también el camino de las combinaciones, que **no encadenan: se extienden**.
> - **`ElementalGrid`** — `Cell.patch` + registro de manchas por **componentes conexas** (dos manchas pegadas son **una batería**, que es como las ve el jugador) · `TransformCells()` con **reversión que se refresca** · `RingAround()` · evento `CellsChanged`.
> - **`ReactionEffect`** — el fogonazo se extrajo a `ApplyBurst()` estático (para que la onda lo reparta celda a celda con la regla de daño en **un solo sitio**) · `DrainAt()` = **R3 por drenaje** · `ClearLiveIn()` · `suppressBurst` para que la zona residual no cobre dos veces.
> - **`ReactionTable`** — `subproduct` + `TerrainAfter`; los **6 subproductos autorados** en `Editor/ReactionTableContent`.
> - **`CombatZone`** — un solo material y `MaterialPropertyBlock` por celda: **relleno que sube con la saturación**, **latido de la mancha cebada** y **marca de terreno transformado**.
> - **`ReactionSystem.Instant()`** — la puerta de las **tres excepciones** (choques en el aire, roca sellada, minas), que no pasan por saturación.
> - **`ElementalSpray`** — dos modos (**H** alterna): *saturar* (el camino real) e *instantáneo* (para el checklist de las 30 casillas), más lectura en pantalla de la batería bajo el punto de disparo.
>
> ⚠️ **Sin compilar en Unity todavía** — escrito y revisado en frío. El primer Play es lo que dirá si `y` (`referenceImpact`) está en su sitio.
>
> ✅ **Ya en código:** `Environment/ElementalGrid` + `HexCoord` (el tablero: celdas horneadas desde biomas, composición determinista — ⚠️ *sigue en **5/2/3**; el diseño dice **5/5** desde el 31/07*) · `Environment/CombatZone` (encierra el encuentro y revela sus celdas) · `Combat/CombatDirector` (abre el combate) · `Data/ReactionTable` + `Editor/ReactionTableContent` (la matriz como datos) · `Environment/ReactionSystem` (R1–R6, delay, gracia con refresco) · `Environment/ReactionEffect` (aviso que late, fogonazo, zona por pulsos) · `Environment/Echo` (amplificación) · `RadiateElement` y el estallido irradiando en `Data/AbilityEffects.cs` · `Editor/WorldBuilder` (mundo libre de 6 biomas).
>
> 🔢 **Los números que hay que medir en el primer playtest**, por orden de impacto: **`referenceImpact`** (= 1y, en `ReactionSystem`) · **`decaySeconds`** de la saturación · **`primedSeconds`** (venteo, 5 s) · **`revertSeconds`** de la rejilla (22 s) · **`spillPerNeighbour`** del derrame de la amplificación · **`DetonationWave.StepSeconds`** (velocidad de la onda) · y el **tamaño de mancha** de [ARENA §1b](ARENA.md), que ahora es balance y no estética.
>
> ❌ **Sigue faltando (independiente de §0.10):** el **techo de 2** de §0.9 · **romper la adquisición de lock-on** (niebla) · y dos bloqueadas por sistemas inexistentes: **cancel de recuperación** (cristalización) y **esquiva** (arenas movedizas).
>
> ✅ **`appearDelay` retirado**: su trabajo lo hacen ahora la carga y la onda (§0.2). El delay que queda (`instantDelay`) es **solo** para las tres excepciones instantáneas y el respaldo sin rejilla.
>
> 🟡 **Costuras conocidas del código nuevo, por si chirrían en el playtest:** ① la propagación del incendio queda **desactivada en la zona residual de una detonación** (la onda ya recorrió la mancha; propagar otra vez saltaría a manchas vecinas **sin pagar saturación**, que es el dominó que §0.10 prohíbe) — sigue viva en el camino instantáneo · ② `radiusScale` de la tabla ahora multiplica un radio **ya del tamaño de la mancha**, así que la tormenta de arena (1,4×) se sale bastante del parche: es coherente con su rasgo de familia, pero es un número a mirar · ③ una detonación **renumera las manchas**, y las baterías huérfanas se tiran — deliberado: *una mancha que acaba de cambiar de elemento no debe conservar la carga de lo que era*.

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
