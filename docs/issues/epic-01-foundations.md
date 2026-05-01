# Epic: Foundations

## User Story

As a player, I want a reliable campaign foundation so that starting, saving, loading, and summarizing progress always feels stable.

## Current Status

Complete

## Scope

- [x] Create solution and project structure.
- [x] Define campaign state models for hero, quest, journal, inventory, and encounter state.
- [x] Add JSON save/load infrastructure.
- [x] Add deterministic new game creation.
- [x] Add deterministic recap generation.
- [x] Add structured content definitions for hero classes, starter equipment, opening quest, encounter loot, and quest rewards.
- [x] Add explicit startup validation for local LLM configuration values.

## Notes

This epic is the baseline for every other feature. No future gameplay or narration work should weaken deterministic state ownership or save reliability.

The startup path now validates local LLM endpoint, model, and verbosity settings before gameplay begins and returns a clear non-zero failure when configuration is invalid.