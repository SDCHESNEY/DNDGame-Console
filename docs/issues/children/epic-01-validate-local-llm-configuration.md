# Validate Local LLM Configuration At Startup

## Parent Epic

- [Epic: Foundations](../epic-01-foundations.md)
- Matching backlog section: [Epic 1: Foundations](../../backlog.md#epic-1-foundations)

## Suggested Labels

- `enhancement`
- `configuration`
- `foundation`

## User Story

As a player, I want invalid local LLM settings to fail fast with clear messages so that narration configuration problems do not surface later as confusing runtime errors.

## Current Status

Completed

## Problem

The app can load endpoint, model, and verbosity values from configuration and environment variables, but it does not currently validate those settings at startup.

## Scope

- Validate required local LLM settings when local narration is enabled.
- Reject obviously invalid endpoint URLs and empty model names.
- Validate allowed verbosity values.
- Return clear console-facing error output and a non-zero exit code.

## Acceptance Criteria

- Starting the app with invalid LLM configuration fails before gameplay begins.
- Error output explains which setting is invalid and why.
- Valid configuration continues to work unchanged.
- Tests cover at least one invalid endpoint and one invalid model or verbosity case.

## Completion Notes

- Startup settings loading now validates local narration configuration before the narrator is constructed.
- Invalid endpoint URLs, empty model names, and unsupported verbosity values produce clear console-facing validation errors.
- Startup returns a non-zero exit code for invalid local LLM configuration.
- Tests cover invalid verbosity during settings load and invalid endpoint handling through the console process boundary.