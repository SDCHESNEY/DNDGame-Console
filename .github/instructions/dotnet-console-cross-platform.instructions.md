---
description: "Use when: creating or modifying C# .NET console applications, Program.cs, command-line parsing, or console app project files intended to run on Windows, macOS, and Linux."
applyTo: "**/Program.cs, **/*.Console.csproj, **/Console/**/*.cs, **/*.csproj, **/appsettings*.json"
---

# Cross-Platform .NET Console Application Guidance

## Core Principles

- Build console applications to run correctly on Windows, macOS, and Linux without OS-specific assumptions.
- Prefer standard .NET runtime and Base Class Library APIs over platform-specific code.
- Keep startup flow simple, predictable, and easy to test.
- Favor clear command-line behavior, explicit exit codes, and useful error messages.

## Project and Runtime Guidance

- Prefer SDK-style .NET console projects created with the standard `dotnet` CLI.
- Target standard cross-platform .NET TFMs such as `net8.0`, `net9.0`, or the project's existing target framework. Avoid Windows-only target frameworks unless explicitly required.
- Do not use Windows-only APIs, registry access, COM, or platform-specific shell behavior unless the requirement explicitly calls for them.
- When platform-specific behavior is required, isolate it behind a small abstraction and provide safe behavior for unsupported platforms.

## Program Structure

- Keep `Program.cs` thin. Move parsing, orchestration, business logic, and file operations into separate classes.
- Prefer async `Main` when the application performs I/O-bound work.
- Return explicit integer exit codes for success, validation errors, and runtime failures.
- Send normal output to standard output and diagnostics or error details to standard error.

## Command-Line Behavior

- Support clear, deterministic argument parsing and validation.
- Prefer simple built-in parsing patterns unless the application requirements justify a command-line library.
- Document default values and required arguments in help text.
- Treat command-line input as untrusted. Validate file paths, option values, and enum-like arguments.

## Cross-Platform Filesystem and Process Rules

- Use `Path.Combine`, `Path.Join`, `Path.DirectorySeparatorChar`, and related .NET APIs instead of hard-coded separators.
- Do not hard-code absolute paths, drive letters, home-directory assumptions, or case-insensitive path behavior.
- Assume filesystem case sensitivity may vary by platform.
- Prefer UTF-8 text handling unless the requirement states otherwise.
- Normalize line endings only when necessary for output comparison or persisted file formats.
- When starting child processes, avoid shell-specific syntax and prefer `ProcessStartInfo` with explicit arguments.

## Environment and Configuration

- Prefer `IConfiguration` and `Microsoft.Extensions.*` hosting abstractions when the application benefits from configuration, logging, and dependency injection.
- Read environment variables in a case-tolerant and defensive way, keeping in mind that operating systems treat them differently.
- Avoid relying on terminal capabilities such as ANSI color, interactive prompts, or specific shell features unless the behavior is optional and checked at runtime.

## Logging and Error Handling

- Prefer built-in logging abstractions when structured logs are useful.
- Keep user-facing error messages concise and actionable.
- Catch exceptions at the application boundary to map them to exit codes and readable error output.
- Do not swallow exceptions silently.

## Testing Expectations

- Design console code so argument parsing, core execution, and file or process interactions can be tested separately.
- Add integration tests for full CLI behavior when arguments, output, and exit codes are important.
- Normalize path and newline expectations in tests so they pass on Windows, macOS, and Linux.

## Copilot Behavior Expectations

- Generate console app code that is portable across Windows, macOS, and Linux by default.
- Prefer built-in .NET APIs before suggesting OS-specific commands or external libraries.
- Do not assume PowerShell, Bash, CMD, or a specific terminal is always available.
- When showing command examples, prefer `dotnet` CLI commands that work across platforms.
- Preserve the workspace's general C# and .NET guidance from the root Copilot instructions.