# DNDGame Console

A cross-platform .NET console RPG inspired by tabletop DND, with deterministic game systems and an optional local LLM narrator.

The project is designed around one strict rule: the game engine owns truth, and the narrator owns presentation. Combat, quest progression, inventory, leveling, persistence, and validation all live in deterministic C# code. A local model can add scene flavor, recaps, journal summaries, and NPC dialogue, but it does not decide game state.

## Current Status

The current vertical slice is playable and includes:

- A cross-platform .NET console application for macOS, Windows, and Linux.
- A main menu flow for starting or loading a campaign.
- Three starter classes: Fighter, Ranger, and Mage.
- A short quest loop built around the ruined watchtower.
- Two deterministic combat encounters.
- Starter inventory, encounter loot, and a quest completion reward.
- JSON save/load support with one file per save slot.
- Deterministic narration fallback.
- Optional local HTTP-backed LLM narration with validation and fallback.
- Unit and process-based integration tests.

## Project Goals

This project aims to bridge the gap between two extremes:

- Traditional CRPGs with reliable systems but limited narrative flexibility.
- Pure LLM adventures with flexible prose but weak consistency and poor long-session reliability.

The intended result is a console RPG that feels like a guided solo campaign with stable rules, persistent state, and optional local-model narration.

## Architecture

The solution uses a hybrid design.

- `DNDGame.Console` handles command-line parsing, menu flow, console input/output, and app configuration.
- `DNDGame.Core` contains deterministic game models, starter content, quest progression, combat resolution, and narration interfaces.
- `DNDGame.Infrastructure` contains persistence and narration adapters.
- `DNDGame.UnitTests` contains unit and process integration tests.

### Solution Layout

```text
DNDGame.sln
|- src/
|  |- DNDGame.Console/
|  |- DNDGame.Core/
|  |- DNDGame.Infrastructure/
|- tests/
|  |- DNDGame.UnitTests/
|- docs/
|  |- ideas.md
|  |- architecture.md
|  |- backlog.md
```

### Implementation Principles

- The game state is the source of truth.
- Combat is deterministic and resolved entirely in C#.
- Quest progression is deterministic and state-driven.
- Save files are JSON and human-readable.
- Local LLM output is validated before use.
- If local LLM narration fails, the app falls back to deterministic narration automatically.

## Gameplay Overview

The current short loop works like this:

1. Create a hero.
2. Travel from Northgate Outpost to the watchtower approach.
3. Defeat the Goblin Scout.
4. Push into the courtyard.
5. Defeat the Hobgoblin Raider.
6. Return to Captain Elira.
7. Receive a level-up and a completion reward.

### Starter Classes

#### Fighter

- Highest health and strongest armor.
- Special action: `Shield Bash`.
- Defensive ability: `Guard Stance`.

#### Ranger

- Balanced health and strong basic attack.
- Special action: `Aimed Shot`.
- Defensive ability: `Evasion`.

#### Mage

- Lowest health and highest burst damage.
- Special action: `Arc Bolt`.
- Defensive ability: `Warding Sigil`.

### Starter Rewards and Loot

- All classes start with a `Minor Potion`.
- The Goblin Scout drops the `Scout's Satchel`.
- The Hobgoblin Raider drops the `Watchtower Sigil`.
- Captain Elira rewards the player with a `Frontier Charm`.

## Requirements

### Runtime

- .NET SDK 9.0 or later.

The project has already been validated in this workspace with `.NET SDK 9.0.301`.

### Optional Local LLM

The local narrator is optional. The game runs without it.

If you want live narration from a local model, you need:

- A local inference service that exposes an HTTP API.
- An endpoint compatible with the app's current request shape.
- A model that can follow instructions and return valid JSON in the form `{"text":"..."}`.

The current implementation targets an Ollama-style `POST /api/generate` flow.

## Building The Project

From the repository root:

```bash
dotnet build DNDGame.sln
```

## Running Tests

Run the full test suite:

```bash
dotnet test DNDGame.sln
```

The tests cover:

- Campaign creation.
- Save/load roundtrips.
- Command-line parsing.
- Quest progression.
- Combat resolution.
- Full quest loop completion.
- Local LLM narration validation.
- Console process integration for menu, save, and combat flows.

## Running The Game

### Interactive Menu

The default way to play is the interactive menu:

```bash
dotnet run --project src/DNDGame.Console -- menu
```

You can also omit `menu`, because no arguments default to menu mode:

```bash
dotnet run --project src/DNDGame.Console
```

### Direct New Campaign

```bash
dotnet run --project src/DNDGame.Console -- new --slot alpha --name Mira --class mage
```

Supported classes:

- `fighter`
- `ranger`
- `mage`

### Load An Existing Save

```bash
dotnet run --project src/DNDGame.Console -- load --slot alpha
```

### Help

```bash
dotnet run --project src/DNDGame.Console -- help
```

## Playing The Game

### Main Menu

When the game starts in menu mode, you can:

- Start a new game.
- Load an existing save.
- Show help.
- Quit.

### Campaign Menu

Inside a campaign, the current implementation supports:

- `Status`: view the current recap, hero health, quest state, abilities, inventory, and encounter status.
- `Journal`: list journal entries and receive a narrator summary.
- `Advance quest`: move the campaign forward when the rules allow it.
- `Fight current encounter`: enter the deterministic combat loop when an encounter is active.
- `Save`: save the current campaign state.
- `Quit to main menu`: save and return.

### Combat

Each encounter currently supports:

- `Attack`
- `Defend`
- `Special`
- `Retreat to campaign menu`

Combat outcomes are resolved by the engine. The narrator only describes those results after the rules engine commits them.

## Configuration

The console project includes a default configuration file:

- [src/DNDGame.Console/appsettings.json](src/DNDGame.Console/appsettings.json)

Current values:

```json
{
  "saveDirectory": null,
  "enableLocalLlmNarration": false,
  "localLlm": {
    "endpointUrl": "http://localhost:11434",
    "modelName": "local-dm",
    "narrativeVerbosity": "balanced"
  }
}
```

### Configuration Fields

#### `saveDirectory`

Optional override for where save files are stored.

If `null`, the app uses the platform's local application data directory.

#### `enableLocalLlmNarration`

- `false`: always use deterministic narration.
- `true`: try HTTP-backed local narration first, then fall back automatically if it fails.

#### `localLlm.endpointUrl`

Base URL for the local LLM service.

Default:

```text
http://localhost:11434
```

#### `localLlm.modelName`

The model identifier sent to the local inference service.

#### `localLlm.narrativeVerbosity`

Allowed values:

- `concise`
- `balanced`
- `rich`

Invalid local LLM settings fail at startup when `enableLocalLlmNarration` is `true`.

## Environment Variable Overrides

The app also supports environment-based overrides at startup.

Supported variables:

- `DNDGAME_SAVE_DIRECTORY`
- `DNDGAME_ENABLE_LOCAL_LLM_NARRATION`
- `DNDGAME_LLM_ENDPOINT_URL`
- `DNDGAME_LLM_MODEL`
- `DNDGAME_LLM_VERBOSITY`

Example:

```bash
export DNDGAME_ENABLE_LOCAL_LLM_NARRATION=true
export DNDGAME_LLM_ENDPOINT_URL=http://localhost:11434
export DNDGAME_LLM_MODEL=llama3
dotnet run --project src/DNDGame.Console -- new --slot llm-demo --name Tarin --class ranger
```

## Save Files

Save files are written as one JSON file per slot.

### Default Save Location

The app resolves the save directory from the platform local application data folder and appends:

```text
DNDGame.Console/saves
```

Examples:

- macOS: `~/Library/Application Support/DNDGame.Console/saves`
- Windows: `%LOCALAPPDATA%\DNDGame.Console\saves`
- Linux: the directory resolved by the current .NET local application data mapping, then `DNDGame.Console/saves`

### Save File Naming

If the slot is `alpha`, the save file will be:

```text
alpha.json
```

Invalid filename characters are normalized when saves are written.

## Local LLM Narration

The current HTTP narrator lives in:

- [src/DNDGame.Infrastructure/Narration/LocalLlmHttpSceneNarrator.cs](src/DNDGame.Infrastructure/Narration/LocalLlmHttpSceneNarrator.cs)

It currently supports narration for:

- Opening scenes.
- Quest updates.
- Combat resolution.
- Campaign recap.
- Journal summary.
- NPC dialogue.

### Request Model

The narrator currently sends a request shaped like this:

- `model`
- `prompt`
- `stream: false`

to:

- `POST {endpointUrl}/api/generate`

### Response Validation

The response must be JSON that can be parsed into a top-level `text` property.

Expected narrator payload:

```json
{
  "text": "The wind cuts across the courtyard as the last defenders falter."
}
```

The adapter rejects responses that:

- are not valid JSON
- do not contain `text`
- contain empty text
- contain fenced code blocks
- exceed length limits
- use an unexpected sentence count

If validation fails, the fallback narrator takes over.

## Deterministic Fallback Narration

If local narration is disabled or fails, the app uses deterministic narration from:

- [src/DNDGame.Infrastructure/Narration/DeterministicSceneNarrator.cs](src/DNDGame.Infrastructure/Narration/DeterministicSceneNarrator.cs)

This guarantees the game remains playable even when no model service is running.

## Development Notes

### Key Files

- [src/DNDGame.Console/Program.cs](src/DNDGame.Console/Program.cs): app bootstrap and narrator selection.
- [src/DNDGame.Console/ConsoleGameApplication.cs](src/DNDGame.Console/ConsoleGameApplication.cs): menu flow and game loop.
- [src/DNDGame.Core/Services/NewCampaignFactory.cs](src/DNDGame.Core/Services/NewCampaignFactory.cs): deterministic starting state.
- [src/DNDGame.Core/Services/CampaignProgressionService.cs](src/DNDGame.Core/Services/CampaignProgressionService.cs): quest stage transitions.
- [src/DNDGame.Core/Services/CombatResolutionService.cs](src/DNDGame.Core/Services/CombatResolutionService.cs): encounter resolution.
- [src/DNDGame.Infrastructure/Persistence/JsonCampaignStorage.cs](src/DNDGame.Infrastructure/Persistence/JsonCampaignStorage.cs): save/load implementation.

### Design Constraints

- Do not let model output mutate canonical state directly.
- Prefer adding new quests and encounters as data definitions before introducing freeform narrative branching.
- Keep process-level behavior cross-platform and avoid shell-specific assumptions.

## Roadmap

The current implementation is intentionally narrow. Natural next steps include:

- More quests and biomes.
- Richer inventory and item use.
- Better encounter variety and enemy abilities.
- More NPC conversations and memory.
- Stronger process integration coverage.
- More robust local LLM compatibility layers.

## Related Documents

- [docs/ideas.md](docs/ideas.md)
- [docs/architecture.md](docs/architecture.md)
- [docs/backlog.md](docs/backlog.md)