# Add Item Use As A Combat Action

## Parent Epic

- [Epic: Combat System](../epic-04-combat-system.md)
- Matching backlog section: [Epic 4: Combat System](../../backlog.md#epic-4-combat-system)

## Suggested Labels

- `enhancement`
- `combat`
- `gameplay`

## User Story

As a player, I want to use inventory items during combat so that consumables and future utility items become part of tactical decision-making.

## Current Status

Completed

## Problem

The current combat loop supports attack, defend, and special actions, but inventory items are not yet usable inside encounters.

## Scope

- Add item selection and resolution for combat-safe items.
- Start with support for currently available consumables such as the minor potion.
- Keep state transitions deterministic.

## Acceptance Criteria

- Combat menus expose an item-use path when eligible items exist.
- Using a combat item mutates inventory and hero state correctly.
- Items that should not be usable in combat are rejected cleanly.
- Tests cover at least one successful item use and one invalid item case.

## Completion Notes

- Encounter screens now expose a combat item path when the player has an eligible consumable.
- Minor potions can now be used during combat, consuming inventory, restoring health, and still allowing deterministic enemy retaliation.
- Non-combat items are rejected cleanly with no state mutation.
- Tests cover successful potion use, invalid quest-item use, renderer contract updates, and a process-level menu flow for item selection.