# Epic: Testing

## User Story

As a developer, I want reliable automated coverage around the deterministic engine and console boundary so that I can extend the game without breaking core flows.

## Current Status

Complete

## Scope

- [x] Add unit tests for campaign creation and save roundtrip.
- [x] Add tests for command parsing.
- [x] Add dedicated tests for recap generation.
- [x] Add combat rule unit tests.
- [x] Add console integration tests through a process boundary.
- [x] Add tests for quest progression and the full short campaign loop.
- [x] Add tests for local LLM narration validation behavior.

## Notes

The deterministic engine, recap builder, and console boundary now all have direct automated coverage across the main gameplay slice.