# Epic: Persistence and Continuity

## User Story

As a returning player, I want my campaign history and save slots to persist cleanly so that I can resume progress confidently over multiple sessions.

## Current Status

In progress

## Scope

- [x] Save campaign state by slot.
- [x] Load campaign state by slot.
- [ ] Add save slot metadata and last-played summary.
- [ ] Add schema migration support for future save evolution.
- [ ] Add journal compaction and recap snapshots for longer campaigns.

## Notes

The current save model is reliable and readable, but it is still optimized for a short campaign slice rather than long-running continuity features.