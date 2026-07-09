# OctoTest Runtime Architecture

## Goal

Keep the project small while giving gameplay code a clear ECS base.

Current gameplay is intentionally simple:

```text
field -> units -> snake movement -> unit values -> save/load -> dashboard
```

The architecture should stay proportional to that scope. Add code only when there is a current behavior that needs it.

## Runtime Flow

```text
GameBootstrapper
-> GameServicesInstaller
-> GameServices
-> EcsWorld + EcsSystems
-> SceneInitializer
-> presenters refresh from ECS state
```

`GameBootstrapper` owns the Unity lifetime of the ECS world and systems.

`GameServices` is the only shared runtime object passed into systems. It stores concrete dependencies that already exist: config, unit catalog, save/load, grid, commands and queries.

`EcsCompositionRoot` is the only place that defines system order.

## Script Layout

Use folders by role, not by implementation accident.

```text
App/Bootstrap
  Unity entry points and composition.

Core/SaveLoad
  Project-level save/load infrastructure.

Data
  Project-level ScriptableObject config.

Gameplay/Units/Data
  Authored unit data: catalog and definitions.

Gameplay/Units/Ecs
  ECS components and request components.

Gameplay/Units/Ecs/Systems
  LeoECS systems. One behavior per system.

Gameplay/Units/Runtime
  Concrete runtime API over ECS: commands, queries, snapshots, stats, grid.

Gameplay/Units/Persistence
  Unit save models and save migrations.

Gameplay/Units/Presentation
  Scene presenters and Unity views for units.

Gameplay/Units/Diagnostics
  Debug-only scene tools.

UI
  Scene UI root.

UI/Dashboard
  Concrete dashboard views and buttons.
```

## Source Of Truth

Gameplay truth lives in ECS components and systems.

Scene and UI MonoBehaviours:

- send requests through `UnitCommands`;
- read state through `UnitQuery`;
- update Unity views;
- do not mutate gameplay state directly.

Save data stores ids and values only. It must not store `GameObject`, `MonoBehaviour`, prefab references or animation state.

## Current ECS Model

Components:

- `UnitComponent`
- `UnitValueComponent`
- `UnitGridPositionComponent`
- `UnitWorldPositionComponent`
- `UnitMovingComponent`

Requests:

- `LoadOrCreateUnitsRequest`
- `StartNewGameRequest`
- `SpawnUnitRequest`
- `MoveUnitsRequest`
- `IncreaseRandomUnitValueRequest`
- `RemoveLastUnitRequest`

Systems:

- `LoadOrStartUnitsSystem`
- `StartNewGameSystem`
- `SpawnUnitSystem`
- `MoveUnitsSystem`
- `UnitMovementSystem`
- `IncreaseRandomUnitValueSystem`
- `RemoveLastUnitSystem`
- `SaveUnitsSystem`
- `CleanupUnitRequestsSystem`

## Extension Rules

Add a new command as:

```text
Request component -> one small system -> optional view button -> focused test
```

Add new unit data through `UnitCatalog` first. Add new components only when the value is runtime state, not authored config.

Do not add a manager, registry, factory or interface until there are at least two concrete call sites or implementations that make the abstraction pay for itself.

Keep presenters concrete. Generic UI/popup infrastructure is not the default for this project.
