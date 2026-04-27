---
description: "Workspace-wide GitHub Copilot instructions for C# and standard .NET development with a strong preference for built-in .NET functionality over third-party libraries."
---

# C# and .NET Development Instructions

## Core Principles

- Prefer standard .NET and ASP.NET Core functionality before introducing third-party packages.
- Generate solutions that are simple, maintainable, and idiomatic for modern C# and .NET.
- Keep dependencies minimal. Only suggest external libraries when the built-in platform does not reasonably cover the requirement.
- Match the existing project structure and coding style when code already exists. Do not re-architect without a clear reason.
- Favor clear, testable designs with small classes and focused methods.

## Language and Framework Guidance

- Write idiomatic C# using modern language features supported by the target SDK.
- Prefer nullable reference types and write code that is explicit about null handling.
- Use async and await for I/O-bound work. Avoid blocking calls such as `.Result`, `.Wait()`, and synchronous wrappers around asynchronous APIs.
- Use `var` when the type is obvious from the right-hand side; otherwise prefer explicit types for readability.
- Prefer expression-bodied members only when they improve readability.
- Use pattern matching, switch expressions, and `nameof` where they make intent clearer.
- Keep methods short and cohesive. Extract private helper methods instead of adding deeply nested control flow.

## Standard .NET First

- Prefer the Base Class Library for collections, text processing, dates, file I/O, networking, and serialization.
- Prefer `System.Text.Json` over external JSON libraries unless a specific missing capability is required.
- Prefer `HttpClient` with `IHttpClientFactory` in ASP.NET Core applications instead of custom HTTP wrappers unless there is a clear project pattern.
- Prefer built-in dependency injection from `Microsoft.Extensions.DependencyInjection`.
- Prefer built-in configuration, options, and logging abstractions from `Microsoft.Extensions.*`.
- Prefer `ILogger<T>` for logging and structured log messages over string concatenation.
- Prefer `DateTimeOffset` for points in time and use UTC consistently unless local time is explicitly required.
- Prefer `CancellationToken` on async public APIs and pass it through to downstream async calls.

## Architecture and Project Structure

- Organize code by feature or domain when practical, not by large generic utility buckets.
- Keep business logic out of controllers, endpoints, and UI layers.
- Use interfaces where they improve testability or define a stable application boundary, not by default for every type.
- Keep models focused on a single responsibility. Separate transport models, domain models, and persistence models when their concerns differ.
- Prefer constructor injection for required dependencies.
- Avoid static state except for true stateless helpers or constants.

## ASP.NET Core Guidance

- Prefer built-in ASP.NET Core features for routing, validation, authentication, authorization, configuration, caching, and middleware.
- Keep endpoint handlers and controllers thin. Delegate business rules to services or domain classes.
- Return appropriate HTTP status codes and problem details for API errors.
- Use options binding for configuration sections instead of reading raw configuration values throughout the codebase.
- Register services with the narrowest practical lifetime: singleton, scoped, or transient based on actual behavior.

## Data and Persistence

- Prefer straightforward data access patterns that are easy to reason about.
- Keep persistence concerns isolated from business rules.
- Use transactions only where consistency requires them.
- Validate inputs at the application boundary before performing persistence operations.

## Error Handling

- Fail fast on invalid arguments and invalid state.
- Catch exceptions only when the code can add context, convert them to a meaningful result, or handle recovery.
- Do not swallow exceptions.
- Use custom exception types sparingly and only when they improve clarity at an application boundary.

## Testing

- Write tests for business logic, parsing, validation, and edge cases.
- Prefer fast unit tests over broad integration coverage when either would validate the same behavior.
- Keep tests deterministic and avoid real network or filesystem dependencies unless the test explicitly targets that integration.
- Structure tests with clear arrange, act, and assert phases.
- Include success cases, boundary cases, and failure cases for non-trivial behavior.

## Performance and Maintainability

- Prefer clarity first, then optimize based on measured need.
- Avoid unnecessary allocations in hot paths, but do not micro-optimize ordinary application code.
- Enumerate sequences once when possible and avoid accidental multiple enumeration.
- Choose the simplest collection and algorithm that satisfies the requirement.

## Copilot Behavior Expectations

- When proposing code, use built-in .NET features before suggesting additional NuGet packages.
- When adding a package, explain why the built-in platform is insufficient.
- When generating examples, use the `dotnet` CLI, SDK-style projects, and standard project layouts.
- Preserve existing naming, namespace, formatting, and folder conventions when they are present.
- Do not introduce alternative architectural patterns unless the task explicitly asks for them.
- Prefer small, incremental edits over broad rewrites.
- If a requirement is ambiguous, make the least surprising choice consistent with standard .NET practices.