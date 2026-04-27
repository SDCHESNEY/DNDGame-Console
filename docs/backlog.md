# DNDGame Backlog

## Prioritization Rules

- Protect deterministic state and save/load reliability first.
- Prefer features that make the first 30 minutes playable over broad system breadth.
- Keep the engine authoritative and treat narration as presentation only.
- Do not expand AI-driven narration unless validation and fallback behavior stay strict.
- Keep the first campaign short, replayable, and easy to test through the console boundary.

## Epic 1: Foundations

**GitHub issue title:** Epic: Foundations

**User story:** As a player, I want a reliable campaign foundation so that starting, saving, loading, and summarizing progress always feels stable.

**Current status:** Mostly complete

**Checklist**

- [x] Create solution and project structure.
- [x] Define campaign state models for hero, quest, journal, inventory, and encounter state.
- [x] Add JSON save/load infrastructure.
- [x] Add deterministic new game creation.
- [x] Add deterministic recap generation.
- [x] Add structured content definitions for hero classes, starter equipment, opening quest, encounter loot, and quest rewards.
- [ ] Add explicit startup validation for local LLM configuration values.

## Epic 2: CLI And Play Loop

**GitHub issue title:** Epic: CLI and Play Loop

**User story:** As a player, I want a clear cross-platform console flow so that I can start, load, inspect, save, and exit a campaign without friction.

**Current status:** Mostly complete

**Checklist**

- [x] Add command-line flows for `menu`, `new`, `load`, and `help`.
- [x] Add explicit exit codes and help text.
- [x] Add an interactive main menu for new game, load game, help, and quit.
- [x] Add campaign menu handling for status, journal, quest advancement, save, and quit.
- [ ] Add dedicated screen rendering helpers for more compact and reusable console layouts.

## Epic 3: Vertical Slice Campaign

**GitHub issue title:** Epic: Vertical Slice Campaign

**User story:** As a solo RPG player, I want a short but complete adventure loop so that I can create a hero, clear the watchtower quest, and reach a satisfying first milestone.

**Current status:** In progress

**Checklist**

- [x] Implement one town hub at Northgate Outpost.
- [x] Implement one wilderness route through the watchtower approach.
- [x] Implement one watchtower quest path with a courtyard follow-up encounter.
- [ ] Implement a distinct boss encounter for the end of the first campaign slice.
- [x] Add three character classes with differentiated combat actions.
- [x] Add quest progression states for accept, approach, clear, and return.

## Epic 4: Combat System

**GitHub issue title:** Epic: Combat System

**User story:** As a player, I want deterministic combat choices and outcomes so that encounters feel tactical, fair, and easy to reason about.

**Current status:** In progress

**Checklist**

- [x] Add enemy models and encounter definitions.
- [x] Add deterministic turn resolution for player and enemy actions.
- [x] Add attack, defend, and special ability actions.
- [ ] Add item use as a combat action.
- [ ] Expand armor and status-effect rules beyond the current lightweight model.
- [x] Add combat result summaries for narration and quest progression.

## Epic 5: Narration Boundary

**GitHub issue title:** Epic: Narration Boundary

**User story:** As a player, I want optional local-model narration with strict guardrails so that I get richer flavor text without losing deterministic game integrity.

**Current status:** Complete for the current vertical slice

**Checklist**

- [x] Add a narrator interface and deterministic fallback implementation.
- [x] Add prompt generation for opening scenes, quest updates, combat summaries, recaps, journal summaries, and NPC dialogue.
- [x] Add a local LLM HTTP adapter for an Ollama-style endpoint.
- [x] Validate model outputs before surfacing them to the player.
- [x] Add graceful fallback when the local model is unavailable or returns invalid content.

## Epic 6: Persistence And Continuity

**GitHub issue title:** Epic: Persistence and Continuity

**User story:** As a returning player, I want my campaign history and save slots to persist cleanly so that I can resume progress confidently over multiple sessions.

**Current status:** In progress

**Checklist**

- [x] Save campaign state by slot.
- [x] Load campaign state by slot.
- [ ] Add save slot metadata and last-played summary.
- [ ] Add schema migration support for future save evolution.
- [ ] Add journal compaction and recap snapshots for longer campaigns.

## Epic 7: Testing

**GitHub issue title:** Epic: Testing

**User story:** As a developer, I want reliable automated coverage around the deterministic engine and console boundary so that I can extend the game without breaking core flows.

**Current status:** Mostly complete

**Checklist**

- [x] Add unit tests for campaign creation and save roundtrip.
- [x] Add tests for command parsing.
- [ ] Add dedicated tests for recap generation.
- [x] Add combat rule unit tests.
- [x] Add console integration tests through a process boundary.
- [x] Add tests for quest progression and the full short campaign loop.
- [x] Add tests for local LLM narration validation behavior.

## Recommended Order For The Next 5 Work Items

1. Add save slot metadata and last-played summaries to improve the load flow.
2. Add explicit configuration validation for local LLM endpoint, model, and verbosity settings.
3. Add dedicated recap builder tests and a few more narrator fallback edge-case tests.
4. Add item use plus richer armor and status-effect rules to combat.
5. Expand the watchtower slice with a true boss encounter and follow-on content.