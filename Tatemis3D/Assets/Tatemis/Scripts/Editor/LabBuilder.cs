#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// **El laboratorio de saturación** ([ER §0.10](../../../../docs/ELEMENTAL-REACTIONS.md)).
    ///
    /// Escena aparte de la Sandbox **a propósito**: la Sandbox *es el gate* —el banco de feel de
    /// Ottorock en melee puro, con su centro neutro deliberado (R5)— y meterle el laboratorio dentro
    /// contaminaría justo la pregunta que más importa: *«¿es divertido pegarle al maniquí?»* no se
    /// puede responder con medio tablero reaccionando.
    ///
    /// 🔑 **Esto no existe para VER el sistema —eso ya se ve en el mundo— sino para MEDIRLO.** Quedan
    /// ocho números sin afinar y ninguno se afina mirando; se afinan con **lectura numérica y
    /// repetición rápida**. De ahí las tres cosas que definen el diseño de la escena:
    ///  ① **manchas de tamaño EXACTO** (1, 3, 4 y 6 celdas) — imposible con el horneado por biomas,
    ///     que produce amebas impredecibles; por eso existe el modo autorado de `ElementalGrid`;
    ///  ② **una mancha por elemento**, y como el spray cicla el entrante con F2, con seis bancos se
    ///     alcanzan las **36 casillas** sin construir 36 sitios;
    ///  ③ **pares de manchas pegadas**, que es lo único que permite ver el **derrame** de la
    ///     amplificación.
    /// El instrumental y el control del tiempo viven en `Debug/LabHUD`.
    public static class LabBuilder
    {
        const string ExamplesDir = "Assets/Tatemis/Examples";
        const string ScenesDir   = "Assets/Tatemis/Scenes";
        const string ScenePath   = ScenesDir + "/Laboratorio.unity";

        const float CellRadius = 3.5f;

        // Formas de mancha, en offsets axiales desde el ancla. Todas CONTIGUAS: si no lo fueran, la
        // rejilla las partiría en varias manchas y el banco mediría otra cosa de la que dice medir.
        static readonly Vector2Int[] S1 = { new Vector2Int(0, 0) };
        static readonly Vector2Int[] S3 = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
        static readonly Vector2Int[] S4 = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1),
                                            new Vector2Int(1, -1) };
        static readonly Vector2Int[] S6 = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1),
                                            new Vector2Int(0, 1), new Vector2Int(-1, 1), new Vector2Int(2, -1) };

        [MenuItem("Tatemis/Laboratorio/Construir")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ottorock = OttorockContent.Generate();
            var dummySpecies = OttorockContent.DummySpecies(Element.Rayo, "ManiquiLab");
            var reactionTable = ReactionTableContent.Generate();
            var typeChart = AssetDatabase.LoadAssetAtPath<TypeChart>($"{ExamplesDir}/TypeChart.asset");
            if (ottorock == null || typeChart == null)
            {
                Debug.LogError("[Lab] Falta contenido base. Construye antes la Sandbox " +
                               "(Tatemis ▸ Sandbox ▸ Construir escena de prueba) y repite.");
                return;
            }

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(52f, -28f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.30f, 0.30f, 0.32f));

            // ───────────── Los bancos ─────────────
            var patches = new List<ElementalGrid.AuthoredPatch>();
            var points = new List<Vector3>();
            var labels = new List<string>();

            // Fila A — TAMAÑO DE MANCHA. Mismo elemento en las cuatro, así que lo único que cambia
            // entre bancos es el tamaño: es la comparación limpia que hace medible el número de
            // ARENA §1b, que pasó de estética a balance y sigue sin afinar.
            Bench(patches, points, labels, "A1 · mancha de 1", Element.Naturaleza, -10, -5, S1);
            Bench(patches, points, labels, "A2 · mancha de 3", Element.Naturaleza, -5, -5, S3);
            Bench(patches, points, labels, "A3 · mancha de 4", Element.Naturaleza, 0, -5, S4);
            Bench(patches, points, labels, "A4 · mancha de 6", Element.Naturaleza, 5, -5, S6);

            // Fila B — UN BANCO POR ELEMENTO. Con el spray ciclando el entrante (F2), seis bancos
            // cubren las 36 casillas de la matriz.
            Bench(patches, points, labels, "B1 · Naturaleza", Element.Naturaleza, -11, 1, S4);
            Bench(patches, points, labels, "B2 · Agua", Element.Agua, -7, 1, S4);
            Bench(patches, points, labels, "B3 · Fuego", Element.Fuego, -3, 1, S4);
            Bench(patches, points, labels, "B4 · Mineral", Element.Mineral, 1, 1, S4);
            Bench(patches, points, labels, "B5 · Rayo", Element.Rayo, 5, 1, S4);
            Bench(patches, points, labels, "B6 · Aire", Element.Aire, 9, 1, S4);

            // Fila C — MANCHAS PEGADAS. El ancla del segundo va 2 en q, que es justo lo que hace que
            // sus celdas toquen las del primero: es lo único que permite ver el DERRAME de la
            // amplificación cargando la mancha de al lado.
            Bench(patches, points, labels, "C1 · Mineral pegado a Rayo", Element.Mineral, -9, 7, S3);
            Bench(patches, null, null,     "C1b · Rayo", Element.Rayo, -7, 7, S3);
            Bench(patches, points, labels, "C2 · Agua pegada a Fuego", Element.Agua, 1, 7, S3);
            Bench(patches, null, null,     "C2b · Fuego", Element.Fuego, 3, 7, S3);

            // ───────────── Rejilla en modo autorado ─────────────
            var gridGo = new GameObject("ElementalGrid (autorada)");
            var grid = gridGo.AddComponent<ElementalGrid>();
            grid.cellRadius = CellRadius;
            grid.biomes.Clear();          // sin biomas: manda `authored`
            grid.authored = patches;
            grid.authoredSpan = 24;

            // ───────────── Jugador ─────────────
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player (Ottorock)";
            player.transform.position = new Vector3(0f, 1f, -14f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.25f, 0.5f, 0.9f));

            player.AddComponent<CharacterController>();
            player.AddComponent<StatusController>();
            player.AddComponent<ElementalEnergy>();
            player.AddComponent<AbilityCaster>();
            player.AddComponent<InputReader>();
            var targeting = player.AddComponent<TargetingSystem>();
            var controller = player.AddComponent<CreatureController>();
            controller.data = new CreatureInstance { species = ottorock, formIndex = 0, level = 1 };
            var spray = player.AddComponent<ElementalSpray>();
            spray.current = Element.Mineral;

            // ───────────── Sistemas ─────────────
            var systems = new GameObject("Systems");
            var bootstrap = systems.AddComponent<CombatBootstrap>();
            SetPrivate(bootstrap, "typeChart", typeChart);
            var reactions = systems.AddComponent<ReactionSystem>();
            SetPrivate(reactions, "table", reactionTable);

            // La zona se abre GRANDE y centrada: aquí el cerco no debe cortar ningún banco (eso ya se
            // valida en el mundo), o cada mancha mediría distinto según dónde cayera el muro.
            var zoneGo = new GameObject("CombatZone");
            zoneGo.transform.SetParent(systems.transform, true);   // `CombatDirector` la busca entre sus HIJOS
            var zone = zoneGo.AddComponent<CombatZone>();
            zone.rings = 14;
            zone.maxRings = 18;
            systems.AddComponent<CombatDirector>();

            var lab = systems.AddComponent<LabHUD>();
            lab.player = player.transform;
            lab.benchPoints = points.ToArray();
            lab.benchLabels = labels.ToArray();

            // ───────────── Maniquíes: uno por elemento, plantado encima ─────────────
            for (int i = 0; i < 6; i++)
            {
                var anchor = new HexCoord(-11 + i * 4, 1);
                Vector3 p = anchor.ToWorld(CellRadius);
                MakeDummy($"Maniquí {i + 1}", new Vector3(p.x, 1f, p.z), dummySpecies, player.transform);
            }

            // ───────────── Cámara ─────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camComp = camGo.AddComponent<Camera>();
            camComp.fieldOfView = 55f;
            camComp.clearFlags = CameraClearFlags.SolidColor;
            camComp.backgroundColor = new Color(0.09f, 0.09f, 0.11f);
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 4f, -20f);

            var camRig = player.AddComponent<PlayerCameraRig>();
            camRig.follow = player.transform;
            camRig.targeting = targeting;
            camRig.input = player.GetComponent<InputReader>();
            camRig.cam = camComp;

            if (!AssetDatabase.IsValidFolder(ScenesDir))
                AssetDatabase.CreateFolder("Assets/Tatemis", "Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[Lab] Laboratorio construido en " + ScenePath + ". Pulsa Play.\n" +
                      "MEDIR: P pausa · O cámara lenta (×1 → ×0,25 → ×0,1) · R reset del tablero · " +
                      "1-9 saltar de banco · F2 cicla el elemento del spray · G dispara · H modo instantáneo.\n" +
                      "Fila A = tamaño de mancha (1/3/4/6) · Fila B = un banco por elemento (cicla el " +
                      "entrante para cubrir las 36 casillas) · Fila C = manchas pegadas, para ver el derrame.\n" +
                      "⚠️ Con cámara lenta se ve la ONDA anillo a anillo: es la única forma de comprobar " +
                      "que arranca donde golpeaste y que cobra UNA vez por criatura.");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        [MenuItem("Tatemis/Laboratorio/Limpiar (borrar escena)")]
        public static void Clean()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                AssetDatabase.DeleteAsset(ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[Lab] Escena del laboratorio borrada. Los assets compartidos no se tocan.");
        }

        /// Añade una mancha autorada y, si se pasan las listas, un punto de teletransporte. Los bancos
        /// "b" (la segunda mitad de un par pegado) pasan null: se llega a ellos desde su pareja.
        static void Bench(List<ElementalGrid.AuthoredPatch> into, List<Vector3> points, List<string> labels,
                          string label, Element element, int q, int r, Vector2Int[] shape)
        {
            var p = new ElementalGrid.AuthoredPatch { label = label, element = element };
            for (int i = 0; i < shape.Length; i++)
                p.cells.Add(new Vector2Int(q + shape[i].x, r + shape[i].y));
            into.Add(p);

            if (points == null) return;
            // El punto de salto queda UN POCO POR DELANTE del banco, no encima: el spray dispara a
            // `reach` unidades hacia donde miras, y aterrizar dentro de la mancha te dejaría
            // disparando al otro lado de ella.
            Vector3 w = new HexCoord(q, r).ToWorld(CellRadius);
            points.Add(new Vector3(w.x, 0f, w.z - 7f));
            labels.Add(label);
        }

        /// Calcado del maniquí de la Sandbox, que es el que está probado. Un maniquí sin
        /// `StaggerReceiver` configurado **no recibe el payload de una reacción**, y entonces el banco
        /// mide la explosión pero no lo que hace — que es la mitad de lo que se viene a medir.
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
        }

        static Material Mat(Color c)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(sh != null ? sh : Shader.Find("Standard"));
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }

        static void SetPrivate(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[Lab] No existe el campo '{field}' en {target.GetType().Name}"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
