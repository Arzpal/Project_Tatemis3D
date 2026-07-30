#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Construye el **mundo libre** de caja gris que pide [ARENA.md](../../docs/ARENA.md):
    /// semiabierto, mediano-pequeño y **continuo** —se camina de un bioma a otro sin cortes— con los
    /// **6 biomas juntos** alrededor de un claro neutro. Al dar Play, `CombatDirector` abre el
    /// encuentro donde estés y la `CombatZone` enmarca el tablero con sus celdas.
    ///
    /// El suelo es UNA superficie continua: los biomas son **discos de color encima**, pura lectura.
    /// El elemento real de cada punto no vive en la geometría sino en `ElementalGrid` (ARENA §1: la
    /// rejilla es un overlay, el terreno no se construye con hexágonos).
    public static class WorldBuilder
    {
        const string ScenesDir = "Assets/Tatemis/Scenes";
        const string ScenePath = ScenesDir + "/Mundo.unity";

        /// Geometría del mundo, **dimensionada contra el tamaño de la arena** (✏️ 2026-07-29): la zona
        /// de combate son 4 anillos ≈ **54 u de punta a punta**, o sea **27 u de radio**. De ahí salen
        /// las tres cifras, y cada una tiene un motivo:
        ///
        ///  · **`BiomeRadius` 44** — bastante mayor que el radio de la arena (27), así que **moverte
        ///    dentro de un bioma te cambia el tablero**: cada rincón tiene sus manchas. Si el bioma
        ///    midiera lo mismo que la arena, daría igual dónde te pares.
        ///  · **`BiomeRing` 50** — los seis se tocan y cubren el mundo. Ya **no hace falta claro
        ///    neutro**: con el modelo de manchas, el suelo sin elemento está **dentro de cada bioma**
        ///    (los huecos entre manchas), así que R5 se ve en cualquier parte.
        ///  · Las **fronteras** entre biomas contiguos son donde se mezclan manchas de dos elementos.
        ///
        /// ⚠️ **El jugador aparece en la Cantera**, no en el centro: es el bioma de Mineral, o sea el
        /// de Ottorock, así que la primera arena le enseña su **amplificación** en casa — y el centro
        /// del mundo queda como un cruce al que ir, no como el sitio por defecto.
        const float BiomeRadius = 44f;
        const float BiomeRing = 50f;
        const float WorldRadius = 105f;

        /// El color sale de `ElementalGrid.ColorOf` —la paleta dictada— pero **apagado**, porque el
        /// disco del bioma es el paisaje de fondo y las **celdas** son las que tienen que resaltar.
        static Color BiomeTint(Element e) => Color.Lerp(ElementalGrid.ColorOf(e), new Color(0.45f, 0.45f, 0.42f), 0.55f);

        struct BiomeDef { public string name; public Element element; }

        /// ⚠️ **El orden del anillo importa** — es lo que decide qué pasa en cada frontera, y las
        /// fronteras son donde se prueban las mezclas sin caminar. Elegido para que **Ottorock**
        /// (Mineral, la criatura del gate) tenga pegados los dos casos que más le definen: la
        /// **Cantera toca el Campo de tormenta** (Mineral › Rayo = su dominancia) y también las
        /// **Mesetas de viento** (Aire › Mineral = su debilidad, la reacción del terreno que se come).
        /// Y la **Laguna toca el Campo de tormenta**, que es la frontera de la ★ (Rayo + Agua).
        static readonly BiomeDef[] Defs =
        {
            new BiomeDef { name = "Laguna",            element = Element.Agua },
            new BiomeDef { name = "Caldera",           element = Element.Fuego },
            new BiomeDef { name = "Espesura",          element = Element.Naturaleza },
            new BiomeDef { name = "Mesetas de viento", element = Element.Aire },
            new BiomeDef { name = "Cantera",           element = Element.Mineral },
            new BiomeDef { name = "Campo de tormenta", element = Element.Rayo },
        };

        [MenuItem("Tatemis/Mundo/Construir mundo libre (6 biomas)")]
        public static void Build()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            var ottorock = OttorockContent.Generate();
            var dummySpecies = OttorockContent.DummySpecies(Element.Rayo, "ManiquiRayo");
            var reactionTable = ReactionTableContent.Generate();
            var typeChart = AssetDatabase.LoadAssetAtPath<TypeChart>($"{OttorockContent.Dir}/TypeChart.asset");
            if (ottorock == null || typeChart == null)
            {
                Debug.LogError("[Mundo] No se pudo generar el contenido. Aborta.");
                return;
            }

            // --- Luz ---
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.1f; light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // --- Suelo continuo (una sola superficie: el mundo NO se construye por biomas) ---
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.localScale = new Vector3(WorldRadius * 0.24f, 1f, WorldRadius * 0.24f);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.62f, 0.60f, 0.54f));

            // --- Biomas: discos de color ENCIMA del suelo, sin collider (son lectura, no geometría) ---
            var biomesRoot = new GameObject("Biomas");
            var gridGo = new GameObject("ElementalGrid");
            var grid = gridGo.AddComponent<ElementalGrid>();
            grid.biomes = new List<ElementalGrid.Biome>();

            for (int i = 0; i < Defs.Length; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i);
                var center = new Vector3(Mathf.Cos(a) * BiomeRing, 0f, Mathf.Sin(a) * BiomeRing);
                var tint = BiomeTint(Defs[i].element);
                MakeBiomeVisual(biomesRoot.transform, Defs[i].name, center, BiomeRadius, tint);
                // Los vecinos del anillo son de quienes salen las manchas "vecinas": así el mundo se
                // lee como un sitio real (el agua se filtra desde la Laguna hacia la Caldera) y no
                // como elementos puestos al azar.
                grid.biomes.Add(new ElementalGrid.Biome
                {
                    name = Defs[i].name, center = center, radius = BiomeRadius,
                    element = Defs[i].element, color = tint,
                    neighbours = new List<Element>
                    {
                        Defs[(i + Defs.Length - 1) % Defs.Length].element,
                        Defs[(i + 1) % Defs.Length].element,
                    },
                });
            }

            // Borde del mundo: un anillo alto que impide caerse al vacío mientras se explora.
            MakeWorldBounds(WorldRadius);

            // Nodos de cristal en la frontera Laguna–Campo de tormenta: golpearlos suelta Rayo sobre
            // celdas de Agua = ★ agua electrificada, la estrella del pitch, sin necesitar una criatura
            // de Rayo (ER §8). Es la única forma de verla hasta que entre esa especie en el slice.
            float aguaA = 0f, rayoA = Mathf.Deg2Rad * 300f;
            var fronteraStar = (new Vector3(Mathf.Cos(aguaA), 0f, Mathf.Sin(aguaA))
                              + new Vector3(Mathf.Cos(rayoA), 0f, Mathf.Sin(rayoA))).normalized * BiomeRing;
            MakeCrystalNode(fronteraStar + new Vector3(2.5f, 0.8f, 0f));
            MakeCrystalNode(fronteraStar + new Vector3(-3f, 0.8f, 2f));

            // --- Jugador ---
            // El jugador aparece en la CANTERA (Mineral, su propio elemento): la primera arena le
            // enseña la amplificación en casa, más las manchas vecinas y la exótica que toque.
            int canteraIdx = System.Array.FindIndex(Defs, d => d.element == Element.Mineral);
            float canteraA = Mathf.Deg2Rad * (60f * canteraIdx);
            var spawn = new Vector3(Mathf.Cos(canteraA), 0f, Mathf.Sin(canteraA)) * BiomeRing;
            var player = MakePlayer(ottorock, spawn + Vector3.up);

            // --- Salvajes: uno en el claro neutro, uno en el corazón de cada bioma y uno en cada
            // frontera. Así hay un sitio ya montado para las tres situaciones que hay que probar:
            // arena neutra (R5) · arena de un solo elemento (amplificación / tu dominancia) · arena de
            // dos elementos (una pareja concreta). Con F5 se reabre la zona donde estés parado.
            MakeDummy("Maniqui (cruce central)", new Vector3(0f, 1f, 0f), dummySpecies, player.transform);
            MakeDummy("Maniqui (junto al jugador)", spawn + new Vector3(0f, 1f, 9f), dummySpecies, player.transform);
            for (int i = 0; i < Defs.Length; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i);
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                MakeDummy($"Maniqui ({Defs[i].name})", dir * BiomeRing + Vector3.up, dummySpecies, player.transform);

                // Frontera con el bioma siguiente del anillo: el punto medio entre los dos corazones.
                var next = Mathf.Deg2Rad * (60f * ((i + 1) % Defs.Length));
                var mid = ((dir + new Vector3(Mathf.Cos(next), 0f, Mathf.Sin(next))).normalized * BiomeRing);
                int j = (i + 1) % Defs.Length;
                MakeDummy($"Maniqui (frontera {Defs[i].element}+{Defs[j].element})", mid + Vector3.up,
                          dummySpecies, player.transform);
            }

            // --- Sistemas ---
            var systems = new GameObject("Systems");
            var bootstrap = systems.AddComponent<CombatBootstrap>();
            SetPrivate(bootstrap, "typeChart", typeChart);
            var reactions = systems.AddComponent<ReactionSystem>();
            SetPrivate(reactions, "table", reactionTable);

            var zoneGo = new GameObject("CombatZone");
            zoneGo.transform.SetParent(systems.transform, false);
            var zone = zoneGo.AddComponent<CombatZone>();
            var director = systems.AddComponent<CombatDirector>();
            SetPrivate(director, "zone", zone);

            var hud = systems.AddComponent<DebugHUD>();
            SetPrivate(hud, "energy", player.GetComponent<ElementalEnergy>());
            SetPrivate(hud, "targeting", player.GetComponent<TargetingSystem>());
            SetPrivate(hud, "caster", player.GetComponent<AbilityCaster>());
            SetPrivate(hud, "controller", player.GetComponent<CreatureController>());
            SetPrivate(hud, "input", player.GetComponent<InputReader>());

            // --- Cámara ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camComp = camGo.AddComponent<Camera>();
            camComp.fieldOfView = 55f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3f, -6f);

            var camRig = player.AddComponent<PlayerCameraRig>();
            camRig.follow = player.transform;
            camRig.targeting = player.GetComponent<TargetingSystem>();
            camRig.input = player.GetComponent<InputReader>();
            camRig.cam = camComp;
            SetPrivate(hud, "camRig", camRig);

            if (!AssetDatabase.IsValidFolder(ScenesDir)) AssetDatabase.CreateFolder("Assets/Tatemis", "Scenes");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            Debug.Log("[Mundo] Construido en " + ScenePath + ". Pulsa Play: el combate se abre solo y el " +
                      "cerco sigue las ARISTAS de las celdas (no corta ninguna) y llega al cielo, así que " +
                      "un volador tampoco se escapa. **F5 reabre la zona donde estés parado.** " +
                      "APARECES EN LA CANTERA (Mineral, tu elemento). El terreno son MANCHAS sobre suelo " +
                      "sin elemento: cada mancha nace de una pieza central y crece a su alrededor, y un " +
                      "bioma solo decide CUÁNTAS siembra de cada tipo — mayoría suyas, un par de sus " +
                      "vecinos y alguna exótica. Por eso toda arena tiene entre 2 y 5 elementos, " +
                      "predominando el del bioma. Camina y pulsa F5 en sitios distintos: el tablero " +
                      "cambia incluso dentro del mismo bioma. Sitios con maniquí: cada bioma, cada " +
                      "frontera, y el cruce central. Celdas: color = elemento · gris oscuro = neutra " +
                      "(ahí no reacciona nada, R5) · gris claro = ambiental.");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        /// Disco de color del bioma. Sin collider y muy plano: es una mancha de lectura sobre el
        /// suelo real, no una plataforma — el jugador nunca "sube" a un bioma.
        static void MakeBiomeVisual(Transform parent, string name, Vector3 center, float radius, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = center + Vector3.up * 0.02f;
            go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        }

        static void MakeCrystalNode(Vector3 pos)
        {
            var node = GameObject.CreatePrimitive(PrimitiveType.Cube);
            node.name = "Nodo de cristal (Rayo)";
            node.transform.position = pos;
            node.transform.rotation = Quaternion.Euler(18f, 35f, 12f);
            node.transform.localScale = new Vector3(0.55f, 1.3f, 0.55f);
            node.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.55f, 0.9f, 1f));
            node.AddComponent<CrystalNode>().charge = Element.Rayo;
            node.AddComponent<HitFlash>();
        }

        static void MakeWorldBounds(float radius)
        {
            var root = new GameObject("Limite del mundo");
            var mat = Mat(new Color(0.22f, 0.22f, 0.26f));
            const int segments = 48;
            float step = 360f / segments;
            float chord = 2f * radius * Mathf.Sin(Mathf.Deg2Rad * step * 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.Deg2Rad * step * i;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.transform.SetParent(root.transform, false);
                w.transform.position = dir * radius + Vector3.up * 2f;
                w.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                w.transform.localScale = new Vector3(chord * 1.1f, 4f, 1f);
                w.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        static GameObject MakePlayer(CreatureSpeciesData species, Vector3 pos)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player (Ottorock)";
            player.transform.position = pos;
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.25f, 0.5f, 0.9f));

            player.AddComponent<CharacterController>();
            player.AddComponent<StatusController>();
            player.AddComponent<ElementalEnergy>();
            player.AddComponent<AbilityCaster>();
            player.AddComponent<InputReader>();
            player.AddComponent<TargetingSystem>();
            var controller = player.AddComponent<CreatureController>();
            controller.data = new CreatureInstance { species = species, formIndex = 0, level = 1 };

            var color = player.AddComponent<ActionColor>();
            color.target = player.GetComponent<Renderer>();
            color.baseColor = new Color(0.25f, 0.5f, 0.9f);
            player.AddComponent<ElementalSpray>();
            MakeNose(player.transform, new Color(0.95f, 0.95f, 0.95f));
            return player;
        }

        static void MakeDummy(string name, Vector3 pos, CreatureSpeciesData species, Transform playerT)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            d.name = name;
            d.transform.position = pos;
            Object.DestroyImmediate(d.GetComponent<CapsuleCollider>());
            d.AddComponent<CharacterController>();
            d.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.85f, 0.3f, 0.3f));

            Vector3 look = playerT.position - pos; look.y = 0f;
            if (look.sqrMagnitude > 0.001f) d.transform.rotation = Quaternion.LookRotation(look);

            var sr = d.AddComponent<StaggerReceiver>();
            sr.species = species;
            sr.maxHealth = 2500f; sr.health = 2500f; sr.staggerRatio = 0.15f;
            d.AddComponent<HitReactor>();
            d.AddComponent<HitFlash>();
            d.AddComponent<StatusController>();
            MakeNose(d.transform, new Color(0.95f, 0.95f, 0.95f));
        }

        static void MakeNose(Transform parent, Color color)
        {
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            Object.DestroyImmediate(nose.GetComponent<BoxCollider>());
            nose.transform.SetParent(parent, false);
            nose.transform.localPosition = new Vector3(0f, 0.55f, 0.5f);
            nose.transform.localScale = new Vector3(0.35f, 0.18f, 0.15f);
            nose.GetComponent<Renderer>().sharedMaterial = Mat(color);
        }

        static Material Mat(Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader != null ? shader : Shader.Find("Standard"));
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }

        static void SetPrivate(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        }
    }
}
#endif
