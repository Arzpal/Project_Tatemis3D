# Assets/Tatemis — Scaffold del prototipo

Estructura base de scripts para el Nivel A. Todo en el namespace `Tatemis`. Es un **esqueleto compilable**: muchos métodos tienen `// TODO` — están pensados para llenarse durante las Fases 0–1 (ver [../../../docs/TASKS.md](../../../docs/TASKS.md)).

## Estructura
```
Scripts/
  Core/         Enums, ICombatant, CombatServices (localizador de servicios)
  Data/         TypeChart, AbilityData, CreatureSpeciesData, AbilityEffects (ScriptableObjects + efectos)
  Combat/       CreatureController, AbilityCaster, ElementalEnergy, StaggerReceiver,
                CaptureSystem, TargetingSystem, TeamManager, CreatureInstance
  Input/        InputReader (new Input System: teclado + gamepad + rebinding)
  Environment/  ElementalSurface, ReactionSystem
  Debug/        DebugHUD (barras IMGUI de energía/aturdimiento)
  Editor/       ExampleContentGenerator (menú "Tatemis")
```

## Puesta en marcha (Fase 0)
1. **Input:** el `InputReader` usa el **new Input System** (ya instalado por el template URP): teclado/ratón + cualquier gamepad, con rebinding en runtime. Deja `Project Settings → Player → Active Input Handling` en **`Input System Package (New)`** (default del template) o `Both`.
2. Crea la escena `Scenes/Sandbox.unity` con un suelo, un jugador (cápsula) y un maniquí (cápsula).
3. **Jugador:** añade `CreatureController` (arrastra los componentes requeridos: `CharacterController`, `ElementalEnergy`, `AbilityCaster`), `InputReader`, `TargetingSystem`.
4. **Maniquí:** añade `StaggerReceiver` (+ un collider).
5. **Contenido de ejemplo (rápido):** menú **`Tatemis → Generar contenido de ejemplo`** crea un `TypeChart`, 3 habilidades y 1 criatura (`Emberling`, Luchador de Fuego) en `Assets/Tatemis/Examples/`. *(O créalos a mano con **Create → Tatemis**.)* Asigna el `Creature_Emberling` al `CreatureInstance` del `CreatureController` del jugador.
6. Pon un `ReactionSystem` en la escena y asígnale el `TypeChart` (registra los servicios en `CombatServices`).
7. Añade el componente **`DebugHUD`** a cualquier GameObject para ver en pantalla las barras de energía y de aturdimiento del objetivo.

## Notas
- **Ejecuta los efectos de habilidad en anim-events**, no en el frame del input (el `AbilityCaster` los ejecuta inmediato solo como placeholder temprano).
- Gotcha de Unity: los `struct` añadidos en el inspector arrancan en `default` (ceros). Por eso `BlockParams` se valida en runtime (regen ≤ 0 → usa un fallback). Al crear formas, pon `energyRegenMultiplier ≈ 2.5` y `weaknessDamageMultiplier = 2`.
- Cuando el proyecto crezca, considera un `Tatemis.asmdef` para compilaciones más rápidas.
