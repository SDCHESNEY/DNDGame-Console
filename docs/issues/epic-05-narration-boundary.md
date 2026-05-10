# Epic: Narration Boundary

## User Story

As a player, I want optional local-model narration with strict guardrails so that I get richer flavor text without losing deterministic game integrity.

## Current Status

Complete for the current vertical slice

## Scope

- [x] Add a narrator interface and deterministic fallback implementation.
- [x] Add prompt generation for opening scenes, quest updates, combat summaries, recaps, journal summaries, and NPC dialogue.
- [x] Add a local LLM HTTP adapter for an Ollama-style endpoint.
- [x] Validate model outputs before surfacing them to the player.
- [x] Add graceful fallback when the local model is unavailable or returns invalid content.

## Notes

This epic is complete for the current slice. The narration boundary now classifies transport and guardrail failures explicitly, falls back only for those expected boundary failures, and leaves unexpected implementation defects visible to tests instead of silently masking them.