# Plan de financiación — DOB Studios como publisher de Tatemis3D

> 🔒 **PRIVADO de Arzpal** — este documento NO se comparte con DOB ni se incluye en materiales de pitch. Es la preparación, no la propuesta.
> **Objetivo:** DOB Studios financia Tatemis como **publisher** (adelanto recuperable + revenue share + sello «DOB Studios presenta»); **la IP y el crédito creativo quedan de Arzpal**. Ventana de pitch: **sept–oct 2026**, con la demo del gate en la mano.
> **Tesis del pitch en una frase:** *"Publicaste el juego donde CUIDAS monstruos elementales; te propongo publicar el juego donde ERES uno. Géneros opuestos, misma audiencia temática, mismo sello: DOB Studios, la casa peruana de los monstruos elementales."*

## 0. Contexto (27-07-2026)

- Contrato actual: locación de servicios **sin exclusividad**, PI acotada a *"en el marco de este contrato"* (cláusula Novena), vence **23-09-2026**, renovable. Precedente: 2 rev-shares propios tolerados por la empresa.
- DOB: 2 proyectos; su **early access sale en ~2 meses** (juego de guardería/cuidado de monstruos elementales — cozy, sin combate). Dueño: fan, pro-iniciativas, misión "levantar la industria peruana".
- Tatemis: Fase 0–1 de código cerradas; **gate sin responder** (falta terreno + IA). Diseño documentado y profundo. Sin arte (mayor carencia).

## 1. Fase 0 — Blindaje (ESTA SEMANA, antes que nada)

1. **Disciplina de cuentas** *(aclarado 27-07: todo ya es personal salvo una excepción)*: desde hoy, Tatemis — y en especial todo lo relativo a este plan/pitch — se trabaja **solo en la cuenta personal de Claude**; la cuenta development@dobstudios.com no se usa más para el proyecto (riesgos: "recursos de la empresa" que difuminan el marco del contrato, buzón bajo control de DOB con el historial del pitch dentro, y pérdida de acceso si termina el contrato). Coste de cambio: cero — todo el contexto vive en el repo. Además: **borrar del historial de la cuenta de empresa las conversaciones de Tatemis** (en especial las que toquen plan/contrato/negociación — quien controle el buzón @dobstudios.com puede entrar a esa cuenta con el código de login por correo) · **verificar con `git remote -v` que el repo privado está en cuenta personal**, no en una org de DOB · confirmar que el asiento de **Unity** de Tatemis es el personal. *(La prueba de autoría no depende de Claude: vive en git + INDECOPI.)*
2. **Registrar la obra en INDECOPI** (Dirección de Derecho de Autor): software + documento de diseño. Barato, da **fecha cierta y titularidad documentada** antes de enseñar nada.
3. **Higiene permanente**: Tatemis jamás aparece en un Informe de Avance · ni una línea de código cruzada entre Tatemis y proyectos DOB (en ninguna dirección) · conservar historial de git como evidencia de desarrollo personal.
4. **Higiene reforzada por el solape temático** (trabajo en su juego de monstruos elementales como developer): separación estricta de taxonomías, nombres, tablas y diseños entre ambos juegos; no discutir diseño de Tatemis por canales de la empresa.
5. Agendar (no ejecutar aún) **revisión con abogado peruano** para el momento de firmar. Releer los 2 contratos rev-share propios: son la plantilla cultural que el dueño ya firma.

## 2. Fase 1 — La demo que responde el gate (ago → mediados de sept)

Alcance mínimo y suficiente (6–8 fines de semana, en solitario):
- **Grids de terreno** según [docs/ARENA.md](docs/ARENA.md) + reescritura del sistema elemental viejo según [docs/ELEMENTAL-REACTIONS.md](docs/ELEMENTAL-REACTIONS.md) §7 (con `sinBando`/respuestas del terreno) + parches de arena (charco, matorral, veta, lava) + **nodo de cristal** (el momento ★ *agua electrificada*).
- **IA mínima con FSM legible** (norte ya dictado: que se sienta que razona): acecho → telegraph → ataque → reposición. Un solo arquetipo de enemigo bien hecho basta.
- Pulido final de feel sobre lo ya existente (hitstop/shake/noqueo ya están).
- **Guion de demo de 10 minutos**: 1) locomoción y feel de golpeo · 2) combate real vs IA (leer, esquivar, castigar) · 3) dominancia ruidosa sobre terreno · 4) el ★ agua electrificada con el nodo · 5) una **respuesta del terreno** ("hasta el elemento débil juega el tablero") · 6) cierre con visión: matriz 6×5=30, brújula de arquetipos, pentágono (la `gdd.html` ya está presentable).

## 3. Fase 2 — El paquete de pitch (2ª mitad de sept)

- **La demo** (build con mando, 10 min guionizados).
- **`gdd.html`** como pitch visual (ya regenerada).
- **One-pager de negocio**: qué pido, qué ofrezco, estructura (§5), y el slide del solape (§6).
- Ensayo del pitch en voz alta, 2 veces mínimo.

## 4. Secuencia y ventana (la parte política)

1. **Renovar el contrato de servicios con normalidad el 23-09**, sin mezclarlo con Tatemis. ⚠️ **Regla de oro: Tatemis JAMÁS entra al Anexo I / Plan de Trabajo** — cualquier acuerdo va en **contrato separado** (si entra al anexo, la cláusula Novena se lo traga).
2. **Pitch DESPUÉS del lanzamiento de su early access** (atención y caja libres; su equipo — arte incluido — queda con capacidad). Condición doble para agendar la reunión: demo lista **Y** su EA lanzado (o con fecha inminente clara).
3. Si su EA va **bien** → argumento extra: "la audiencia de monstruos elementales ya está validada; déjame servirle el juego de acción". Si va **mal** → posponer el ask de caja y reformular a aporte **en especie** (horas de artista) o esperar trimestre.

## 5. El deal que propongo (term sheet borrador — números ajustables a su realidad)

1. **IP**: 100% de Arzpal (registrada en INDECOPI). Innegociable.
2. **Licencia de publicación** exclusiva, por **plataformas y plazo definidos** (5–7 años), con **reversión** a Arzpal al vencer o si se incumplen aportes.
3. **Adelanto recuperable por tramos/hitos**, mixto: caja (estipendio mensual ~S/ 2 500–3 500 × 9–12 meses hasta el vertical slice) + **especie valorada a precio justo** (artista part-time del equipo post-EA, QA puntual, algo de audio).
4. **Recoup**: DOB recupera el 100% del adelanto (caja + especie) desde el revenue neto (definir base: neto tras el corte de la tienda e impuestos).
5. **Split post-recoup**: **60/40 a 65/35 a favor de Arzpal** (línea roja: no bajar de 55/45).
6. **Sello y marketing**: «DOB Studios presenta» + compromiso mínimo de marketing/publishing definido por escrito.
7. **Crédito y control creativo**: "un juego de **Arzpal**"; decisiones creativas finales mías; feedback de DOB por hitos, no dirección.
8. **Cuenta de tienda y transparencia**: preferencia — página/cuenta de Steam mía y yo liquido su share; aceptable — cuenta DOB con reporting mensual contractual y transferencia de página en la reversión.
9. **Secuelas/DLC**: derecho de **primera negociación** para DOB (no de veto, no automático).
10. **Contrato independiente** de la locación de servicios; revisión de abogado antes de firmar.

**Qué gana DOB (así se vende):** 2ª IP original peruana en catálogo · estrena línea de negocio publisher (encaja con su misión) · upside con riesgo acotado (recoup first) · usa capacidad ociosa del equipo post-EA · narrativa "casa peruana de los monstruos elementales".

## 6. El solape conceptual — de riesgo a argumento

- **Posicionamiento (decirlo proactivamente, sin esperar la objeción):** su juego = *cuidar* monstruos elementales (cozy/management, sin combate); Tatemis = *ser* el monstruo (acción en tiempo real). **Géneros opuestos, cero canibalización de gameplay, audiencia temática adyacente** (el fan de criaturas elementales juega ambos). Juntos forman **identidad de sello**, no competencia: cross-promo natural (wishlist del EA → Tatemis y viceversa).
- "Monstruos elementales" es un género establecido (Pokémon, Temtem…): nadie es dueño del tropo; la expresión concreta de Tatemis (sistema de 2 triángulos, terreno fijo, reacciones, arquetipos) es original, documentada y con fechas (git + INDECOPI).
- **Universos separados**: nada de fusionar IPs ni "mundo compartido" (mezclaría titularidades). Como máximo, guiños de marketing/cross-promo acordados por escrito.

## 7. Escenarios y líneas rojas

| Escenario | Respuesta preparada |
|---|---|
| "Mejor lo hacemos juego de la empresa y te doy royalties" | No cedo IP. Contra-oferta: subo su % post-recoup o amplío la licencia. Si insiste → gracias, sigo solo (BATNA). |
| "Métele Tatemis a tu contrato / al Anexo" | Nunca. Contrato separado o no hay trato. |
| "50/50 desde el sol uno, sin recoup" | El recoup-first le protege a él también (recupera antes de repartir); es el estándar. |
| Especie valorada cara (horas de equipo infladas) | Tarifas de mercado por escrito; la especie cuenta para el recoup, no cambia el split. |
| "No, gracias" | Agradecer, rendir igual de bien en el trabajo, seguir construyendo. La demo sirve para **publishers externos** (indie/LATAM) — el plan no muere con este no. |

**Líneas rojas (walk-away):** cesión de IP · licencia perpetua o sin reversión · Tatemis dentro del Anexo I · pérdida del control creativo o del crédito "Arzpal" · split post-recoup peor que 55/45 · cualquier exclusividad sobre mi persona (el deal ata al juego, no a mí).

## 8. Checklist antes de firmar

- [ ] INDECOPI registrado (Fase 0) · [ ] cuentas migradas · [ ] abogado revisó el contrato de publishing · [ ] base de "revenue neto" definida con ejemplos numéricos · [ ] hitos y valoración de especie por escrito · [ ] cláusula de reversión y de incumplimiento de aportes · [ ] reporting y auditoría · [ ] el contrato de servicios renovado quedó intacto y aparte.
