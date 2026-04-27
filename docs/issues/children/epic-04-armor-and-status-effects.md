# Expand Armor And Status Effect Combat Rules

## Parent Epic

- [Epic: Combat System](../epic-04-combat-system.md)
- Matching backlog section: [Epic 4: Combat System](../../backlog.md#epic-4-combat-system)

## Suggested Labels

- `enhancement`
- `combat`
- `systems`

## User Story

As a player, I want richer defensive and status-effect rules so that class choices and encounter design feel deeper without sacrificing determinism.

## Problem

The current combat model is intentionally lightweight. It supports damage, defense, and specials, but armor interactions and status-effect systems are still minimal.

## Scope

- Define a small deterministic ruleset for armor interactions and status effects.
- Integrate those rules into combat resolution and recap text.
- Keep the first implementation small enough to remain testable and readable.

## Acceptance Criteria

- At least one armor-related rule and one status-effect rule are represented in deterministic combat logic.
- Combat summaries and recaps reflect those outcomes accurately.
- Tests cover the new rule paths without introducing randomness.