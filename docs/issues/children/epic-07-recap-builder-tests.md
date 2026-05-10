# Add Dedicated Recap Builder Tests

## Parent Epic

- [Epic: Testing](../epic-07-testing.md)
- Matching backlog section: [Epic 7: Testing](../../backlog.md#epic-7-testing)

## Suggested Labels

- `testing`
- `coverage`
- `enhancement`

## User Story

As a developer, I want dedicated recap builder coverage so that summary output remains stable as campaign state and presentation rules evolve.

## Current Status

Completed

## Problem

The current test suite covers campaign creation, combat, progression, process flows, and narration validation, but recap generation does not yet have its own focused tests.

## Scope

- Add direct tests for `CampaignRecapBuilder`.
- Cover representative cases for hero state, quest stage, inventory, encounter state, and journal summaries.
- Keep assertions focused on meaningful output fragments rather than brittle full-text snapshots.

## Acceptance Criteria

- Recap builder behavior has dedicated unit coverage.
- Tests cover both with-encounter and without-encounter cases.
- Output assertions remain stable across platforms.

## Completion Notes

- `CampaignRecapBuilder` now has dedicated unit tests separate from broader progression and process coverage.
- Tests cover both no-encounter and active-encounter recap output.
- Assertions focus on stable, meaningful fragments and normalize line endings for cross-platform safety.