#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Autoría del contenido del GATE: Ottorock (modo Libre) según su ficha, como assets reales.
    /// Kit Libre: Movilidad = Embestida Garra · Flex = Mordisco Ferroso · básico melee.
    /// La Signature (Cambio de estado Libre↔Sujeción) es post-gate → se omite (queda null).
    /// La cadena de 5 del básico necesita un contador de combo que aún no existe → por ahora
    /// un golpe spameable. Números = placeholders de caja gris, se afinan en playtest.
    public static class OttorockContent
    {
        public const string Dir = "Assets/Tatemis/Examples";

        [MenuItem("Tatemis/Contenido/Generar Ottorock (Libre)")]
        static void GenerateMenu() => Generate();

        public static CreatureSpeciesData Generate()
        {
            EnsureDir();
            EditorUtility.SetDirty(CreateOrLoad<TypeChart>($"{Dir}/TypeChart.asset"));

            // Básicos — CADENA DE 5 (sin coste). Los cuatro primeros son NEUTROS por regla de diseño
            // (GDD §7): el básico es la columna vertebral del combate y llevar elemento haría imposible
            // pelear contra quien te supera. El quinto es el finisher: sigue pegando neutro, pero
            // ESTALLA en Mineral — la recompensa por sostener la cadena entera.
            var basicos = new List<AbilityData>
            {
                // El step-in sube junto con el empuje (ver BasicLink): el golpe manda más al enemigo,
                // pero tú te acercas más, así que la cadena sigue cerrando distancia.
                BasicLink(1, "Garra izquierda",  14f, 0.38f, 0.9f),
                BasicLink(2, "Garra derecha",    14f, 0.36f, 0.9f),
                BasicLink(3, "Zarpazo doble",    18f, 0.42f, 1.1f),
                BasicLink(4, "Revés",            20f, 0.44f, 1.2f),
            };
            // 5º — remate: más lento y más caro de comprometer, con estallido Mineral al conectar.
            basicos.Add(Ability($"{Dir}/Ability_Ottorock_Basico5.asset", "ottorock_basico_5", "Garra sísmica",
                AbilitySlot.Flex, 0f, 0.7f, new AbilityPhase {
                    name = "Remate", exit = new AfterSeconds { seconds = 0.62f },
                    effects = new List<AbilityEffect> {
                        new DashMotion { distance = 1.3f, time = 0.1f },
                        new MeleeHitbox { range = 2.2f, radius = 1.6f },
                        new DealDamage { amount = 26f, staggerAmount = 30f, when = ApplyWhen.OnHit },
                        // El remate eleva un poco: deja al enemigo a tiro para seguir en el aire.
                        // Remate: empuja fuerte, pero tampoco eleva — misma regla que el mordisco.
                        new Knockback { speed = 16f, duration = 0.14f, flinch = 0.2f,
                                        lift = 0f, hang = 0.55f, when = ApplyWhen.OnHit },
                        // El estallido SÍ lleva elemento (siempre el de la especie) → aquí sí luce el 1.5x.
                        new ElementalBurst { radius = 2.8f, damage = 16f, staggerAmount = 16f,
                                             forwardOffset = 1.2f, when = ElementalBurst.Timing.OnHit },
                    }
                }, AbilityElement.Neutro));

            // Movilidad — Embestida Garra: dash corto + daño al objetivo + slow breve, y al FRENAR
            // estalla en Mineral donde cae (en Fase 2 ese estallido reaccionará con el suelo del sitio).
            // Es la primera habilidad con cámara propia: se aleja y se pone detrás mientras dura.
            // Coreografía en 3 tiempos — la cámara viaja mientras el cuerpo está QUIETO, en los dos
            // extremos, y el desplazamiento ocurre con la cámara ya colocada. Esa separación es lo que
            // quita el mareo: nunca se mueven el mundo y el encuadre a la vez.
            var camaraEmbestida = new System.Func<float, CameraShot>(hold => new CameraShot {
                distance = 12f, pitch = 18f, fromBehind = true, behindSpeed = 260f, holdAfter = hold,
            });
            var embestida = Ability($"{Dir}/Ability_Ottorock_EmbestidaGarra.asset", "ottorock_embestida", "Embestida Garra",
                AbilitySlot.Movilidad, 12f, 2.5f, new List<AbilityPhase> {
                    // ① Carga: se planta (también en el aire) y la cámara viaja hasta su sitio.
                    new AbilityPhase {
                        name = "Carga", rooted = true, exit = new AfterSeconds { seconds = 0.3f },
                        effects = new List<AbilityEffect> { camaraEmbestida(0.12f) },   // hold = cubre el salto de fase
                    },
                    // ② Embestida: el desplazamiento, con el encuadre ya puesto.
                    // ⚠️ La fase dura MÁS que el DashMotion a propósito: si terminara antes, la fase
                    // siguiente (rooted) cortaría el desplazamiento a media trayectoria. Ese margen
                    // es también lo que hace que el estallido salte justo al frenar.
                    new AbilityPhase {
                        name = "Embestida", exit = new AfterSeconds { seconds = 0.35f },
                        effects = new List<AbilityEffect> {
                            camaraEmbestida(0f),   // al acabar suelta la cámara: empieza a volver
                            new DashMotion { distance = 6f, time = 0.28f, maxPitch = 35f },
                            // Multi-hit: la embestida reparte ~3-4 golpes por su recorrido (uno cada
                            // 0.09 s de los 0.28 s del dash). Arrollarlo entero suma ~24-32, lo que
                            // costaba de un solo golpe; rozarlo al esquivar cuesta uno solo.
                            // (El slow NO va aquí: es del estallido, fase ③.)
                            new SweepHitbox { radius = 1.3f, forwardOffset = 0.8f, hitInterval = 0.09f },
                            new DealDamage { amount = 8f, staggerAmount = 6f, when = ApplyWhen.OnHit },
                            // Un empujón por golpe, de duración ~ al intervalo: encadenados se leen
                            // como un arrastre continuo, y un solo golpe solo aparta. Va en la
                            // dirección 3D del dash, así que embestir en diagonal **se lo lleva hacia
                            // arriba contigo** (ref. side B de Meta Knight) en vez de dejarlo abajo;
                            // el `hang` impide que se desplome a media embestida.
                            // ⚠️ REGLA: el arrastre NO puede ir más rápido que el propio dash (6 u /
                            // 0.28 s ≈ 21 u/s), o el enemigo se escapa por delante y no hay remate.
                            // Con 18 va un pelín más lento que tú: te lo llevas y lo alcanzas.
                            // Vertical calibrado para acabar a TU MISMA altura (~3.4 u), no encima.
                            // El `hang` largo le hace ESPERARTE: entre la embestida y el deslizamiento
                            // pasan ~0.7 s antes de que puedas encadenar el mordisco.
                            new Knockback { speed = 18f, duration = 0.14f, flinch = 0.1f,
                                            useMotionDirection = true, verticalScale = 0.6f,
                                            lift = 2f, hang = 1f, when = ApplyWhen.OnHit },
                            // Correa de seguridad: pase lo que pase con el empuje, no se aleja más de
                            // 2.6 u ni se despega de tu altura. Así el remate aéreo SIEMPRE llega, en
                            // vez de depender de que las velocidades cuadren.
                            new CarryToFront { distance = 2.6f, speed = 26f, duration = 0.3f,
                                               matchHeight = true, heightOffset = 0.3f, flinch = 0f,
                                               when = ApplyWhen.OnHit },
                        }
                    },
                    // ③ Deslizamiento: NO frena en seco — revienta el estallido (con su propio parón,
                    // más fuerte que el de un golpe) y el cuerpo **sigue por donde iba**, patinando
                    // hasta pararse. Es el deslizamiento de nutria, no un salto felino: el gesto se
                    // completa en vez de cortarse. Tiene fase propia para poder colgarle sus payloads
                    // sin compartirlos con la hitbox del arrollamiento — el SLOW es de quien pilla la
                    // explosión, no de quien fue arrollado por el camino.
                    new AbilityPhase {
                        name = "Deslizamiento", exit = new AfterSeconds { seconds = 0.36f },
                        effects = new List<AbilityEffect> {
                            new ElementalBurst { radius = 2.6f, damage = 14f, staggerAmount = 14f,
                                                 when = ElementalBurst.Timing.OnCast,
                                                 reportHits = true, hitstop = 0.14f },
                            new ApplyStatus { type = StatusType.Ralentizado, duration = 2f, when = ApplyWhen.OnHit },
                            new DashMotion { distance = 1.9f, time = 0.34f, easeOut = true, continueForward = true },
                        },
                    },
                });

            // Flex — Mordisco Ferroso: mordida fuerte (daño alto) hacia donde mira, y el impacto
            // estalla en Mineral junto con el daño.
            var mordisco = Ability($"{Dir}/Ability_Ottorock_MordiscoFerroso.asset", "ottorock_mordisco", "Mordisco Ferroso",
                AbilitySlot.Flex, 20f, 3f, new AbilityPhase {
                    name = "Mordisco", exit = new AfterSeconds { seconds = 0.5f },
                    effects = new List<AbilityEffect> {
                        new DashMotion { distance = 1.5f, time = 0.1f },     // step-in mayor (acompaña al empuje)
                        new MeleeHitbox { range = 2.3f, radius = 1.5f },
                        new DealDamage { amount = 50f, staggerAmount = 35f, when = ApplyWhen.OnHit },
                        // `lift = 0` a propósito: NO eleva, solo SOSTIENE lo que ya esté arriba (`hang`).
                        // La regla del kit es que quien abre la ventana aérea es la embestida y punto;
                        // si cada golpe empujara un poco hacia arriba, el combo iría subiendo solo.
                        new Knockback { speed = 12f, duration = 0.12f, flinch = 0.18f,
                                        lift = 0f, hang = 0.55f, when = ApplyWhen.OnHit },
                        new ElementalBurst { radius = 2.2f, damage = 15f, staggerAmount = 15f,
                                             forwardOffset = 1f, when = ElementalBurst.Timing.OnHit },
                    }
                });

            // Locomoción de Ottorock-Libre: 4 andares. El deslizamiento (nutria) es el andar de tope,
            // y es LECTURA, no física — el stick sigue mandando 1:1, así que soltarlo para en seco.
            var locLibre = CreateOrLoad<LocomotionProfile>($"{Dir}/Locomotion_Ottorock_Libre.asset");
            locLibre.gaits = new List<LocomotionProfile.Gait> {
                new LocomotionProfile.Gait { name = "Paso corto",    threshold = 0.1f },
                new LocomotionProfile.Gait { name = "Caminar",       threshold = 0.5f },
                new LocomotionProfile.Gait { name = "Deslizamiento", threshold = 0.75f },
            };
            locLibre.maxSpeedMultiplier = 1f;
            locLibre.blockMaxInput = 0.25f;   // bloqueando solo llega a paso corto
            EditorUtility.SetDirty(locLibre);

            // Sujeción (post-gate): mismo recorrido de stick, pero **sin deslizamiento** —con la roca
            // en brazos no patina— y tope equivalente al 0.75 de Libre. Se genera ya como referencia
            // de cómo un stance sobrescribe el perfil (ABILITY-EXTRACTION §8.1).
            var locSujecion = CreateOrLoad<LocomotionProfile>($"{Dir}/Locomotion_Ottorock_Sujecion.asset");
            locSujecion.gaits = new List<LocomotionProfile.Gait> {
                new LocomotionProfile.Gait { name = "Paso corto", threshold = 0.1f },
                new LocomotionProfile.Gait { name = "Caminar",    threshold = 0.5f },
            };                                 // sin deslizamiento: con la roca en brazos no patina
            locSujecion.maxSpeedMultiplier = 0.75f;   // a fondo llega a lo que Libre da al 0.75
            locSujecion.blockMaxInput = 0.2f;
            locSujecion.inputRamp = 0.26f;     // pesado: tarda más en arrancar
            EditorUtility.SetDirty(locSujecion);

            // Ottorock — Luchador de Mineral (1 forma, solo modo Libre).
            var ottorock = CreateOrLoad<CreatureSpeciesData>($"{Dir}/Creature_Ottorock.asset");
            ottorock.id = "ottorock"; ottorock.displayName = "Ottorock";
            ottorock.element = Element.Mineral; ottorock.archetype = Archetype.Luchador;
            ottorock.forms = new List<EvolutionForm> {
                new EvolutionForm {
                    formName = "Libre",
                    stats = new BaseStats { maxHealth = 4000f, damage = 14f, moveSpeed = 6f },   // Vida 8/10 × 500 HP/punto (CREATURES §4)
                    locomotion = locLibre,   // moveSpeed = velocidad a stick a fondo; los andares no la tocan
                    movilidad = embestida, flex = mordisco, basicChain = basicos,   // signature = null (post-gate)
                    block = new BlockParams { energyRegenMultiplier = 2.5f, weaknessDamageMultiplier = 2f },
                    levelsInForm = 2,
                }
            };
            EditorUtility.SetDirty(ottorock);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return ottorock;
        }

        /// Especie mínima solo para dar elemento al maniquí (Rayo → el Mineral de Ottorock luce 1.5x).
        public static CreatureSpeciesData DummySpecies(Element element, string name)
        {
            EnsureDir();
            var s = CreateOrLoad<CreatureSpeciesData>($"{Dir}/Species_{name}.asset");
            s.id = name.ToLower(); s.displayName = name; s.element = element;
            s.archetype = Archetype.Guardian;
            s.forms = new List<EvolutionForm>();
            EditorUtility.SetDirty(s);
            AssetDatabase.SaveAssets();
            return s;
        }

        /// Eslabón corriente de la cadena de básicos: melee corto, neutro y sin coste. Lo único que
        /// cambia entre ellos es el daño y el ritmo — la variación es lo que hace que se sienta cadena.
        static AbilityData BasicLink(int n, string name, float damage, float seconds, float stepIn) =>
            Ability($"{Dir}/Ability_Ottorock_Basico{n}.asset", $"ottorock_basico_{n}", name,
                AbilitySlot.Flex, 0f, 0f, new AbilityPhase {
                    name = "Golpe", exit = new AfterSeconds { seconds = seconds },
                    effects = new List<AbilityEffect> {
                        new DashMotion { distance = stepIn, time = 0.08f },   // step-in sutil hacia la mira
                        new MeleeHitbox { range = 2.0f, radius = 1.4f },
                        new DealDamage { amount = damage, staggerAmount = damage, when = ApplyWhen.OnHit },
                        // ⚠️ El empuje debe quedar POR DEBAJO del step-in o el enemigo se aleja y la
                        // cadena se rompe sola: aquí 9 × 0.08 = 0.72 u empujado contra `stepIn` de
                        // avance. Subir uno obliga a subir el otro.
                        new Knockback { speed = 9f, duration = 0.08f, flinch = 0.12f, hang = 0.5f, when = ApplyWhen.OnHit },
                    }
                }, AbilityElement.Neutro);

        static AbilityData Ability(string path, string id, string name, AbilitySlot slot,
                                   float cost, float cd, AbilityPhase phase,
                                   AbilityElement element = AbilityElement.Especie)
            => Ability(path, id, name, slot, cost, cd, new List<AbilityPhase> { phase }, element);

        /// Varias fases = habilidad con tiempos (carga → acción → recuperación).
        static AbilityData Ability(string path, string id, string name, AbilitySlot slot,
                                   float cost, float cd, List<AbilityPhase> phases,
                                   AbilityElement element = AbilityElement.Especie)
        {
            var a = CreateOrLoad<AbilityData>(path);
            a.id = id; a.displayName = name; a.slot = slot; a.element = element;
            a.energyCost = cost; a.cooldown = cd;
            a.phases = phases;
            EditorUtility.SetDirty(a);
            return a;
        }

        static void EnsureDir()
        {
            if (!AssetDatabase.IsValidFolder(Dir))
                AssetDatabase.CreateFolder("Assets/Tatemis", "Examples");
        }

        static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}
#endif
