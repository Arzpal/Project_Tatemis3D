# Tatemis3D · Sistema de reacciones elementales

> **TL;DR:** El terreno es **un tercer jugador con historia**. Un **golpe elemental puro solo hace daño**; los *efectos* nacen únicamente al mezclarse con el terreno, en **cinco salidas** (§0.2). Un impacto **no dispara nada: deposita saturación** (§0.10). La **mancha** se carga, queda **cebada**, y el siguiente golpe la revienta con una **onda que la recorre**, **transforma el suelo**, y ese suelo **vuelve solo** al cabo de un rato. Pocos acontecimientos, grandes, y un tablero que cambia con lo que hacen los monstruos. Complementa [GDD §8](GDD.md).
> **Qué es este documento:** solo el modelo de diseño **cerrado**. La implementación vive en [ARCHITECTURE.md](ARCHITECTURE.md); el estado del código en [TASKS.md](TASKS.md); el historial de cambios, lo que sigue en discusión y la versión completa en [archive/ELEMENTAL-REACTIONS-completo-2026-10-14.md](archive/ELEMENTAL-REACTIONS-completo-2026-10-14.md). La numeración de secciones se conserva porque otros documentos la citan.
> **Mapa:** **§0 la capa base ← empezar aquí** · **§0.10 saturación del terreno: CUÁNDO ocurre todo lo demás** · §1 las tres capas · §3 reglas de resolución (R1–R6) · §4 las 15 parejas · §5 excepciones por especie · §6 legibilidad

---

## 0. La capa base

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

> **El dominante pone el VERBO, el dominado pone la MATERIA.** Una reacción lleva el verbo **del elemento que manda**, y del perdedor solo hereda **de qué está hecha**. Por eso la tormenta de arena (Aire › Mineral) tapa la visión (verbo de Aire) y araña porque es roca pulverizada (materia de Mineral), pero **no ralentiza**: ralentizar es el verbo de Mineral, y ahí Mineral no manda. Esta regla es la que impide que los seis verbos se contaminen entre sí.

**El espejo Naturaleza ↔ Rayo** es deliberado: uno te quita moverte y te deja pegar; el otro te quita encadenar y te deja moverte. Están en triángulos distintos, así que **nunca se dominan**: solo se combinan. **El espejo Mineral ↔ Aire** también: uno ancla y pesa, el otro suspende y dispersa; y esos dos **sí** son pareja de dominancia (Aire › Mineral).

> **El verbo de Rayo es el más sistémico:** el bucle del juego es *combo → daño continuo → aturdimiento → captura*, y el aturdimiento decae si dejas de golpear ([GDD §8.2](GDD.md)). Un efecto cuyo trabajo es **impedir que encadenes** no te quita vida: te quita la captura.
>
> **Los ticks van por reloj, no por azar.** La referencia es la parálisis de Pokémon, pero en tiempo real una interrupción aleatoria no se lee como castigo sino como *input perdido*. Con ticks cronometrados el jugador **aprende el ritmo y cuela el combo entre ellos**: misma sensación, y premia habilidad.

### 0.2 · Un elemento puro solo hace daño

**Los verbos NO son propiedades del elemento suelto.** Fuego solo no propaga; Rayo solo no paraliza. **Los efectos ambientales nacen exclusivamente de la mezcla con el terreno**, como en el mundo real. Un golpe elemental que no toca terreno elemental es *daño de su tipo y nada más*, y eso es una opción válida y deliberada: es **segura e inmediata**, y en un duelo de dominancia el ×1.5 contra el rival ya es su recompensa.

De ahí salen **las cinco salidas**, todas distintas y todas explicables en una frase. Esta tabla dice **QUÉ** sale de cada pareja; **CUÁNDO** sale lo decide la saturación (§0.10).

| Dónde cae el golpe | Qué pasa | Daño | Efecto |
|---|---|---|---|
| **Suelo neutro** | nada, solo el golpe | de tu tipo | ninguno |
| **Tu mismo elemento** → **amplificación** | tu explosión **se repite** tras un delay corto (19 de fuego → otro 19 de fuego) | de tu tipo, **dos veces** | ninguno *(no hay mezcla que lo produzca)* |
| **Un elemento al que superas** → **reacción** | estalla la reacción, y es **tuya** (R6: no te toca) | **de tu elemento** | **tu verbo** |
| **Un elemento que te supera** → **reacción del terreno** | estalla la misma reacción, pero es **suya** y **sin bando** | **del elemento del terreno** | **su verbo** |
| **Cruzando triángulos** → **combinación** | se transforma la zona | **neutro** *(de ningún elemento)* | **los dos verbos mezclados** |

> **Reacción = explosión · combinación = transformación.** Elegir entre pegar directo y apuntar al suelo es una decisión real: el golpe directo es seguro e inmediato; apuntar al suelo **invierte** en una mancha que el rival **ve llenarse**, y cuando revienta pega mucho más pero **tarda en llegar hasta él porque viaja**. Funciona en los dos sentidos: si cebas una mancha bajo tus pies, ese aviso es también el tuyo.
>
> *Dos verbos crean zona por su propia naturaleza, **propagar** (Fuego) y **nubelizar** (Aire), así que sus reacciones duran en vez de estallar y punto. Es deliberado: **manda el verbo**, no la simetría.*

### 0.2b · Los elementos también chocan entre sí

Las mezclas **no necesitan terreno**: dos entregas elementales que se encuentran en el aire resuelven su pareja **en el punto del choque**. Ottorock lanza su roca, Mantagale le responde con un tornado, las dos colisionan y ahí nace la reacción. Vale para **cualquier entrega elemental**, no solo proyectiles: un estallido melee contra un proyectil entrante también cuenta. La regla es *dos elementos que se tocan reaccionan*, sin enumerar casos.

- **Es imparcial**: la mezcla proviene de los dos, así que **afecta a los dos** (mismo principio que la reacción del terreno: nadie es dueño de lo que provocaron entre ambos).
- **Mismo elemento → amplificación**: los proyectiles se anulan, pero la explosión suena dos veces.
- **El suelo de debajo no cuenta**: ya resolvió una vez. Las reacciones **nunca se encadenan** (R1).
- Le da **uso defensivo** a los ataques elementales: un proyectil se puede *contestar*, no solo esquivar. Es la interacción entre criaturas a distancia.
- *Consecuencia para el melee:* pegar de cerca te deja dentro del radio de una mezcla imparcial. Lo que te protege es golpear y salir, y que los estallidos melee nazcan **por delante del cuerpo**. Comerse más ambiente es el peaje justo del cuerpo a cuerpo, y parte de por qué existe el rango.

> **Peso objetivo:** una reacción vale **casi lo que una habilidad promedio**. En un duelo completo, el reparto del daño total es **30% básicos / 40% habilidades / 30% reacciones y combinaciones**. El terreno **no es un extra**: pesa lo mismo que tus básicos, y *un combo bueno consiste en encadenar básicos y habilidades apuntando a zonas elementales*.

### 0.3 · El efecto se pega

Los efectos ocurren en la zona, **pero se quedan pegados a quien alcanzan**. Dentro de la zona la duración es **eterna** (hasta que la zona muera); al salir, el efecto **decrece** hasta apagarse. Una quemadura sigue quemando fuera del incendio, cada vez menos.

**Una carga, no seis reglas:** cada efecto lleva una **carga que sube dentro y baja fuera**, y su intensidad *es* esa carga. Las tres propiedades salen de un solo número: dentro se mantiene, fuera decae, y volver a entrar recarga. Y regala una cuarta: **cuanto más tiempo estuvo dentro, más le dura fuera**, así que forzar la permanencia es la jugada premium.

Esto es lo que hace que **las zonas no necesiten CC para rentar**: empujar al enemigo un instante ya paga.

> **Dos cosas distintas, dos nombres.** **«Carga»** = la intensidad 0..1 **pegada al cuerpo** de esta sección. **«Saturación»** = lo que se acumula **en el terreno** (§0.10). Son sistemas separados y solo comparten la palabra «acumular».

### 0.4 · Periodo de gracia

Un efecto elemental vivo **no se puede convertir durante un periodo de gracia corto**. Cubre dos casos: **dos efectos del mismo golpe pisándose** y **el rebote inmediato** dentro de una misma detonación. El anti-spam lo hacen los umbrales de saturación (§0.10): no se puede escupir efectos sobre un punto si cada uno cuesta 3 o 5 unidades.

> **Repetir la MISMA reacción la refresca, no se bloquea.** La gracia existe para impedir *cambiar* de efecto, no para castigar la insistencia. Sin esto se rompe la mecánica insignia de Ottorock: su cadena de 3 en Sujeción reproduce la combinación sellada en cada golpe, y los golpes 2 y 3 caerían sobre su propia zona en gracia y no harían nada.

### 0.5 · Composición de la arena

Proporción objetivo de celdas: **5 elementales / 5 neutras**. Cada **bioma** sesga esa mezcla hacia ciertos elementos: en una laguna la mayoría de celdas son de Agua. **Eso es lo que recompensa la versatilidad del equipo**: el jugador elige entre especializarse en un grupo de elementos, cubrir un triángulo entero, o variar.

> **Lo ambiental es un MODIFICADOR, no una clase de celda.** El segundo sistema ambiental (ceniza, bruma, nieve; [GDD §8.1](GDD.md)) **se pinta encima de cualquier celda**, sea elemental o neutra. El tablero se lee con **dos** categorías *(pasa algo aquí, o no)* y los dos sistemas ambientales pueden ocupar el 100 % de la arena sin quitarse sitio.
>
> **Las neutras no son terreno muerto:** se **cargan a 2 unidades** (§0.10), así que son **la mitad del tablero en potencia**. No existe la arena que te deja sin sistema: **te la fabricas**, solo que cuesta el doble que jugar terreno natural.

> Provocar reacciones con un elemento al que eres débil **no siempre es malo**: sigue siendo daño al enemigo. Solo cambia el gameplay: tus ataques pasan a llevar riesgo para ti.

### 0.6 · Anatomía universal del ataque

Todo ataque del juego (básicos, habilidades) tiene **tres tramos**, y de ahí sale qué se puede parar:

1. **Arranque: interrumpible.** Lo cortan los ticks de Rayo, los golpes y cualquier otra cosa. **Este tramo *es* el "te corté a media animación".**
2. **El golpe: comprometido.** Movimiento del golpe + hitbox. A partir de aquí **entra sí o sí**.
3. **Recuperación: cancelable.** El jugador puede **cortar la cola con otra habilidad** para encadenar más seguido.

> Tres consecuencias: le da a Rayo un **contrajuego real** (castiga a quien *empieza* ataques, no a quien ya está comprometido → la respuesta es entrar ya atacando y no dejar huecos) · la recuperación cancelable **es un sistema de combos que alimenta la captura**, porque encadenar más seguido sube el multiplicador de combo y con él el aturdimiento · y la embestida de Ottorock tiene exactamente esta forma (carga → embestida → deslizamiento).

### 0.7 · De quién es cada efecto

**Una sola frase genera los cinco casos: el efecto pertenece al elemento que MANDA; si no manda nadie, no es de nadie.**

| Situación | ¿Quién manda? | ¿Te afecta a ti? |
|---|---|---|
| Reacción sobre un elemento que superas | **tú** | **No**: R6 te respeta |
| Amplificación (tu propio elemento) | **tú** | **No**: es tu explosión otra vez |
| Reacción del terreno (te supera) | el terreno | **Sí**, sin bando |
| **Combinación** (cruzando triángulos) | **nadie** | **Sí** |
| Choque de elementos (§0.2b) | **nadie** | **Sí** |

**La diferencia entre las dos reacciones no es *dónde* pasan (las dos son sobre el terreno) sino *quién gana*, y eso lo decide el triángulo.** Ottorock (Mineral) golpea un parche de Rayo: Mineral › Rayo, manda lo suyo, la reacción es de Mineral y le respeta. El mismo Ottorock golpea un parche de Aire: Aire › Mineral, manda el suelo, sale la reacción de Aire y no le respeta. **Si tu elemento manda, lo controlas; si manda el del suelo, lo has despertado.**

> **R6 protege del ESTALLIDO, no de la ZONA.** Tu reacción no te golpea al nacer, pero si deja una zona que dura (incendio, nube de arena, gravedad), **esa zona no distingue a nadie** y te afecta si entras. Por eso las reacciones con zona son más peligrosas de usar que las instantáneas, aunque sean tuyas.
>
> **Por qué las combinaciones no tienen bando:** si te respetaran, ponerlas sería *siempre* bueno y no habría decisión. Es el riesgo propio lo que convierte **dónde** la pones en una jugada. Y sale del principio de base: el terreno es **un tercer jugador**, y un tercer jugador no toma partido. Regalo: puedes **montarte en tu propia corriente** o pelear dentro de tus propias mini explosiones.

### 0.8 · Las tres fuentes de efecto elemental

Fuera de estas tres **no hay efectos**, solo daño de tipo:
1. **El terreno:** 5 de cada 10 celdas (§0.5).
2. **El choque entre elementos** (§0.2b): en el aire, sin suelo de por medio.
3. **Lo que una criatura lleva encima:** la **roca sellada de Ottorock**, única excepción a R5.

> Consecuencia de balance: sobre suelo neutro el juego se queda en daño + kit, **sin verbos**. Eso convierte la **composición de la arena en una palanca de balance de primer orden**, y hace que el nodo de cristal y la roca sellada valgan mucho más de lo que parecen: son las dos formas de tener efectos donde el suelo no los da.

### 0.9 · Techo de efectos y esquivabilidad

**Máximo 2 efectos pegados a la vez**, y el techo **no es el objetivo**: el juego apunta a **uno o ninguno**. Si el jugador se posiciona bien, sale limpio. **Todo tiene que ser esquivable**, y el 30% del daño que aporta el terreno sale de **acertar** reacciones sobre el rival, no de desgaste ambiental inevitable. Si estar bajo un efecto es el estado normal, los efectos dejan de leerse como acontecimientos.

> **Regla:** *si un efecto no se ve en el cuerpo del afectado, no entra en §4.* Es lo que separa "el barro me está castigando" de "la esquiva de este juego a veces falla".

### 0.10 · Saturación del terreno (gobierna CUÁNDO ocurren las cinco salidas de §0.2)

> **El terreno acumula, no reacciona.** Un impacto elemental **no dispara nada**: deposita **saturación** en la mancha que pisa. Cuando la mancha se llena queda **cebada**, y el siguiente impacto la revienta, mucho más fuerte y mucho más lejos, **transforma el suelo**, y ese suelo **vuelve solo** al cabo de un rato.

**Por qué.** Con la mitad del suelo elemental, una reacción por impacto convierte el estado por defecto en *«siempre está pasando algo»*: estímulos difusos que **dejan de leerse como acontecimientos**. La solución no es quitar terreno: es **concentrar el mismo daño en menos eventos y más grandes**.

#### La escalera de umbrales

**`y` = una unidad de impacto** = la fuerza de impacto de un estallido de habilidad medio. Un básico aporta ≈ **0,3-0,5y**.

| Dónde cae | Umbral | Qué sale | Qué deja |
|---|---|---|---|
| **Tu mismo elemento** | **1y** | **amplificación**: instantánea, **sin cebado ni delay**; es el carril rápido | nada, pero su radio **se sale de la celda** y deposita saturación en las manchas vecinas |
| **Suelo neutro** | **2y** | la celda **se vuelve elemental** | una mancha de **una sola celda** |
| **Cruzando triángulos** | **3y** | **combinación**: rango y efecto potenciados | **suelo neutro** |
| **Dentro de tu triángulo** | **5y** | **reacción**: mucho más daño y mucho más rango | **el elemento subproducto** (§4) |

La escalera **ordena por frecuencia y por potencia en el mismo sentido**: lo que más sale es lo que menos hace. Las combinaciones son *transformaciones que duran* (§4) y son la **textura**; las reacciones son *explosiones* y son el **acontecimiento**. El ratio **1/2/3/5** es fijo; el objetivo es **un acontecimiento de terreno cada ~15 s**, cada uno ≈ **4-5 veces una habilidad media**.

**Se mide en fuerza de impacto, no en daño infligido.** Medido en daño, la velocidad de carga dependería del stat de daño de la especie y el terreno sería un chequeo de estadística. Medido en fuerza de impacto, que se autora por habilidad, **cargar rápido es una decisión de diseño**, no un efecto lateral.

#### La batería es la MANCHA

Acumula la **mancha entera** (las 3-6 celdas conectadas del mismo elemento de [ARENA §1b](ARENA.md)), no la celda suelta.
- **Acertar es fácil**, así que no hace falta agrandar las celdas.
- **La explosión ES la mancha**, así que el «mucho más rango» sale gratis y **con forma de ameba**, no de círculo: la geografía del tablero pasa de estética a táctica.
- **El cerco de la zona de combate CORTA las manchas.** Dos celdas vecinas del mismo elemento **no son la misma mancha si el muro pasa entre ellas**: *si no puedes ir andando de una celda a la otra sin salir de la arena, no son la misma mancha.* Consecuencia de diseño: **dónde cae el cerco cambia el tablero, no solo lo delimita**. Una mancha grande partida en dos son **dos bombas pequeñas en vez de una enorme**, así que colocar la zona ([ARENA §1](ARENA.md)) es una decisión de balance.

#### El medidor es uno, y se disputa

**Un medidor por mancha**, con dueño = el elemento que la está cargando. Un elemento distinto **la baja** en vez de subirla; al llegar a 0 **cambia de dueño**, y con él el umbral. Una sola barra: legible, y convierte el terreno en **objetivo disputado**, que es el «tercer jugador» con cara.

**Defender es mucho más barato que robar.** Al que va por 4y de 5 le falta 1y; al rival le cuestan 4y vaciar **más** 3y llenar. Negar terreno es el último recurso, no el reflejo.

**Gracia y luego decaimiento.** Tras el último golpe el medidor **se sostiene** durante un periodo de gracia (no se mueve salvo que alguien lo suba o lo baje) y **solo después** empieza a bajar. La gracia da tiempo a reposicionarse, encadenar y elegir cuándo seguir; el decaimiento impide que el tablero entero termine cebado.

#### Cebada, no automática

Al llenarse, la mancha **no estalla: queda cebada**, y detona con **el siguiente impacto elemental**. El jugador elige el momento y el terreno pasa a ser **remate de combo**, que es justo lo que pide §0.2 (*«un combo bueno consiste en encadenar básicos y habilidades apuntando a zonas elementales»*). **Guardarraíl:** si nadie la detona en **~5 s**, **se ventea**: fogonazo débil, sin efecto, medidor a cero.

#### La detonación se ENCADENA

La mancha **no revienta de golpe**: revienta **la celda que recibió el impacto** y la **onda avanza por vecindad** al resto de la mancha, dejando terreno transformado detrás. Es el mecanismo del incendio de §4, generalizado a los seis elementos.

- **El jugador APUNTA la onda.** Nace donde cae el golpe: detonar por el borde cercano la manda lejos de ti; por el lejano te la trae encima, pero alcanza al que huye. Decisión nueva **sin input nuevo**.
- **La onda ES la ventana de esquiva.** El tiempo para apartarse lo da **el viaje**, y se ve.
- **Golpea a cada criatura UNA sola vez.** Si no, quedarse en el centro de una mancha de 6 serían 6 impactos y la reacción dejaría de ser acontecimiento para ser bomba. **Se ve recorriendo, cobra una vez.**
- **La onda va más rápido que correr**, o se escapa hacia delante. Se esquiva **de lado o hacia atrás**: con decisión, no con velocidad.
- **Se encadenan las REACCIONES; las combinaciones se EXTIENDEN.** Una reacción es explosión: recorre. Una combinación es transformación: **ocupa la mancha**. La amplificación no encadena nada.

#### Qué deja detrás

**Toda detonación cambia el tablero: la mancha adopta el subproducto y el ANILLO de celdas que la rodea se mancha con él.** Cuando subproducto y mancha coinciden, **la mancha se expande**: no existe ningún caso nulo.

- **Reacciones** → el subproducto de §4. **Combinaciones** → **neutro** (nadie ganó; el suelo se gastó).
- **Una detonación NO deposita saturación.** Es el cobro, no una entrada. Sin esta regla el anillo que acaba de crear se cargaría con el propio estallido y habría **dominó infinito**.
- **Una detonación borra los efectos vivos en su alcance**: el suelo que los sostenía dejó de ser ese elemento.

#### Reversión con refresco

La celda transformada **vuelve a su estado horneado** pasado un cooldown, pero **si durante ese cooldown se pelea encima, el cooldown se refresca**. **El suelo que se disputa se queda cambiado; el que se abandona, se cura.** El combate deja cicatriz donde de verdad ocurrió, y la arena **no se agota nunca**.

- **Refresca CARGAR, no solo detonar.** Basta con **mover el medidor** (subir el tuyo o bajar el del rival) para reiniciar el reloj, así una mancha que estás cargando no se revierte a mitad de la carga.
- **El doble de tiempo en las celdas que nacieron NEUTRAS.** Fabricarte tablero es un proceso de **dos pasos** (convertir la neutra a 2y y *después* cargarla a 3y o 5y), y con el mismo reloj se deshacería a mitad del proceso. El terreno natural sigue siendo mejor (más barato, mancha más grande), pero el fabricado **da tiempo a usarlo**.

#### El golpe que no llena habla, pero no hace

Cada impacto que no completa la mancha produce **un destello mudo de la mezcla correcta** (el vapor sisea, la hierba chisporrotea) **sin daño, sin estado y sin zona**, que **crece con la saturación**: casi imperceptible al principio, escandaloso cuando falta poco. Hace dos trabajos:
1. **Enseña las 15 parejas.** Con las mezclas hechas raras, es como el jugador aprende qué produce cada combinación.
2. **Es el telegraph.** Una mancha a punto de reventar **se ve y se oye desde lejos**, y por eso la detonación puede permitirse ser enorme sin ser injusta.

**La saturación visible no es pulido, es la mitad de la mecánica.** Si el hexágono no se ve llenarse, el jugador no percibe carga: percibe que golpear el suelo dejó de hacer nada. Por eso hay **dos señales para dos distancias**:
- **De cerca, el hexágono se llena:** crece desde el centro y se enciende con el color del elemento que lo carga. **Crecer** es lo que se lee como medidor.
- **De lejos, la columna:** una barra vertical en el centro de la mancha cuya **altura es el medidor**. Permite decidir *«voy a esa zona»* sin estar encima.
- **Cebada, las dos laten en blanco.** Ese latido es lo que hace justa una explosión enorme.

#### Las tres excepciones instantáneas

No pasan por saturación y resuelven en el acto (más flojas que una detonación, a cambio de ser inmediatas):
1. **Los choques en el aire** (§0.2b): no hay suelo donde acumular.
2. **La roca sellada de Ottorock** (§5): es su mecánica insignia y la única fuente portátil de efecto (§0.8).
3. **Las minas encendidas de Pyrecko** (§5): el campo de minas sigue siendo un campo de minas.

---

## 1. Las tres capas (no confundirlas)

**① Terreno: el tablero, y tiene historia.**
Al entrar en combate se **acota una zona** dividida en **celdas hexagonales**. Cada celda tiene **o un elemento, o nada** (lo ambiental es un modificador encima, §0.5), horneado desde el bioma. **El terreno arranca en un estado y ese estado se mantiene mientras nadie lo trabaje**; una detonación lo transforma, y la reversión con refresco lo devuelve (§0.10). Los 6 terrenos elementales:

| Elemento | Terreno |
|---|---|
| **Fuego** | roca volcánica |
| **Agua** | río |
| **Naturaleza** | pasto / hierba alta |
| **Mineral** | zona de menas (minerales incrustados en la roca) |
| **Rayo** | piedra electrificada (como la volcánica, pero de electricidad) |
| **Aire** | corrientes de aire (géiseres que expulsan aire en vez de agua) |

**Los 6 elementos tienen terreno propio a propósito:** sin terreno de Aire, la dominancia Rayo › Aire sería inalcanzable. Las celdas **neutras** no reaccionan, pero **se cargan a 2y y se vuelven elementales** (§0.10). El mundo es continuo: la rejilla es un **overlay** que marca el elemento predominante de cada sección, con una **zona de combate enmarcada** ([ARENA.md](ARENA.md)).
> *Aparte y sin relación con la tabla de tipos:* los **fenómenos físicos y climáticos** (polvo al golpear el suelo, nieve, lluvia, viento) son el **segundo sistema ambiental** del [GDD §8.1](GDD.md): física que las habilidades explotan, sin pasar por elementos. Puede nevar sin que exista un tipo Hielo. Las dos capas conviven en la misma arena sin tocarse.

**② Reacciones: temporales, encima del terreno.**
Lo que produce un impacto elemental sobre terreno de otro elemento. **Duran poco y se van.**

**③ Marcas de especie: independientes de todo lo anterior.**
Cosas que siembra una especie concreta y que **no son terreno ni reacción**: el **«combustible»** de Pyrecko, los tornados de Mantagale. Reglas propias en §5.

## 3. Reglas de resolución

**R1 · Siempre contra el terreno.** Un impacto resuelve contra el **elemento del terreno de ahora mismo**, nunca contra un efecto que siga activo encima. *(Ottorock convierte un trozo de río en **arenas movedizas**; dos segundos después Pyrecko dispara ahí: el fuego se encuentra el **río**, no las arenas → el **géiser** del terreno, que le eleva a él también.)* Esto impide la explosión combinatoria: la matriz es 6×6 y punto. Si una detonación convirtió el pasto en ceniza, el siguiente impacto resuelve contra **Mineral**: **lo que cambia es la casilla, no la tabla**. **También vale para los choques en el aire** (§0.2b): resuelven una vez y el suelo de debajo no vuelve a contar.

**R2 · La duración es proporcional al impacto.** Cuanto más fuerte el golpe que irradia el elemento, más dura la reacción. Premia meter la habilidad pesada en el sitio correcto en vez de escupir habilidades baratas.

**R3 · Atacar una zona viva la DRENA.** Golpear **dentro** de una zona viva con **otro elemento** le **drena duración** (el mismo tira y afloja del medidor de §0.10). **Ese impacto no deposita saturación**: gastas el golpe en limpiar en vez de en cargar, y así el counterplay conserva su coste. Tiene que caer **dentro**, no rozarla: el propósito es puntería, no proximidad. Además, **una detonación borra los efectos vivos en su alcance** sin pagar nada.

**R4 · Regla direccional.** La misma pareja resuelve distinto según quién entra. Entra el **predador** → **dominancia** estándar (R6 protege a su bando). Entra la **presa** → **respuesta del terreno**: se dispara la **misma reacción** de la fila del predador, plena pero **sin bando** (excepción de R6). No puedes quemar lo mojado: **lo mojado te vaporiza**; apagar lo ardiente sigue siendo la dominancia normal del Agua. *(En espejos del mismo elemento, el terreno se vuelve el tercer jugador: ambos pueden provocarlo y ambos deben respetarlo.)*

**R5 · Superficie neutra → nada.** Sin elemento de terreno no hay reacción en el momento del golpe. **Única excepción: la roca sellada de Ottorock** (§5), que lleva su reacción ya resuelta y por eso funciona también sobre neutro.

**R6 · El estallido no golpea a quien lo provoca** (ni a su bando). El **efecto de zona** resultante sí es neutral: afecta a quien entre, de cualquier bando. *(Sin esto, Ottorock se electrocuta a sus propios pies al agarrar sobre Rayo, y un Ottorock salvaje se regalaría el noqueo solo.)* **Dos casos SIN bando ni en el estallido:** la **respuesta del terreno** y **todas las combinaciones y choques**. El cuadro completo está en §0.7.

## 4. Las 15 parejas: 6 reacciones + 9 combinaciones

Las 15 se derivan de los verbos de §0.1, y cada una tiene **una función que ninguna otra tiene**.

**Los dos triángulos:** ① **Naturaleza › Agua › Fuego › Naturaleza** · ② **Mineral › Rayo › Aire › Mineral**. Dentro de un triángulo siempre hay **reacción**: es *tuya* si entra el predador y *del terreno, sin bando*, si entra la presa (R4). Entre triángulos, siempre **combinación**. Y sobre tu **mismo** elemento, **amplificación** (§0.2).

Cada elemento tiene **2 relaciones de triángulo** (a quién supera, quién lo supera) **+ una firma** que explica su fila entera:

| Elemento | Firma | Elemento | Firma |
|---|---|---|---|
| **Fuego** | quema; el aire lo aviva | **Naturaleza** | absorbe y propaga |
| **Agua** | conduce, apaga, embarra | **Mineral** | endurece, cristaliza |
| **Rayo** | electrifica lo que toca | **Aire** | agita, aviva y erosiona |

### Las 6 reacciones: derivadas del verbo del dominante

Cada una hace **daño del elemento que manda** (≈ una habilidad promedio) **+ su verbo**. Se disparan en los dos sentidos (R4): si entra el predador es *suya* y no le toca (R6); si entra la presa, es **del terreno** y no tiene bando.

| Pareja | Reacción | Qué hace = el verbo del dominante |
|---|---|---|
| **Naturaleza › Agua** | **Crecimiento explosivo**: las raíces beben el charco | **Atrapa**: quien pilla no se mueve, pero **puede atacar y bloquear**. **Solo suelo**: no alcanza a quien vuela, que es la salida natural. Es la mejor herramienta de **setup** del sistema: lo único que fija a alguien dentro de la zona de otro. |
| **Agua › Fuego** | **Géiser**: el agua apaga el fuego | **Eleva**: agua caliente y humo que te suben. Es la reacción que **abre el juego aéreo**, y lo que permite que un melee de Agua o de Fuego alcance a un volador. Al caer, la inercia de la corriente te sigue arrastrando. |
| **Fuego › Naturaleza** | **Incendio** | **Se propaga**: prende y **se extiende poco a poco, solo por terreno de Naturaleza** (el parche define hasta dónde llega) hasta apagarse solo. Quien lo toca se lleva la quemadura puesta. |
| **Mineral › Rayo** | **Energía geo**: la roca hace de toma de tierra | **Pesa**: zona de **más gravedad** que imanta hacia abajo. No impide despegar: hace que despegar **valga menos**, acortando la suspensión. Ralentiza y arranca del aire lo que vuele → es la reacción **anti-aérea** del juego. |
| **Rayo › Aire** | **Descarga de tormenta** | **Corta el combo**: ticks por reloj que solo muerden en el **arranque** de un ataque (§0.6). No te quita vida: te quita la captura, porque sin cadena el aturdimiento decae. |
| **Aire › Mineral** | **Tormenta de arena**: el viento erosiona la roca | **Nubeliza**: levanta la roca en una nube que cubre bastante más área de la que golpeó y **tapa la visión**; araña porque está hecha de roca pulverizada. **No ralentiza** (eso es el verbo de Mineral, y aquí no manda). Es la que **se come Ottorock**: sobre corrientes de aire, sus propios ataques le dejan ciego. |

#### Qué terreno deja cada reacción

Al detonar (§0.10), la mancha adopta el **subproducto** y el **anillo** que la rodea se mancha con él. Cada resultado sale del **fenómeno físico**:

| Reacción | Deja terreno de | Por qué |
|---|---|---|
| **Naturaleza › Agua**: crecimiento explosivo | **Naturaleza** | las raíces se beben el charco y se quedan |
| **Agua › Fuego**: géiser | **Aire** | el vapor |
| **Fuego › Naturaleza**: incendio | **Mineral** | la ceniza |
| **Mineral › Rayo**: energía geo | **Fuego** | la roca se sobrecalienta con la carga |
| **Rayo › Aire**: descarga de tormenta | **Agua** | la lluvia después de la tormenta |
| **Aire › Mineral**: tormenta de arena | **Rayo** | la arena en suspensión **se carga de estática** *(las tormentas de polvo generan campos eléctricos por fricción)* |

> **La paridad es una regla de balance.** Los seis elementos aparecen **exactamente una vez** como resultado, así que la tabla es una **permutación**: **ningún elemento puede extinguirse del tablero ni comérselo**. Con la paridad, **el tablero cambia sin derivar**.
>
> **Las 9 combinaciones dejan NEUTRO.** No manda nadie, así que nadie reclama el suelo: se gastó.
>
> **Ningún caso es nulo, gracias al anillo.** *Crecimiento explosivo* deja Naturaleza, y una de sus dos direcciones (entra Agua sobre terreno de Naturaleza) ocurre sobre suelo **que ya es Naturaleza**: la mancha no cambia, pero **el anillo sí**, así que su función ahí es **expandir la mancha**. La única reacción llamada *crecimiento* es la única que hace crecer terreno. Y tiene una lectura preciosa: **una criatura de Agua peleando sobre hierba riega el prado del rival**, justo la pareja donde ella pierde.

> **Gatillo inverso:** estas 6 filas también se disparan **al revés**: entra la presa sobre el terreno de su predador y el terreno responde con la **misma reacción, sin bando** (R4/R6). *Fuego sobre río → **géiser** con daño de Agua, que eleva también al que disparó; Mineral sobre corrientes de aire → **tormenta de arena**, que ciega también a Ottorock.*

### Las 9 combinaciones: derivadas de los DOS verbos

Cross-triángulo: **ninguno domina**, así que el daño es **neutro** y **no tienen bando** (te afectan a ti también, §0.7). Son **transformaciones**: zonas que duran, no estallidos. Cada una tiene **una función distinta de las otras ocho y de las 6 reacciones**.

| Pareja | Nombre | Función única | Qué hace |
|---|---|---|---|
| **Mineral + Agua** | **Arenas movedizas** | **Anti-fuga** | Peso sobre corriente = fluido denso que **te traga**. Dentro, el dash y el salto salen **enteros y a distancia normal** (recortar la distancia se leería como un bug, no como barro), pero **al aterrizar te hundes y la recuperación se alarga**. Escapar funciona; escapar te deja vendido. |
| **Mineral + Fuego** | **Colada** | **La única zona que AVANZA** | El fuego con peso encima no salta de mata en mata: **se vierte**. Nace en el impacto y repta despacio en la dirección del golpe, comiéndose el suelo jugable, hasta solidificar y morir. Quien la toca sale con roca fundida que sigue quemando mientras se enfría. **Al llegar a una celda de Agua se apaga**: es su muerte, y es la respuesta del rival. Es lo único del sistema que **devuelve distancia**. Sin bando y avanza = **te persigue a ti también**: la más exigente de usar. |
| **Mineral + Naturaleza** | **Cristalización** | **Te quita el CANCEL** | Entrelazar deja actuar; Peso dice que lo que arranca no se detiene. Peleas, bloqueas y te mueves **a velocidad normal**, pero **pagas cada acción entera**: no puedes cortar la recuperación para encadenar (§0.6). No te quita daño, **te quita captura** (sin cadena apretada no sube el combo). Espejo exacto de Rayo: *él te corta el golpe, esta no te deja cortarlo a ti*. |
| **Rayo + Agua** ★ | **Corriente** *(agua electrificada)* | **El único arrastre** | Cinta transportadora eléctrica: lámina **direccional** (la dirección la fija el vector de tu golpe aplanado al suelo, así que **tú la apuntas**) que lleva a lo que pise, con inercia decreciente al salir. Encima, el tick de Rayo mordiendo el **arranque de tus ataques**, nunca tu movimiento. Puedes salir siempre, pero gastas la huida en irte en vez de en contraatacar. |
| **Rayo + Fuego** | **Mini explosiones** | **Amplifica** *(la única que sube el daño ajeno)* | Un tick de más (Rayo) + el daño extendido en el tiempo (Fuego): **cada impacto dentro de la zona lo sigue una mini explosión** que repite un porcentaje del golpe original. Junto a la niebla, **una de las dos zonas que QUIERES pisar**. Sin bando, así que quien gane el intercambio gana más. **El eco no dispara reacciones**: ya *es* la combinación, y encadenar rompería R1. |
| **Rayo + Naturaleza** | **Amarra** | **Anti-kiting** | Rayo pone su **metrónomo** y Naturaleza el **anclaje**: cada tantos ticks, un **tirón** te devuelve al punto donde te alcanzó, cada víctima con su propia estaca. Entre tirón y tirón corres normal, y tu ofensiva no se toca; es *luchar por soltarte*. Discreto y no continuo **a propósito**: una fuerza constante contra tu input se lee como mandos rotos, un tirón se lee como acontecimiento. Cuanto más aguantes fuera, más débil el siguiente, hasta que la correa rompe. |
| **Aire + Agua** | **Niebla** | **Desinforma** | Capa baja, ancha y opaca: **a través de ella no se puede FIJAR objetivo**, y esconde igual a quien está dentro que a quien mira desde fuera. **La otra zona que quieres pisar.** Los proyectiles ya guiados no se pierden: se corta la **adquisición**, no el guiado. |
| **Aire + Fuego** | **Mecha** | **Temporizador** *(la única que no hace nada mientras dura)* | El fuego nubelizado no arde: **se acumula**. La nube crece y se satura toda su vida sin dañar, y **deflagra de golpe en todo el volumen al final**, aire incluido. Por eso es **la única que alcanza a un volador**, pero como evento puntual del que puedes irte. **Detona solo por su reloj**, nunca por golpes, o Pyrecko tendría un detonador gratis sobre su kit de marcas. Aquí lo peligroso es el final. |
| **Aire + Naturaleza** | **Velo de esporas** | **Interdicta lo lanzado** | La maraña deja el suelo y queda **colgando**. Se atraviesa andando, saltando o dasheando sin efecto: lo que se come es **el proyectil elemental que la cruza**, que **detona ahí**. El velo se comporta como un elemento estacionario para la regla de choques (§0.2b). Es el **contrajuego natural contra un Zonificador**. Solo detona **lo elemental**: disparar sin cargar elemento la atraviesa. |

> **El menú de Ottorock.** Las tres de Mineral son las que él sella en Sujeción, así que **el jugador elige entre ellas**. Por eso tienen **radio y duración muy distintos**: cristalización pequeña y larga · colada estrecha y direccional · arenas amplia y corta. Así la elección es real y su mecánica insignia no es decorativa.
>
> **Aturdimiento ≠ stun.** El "aturdimiento" llena la **barra de captura** ([GDD §8.2](GDD.md)); no es un control que te quite el mando. Los CC reales (atrapar, arrastrar, tirón) están dichos por su nombre.

## 5. Excepciones por especie

**Ottorock: combinación sellada portátil** ([ficha](CREATURE-OTTOROCK.md)). Es su mecánica insignia en Sujeción: al **agarrar** el terreno se resuelve **una vez** `Mineral + elemento del terreno` y el resultado queda **sellado en el trozo**. A partir de ahí **cada impacto reproduce esa misma mezcla**, en cualquier superficie, **incluida la neutra**. Es la **única fuente portátil de efecto** del juego (§0.8). Elige su combinación eligiendo **dónde agarra**: río → **arenas movedizas** · roca volcánica → **colada** · pasto → **cristalización**.
- **Instantánea: no pasa por saturación.** Cada impacto reproduce su combinación **en el acto**, sin cargar ninguna mancha. Es más floja que una detonación, y a cambio es inmediata.
- Terreno neutro al agarrar → **roca pura**: el trozo reacciona como **Mineral** contra el terreno donde impacte.
- **Solo se sella lo que es COMBINACIÓN.** Si el agarre dispara una **reacción** (sobre Rayo, que él domina; sobre Aire, que le domina) o una **amplificación** (sobre sus propias menas), esta se consume ahí y el trozo queda **puro**.
- Al volver a Libre (romper o lanzar la roca) **pierde la combinación**.

**Pyrecko: el «combustible»** ([ficha](CREATURE-PYRECKO.md)). Marca de especie, **no** elemental:
- **Inerte**: solo su Signature la **enciende**, y encenderla **no la detona: la arma**. Una marca encendida **estalla en Fuego al más mínimo contacto**: **daño de Fuego** a quien la toca, y **el estallido irradia Fuego contra su celda** → ese fuego resuelve normalmente contra el terreno (sobre terreno de Agua → **respuesta del terreno**: el **géiser**, sin bando y con daño **de Agua**, que le eleva a él si está dentro). El campo encendido es un **campo de minas**: *no pisarlo* es el counterplay.
- **Su kit no aplica ningún efecto por sí solo**: todo lo que produce es **daño de Fuego + entrega de Fuego**; los efectos nacen solo de la mezcla.
- **Solo desaparece al expirar (10 s) o al estallar**, y estalla **una sola vez**: encendida es una **mina de un uso, que se consume al contacto**, así que el campo se gasta según lo pisan.
- **La mina encendida es instantánea: no pasa por saturación.** Al pisarla, el estallido resuelve ahí mismo contra el terreno. Si depositara saturación dejaría de ser una amenaza para convertirse en una máquina de cargar tablero.
- **Pertenece al lugar, no al material:** si Ottorock arranca el trozo de terreno donde hay combustible, se lleva **el terreno, no la marca**.

**Mantagale: tornados que empujan y detonan por colisión** ([ficha](CREATURE-MANTAGALE.md) §C).
- Sus **tornados empujan durante todo su recorrido** (negación de área), pero son **inertes en lo elemental**: no tocan el terreno ni disparan reacción al viajar. Como el combustible de Pyrecko, son una marca de especie con su propia lógica.
- La reacción ocurre **al reventar**, y revientan por **colisión de fuerzas opuestas**: el tornado empuja, su Ciclón **atrae**, y el choque los detona. La reacción resuelve contra el terreno **del punto de colisión**, con el enemigo arrastrado allí dentro.
- El Ciclón **arrastra al enemigo por el suelo** hasta el centro (no lo teletransporta): al volver, **atraviesa los tornados sembrados por el camino**.
- Su fuerza **depende del terreno de la arena**: es la que "juega el tablero". Sobre **neutro** no consigue nada. Sobre **Rayo**, sus tornados al reventar provocan la **descarga de tormenta del terreno, sin bando**, que además le corta el **arranque** de sus propios ataques: es su tablero de **alto riesgo**.
- **Peces espíritu: el combustible de su vuelo.** Cada detonación que ella provoca hace nacer **5 peces de golpe**; viajan hasta ella y **se consumen uno a uno, rápido**. **El pez no es altura: es sustentación.** Mantagale **planea siempre**, y los peces **invierten el signo del planeo**: con peces asciende, sin peces desciende. En el techo no se desperdician: compran **permanencia**, así que **acertarlo todo la mantiene arriba**. **Una detonación = un vuelo**: su altura es un **arco**, sube de golpe y baja preparando la siguiente.
- **Los peces cuentan como PROYECTILES:** un **ataque elemental** los destruye **en tránsito**; los neutros no. Su altura es **un recurso atacable**, y el **velo de esporas** se los mata, incluido el suyo propio. **Los 5 viajan por curvas distintas**, así que no se pueden reventar de una pasada: matarle 2 = sube 3 escalones. El contrajuego es un **gradiente**, y su ventana es **el viaje** (consumido = intocable).
- **El choque que revienta un pez NO le repone otro:** la mezcla que nace ahí (§0.2b) **no es de nadie** (§0.7). El que dispara paga un fogonazo **imparcial** por cada pez.
- **Solo le dan peces las detonaciones que provoca ELLA**, no las de sus compañeros.
- **Sus ráfagas (básico) son NEUTRAS sin potenciar:** solo son **entrega de Aire** cuando la Flex las potencia. Si no, el velo de esporas que ella misma genera sobre pasto le detonaría sus propios básicos encima.

## 6. Legibilidad
- **Reacción = ruidosa (explosión) · combinación = suave (transformación) · respuesta del terreno = el VFX de su reacción espejo con matiz de "provocada"** (arranca desde el terreno hacia el atacante, no desde el impacto hacia fuera). Los únicos "no pasa nada" son el **suelo neutro** y, solo en cuanto a efecto, la **amplificación**, que sí pega.
- **Una reacción tiene que separarse en luminancia del terreno donde nace.** Es una restricción dura, crítica en las combinaciones, que no tienen fogonazo.
- **Si un efecto no se ve en el cuerpo del afectado, no entra en §4** (§0.9).
- **Toda reacción avisa antes de existir:** el aviso es **la mancha llenándose a la vista** y luego **latiendo cebada** (§0.10). Dura todo lo que tarde el combate en cargarla, y por eso la detonación puede permitirse ser enorme sin ser injusta.
- **La saturación de la celda ES interfaz, no decoración.** El hexágono tiene que **verse llenar** y la mancha cebada tiene que **verse y oírse desde lejos**.
- **El destello mudo crece con la carga** (§0.10): es a la vez **el tutorial permanente de las 15 parejas** y **el telegraph**.
- **La onda de la detonación se lee como frente que viaja**, no como fogonazo: es lo que la hace esquivable **de lado**, y lo que comunica que nació donde cayó el golpe.
- **El terreno transformado se distingue del horneado** (un borde o un tono ligeramente distinto que **se apaga al revertir**), o el jugador no sabría que la ceniza de esa mancha la hizo él.
- Los efectos **se dispersan solos** (R2) y el terreno vuelve: **el mundo cambia, pero nunca queda pintado.**
