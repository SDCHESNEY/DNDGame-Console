# Add Reusable Console Screen Rendering Helpers

## Parent Epic

- [Epic: CLI and Play Loop](../epic-02-cli-and-play-loop.md)
- Matching backlog section: [Epic 2: CLI And Play Loop](../../backlog.md#epic-2-cli-and-play-loop)

## Suggested Labels

- `enhancement`
- `console-ui`
- `usability`

## User Story

As a player, I want more consistent console screens so that menus, recaps, and encounter output stay readable as the game grows.

## Problem

`ConsoleGameApplication` currently writes most screen content inline. The flow works, but formatting logic is spread across the main application loop and will get harder to maintain as more screens are added.

## Scope

- Extract reusable helpers for headings, menus, narrated blocks, and compact stat displays.
- Keep output cross-platform and shell-independent.
- Avoid changing deterministic gameplay behavior.

## Acceptance Criteria

- Main menu, campaign menu, and encounter output use shared rendering helpers.
- Output remains readable on macOS, Windows, and Linux terminals.
- Existing process integration tests continue to pass or are updated only for intended formatting changes.