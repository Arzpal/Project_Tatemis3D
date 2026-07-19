#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tatemis.EditorTools
{
    /// Genera assets de ejemplo (TypeChart, habilidades por fases y 1 criatura) de forma correcta,
    /// sin escribir YAML a mano. Menú: Tatemis -> Generar contenido de ejemplo.
    public static class ExampleContentGenerator
    {
        const string Dir = "Assets/Tatemis/Examples";

        [MenuItem("Tatemis/Generar contenido de ejemplo")]
        public static void Generate()
        {
            EnsureDir();

            var chart = CreateOrLoad<TypeChart>($"{Dir}/TypeChart.asset");
            EditorUtility.SetDirty(chart);

            // Golpe — ataque básico universal (1 fase, sin coste; el slot no aplica al básico).
            var golpe = CreateOrLoad<AbilityData>($"{Dir}/Ability_GolpeBasico.asset");
            golpe.id = "golpe_basico"; golpe.displayName = "Golpe"; golpe.slot = AbilitySlot.Flex;
            golpe.energyCost = 0f; golpe.cooldown = 0.4f;
            golpe.phases = new List<AbilityPhase> {
                new AbilityPhase {
                    name = "Golpe",
                    exit = new AfterSeconds { seconds = 0.4f },
                    effects = new List<AbilityEffect> {
                        new MeleeHitbox { range = 2.2f, radius = 1.6f },
                        new DealDamage { amount = 12f, staggerAmount = 10f, when = ApplyWhen.OnHit }
                    }
                }
            };
            EditorUtility.SetDirty(golpe);

            // Embestida — dash + daño (1 fase).
            var embestida = CreateOrLoad<AbilityData>($"{Dir}/Ability_Embestida.asset");
            embestida.id = "embestida"; embestida.displayName = "Embestida"; embestida.slot = AbilitySlot.Movilidad;
            embestida.energyCost = 25f; embestida.cooldown = 3f;
            embestida.phases = new List<AbilityPhase> {
                new AbilityPhase {
                    name = "Embestida",
                    exit = new AfterSeconds { seconds = 0.25f },
                    effects = new List<AbilityEffect> {
                        new DashMotion { distance = 6f, time = 0.18f },
                        new DealDamage { amount = 8f, staggerAmount = 18f, when = ApplyWhen.OnCast }
                    }
                }
            };
            EditorUtility.SetDirty(embestida);

            // Ráfaga — mancha el terreno + ralentiza (1 fase).
            var rafaga = CreateOrLoad<AbilityData>($"{Dir}/Ability_Rafaga.asset");
            rafaga.id = "rafaga"; rafaga.displayName = "Ráfaga"; rafaga.slot = AbilitySlot.Flex;
            rafaga.energyCost = 35f; rafaga.cooldown = 5f;
            rafaga.phases = new List<AbilityPhase> {
                new AbilityPhase {
                    name = "Ráfaga",
                    exit = new AfterSeconds { seconds = 0.3f },
                    effects = new List<AbilityEffect> {
                        new StainTerrain { radius = 3.5f },
                        new ApplyStatus { type = StatusType.Ralentizado, duration = 3f, when = ApplyWhen.OnCast }
                    }
                }
            };
            EditorUtility.SetDirty(rafaga);

            // Emberling — Luchador de Fuego.
            var creature = CreateOrLoad<CreatureSpeciesData>($"{Dir}/Creature_Emberling.asset");
            creature.id = "emberling"; creature.displayName = "Emberling";
            creature.element = Element.Fuego; creature.archetype = Archetype.Luchador;
            creature.forms = new List<EvolutionForm> {
                new EvolutionForm {
                    formName = "Base",
                    stats = new BaseStats { maxHealth = 120f, damage = 12f, moveSpeed = 6f },
                    // básico = golpe (universal, sin coste). signature reutiliza golpe como placeholder.
                    movilidad = embestida, flex = rafaga, signature = golpe, basic = golpe,
                    block = new BlockParams { energyRegenMultiplier = 2.5f, weaknessDamageMultiplier = 2f },
                    levelsInForm = 2,
                }
            };
            EditorUtility.SetDirty(creature);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Tatemis] Contenido de ejemplo generado en {Dir}");
            EditorGUIUtility.PingObject(creature);
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
