# Epic: CLI and Play Loop

## User Story

As a player, I want a clear cross-platform console flow so that I can start, load, inspect, save, and exit a campaign without friction.

## Current Status

Complete

## Scope

- [x] Add command-line flows for `menu`, `new`, `load`, and `help`.
- [x] Add explicit exit codes and help text.
- [x] Add an interactive main menu for new game, load game, help, and quit.
- [x] Add campaign menu handling for status, journal, quest advancement, save, and quit.
- [x] Add dedicated screen rendering helpers for more compact and reusable console layouts.

## Notes

The play loop is functional and now uses shared rendering helpers for key screen types. Further console UX work can build on the extracted renderer instead of spreading formatting logic through the main loop.