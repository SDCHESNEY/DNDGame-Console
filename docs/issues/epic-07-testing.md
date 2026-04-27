# Epic: Testing

## User Story

As a developer, I want reliable automated coverage around the deterministic engine and console boundary so that I can extend the game without breaking core flows.

## Current Status

Mostly complete

## Scope

- [x] Add unit tests for campaign creation and save roundtrip.
- [x] Add tests for command parsing.
- [ ] Add dedicated tests for recap generation.
- [x] Add combat rule unit tests.
- [x] Add console integration tests through a process boundary.
- [x] Add tests for quest progression and the full short campaign loop.
- [x] Add tests for local LLM narration validation behavior.

## Notes

The highest-value remaining gap is recap-specific coverage. The rest of the critical path already has deterministic and process-level protection.