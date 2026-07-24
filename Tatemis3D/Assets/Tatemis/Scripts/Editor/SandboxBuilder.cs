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

            // 4) Suelo grey-box (50x50) — neutro (el gate es sin reacciones, regla R5).
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
            MakeNose(player.transform, new Color(0.95f, 0.95f, 0.95f));   // indicador de "cara" (claro para que resalte)

            // 7) Maniquíes golpeables — elemento Rayo (el Mineral de Ottorock lo supera → 1.5x visible).
            // Varios y separados, para probar la adquisición en pantalla y el cambio de objetivo.
            MakeDummy("Maniqui A", new Vector3(0f, 1f, 6f), dummySpecies, player.transform);
            MakeDummy("Maniqui B", new Vector3(7f, 1f, 9f), dummySpecies, player.transform);
            MakeDummy("Maniqui C", new Vector3(-7.5f, 1f, 7f), dummySpecies, player.transform);

            // 8) Sistemas: cablear el TypeChart global + HUD de debug.
            var systems = new GameObject("Systems");
            var bootstrap = systems.AddComponent<CombatBootstrap>();
            SetPrivate(bootstrap, "typeChart", typeChart);
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
                      "Shift bloqueo · Espacio salto · Esc libera el ratón.");
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
