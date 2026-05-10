# Add Schema Migration Support For Save Files

## Parent Epic

- [Epic: Persistence and Continuity](../epic-06-persistence-and-continuity.md)
- Matching backlog section: [Epic 6: Persistence And Continuity](../../backlog.md#epic-6-persistence-and-continuity)

## Suggested Labels

- `enhancement`
- `persistence`
- `technical-debt`

## User Story

As a returning player, I want older save files to remain usable after updates so that I do not lose campaign progress as the game evolves.

## Current Status

Completed

## Problem

The save format is still simple and readable, but there is no migration pipeline yet for future schema changes.

## Scope

- Define a version-aware migration approach for `CampaignState` persistence.
- Support loading older save payloads into the current model.
- Keep the migration path explicit and testable.

## Acceptance Criteria

- Save files include or continue to preserve schema version information.
- The storage layer can migrate at least one older version shape into the current format.
- Migration failures are reported clearly.
- Tests cover successful and failed migration behavior.

## Completion Notes

- Save persistence is now schema-version aware, with the current format written as schema version 2.
- The storage layer migrates legacy schema version 1 payloads into the current `CampaignState` shape during load.
- Unsupported future schema versions now fail with a clear migration error instead of silently loading bad state.
- Tests cover successful legacy migration and explicit unsupported-version failure.