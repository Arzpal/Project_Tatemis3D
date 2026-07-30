#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tatemis.EditorTools
{
    /// Construye por código la escena de prueba (grey-box) lista para pulsar Play:
    /// suelo + paredes + luz, jugador (Emberling) con todos los componentes cableados,
    /// maniquí golpeable, cámara 3ª persona y HUD de debug. Menú: Tatemis -> Sandbox.
    ///
    /// Objetivo: probar el DoD de Fase 0 (moverse + lock-on) y de propina el feel base
    /// (ataque básico + embestida contra el maniquí) sin tocar la GUI del editor.
    public static class SandboxBuilder
    {
        const string ExamplesDir = "Assets/Tatemis/Examples";
        const string ScenesDir   = "Assets/Tatemis/Scenes";
        const string ScenePath   = ScenesDir + "/Sandbox.unity";

        [MenuItem("Tatemis/Sandbox/Construir escena de prueba")]
        public static void Build()
        {
            // 1) Escena nueva vacía (cierra la Sandbox si estaba abierta, antes de borrarla).
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2) Rebuild limpio: borrar lo generado y volver a crearlo desde cero.
            CleanGenerated();
            var ottorock = OttorockContent.Generate();
            var dummySpecies = OttorockContent.DummySpecies(Element.Rayo, "ManiquiRayo");
            var reactionTable = ReactionTableContent.Generate();
            var typeChart = AssetDatabase.LoadAssetAtPath<TypeChart>($"{ExamplesDir}/TypeChart.asset");
            if (ottorock == null || typeChart == null)
            {
                Debug.LogError("[Sandbox] No se pudo generar el contenido de Ottorock. Aborta.");
                return;
            }

            // 3) Luz direccional.
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 4) Suelo grey-box (50x50) — neutro (el gate es sin reacciones, regla R5); el terreno
            // elemental vive en el perímetro (5b), sin tocar el centro donde pelea el gate.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.62f, 0.60f, 0.54f));   // arena cálida clara

            // 5) Paredes/obstáculos — tonos distintos para leer la geometría.
            var wallMat   = Mat(new Color(0.30f, 0.35f, 0.46f));   // azul-gris frío
            var pillarMat = Mat(new Color(0.55f, 0.42f, 0.28f));   // marrón cálido (destaca de las paredes)
            MakeWall("Pared N", new Vector3(0f, 1.5f, 24f), new Vector3(50f, 3f, 1f), wallMat);
            MakeWall("Pared S", new Vector3(0f, 1.5f, -24f), new Vector3(50f, 3f, 1f), wallMat);
            MakeWall("Pared E", new Vector3(24f, 1.5f, 0f), new Vector3(1f, 3f, 50f), wallMat);
            MakeWall("Pared O", new Vector3(-24f, 1.5f, 0f), new Vector3(1f, 3f, 50f), wallMat);
            MakeWall("Pilar 1", new Vector3(6f, 1.5f, 3f), new Vector3(1.5f, 3f, 1.5f), pillarMat);
            MakeWall("Pilar 2", new Vector3(-5f, 1.5f, -4f), new Vector3(1.5f, 3f, 1.5f), pillarMat);

            // 5b) Laboratorio elemental (Fase 2, T0.2b): parches de terreno en el perímetro — el
            // centro sigue neutro para que el gate (melee puro, R5) se pruebe igual que siempre.
            // Con Ottorock (Mineral): Rayo = SU dominancia (descarga a tierra) · Aire = la respuesta
            // del terreno en su contra (esquirlas sin bando) · Agua/Fuego/Naturaleza = combinaciones
            // · la veta (Mineral) = mismo elemento, nada — cada regla tiene su rincón. El resto de
            // casillas se disparan con el spray (F2/G).
            // Parches de 12×12 (esquinas) y 8×12 (laterales): con celda de 1.75 salen ~7-9 hexágonos
            // por parche, que ya se leen como REJILLA. A 8×8 salía uno suelto, que no comunica nada.
            MakePatch("Charco (Agua)", new Vector3(-16f, 0f, 16f), new Vector2(12f, 12f),
                      Element.Agua, new Color(0.25f, 0.45f, 0.85f));
            MakePatch("Roca volcánica (Fuego)", new Vector3(16f, 0f, 16f), new Vector2(12f, 12f),
                      Element.Fuego, new Color(0.75f, 0.25f, 0.12f));
            MakePatch("Matorral (Naturaleza)", new Vector3(-16f, 0f, -16f), new Vector2(12f, 12f),
                      Element.Naturaleza, new Color(0.3f, 0.6f, 0.25f));
            MakePatch("Veta de menas (Mineral)", new Vector3(16f, 0f, -16f), new Vector2(12f, 12f),
                      Element.Mineral, new Color(0.55f, 0.6f, 0.7f));
            MakePatch("Piedra electrificada (Rayo)", new Vector3(19f, 0f, 0f), new Vector2(8f, 12f),
                      Element.Rayo, new Color(0.85f, 0.8f, 0.3f));
            MakePatch("Corrientes de aire (Aire)", new Vector3(-19f, 0f, 0f), new Vector2(8f, 12f),
                      Element.Aire, new Color(0.6f, 0.85f, 0.9f));
            // El nodo va DENTRO del borde del charco: su descarga (Rayo) cae sobre Agua → ★.
            MakeCrystalNode(new Vector3(-13.5f, 0.8f, 13.5f));

            // 6) Jugador — cápsula con CharacterController (sin la CapsuleCollider del primitivo).
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player (Ottorock)";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.25f, 0.5f, 0.9f));

            player.AddComponent<CharacterController>();          // center 0, height 2, radius 0.5 = casa con la cápsula
            player.AddComponent<StatusController>();             // por si lo ralentizan/aturden
            player.AddComponent<ElementalEnergy>();
            player.AddComponent<AbilityCaster>();
            player.AddComponent<InputReader>();
            var targeting = player.AddComponent<TargetingSystem>();
            var controller = player.AddComponent<CreatureController>();
            controller.data = new CreatureInstance { species = ottorock, formIndex = 0, level = 1 };

            var color = player.AddComponent<ActionColor>();      // cambia de color según la acción
            color.target = player.GetComponent<Renderer>();
            color.baseColor = new Color(0.25f, 0.5f, 0.9f);
            player.AddComponent<ElementalSpray>();               // debug: F2 cicla elemento · G dispara
            MakeNose(player.transform, new Color(0.95f, 0.95f, 0.95f));   // indicador de "cara" (claro para que resalte)

            // 7) Maniquíes golpeables — elemento Rayo (el Mineral de Ottorock lo supera → 1.5x visible).
            // Varios y separados, para probar la adquisición en pantalla y el cambio de objetivo.
            MakeDummy("Maniqui A (neutro)", new Vector3(0f, 1f, 6f), dummySpecies, player.transform);
            MakeDummy("Maniqui B (neutro)", new Vector3(7f, 1f, 9f), dummySpecies, player.transform);
            // ⚠️ Tres maniquíes MÁS, plantados sobre parches. Sin ellos, el payload de las reacciones
            // (daño y aturdimiento) no se aplica hoy a NADIE: el centro es neutro por R5, los
            // maniquíes no tienen IA que los lleve a un parche, y empujarlos hasta allí a golpes son
            // 20+ básicos. Con estos tres, los puntos ①②③ del checklist se prueban de pie.
            MakeDummy("Maniqui Rayo (dominancia)", new Vector3(18f, 1f, 0f), dummySpecies, player.transform);
            MakeDummy("Maniqui Charco (★)", new Vector3(-15f, 1f, 15f), dummySpecies, player.transform);
            MakeDummy("Maniqui Aire (provocada)", new Vector3(-19f, 1f, 3f), dummySpecies, player.transform);

            // 8) Sistemas: cablear el TypeChart global + el sistema de reacciones + HUD de debug.
            var systems = new GameObject("Systems");
            var bootstrap = systems.AddComponent<CombatBootstrap>();
            SetPrivate(bootstrap, "typeChart", typeChart);
            var reactions = systems.AddComponent<ReactionSystem>();
            SetPrivate(reactions, "table", reactionTable);
            var hud = systems.AddComponent<DebugHUD>();
            SetPrivate(hud, "energy", player.GetComponent<ElementalEnergy>());
            SetPrivate(hud, "targeting", targeting);
            SetPrivate(hud, "caster", player.GetComponent<AbilityCaster>());
            SetPrivate(hud, "controller", controller);
            SetPrivate(hud, "input", player.GetComponent<InputReader>());   // panel de diagnóstico (F1)

            // 9) Cámara 3ª persona estilo BotW/TOTK (script propio; ver docs/CAMERA-TARGETING.md).
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camComp = camGo.AddComponent<Camera>();
            camComp.fieldOfView = 55f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3f, -6f);

            var camRig = player.AddComponent<PlayerCameraRig>();
            camRig.follow = player.transform;
            camRig.targeting = targeting;
            camRig.input = player.GetComponent<InputReader>();
            camRig.cam = camComp;
            SetPrivate(hud, "camRig", camRig);   // para dibujar la retícula al apuntar

            // 10) Guardar la escena y dejarla como escena de arranque.
            if (!AssetDatabase.IsValidFolder(ScenesDir))
                AssetDatabase.CreateFolder("Assets/Tatemis", "Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            Debug.Log("[Sandbox] Escena construida (Ottorock) en " + ScenePath +
                      ". Pulsa Play. Controles: WASD strafe · ratón apunta (el cuerpo mira a la mira) · Q lock-on (asistencia) · clic izq golpe de garra · " +
                      "1 Embestida Garra · 2 Mordisco Ferroso · 3 (Signature: post-gate, sin efecto) · " +
                      "Shift bloqueo · Espacio salto · F2/G spray elemental (cicla/dispara) · Esc libera el ratón. " +
                      "Laboratorio elemental en el perímetro, con un maniquí plantado en cada prueba: piedra electrificada " +
                      "(descarga a tierra), charco + nodo de cristal (★ agua electrificada) y corrientes de aire " +
                      "(esquirlas SIN bando: ahí el estallido te EMPUJA también a ti). ⚠️ El jugador aún no recibe DAÑO " +
                      "(TakesDamage = false): por eso no verás número sobre él — está pendiente de ratificar VITALITY-DEATH.");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        [MenuItem("Tatemis/Sandbox/Limpiar (borrar escena + assets generados)")]
        public static void Clean()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CleanGenerated();
            Debug.Log("[Sandbox] Limpiado: escena Sandbox y carpeta " + ExamplesDir + " borradas.");
        }

        /// Borra los artefactos generados por el builder: la escena Sandbox y toda la carpeta
        /// de contenido generado (Examples). Los scripts y los docs no se tocan.
        static void CleanGenerated()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                AssetDatabase.DeleteAsset(ScenePath);
            if (AssetDatabase.IsValidFolder(ExamplesDir))
                AssetDatabase.DeleteAsset(ExamplesDir);
            AssetDatabase.Refresh();
        }

        static void MakeWall(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = name;
            w.transform.position = pos;
            w.transform.localScale = scale;
            w.GetComponent<Renderer>().sharedMaterial = mat;
        }

        /// Parche de terreno elemental (ER §8): en caja gris, un quad etiquetado + la rejilla
        /// hexagonal leve de ARENA.md. La raíz va SIN escala (los hexágonos no se deforman); el
        /// collider vive en el cube visual y `GetComponentInParent` encuentra la etiqueta igual.
        static void MakePatch(string name, Vector3 pos, Vector2 size, Element element, Color color)
        {
            var patch = new GameObject(name);
            patch.transform.position = pos;
            patch.AddComponent<ElementalSurface>().element = element;

            var grid = patch.AddComponent<HexGridOverlay>();
            grid.area = size;
            // Solo el TONO sale del parche; el alpha lo pone la rejilla (leve, ARENA §1) — pisarlo
            // con el 1 que devuelve Lerp la dibujaría sólida.
            var line = Color.Lerp(color, Color.white, 0.55f);
            grid.lineColor = new Color(line.r, line.g, line.b, grid.lineColor.a);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Suelo elemental";
            visual.transform.SetParent(patch.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            visual.transform.localScale = new Vector3(size.x, 0.04f, size.y);
            visual.GetComponent<Renderer>().sharedMaterial = Mat(color);
        }

        /// Nodo de cristal cargado (ER §8): golpearlo suelta Rayo donde está plantado — junto al
        /// charco produce la ★ agua electrificada, la única del slice sin criaturas de Rayo/Agua.
        static void MakeCrystalNode(Vector3 pos)
        {
            var node = GameObject.CreatePrimitive(PrimitiveType.Cube);
            node.name = "Nodo de cristal (Rayo)";
            node.transform.position = pos;
            node.transform.rotation = Quaternion.Euler(18f, 35f, 12f);   // un cubo torcido lee "cristal" en caja gris
            node.transform.localScale = new Vector3(0.55f, 1.3f, 0.55f);
            node.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.55f, 0.9f, 1f));
            node.AddComponent<CrystalNode>().charge = Element.Rayo;
            node.AddComponent<HitFlash>();                               // que golpearlo SE VEA
        }

        /// Maniquí golpeable: CharacterController (recibe knockback y sigue siendo detectable),
        /// StaggerReceiver con la vida de referencia (5/10), HitReactor y HitFlash. Arranca mirando al jugador.
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
            // Referencia estándar: Vida 5/10 → 2500 HP (500 por punto de hexágono, ver CREATURES §4).
            // El ancla es el TIEMPO: ~1-2 min contra un enemigo que se mueva. El aturdimiento se deriva
            // de la vida (staggerRatio 0.15 → maxStagger 375) y sube con combo; ver StaggerReceiver.
            sr.maxHealth = 2500f; sr.health = 2500f; sr.staggerRatio = 0.15f;
            d.AddComponent<HitReactor>();                                // knockback / carry / flinch
            d.AddComponent<HitFlash>();                                  // destello al recibir (color = efectividad)
            d.AddComponent<StatusController>();                          // slow/vulnerable de las habilidades
            MakeNose(d.transform, new Color(0.95f, 0.95f, 0.95f));       // indicador de "cara"
        }

        /// Rectángulo pequeño a la altura de la "cara", al frente, para ver hacia dónde encara. Sin collider.
        static void MakeNose(Transform parent, Color color)
        {
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            Object.DestroyImmediate(nose.GetComponent<BoxCollider>());   // que no estorbe a hitboxes/raycast
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
