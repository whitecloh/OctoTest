# Middle Unity Developer Test

## Overview

This project is a compact Unity implementation for the Octo Games test task.

It covers:

- generic JSON save/load utility;
- LeoECS-based unit gameplay state;
- grid-based unit spawn and snake-like movement;
- unit values, stats, save and restore flow;
- concrete dashboard UI built with uGUI and TextMeshPro;
- MVP-style UI boundary: presenters update views and send commands;
- scene-scoped gameplay entity registry;
- ScriptableObject config and unit catalog;
- focused EditMode tests for gameplay flow and UI refresh throttling.

The scope is intentionally small. The code avoids generic managers, service locators, scene scans in UI loops, repeated `GetComponent` calls, and log spam from frequently updated systems.

## Architecture

The current runtime uses LeoECS as the source of gameplay truth and a small MVP layer for Unity UI.

Runtime flow:

```text
GameBootstrapper
-> GameServicesInstaller
-> GameServices
-> EcsWorld + EcsSystems
-> SceneInitializer
-> presenters refresh Unity views from ECS state
```

Main rules:

- gameplay state lives in ECS components;
- gameplay changes happen in ECS systems;
- UI sends requests through `UnitCommands`;
- UI reads state through `UnitQuery`;
- MonoBehaviours display scene/UI state, not gameplay rules;
- save data stores ids and values, not scene objects or prefab references.

For the detailed architecture notes, see `Assets/_Game/Docs/Architecture.md`.

## Script Layout

```text
Assets/_Game/Scripts/App/Bootstrap
  Unity entry points and runtime composition.

Assets/_Game/Scripts/Core/SaveLoad
  Reusable save/load infrastructure.

Assets/_Game/Scripts/Data
  Project-level ScriptableObject config.

Assets/_Game/Scripts/Gameplay/Entities
  Scene-scoped active gameplay entity tracking.

Assets/_Game/Scripts/Gameplay/Units/Data
  Authored unit data: catalog and definitions.

Assets/_Game/Scripts/Gameplay/Units/Ecs
  ECS components and request components.

Assets/_Game/Scripts/Gameplay/Units/Ecs/Systems
  LeoECS systems. One behavior per system.

Assets/_Game/Scripts/Gameplay/Units/Runtime
  Concrete runtime API over ECS: commands, queries, snapshots, stats, grid.

Assets/_Game/Scripts/Gameplay/Units/Persistence
  Unit save DTOs and save migrations.

Assets/_Game/Scripts/Gameplay/Units/Presentation
  Scene presenters and Unity views for units.

Assets/_Game/Scripts/Gameplay/Units/Diagnostics
  Debug-only scene tools.

Assets/_Game/Scripts/UI
  Scene UI root.

Assets/_Game/Scripts/UI/Dashboard
  Concrete dashboard views and buttons.
```

## Bootstrap

Files:

- `App/Bootstrap/GameBootstrapper.cs`
- `App/Bootstrap/GameServicesInstaller.cs`
- `App/Bootstrap/GameServices.cs`
- `App/Bootstrap/EcsCompositionRoot.cs`
- `App/Bootstrap/SceneInitializer.cs`

`GameBootstrapper` owns the Unity lifetime of the ECS world and systems. It creates the world, builds concrete services, creates systems through `EcsCompositionRoot`, initializes scene presenters, sends the initial load request, and runs ECS every frame.

`EcsCompositionRoot` is the only place that defines system order:

1. `LoadOrStartUnitsSystem`
2. `StartNewGameSystem`
3. `SpawnUnitSystem`
4. `MoveUnitsSystem`
5. `UnitMovementSystem`
6. `IncreaseRandomUnitValueSystem`
7. `RemoveLastUnitSystem`
8. `SaveUnitsSystem`
9. `CleanupUnitRequestsSystem`

## Designer-Facing Data

Designer-facing project values live in `GameConfig`:

- save directory and file name;
- grid dimensions and spacing;
- initial and maximum unit count;
- unit movement speed;
- value increase amount;
- UI refresh interval and label format.

Unit ids, sprites, colors, and start values live in `UnitCatalog` / `UnitDefinition`.

Scene references, prefabs, UI, catalog, and config assets are authored in Unity and validated at boot.

## Save/Load Utility

Files:

- `Core/SaveLoad/ISaveLoadService.cs`
- `Core/SaveLoad/ISaveSerializer.cs`
- `Core/SaveLoad/JsonSaveSerializer.cs`
- `Core/SaveLoad/SaveLoadResult.cs`
- `Core/SaveLoad/SaveLoadService.cs`

Example:

```csharp
[System.Serializable]
public sealed class PlayerProgress
{
    public int Level;
    public int Coins;
}

var saveLoad = new SaveLoadService(new JsonSaveSerializer());
saveLoad.Save("progress", new PlayerProgress { Level = 3, Coins = 250 });

SaveLoadResult<PlayerProgress> result =
    saveLoad.Load("progress", new PlayerProgress());
```

Behavior:

- saves go to `Application.persistentDataPath/Saves` by default;
- file names without extension receive `.json`;
- missing files, empty files, invalid JSON, invalid file names, and I/O errors return a failed result instead of crashing gameplay code;
- existing saves are replaced through a temp file, with a `.bak` copy kept when the platform supports file replacement;
- `JsonSaveSerializer` uses Unity `JsonUtility`, so DTOs should be Unity-serializable field-based classes.

## Unit Gameplay

Current gameplay:

```text
field -> units -> snake movement -> unit values -> save/load -> dashboard
```

Runtime API:

- `UnitCommands` creates ECS request entities;
- `UnitQuery` exposes read-only ECS snapshots and stats inputs;
- `UnitSnapshot` is the read model used by presenters;
- `UnitStatsService` calculates dashboard stats;
- `UnitSpawnGrid` owns grid point positions and reservations.

ECS components:

- `UnitComponent`
- `UnitValueComponent`
- `UnitGridPositionComponent`
- `UnitWorldPositionComponent`
- `UnitMovingComponent`

ECS requests:

- `LoadOrCreateUnitsRequest`
- `StartNewGameRequest`
- `SpawnUnitRequest`
- `MoveUnitsRequest`
- `IncreaseRandomUnitValueRequest`
- `RemoveLastUnitRequest`

Dashboard commands:

- `Play`: starts a new game and creates the configured initial unit count;
- `Spawn`: creates one unit at the first free grid point;
- `Move`: moves stationary units one grid point forward as a chain;
- `Value +`: increases a random unit value;
- `Remove`: removes the last unit.

Movement is split into two systems:

- `MoveUnitsSystem` builds and applies the movement plan by adding `UnitMovingComponent`;
- `UnitMovementSystem` moves units toward their reserved target positions.

If a save happens while a unit is moving, the saved `pointIndex` is the reserved target cell. On load, units are restored as stationary on that cell.

## Unit Save Data

Files:

- `Gameplay/Units/Persistence/UnitsSaveData.cs`
- `Gameplay/Units/Persistence/UnitSaveData.cs`
- `Gameplay/Units/Persistence/IUnitsSaveMigration.cs`
- `Gameplay/Units/Persistence/UnitsSaveMigrationPipeline.cs`

Saved unit state is intentionally minimal:

- `runtimeId`;
- `dataId`;
- `value`;
- `pointIndex`.

The save file does not store `GameObject`, `MonoBehaviour`, prefab, sprite, animation, or transform references. Unit visuals are restored from `UnitCatalog` by `dataId`.

`UnitsSaveData` inherits from `UnitsSaveData_v_1_0`, following a versioned DTO pattern. `UnitsSaveMigrationPipeline` is the extension point for future save versions.

## Dashboard UI

Files:

- `UI/UiRoot.cs`
- `UI/Dashboard/DashboardWindowView.cs`
- `UI/Dashboard/UnitsDashboardView.cs`
- `UI/Dashboard/UnitsStatsView.cs`
- `UI/Dashboard/DashboardButtonData.cs`
- `UI/Dashboard/DashboardButtonPanelView.cs`
- `UI/Dashboard/DashboardButtonView.cs`
- `Gameplay/Units/Presentation/UnitsDashboardPresenter.cs`

The dashboard is intentionally concrete. The project currently has one runtime dashboard, so there is no generic popup service or popup registry.

Scene flow:

1. `SceneInitializer` receives the scene-authored `UiRoot`.
2. `UiRoot` exposes the assigned `UnitsDashboardView`.
3. `UnitsDashboardPresenter` configures title, body, buttons, callbacks, and stats view.
4. Button callbacks call `UnitCommands`.
5. Stats are read through `UnitQuery` and written by `UnitsStatsView`.

Recommended Unity components:

- `Canvas`, `CanvasScaler`, `GraphicRaycaster` on the UI root;
- `Image` for panel/overlay visuals;
- `TextMeshProUGUI` for title, body, stats, and button labels;
- `Button` for actions;
- layout groups for button layout;
- `CanvasGroup` for visibility, interaction, and raycast blocking.

If the project later needs story choices, tutorials, warnings, or confirmations, add concrete views such as `ChoicePopupView` or `WarningPopupView`. Do not add generic popup infrastructure until there is a real second popup workflow.

## UI Refresh

`UnitsDashboardPresenter` refreshes stats through `RefreshThrottle`, using `GameConfig.UnitsViewUpdateInterval`.

The UI does not:

- scan scene objects;
- call `GetComponent` in update loops;
- calculate gameplay state from transforms;
- log every refresh.

This directly addresses the original `CharactersView` performance/refactoring task from the test assignment.

## Scene Presentation

Files:

- `Gameplay/Units/Presentation/UnitsScenePresenter.cs`
- `Gameplay/Units/Presentation/UnitView.cs`
- `Gameplay/Units/Presentation/RefreshThrottle.cs`

`UnitsScenePresenter` synchronizes Unity scene objects with ECS snapshots. It creates or reuses `UnitView` instances, updates visuals from `UnitCatalog`, applies positions, hides removed units, and draws spawn/movement gizmos in the Scene view.

It does not own gameplay rules. Gameplay rules live in ECS systems.

## Gameplay Entity Tracking

Files:

- `Gameplay/Entities/IGameplayEntity.cs`
- `Gameplay/Entities/GameplayEntity.cs`
- `Gameplay/Entities/GameplayEntityRegistry.cs`
- `Gameplay/Entities/GameplayEntityRegistryScope.cs`

`GameplayEntity` registers in `OnEnable` and unregisters in `OnDisable` / `OnDestroy`. `MarkRemoved()` unregisters the entity and prevents future registration.

Query active entities by reusing a caller-owned list:

```csharp
private readonly List<IGameplayEntity> activeEntities = new();

registryScope.Registry.GetActiveEntities(activeEntities);
```

The registry skips and prunes null, destroyed, disabled, inactive, and removed entities.

## Debug Tools

`UnitsDebugTool` is attached to the main scene with auto-run disabled. In Play Mode its context menu can run:

- `Run Safe Stress Test`: bounded spawn/move/value commands with waits between steps;
- `Run Throttling Probe`: several quick value changes followed by a refresh-count check.

The tool does not execute unless explicitly triggered or its auto-run flags are enabled in the inspector.

## Automated Checks

EditMode tests live in `Assets/_Game/Tests/EditMode`:

- `UnitEcsFlowTests` validates bounded spawn, movement, save/load restore, and max-unit behavior;
- `RefreshThrottleTests` verifies that burst UI refresh requests are coalesced until the refresh interval passes.

Run them from Unity via:

```text
Window > General > Test Runner > EditMode > Run All
```

Batchmode command, when the project is not already open in Unity:

```powershell
& 'U:\UNITY\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'U:\UNITY PROJECTS\OctoTest' -runTests -testPlatform EditMode -testResults 'Logs\editmode-test-results.xml' -logFile 'Logs\editmode-test-run.log'
```

Quick compile gate used during refactoring:

```powershell
dotnet build "OctoTest.sln" --no-restore
```

## Manual Checklist

1. Open `Assets/_Game/Scenes/MainScene.unity`.
2. Enter Play Mode and confirm the units dashboard opens.
3. Press `Spawn`, `Move`, `Value +`, and `Remove`.
4. Confirm scene units and dashboard stats update.
5. Re-enter Play Mode and confirm saved unit state is restored.
6. Corrupt the save file manually and confirm the game starts safely instead of crashing.

## Extension Rules

Add a new gameplay command as:

```text
request component -> one small system -> optional view button -> focused test
```

Add new unit authored data through `UnitCatalog` first. Add ECS components only when the value is runtime state, not authored config.

Avoid new managers, registries, factories, or interfaces until there are at least two concrete use cases that make the abstraction useful.

## Possible Extensions

- localized dashboard and popup text keys;
- concrete popup views for story choices, confirmations, warnings, and tutorials;
- Addressables for UI, unit prefabs, backgrounds, videos, and VN content;
- async save/load for larger save files;
- editor validation tools for `GameConfig` and `UnitCatalog`;
- profiler pass for UI rebuilds, allocations, and save/load spikes.
