# Add A Boss Encounter To Close The First Campaign Slice

## Parent Epic

- [Epic: Vertical Slice Campaign](../epic-03-vertical-slice-campaign.md)
- Matching backlog section: [Epic 3: Vertical Slice Campaign](../../backlog.md#epic-3-vertical-slice-campaign)

## Suggested Labels

- `enhancement`
- `gameplay`
- `content`

## User Story

As a solo RPG player, I want the first campaign slice to end with a distinct boss encounter so that the opening adventure has a stronger climax and payoff.

## Current Status

Completed

## Problem

The current watchtower loop is playable and satisfying, but it ends on the second deterministic encounter instead of a clearly distinct finale.

## Scope

- Add a boss encounter that follows the existing watchtower progression.
- Define boss-specific content, stats, and progression behavior.
- Preserve deterministic combat resolution and save/load compatibility.

## Acceptance Criteria

- The first campaign slice includes a clearly distinct boss encounter.
- Quest progression and rewards account for the new finale.
- The recap and narration paths reflect the new encounter correctly.
- Tests cover the happy path through the updated slice.

## Completion Notes

- The watchtower route now culminates in a summit boss encounter against Raider Captain Vark after the courtyard fight.
- Quest progression adds a dedicated boss stage before the tower is marked cleared and the return trip begins.
- Rewards now distinguish the courtyard from the final boss, with the watchtower sigil moving to the boss finale.
- Tests cover the new progression step, the full deterministic quest loop, and the process-level console happy path.