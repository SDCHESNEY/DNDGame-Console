# Epic: CLI and Play Loop

## User Story

As a player, I want a clear cross-platform console flow so that I can start, load, inspect, save, and exit a campaign without friction.

## Current Status

Mostly complete

## Scope

- [x] Add command-line flows for `menu`, `new`, `load`, and `help`.
- [x] Add explicit exit codes and help text.
- [x] Add an interactive main menu for new game, load game, help, and quit.
- [x] Add campaign menu handling for status, journal, quest advancement, save, and quit.
- [ ] Add dedicated screen rendering helpers for more compact and reusable console layouts.

## Notes

The play loop is already functional. Remaining work is mainly about presentation, reuse, and keeping the console UX readable as the feature set grows.