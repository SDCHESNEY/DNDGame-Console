# Add Save Slot Metadata And Last-Played Summaries

## Parent Epic

- [Epic: Persistence and Continuity](../epic-06-persistence-and-continuity.md)
- Matching backlog section: [Epic 6: Persistence And Continuity](../../backlog.md#epic-6-persistence-and-continuity)

## Suggested Labels

- `enhancement`
- `persistence`
- `usability`

## User Story

As a returning player, I want save slots to show useful metadata so that I can quickly pick the right campaign to resume.

## Current Status

Completed

## Problem

The current load flow can list slot names, but it does not surface last-played details or a concise summary of campaign progress.

## Scope

- Add metadata for save slots such as last played time, hero name, class, and quest stage summary.
- Surface that metadata in the load flow without breaking existing saves.
- Keep the persisted format readable and deterministic.

## Acceptance Criteria

- The load experience shows more than just slot names.
- Save metadata stays in sync with the canonical campaign state.
- Existing saves continue to load or degrade gracefully.
- Tests cover metadata generation and display.

## Completion Notes

- Save-slot metadata is now derived directly from persisted campaign state rather than stored separately, so it stays synchronized with the canonical save file.
- The help and menu load flows now show hero name, class, quest stage, a short campaign summary, and last-played time for each slot.
- Invalid or unreadable save files degrade gracefully in metadata listings instead of breaking the whole load experience.
- Tests cover both metadata generation in storage and console display in the interactive load flow.