# DNDGame Console Play Guide

This guide shows how to install the prerequisites, configure the game, and play the current vertical slice.

## What You Need

- .NET SDK 9.0 or later
- macOS, Windows, or Linux
- Optional: a local HTTP-backed LLM service if you want live narration

This repository has already been validated with .NET SDK 9.0.301.

## Install And Build

From the repository root, restore and build the solution:

```bash
dotnet build DNDGame.sln
```

If you want to confirm the solution is healthy before playing:

```bash
dotnet test DNDGame.sln
```

## Start The Game

The normal way to play is the interactive menu:

```bash
dotnet run --project src/DNDGame.Console
```

You can also start menu mode explicitly:

```bash
dotnet run --project src/DNDGame.Console -- menu
```

The built-in help screen is:

```bash
dotnet run --project src/DNDGame.Console -- help
```

The current command set is:

- `menu`
- `new --slot <name> --name <hero> --class <fighter|ranger|mage>`
- `load --slot <name>`
- `help`

## Configure The Game

The console app reads settings from [src/DNDGame.Console/appsettings.json](/Users/shawnchesney/VSCodeProjects/DNDGame-Console/src/DNDGame.Console/appsettings.json).

Default configuration:

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

### Core Settings

- `saveDirectory`: optional override for where save files are written. If this is `null`, the app uses the platform application data folder.
- `enableLocalLlmNarration`: when `false`, the game uses deterministic narration only. When `true`, it tries the local HTTP narrator first and falls back if narration fails.
- `localLlm.endpointUrl`: absolute `http` or `https` URL for the local narrator service.
- `localLlm.modelName`: model identifier sent to the narrator service.
- `localLlm.narrativeVerbosity`: must be `concise`, `balanced`, or `rich`.

### Environment Variable Overrides

You can override the config file without editing JSON:

- `DNDGAME_SAVE_DIRECTORY`
- `DNDGAME_ENABLE_LOCAL_LLM_NARRATION`
- `DNDGAME_LLM_ENDPOINT_URL`
- `DNDGAME_LLM_MODEL`
- `DNDGAME_LLM_VERBOSITY`

Example with a custom save directory and deterministic narration:

```bash
DNDGAME_SAVE_DIRECTORY="$PWD/.saves" \
DNDGAME_ENABLE_LOCAL_LLM_NARRATION=false \
dotnet run --project src/DNDGame.Console -- menu
```

### Optional Local LLM Setup

Local narration is optional. The game is fully playable without it.

If you enable local narration, make sure:

- the endpoint is reachable over HTTP or HTTPS
- the service can handle an Ollama-style `POST /api/generate` request flow
- the model returns JSON shaped like `{"text":"..."}`

If the endpoint URL is invalid while local narration is enabled, the app exits early with a configuration error instead of starting with bad settings.

## How The Current Game Loop Works

The current vertical slice is a short quest centered on a ruined watchtower.

The expected loop is:

1. Create a hero.
2. Accept the watchtower assignment from Captain Elira.
3. Advance to the approach.
4. Fight the Goblin Scout.
5. Advance into the courtyard.
6. Fight the Hobgoblin Raider.
7. Return to Captain Elira.
8. Save and quit or continue inspecting status and journal entries.

### Starter Classes

- `fighter`: highest durability, good for a first run
- `ranger`: balanced offense and survivability
- `mage`: lower health, higher burst damage

### Main Menu Options

When the game starts in menu mode, you can choose:

- `1. New game`
- `2. Load game`
- `3. Help`
- `4. Quit`

### Campaign Menu Options

Inside a campaign, the current menu supports:

- `1. Status`
- `2. Journal`
- `3. Advance quest`
- `4. Fight current encounter` when an encounter is active
- `5. Save`
- `6. Quit to main menu`

### Combat Options

Combat always supports:

- `1. Attack`
- `2. Defend`
- `3. Special`

If you have a combat-safe item such as the starting `Minor Potion`, the encounter screen also shows:

- `4. Use item`
- `5. Retreat to campaign menu`

Otherwise, retreat remains option `4`.

## Quick Start

Create and save a hero immediately from the command line:

```bash
dotnet run --project src/DNDGame.Console -- new --slot alpha --name Mira --class fighter
```

Load that save later:

```bash
dotnet run --project src/DNDGame.Console -- load --slot alpha
```

This is useful if you want to seed a campaign quickly, but the interactive menu is the better first experience because it walks you straight into the campaign loop.

## Tutorial: First Watchtower Run

This short walkthrough uses the interactive menu and a `fighter`, because it is the most forgiving class for a first session.

### 1. Launch The Menu

```bash
dotnet run --project src/DNDGame.Console -- menu
```

At the main menu:

- enter `1` for `New game`

When prompted, enter:

- save slot: `tutorial-run`
- hero name: any name you want, for example `Mira`
- class: `fighter`

The game creates the save, prints the opening scene, and moves you into the campaign menu.

### 2. Check Your Starting State

In the campaign menu:

- enter `1` for `Status`

Look for these starting details:

- your hero summary
- current quest stage
- your abilities
- your inventory, including a `Minor Potion`

### 3. Advance To The First Fight

Back at the campaign menu:

- enter `3` for `Advance quest`

This pushes the story forward and eventually activates the first encounter.

When an encounter becomes active, the campaign menu adds:

- `4. Fight current encounter`

### 4. Win The First Encounter

Enter `4` to start combat.

For a simple first fight, use this approach:

- open with `1` for `Attack`
- use `3` for `Special` when you want a stronger class action
- use `4` for `Use item` if your health gets low and you want to spend the `Minor Potion`

After each turn, the engine resolves the outcome and prints a narrated combat summary.

When the enemy is defeated, the encounter closes and you return to the campaign menu.

### 5. Clear The Courtyard

From the campaign menu:

- enter `3` for `Advance quest` again to move deeper into the watchtower
- enter `4` when the next encounter becomes available

Use the same combat pattern until the second enemy goes down.

### 6. Return To Captain Elira

After the second encounter:

- enter `3` for `Advance quest` until the quest reaches `ReturnedToCaptain`

At that point, the game prints Captain Elira's closing dialogue and the quest recap reflects the completed return step.

### 7. Save And Exit

From the campaign menu:

- enter `5` to save immediately if you want an explicit save confirmation
- enter `6` to quit to the main menu

Quitting to the main menu also saves the campaign.

From the main menu:

- enter `4` to exit the app

## Loading A Saved Campaign Later

Start menu mode again:

```bash
dotnet run --project src/DNDGame.Console -- menu
```

Then:

1. Enter `2` for `Load game`.
2. Review the listed save slot metadata.
3. Type the save slot name, for example `tutorial-run`.

The game prints a recap and returns you to the campaign menu.

## Troubleshooting

- If the app says a save slot was not found, confirm the slot name and the configured save directory.
- If local narration is enabled and startup fails, validate `localLlm.endpointUrl`, `localLlm.modelName`, and `localLlm.narrativeVerbosity`.
- If you want a clean test run, point `DNDGAME_SAVE_DIRECTORY` at an empty folder.

## Verified Against The Current Build

These instructions were checked against the current console application by:

- running the real `help` command
- running the focused console process integration tests for menu, save, load, combat, and item use flows