# Compact Journals And Generate Recap Snapshots

## Parent Epic

- [Epic: Persistence and Continuity](../epic-06-persistence-and-continuity.md)
- Matching backlog section: [Epic 6: Persistence And Continuity](../../backlog.md#epic-6-persistence-and-continuity)

## Suggested Labels

- `enhancement`
- `persistence`
- `narration`

## User Story

As a returning player, I want long campaigns to preserve readable history and concise summaries so that save files and recaps stay useful over time.

## Current Status

Completed

## Problem

The current journal works well for a short slice, but longer campaigns will eventually need compaction and recap snapshot support to stay readable and efficient.

## Scope

- Define a strategy for compacting or summarizing older journal entries.
- Add recap snapshots or equivalent summary artifacts for longer-running campaigns.
- Preserve key historical events needed for player-facing continuity.

## Acceptance Criteria

- Long-running campaigns can present concise recaps without dumping the full raw journal every time.
- Journal compaction preserves important milestones.
- Save/load behavior remains deterministic and testable.

## Completion Notes

- Save normalization now compacts oversized journals at persistence time and moves older history into recap snapshots.
- Recap snapshots are persisted as part of campaign state and surfaced in recap output and the journal view.
- Compaction keeps recent raw journal entries while preserving older milestone history in deterministic summary form.
- Tests cover journal compaction and recap rendering of persisted snapshots.