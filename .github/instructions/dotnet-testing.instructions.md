---
description: "Use when: writing or updating C# unit tests, integration tests, test projects, or test-related .csproj files for this workspace."
applyTo: "**/*Tests.cs, **/*Test.cs, **/Tests/**/*.cs, **/Test/**/*.cs, **/*.Tests.csproj, **/*.IntegrationTests.csproj, **/*.UnitTests.csproj"
---

# .NET Testing Conventions

## Core Principles

- Prefer fast, deterministic tests that validate one behavior at a time.
- Write unit tests for business rules, parsing, transformations, and validation logic.
- Write integration tests only where behavior depends on real framework wiring, persistence, filesystem interaction, process execution, or multiple components working together.
- Keep test code as readable as production code. Avoid clever abstractions that hide the behavior under test.

## Framework and Tooling Guidance

- Prefer the test framework already used by the solution. Do not introduce a second test framework without a clear reason.
- Prefer built-in .NET testing support and standard `dotnet test` workflows.
- Add third-party assertion or mocking libraries only when the default framework APIs are clearly insufficient.
- When mocking is needed, mock application boundaries and expensive collaborators, not simple data objects.

## Test Structure

- Follow the naming style already present in the nearest test project. If no style exists, use `MethodName_StateUnderTest_ExpectedBehavior`.
- Keep each test focused on a single observable outcome.
- Use clear arrange, act, and assert structure, but do not add comments such as `Arrange`, `Act`, or `Assert` unless the surrounding tests already use them.
- Prefer helper methods, builders, or fixtures only when they reduce repetition without hiding intent.
- Keep test setup local to the test unless many tests genuinely share the same required context.

## Unit Testing Guidance

- Test public behavior, not private implementation details.
- Prefer real value objects and simple in-memory collaborators over mocks when that keeps tests simpler.
- Cover success paths, edge cases, invalid inputs, and failure paths for non-trivial logic.
- Use data-driven tests for repeated input-output scenarios when the framework supports them.
- Avoid time, randomness, environment variables, and filesystem dependencies in unit tests unless they are explicitly abstracted.

## Integration Testing Guidance

- Use integration tests to verify component wiring, serialization, persistence behavior, CLI execution, hosted services, or end-to-end flows through meaningful boundaries.
- Keep integration tests isolated from each other and clean up any filesystem or process state they create.
- Prefer temporary directories, ephemeral test data, and disposable resources.
- Avoid depending on machine-specific paths, usernames, shell profiles, or OS-specific defaults.
- Mark or organize slower integration tests distinctly from unit tests when the chosen framework and project layout support it.

## Assertions and Diagnostics

- Assert on externally visible behavior and meaningful outcomes.
- Prefer precise assertions over broad truthy checks.
- Include useful failure context in assertions when the framework supports it.
- Verify exception types and relevant messages only when the message content is part of the contract.

## Console and Process Testing

- For console application tests, prefer invoking the application through a process boundary for integration coverage of arguments, exit codes, standard output, and standard error.
- Normalize line endings in assertions when output may differ between Windows and Unix-like systems.
- Avoid asserting on platform-specific path separators unless that difference is the behavior under test.
- When asserting console output, prefer stable fragments or normalized full-text expectations over brittle whitespace-sensitive snapshots.

## Copilot Behavior Expectations

- Generate tests that fit the existing test framework, naming style, and project organization.
- Do not introduce additional test packages by default.
- Prefer minimal, focused fixtures and setup.
- When adding integration tests, keep them separate from pure unit tests when practical.
- If the production code is hard to test, prefer small refactors that improve testability over large rewrites.