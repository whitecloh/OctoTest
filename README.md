# Middle Unity Developer Test

## Overview

This project contains a compact Unity implementation for the test task:

- generic JSON save/load utility;
- concrete dashboard popup built with uGUI and TextMeshPro;
- refactored `CharactersView` with throttled UI refresh;
- grid-based unit spawn/movement demo;
- scene-scoped gameplay entity registry;
- ScriptableObject settings and a simple bootstrap layer.

The scope is intentionally small. The code avoids recurring `GetComponent` calls in UI update loops, repeated scene scans, and log spam from frequently updated systems.

## Coding Principles

### Separation of concerns

The implementation keeps gameplay state, scene visuals, UI binding, and persistence in separate classes:

- `GameBootstrapper` starts the scene.
- `GameServicesInstaller` creates runtime services through a small `ServiceRegistry` composition root.
- `SceneInitializer` connects scene-authored objects to services.
- `SaveLoadService` handles file persistence.
- `UnitService` owns unit state, commands, grid reservations, and save/load.
- `UnitCatalog` stores designer-authored unit ids, sprites, colors, and start values.
- `UnitMovementPlanner` calculates grid movement plans using reusable buffers.
- `UnitSaveMapper` converts runtime unit state to and from save DTOs.
- `UnitsSaveMigrationPipeline` provides an extension point for future save migrations.
- `UnitSceneService` owns unit prefab instances and movement animation.
- `UnitSceneService` draws editor gizmos for spawn points and movement order.
- `UnitsDashboardPresenter` connects popup button callbacks to unit commands.
- `UnitStatsService` calculates display data.
- `CharactersView` only writes already calculated stats to a label.
- `GameplayEntityRegistryScope` owns per-scene active gameplay entity tracking.

### Designer-friendly setup

Designer-facing values live in `OctoTestSettings`:

- save directory and file name;
- grid dimensions and spacing;
- initial and maximum unit count;
- unit movement speed;
- UI refresh interval and label format.

Unit ids, sprites, colors, and start values live in `UnitCatalog`. The popup, button prefab, unit prefab, catalog, and scene references are authored in Unity assets/prefabs and validated at boot.

## Save/Load Utility

Files:

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

## Dashboard Popup

Files:

- `UI/Popups/PopupButtonData.cs`
- `UI/Popups/PopupRequest.cs`
- `UI/Popups/PopupService.cs`
- `UI/Popups/IPopupView.cs`
- `UI/Popups/PopupView.cs`
- `UI/Popups/DashboardPopupView.cs`
- `UI/Popups/PopupButtonPanelView.cs`
- `UI/Popups/PopupButtonView.cs`
- `UI/Popups/UiRoot.cs`

The current popup is intentionally concrete: it contains title/body texts, a 1-5 button panel, and the embedded `CharactersView` panel used by the unit dashboard. If another popup type is needed later, it should be added as another isolated `PopupView` implementation and registered separately.

The scene flow is:

1. `UiRoot` stores scene popup views.
2. `SceneInitializer` registers them in `PopupService`.
3. `UnitsDashboardPresenter` shows `DashboardPopupView` with a `PopupRequest`.
4. `PopupButtonPanelView` reuses a small grow-only pool of `PopupButtonView` instances from the assigned button prefab.

`PopupService` also exposes generic `Show<TPopup, TRequest>()` and `LoadAndShow<TPopup, TRequest>()` methods for future isolated popup implementations.

Recommended Unity components:

- `Canvas`, `CanvasScaler`, `GraphicRaycaster` on the UI root;
- `Image` for popup panel/overlay visuals;
- `TextMeshProUGUI` for title, body, stats, and button labels;
- `Button` for actions;
- `HorizontalLayoutGroup` or `VerticalLayoutGroup` for button layout;
- optional `CanvasGroup` for visibility and raycast blocking.

## Unit Demo

Files:

- `Gameplay/Units/UnitRuntimeState.cs`
- `Gameplay/Units/UnitCatalog.cs`
- `Gameplay/Units/UnitDefinition.cs`
- `Gameplay/Units/UnitService.cs`
- `Gameplay/Units/UnitMovementPlanner.cs`
- `Gameplay/Units/UnitSaveMapper.cs`
- `Gameplay/Units/UnitSpawnGrid.cs`
- `Gameplay/Units/UnitSceneService.cs`
- `Gameplay/Units/UnitView.cs`
- `Gameplay/Units/UnitStats.cs`
- `Gameplay/Units/UnitStatsService.cs`
- `Gameplay/Units/UnitsDashboardPresenter.cs`
- `Gameplay/Units/UnitsSaveData.cs`
- `Gameplay/Units/UnitSaveData.cs`
- `Gameplay/Units/UnitsSaveMigrationPipeline.cs`
- `Views/CharactersView.cs`

Dashboard buttons:

- `Play`: starts a new game and creates the configured initial unit count;
- `Spawn`: creates one unit at the first free grid point;
- `Move`: moves stationary units one grid point forward as a chain;
- `Value +`: increases a random unit value;
- `Remove`: removes the last unit.

New units are created from a random `UnitDefinition` in `UnitCatalog`. The selected definition controls the saved `dataId`, initial value, sprite, and color.

Saved unit state is minimal:

- `runtimeId`;
- `dataId`;
- `value`;
- `pointIndex`.

`UnitsSaveData` inherits from `UnitsSaveData_v_1_0`, following a versioned DTO pattern. Save fields are private `[SerializeField]` fields exposed through properties and small methods, so runtime code does not depend on raw serialized fields. `UnitsSaveMigrationPipeline` is kept as the extension point for future save migrations.

Runtime movement state is not saved. If a save happens while a unit is moving, the saved `pointIndex` is the target cell. On load, units are restored as stationary on that cell. Invalid out-of-grid cells and duplicate occupied cells are skipped.

UI stats are event-driven and throttled by `OctoTestSettings.UnitsViewUpdateInterval`. `CharactersView` does not scan scene objects and does not own gameplay references.

`UnitSceneService` can draw spawn point gizmos in the Scene view. The first point is marked with a separate origin color, each point is drawn as a wire sphere, and optional path lines show the current movement order.

## Gameplay Entity Tracking

Files:

- `Gameplay/Entities/IGameplayEntity.cs`
- `Gameplay/Entities/GameplayEntity.cs`
- `Gameplay/Entities/GameplayEntityRegistry.cs`
- `Gameplay/Entities/GameplayEntityRegistryScope.cs`

`GameplayEntity` registers in `OnEnable` and unregisters in `OnDisable`/`OnDestroy`. `MarkRemoved()` unregisters the entity and prevents future registration. Registration is scoped through `GameplayEntityRegistryScope`, so each scene/context can own its own registry instead of relying on static global state.

Query without allocations by reusing a caller-owned list:

```csharp
private readonly List<IGameplayEntity> activeEntities = new();

registryScope.Registry.GetActiveEntities(activeEntities);
```

The registry skips and prunes null, destroyed, disabled, inactive, and removed entities.

## Debug Tools

`UnitsDebugTool` is attached to the main scene with auto-run disabled. In Play Mode its context menu can run:

- `Run Safe Stress Test`: bounded spawn/move/value commands with waits between steps;
- `Run Throttling Probe`: several quick value changes followed by a refresh-count check.

The tool uses conservative limits and does not execute unless explicitly triggered or its auto-run flags are enabled in the inspector.

## Automated Checks

EditMode tests live in `Assets/Tests/EditMode`:

- `UnitServiceStressTests` runs bounded spawn, value, move, completion, and save validation without opening a scene;
- `RefreshThrottleTests` verifies that burst UI refresh requests are coalesced until the refresh interval passes.

Run them from Unity via `Window > General > Test Runner > EditMode > Run All`.

Batchmode command, when the project is not already open in Unity:

```powershell
& 'U:\UNITY\6000.3.10f1\Editor\Unity.exe' -batchmode -quit -projectPath 'U:\UNITY PROJECTS\OctoTest' -runTests -testPlatform EditMode -testResults 'Logs\editmode-test-results.xml' -logFile 'Logs\editmode-test-run.log'
```

## Manual Checklist

1. Open `Assets/Scenes/MainScene.unity`.
2. Enter Play Mode and confirm the dashboard popup opens.
3. Press `Spawn`, `Move`, `Value +`, and `Remove`.
4. Confirm scene units and dashboard stats update.
5. Re-enter Play Mode and confirm saved unit state is restored.
6. Corrupt the save file manually and confirm the game creates a valid initial state instead of crashing.

## Possible Extensions

- localized popup text keys;
- separate popup implementations for other use cases;
- popup queueing/stacking;
- a shared UI item pool utility if several screens need the same reuse pattern.
