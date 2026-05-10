# Epic: Persistence and Continuity

## User Story

As a returning player, I want my campaign history and save slots to persist cleanly so that I can resume progress confidently over multiple sessions.

## Current Status

Complete

## Scope

- [x] Save campaign state by slot.
- [x] Load campaign state by slot.
- [x] Add save slot metadata and last-played summary.
- [x] Add schema migration support for future save evolution.
- [x] Add journal compaction and recap snapshots for longer campaigns.

## Notes

The current save model is reliable, version-aware, and better suited for longer-running campaigns. Saves now support schema migration, save-slot metadata, and recap-backed journal compaction while preserving deterministic loading behavior.