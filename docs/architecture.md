# DNDGame Console Architecture

## Summary

The current solution is a playable, cross-platform console RPG vertical slice built around one core rule: the deterministic engine owns truth and the narrator owns presentation.

- Deterministic C# code owns combat, quest progression, hero state, inventory, save/load, and validation.
- The console layer coordinates menu flow, command parsing, and player input/output.
- Infrastructure adapters handle JSON persistence and optional local HTTP-backed narration.
- A local model may add flavor text, but it cannot mutate campaign state.

This design keeps the game portable across macOS, Windows, and Linux while preventing narration from becoming the source of truth.

## Current Vertical Slice

The implemented slice supports:

- Main menu play through `menu`, plus direct `new`, `load`, and `help` commands.
- Three starter classes: Fighter, Ranger, and Mage.
- A short watchtower quest loop that starts at Northgate Outpost.
- Two deterministic encounters: Goblin Scout and Hobgoblin Raider.
- Starter inventory, encounter loot, and a quest completion reward.
- JSON save/load by slot.
- Deterministic narration fallback.
- Optional local LLM narration for opening scenes, quest updates, combat summaries, recaps, journal summaries, and NPC dialogue.
- Unit tests and process-based console integration tests.

## Solution Layout

```text
DNDGame.sln
|- src/
|  |- DNDGame.Console/
|  |  |- Program.cs
|  |  |- ConsoleGameApplication.cs
|  |  |- CommandLineOptions.cs
|  |  |- GameAppSettings.cs
|  |  |- AppSettingsLoader.cs
|  |  |- appsettings.json
|  |
|  |- DNDGame.Core/
|  |  |- Content/
|  |  |- Models/
|  |  |- Services/
|  |
|  |- DNDGame.Infrastructure/
|     |- Narration/
|     |- Persistence/
|
|- tests/
|  |- DNDGame.UnitTests/
|
|- docs/
   |- ideas.md
   |- architecture.md
   |- backlog.md
   |- issues/
```

## Responsibilities By Project

### DNDGame.Console

`DNDGame.Console` is the application boundary.

- `Program.cs` loads configuration, constructs persistence and narration dependencies, and starts the application.
- `CommandLineOptions` parses `menu`, `new`, `load`, and `help` commands with explicit validation.
- `ConsoleGameApplication` owns the interactive main menu, campaign menu, encounter loop, help output, and exit codes.
- `AppSettingsLoader` reads `appsettings.json` and applies environment variable overrides for saves and local LLM settings.

The console project should remain orchestration-focused. It should not own combat rules, quest state transitions, or persistence logic.

### DNDGame.Core

`DNDGame.Core` contains the deterministic game engine and the canonical state model.

- `Models` defines the source-of-truth types such as `CampaignState`, `Hero`, `QuestProgress`, `EncounterState`, `EnemyState`, `InventoryItem`, `JournalEntry`, and `LocalLlmSettings`.
- `Content` defines starter classes, abilities, loot, quest metadata, and encounter definitions in `StarterGameContent`.
- `Services` contains deterministic behavior including:
  - `NewCampaignFactory`
  - `CampaignRecapBuilder`
  - `CampaignProgressionService`
  - `CombatResolutionService`
  - `ISceneNarrator`
  - `ICampaignStorage`

This project has no dependency on HTTP, terminal behavior, or filesystem-specific implementation details.

### DNDGame.Infrastructure

`DNDGame.Infrastructure` contains adapters that connect the deterministic core to the outside world.

- `Persistence/JsonCampaignStorage` serializes campaign state with `System.Text.Json`.
- `Persistence/DefaultSaveDirectoryProvider` resolves a cross-platform default save directory rooted in local application data.
- `Narration/DeterministicSceneNarrator` provides stable fallback text for every narrator entry point.
- `Narration/LocalLlmHttpSceneNarrator` talks to an Ollama-style `POST /api/generate` endpoint and validates the returned payload.
- `Narration/FallbackSceneNarrator` wraps the HTTP narrator and falls back to deterministic narration on failure.

Infrastructure can fail or degrade, but it must never compromise deterministic state integrity.

### DNDGame.UnitTests

`DNDGame.UnitTests` validates both the engine and the console boundary.

- Unit tests cover campaign creation, command parsing, quest progression, combat resolution, save/load roundtrips, the short full-quest loop, and local LLM response validation.
- Process integration tests launch the built console application through `dotnet <assembly.dll>` and assert on exit codes, standard output, and save behavior.

The test suite is intentionally biased toward deterministic, cross-platform checks.

## Runtime Flow

### Startup

1. `Program.cs` loads `GameAppSettings` from `appsettings.json` and environment overrides.
2. The app resolves a save directory and creates `JsonCampaignStorage`.
3. The app chooses narration behavior:
   - deterministic narration only when local narration is disabled
   - `FallbackSceneNarrator` wrapping `LocalLlmHttpSceneNarrator` and `DeterministicSceneNarrator` when enabled
4. `ConsoleGameApplication.RunAsync` parses commands and dispatches the selected flow.

### New Campaign

1. Parse and validate the `new` command or collect inputs from the interactive menu.
2. Create a `CampaignState` through `NewCampaignFactory`.
3. Save the initial campaign immediately.
4. Build opening narration through the configured `ISceneNarrator`.
5. Render the recap and narration, then continue into the campaign loop when launched from the menu.

### Load Campaign

1. Parse the `load` command or select an existing save from the menu.
2. Load the campaign from `JsonCampaignStorage`.
3. Build a deterministic recap through `CampaignRecapBuilder`.
4. Optionally narrate the recap through `ISceneNarrator`.
5. Enter the campaign loop.

### Campaign Loop

The main in-game menu currently supports:

- status recap
- journal listing plus narrated journal summary
- quest advancement through `CampaignProgressionService`
- encounter entry when a current encounter exists
- save
- quit to main menu

Quest advancement is deterministic and stage-driven. The engine decides whether the player moves to the watchtower approach, the courtyard, or the return-to-captain state.

### Encounter Loop

The combat loop operates entirely on deterministic state.

- The player chooses `Attack`, `Defend`, `Special`, or retreat back to the campaign menu.
- `CombatResolutionService` applies damage, defensive effects, encounter resolution, loot, quest-stage changes, and defeat handling.
- Combat summaries are passed to the narrator, but the narrator does not decide outcomes.

## State Ownership Rules

- `CampaignState` is the source of truth.
- The narrator may describe state, but it may not mutate or invent state.
- `StarterGameContent` defines the structured opening slice content used by the deterministic services.
- Journal entries are player-facing history derived from engine events.
- Save files reflect canonical state, not rendered narration.

## Current Domain Model

The current playable slice centers around these core types:

- `Hero`: name, class, level, health, armor, and class-driven combat capabilities.
- `QuestProgress`: quest id, title, objective, and explicit `QuestStage` values.
- `EncounterState` and `EnemyState`: the active fight, enemy stats, and encounter metadata.
- `InventoryItem`: starter items, combat rewards, and completion rewards.
- `JournalEntry`: timestamped records of quest, combat, and travel events.
- `CampaignState`: schema version, slot, region, location, hero, quest, journal, inventory, and optional current encounter.

This model is still intentionally compact, but it now supports a complete short campaign loop instead of just an initial scaffold.

## Narration Contract

The narration boundary is intentionally narrow.

- Input: grounded campaign state plus a deterministic summary or context string.
- Output: short player-facing prose.
- Validation: the local model must return JSON in the form `{"text":"..."}`.
- Guardrails: responses are rejected if they are empty, malformed, code-fenced, too long, or structurally invalid.
- Failure mode: the app falls back to deterministic narration.

This contract supports richer prose without allowing narration to become a gameplay authority.

## Persistence Strategy

- Use `System.Text.Json` with human-readable JSON save files.
- Store one file per save slot.
- Default saves to a local application data directory through `DefaultSaveDirectoryProvider`.
- Allow the save directory to be overridden through configuration or environment variables.
- Keep the persistence format simple until schema migration becomes necessary.

## Cross-Platform Considerations

- Use standard .NET console and filesystem APIs only.
- Avoid shell-specific behavior and platform-specific command syntax.
- Use path APIs instead of hard-coded separators.
- Keep tests resilient to newline differences across operating systems.
- Execute integration coverage through `dotnet` process invocation rather than shell-specific wrappers.

## Near-Term Architectural Gaps

- Add explicit configuration validation for local LLM settings at startup.
- Add save slot metadata and last-played summaries.
- Add schema migration support once the save format evolves.
- Add item use, richer armor handling, and status effects to combat.
- Add a distinct boss encounter to extend the first campaign slice.