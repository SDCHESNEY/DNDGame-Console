# DNDGame Console Architecture

## Summary

This project should use a hybrid architecture.

- Deterministic .NET code owns rules, combat, persistence, progression, validation, and state transitions.
- A local LLM owns narration, scene flavor, recaps, and optional dialogue generation.
- JSON save files are the canonical persistence format.
- The console app is a thin shell over application services and infrastructure adapters.

This keeps the game portable across macOS, Windows, and Linux while preventing the model from becoming the source of truth.

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
```

## Responsibilities By Project

### DNDGame.Console

- Parse command-line arguments.
- Load local configuration.
- Coordinate new game and load game flows.
- Print player-facing output and exit codes.
- Stay thin. Do not put combat rules, persistence rules, or campaign mutation logic here.

### DNDGame.Core

- Define the canonical game state models.
- Define interfaces for persistence and narration boundaries.
- Implement deterministic campaign creation and recap behavior.
- Hold portable business rules that do not depend on filesystem, terminal, or HTTP concerns.

### DNDGame.Infrastructure

- Persist saves to disk.
- Normalize save locations across operating systems.
- Provide a deterministic narrator now and a local LLM-backed narrator later.
- Translate external concerns into core interfaces.

### DNDGame.UnitTests

- Verify campaign initialization.
- Verify serialization and save/load behavior.
- Expand to cover combat, quest progression, command parsing, and prompt validation.

## Runtime Flow

### New Game

1. Parse arguments.
2. Build a new `CampaignState` using deterministic rules.
3. Save the initial campaign state immediately.
4. Pass grounded state into the narration boundary.
5. Render the deterministic summary plus narrated opening scene.

### Load Game

1. Parse arguments.
2. Load a save slot from disk.
3. Build a concise recap from canonical state.
4. Render the recap and continue into future gameplay flows.

## State Ownership Rules

- `CampaignState` is the source of truth.
- The narrator may describe state but may not mutate it.
- Save files should be versioned from the first playable build.
- Journal text is player-facing memory.
- Structured quest, hero, and world fields are engine-facing memory.

## Initial Domain Model

The first scaffold uses a deliberately small state model.

- `Hero`: name, class, level, and health.
- `QuestProgress`: active quest id, title, objective, completion flag.
- `JournalEntry`: timestamped narrative history.
- `CampaignState`: schema version, slot, location, region, hero, active quest, and journal.

This is enough to support a playable vertical slice without committing to full tabletop complexity too early.

## LLM Integration Contract

The codebase should target a local LLM through a narrow interface.

- Input: grounded campaign state and explicit narrator constraints.
- Output: plain text scene narration or structured validated response shapes later.
- Failure mode: if the model is unavailable or invalid, fall back to deterministic narration.

The current scaffold intentionally ships with deterministic narration only. That keeps the runnable baseline stable before adding HTTP, prompt templates, and response validation.

## Persistence Strategy

- Use `System.Text.Json`.
- Store one JSON file per save slot.
- Use a cross-platform save root based on local application data.
- Keep save files human-readable for debugging and manual recovery.
- Add backward-compatible migrations once the schema starts changing.

## Cross-Platform Considerations

- Use standard .NET console APIs only.
- Avoid shell-specific behavior.
- Avoid hard-coded paths and separators.
- Keep line-ending-sensitive assertions out of gameplay logic.
- Use explicit exit codes for CLI flows.

## Next Architectural Extensions

- Add combat services and enemy models to `DNDGame.Core`.
- Add content definitions for quests, encounters, and loot.
- Add a real local LLM adapter in `DNDGame.Infrastructure`.
- Add integration tests that execute the console app as a process.
- Add a richer interactive loop once deterministic flows are stable.