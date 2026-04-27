# DNDGame MVP Backlog

## Prioritization Rules

- Protect deterministic state and save/load reliability first.
- Prefer features that make the first 30 minutes playable over broad system breadth.
- Do not add freeform AI behavior until the game can validate and recover from bad outputs.
- Keep the first campaign short and replayable.

## Epic 1: Foundations

- [x] Create solution and project structure.
- [x] Define initial campaign state models.
- [x] Add JSON save/load infrastructure.
- [x] Add deterministic new game creation.
- [x] Add deterministic recap generation.
- [ ] Add structured content definitions for hero classes, starter equipment, and initial quest.
- [ ] Add configuration validation for future local LLM settings.

## Epic 2: CLI And Play Loop

- [x] Add command-line flows for `new` and `load`.
- [x] Add explicit exit codes and help text.
- [ ] Add interactive main menu for new game, load game, and continue.
- [ ] Add command handling for `help`, `status`, `journal`, `save`, and `quit`.
- [ ] Add screen rendering helpers for compact console layouts.

## Epic 3: Vertical Slice Campaign

- [ ] Implement one town hub.
- [ ] Implement one wilderness route.
- [ ] Implement one watchtower dungeon.
- [ ] Implement one boss encounter.
- [ ] Add three character classes with differentiated combat actions.
- [ ] Add quest progression states for intro, explore, clear, and return.

## Epic 4: Combat System

- [ ] Add enemy models and encounter definitions.
- [ ] Add deterministic turn order.
- [ ] Add attack, defend, item, and special ability actions.
- [ ] Add health, damage, armor, and status-effect rules.
- [ ] Add combat result summaries for narration.

## Epic 5: Narration Boundary

- [x] Add a narrator interface and deterministic fallback implementation.
- [ ] Add prompt templates for opening scenes, combat summaries, and recaps.
- [ ] Add local LLM HTTP adapter for Ollama or equivalent local endpoint.
- [ ] Validate model outputs before surfacing them to the player.
- [ ] Add graceful fallback when the local model is unavailable.

## Epic 6: Persistence And Continuity

- [x] Save campaign state by slot.
- [x] Load campaign state by slot.
- [ ] Add save slot metadata and last-played summary.
- [ ] Add schema migration support.
- [ ] Add journal compaction and recap snapshots for long campaigns.

## Epic 7: Testing

- [x] Add first unit tests for campaign creation and save roundtrip.
- [ ] Add tests for command parsing.
- [ ] Add tests for recap generation.
- [ ] Add combat rule unit tests.
- [ ] Add console integration tests through a process boundary.

## Recommended Order For The Next 5 Work Items

1. Add starter content definitions for classes, opening quest, and opening location.
2. Add command parsing tests and recap tests.
3. Implement a simple interactive menu loop on top of the current CLI flows.
4. Add the first deterministic combat encounter.
5. Add the local LLM adapter behind the existing narration interface.