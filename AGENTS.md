# AGENTS.md

Development happens only through Victor and Cursor agents.

## What this repo is

AgentConnectLibrary is a .NET 10 class library that calls [AgentConnect](https://github.com/vdiamond/AgentConnect) Approved v9. It is a DLL. It is not a NuGet package.

`AgentConnectClient` is this library's client. `Mcp` has one async method for each of the 19 MCP tools. `Rest` has one async method for each of the 33 REST endpoints. A caller passes the MCP URL, the REST base URL, a Member token, and an optional platform admin key once. Each call is asynchronous, uses a 30 second timeout unless the caller changes it, and returns a typed model. A call does not retry. A list call returns one page.

Status: the library is in the tree at version `0.6.0` (`Version` in the project file, and the MCP client info). `IsPackable` and `GeneratePackageOnBuild` are false.

## Build, run, and test

`global.json` pins SDK `10.0.404` with `rollForward` set to `latestFeature`.

From the repository root:

```bash
dotnet build
dotnet test
```

Both commands were run with SDK 10.0.404. The build succeeded with 0 warnings and 0 errors. The test run passed 38 tests, failed 0, and skipped 0. Tests use the in-process stand-in in `tests/AgentConnectLibrary.Tests/TestSupport.cs`. They do not call a live AgentConnect server. `TimeoutTests` waits out the 30 second default timeout.

There is no host to run. A Debug build writes `src/AgentConnectLibrary/bin/Debug/net10.0/AgentConnectLibrary.dll`.

This repository has no CI workflow.

## Layout

| Path | What it is |
|---|---|
| `AgentConnectLibrary.slnx` | Solution. Build and test from the repository root. |
| `Directory.Build.props` | Shared `Nullable`, `ImplicitUsings`, `LangVersion` `latest`, `TreatWarningsAsErrors`. |
| `global.json` | SDK pin. |
| `.editorconfig` | Shared formatting. |
| `src/AgentConnectLibrary` | `net10.0` library. Root namespace `AgentConnectLibrary`. Public entry is `AgentConnectClient` (`Mcp`, `Rest`). Models are sealed records in this project. `AgentConnectTransport`, `JsonWire`, and `LibraryMessages` are internal. |
| `tests/AgentConnectLibrary.Tests` | xUnit. Project-references the library. `CatalogTests`, `BehaviorTests`, `TimeoutTests`, `PathSegmentTests`. |

## Dependencies on other vdiamond repos

**[AgentConnect](https://github.com/vdiamond/AgentConnect)** is the server this library calls.

- Called at runtime over MCP Streamable HTTP (`ModelContextProtocol`, `HttpTransportMode.StreamableHttp`, `OwnsSession` false) and over REST (`HttpClient`). `src/AgentConnectLibrary/AgentConnectLibrary.csproj` has no `ProjectReference`. There is no git submodule and no sibling-folder path into that repo.
- No clone of AgentConnect is required to build or test. The tests stand in for the server.
- Package on the library: `ModelContextProtocol` 2.4.0. The test project references `src/AgentConnectLibrary/AgentConnectLibrary.csproj` and the packages `Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, and `xunit.runner.visualstudio` 3.1.4.
- `CatalogTests` locks the Approved v9 method lists (`T-3` for the 19 MCP tools, `T-4` for the 33 REST endpoints).

No vdiamond consumer is named in this repository. Whether [AgentConnectClient](https://github.com/vdiamond/AgentConnectClient) consumes this library is to be confirmed.

## Conventions

`.cursor/rules/coding-standards.mdc` is the governing standard. Existing code may not yet comply. Bring it into compliance when you touch it.

What the code does today:

- `net10.0`, nullable reference types, implicit usings, and the latest C# language version, from `Directory.Build.props`. Warnings are errors in every configuration.
- File-scoped namespaces, braces on every block, and 4-space indentation (`.editorconfig`; 2 spaces for project and markdown files).
- `sealed` classes and `sealed record` types. `Optional<T>` is a `readonly record struct`. Private members sit in `#region private`.
- XML documentation on the public API (`GenerateDocumentationFile` is true on the library).
- `ConfigureAwait(false)` on library awaits. `ArgumentNullException.ThrowIfNull` on the options constructor.
- xUnit. Spec cases are marked with a `Spec` trait (`T-1` … `T-35`). `PathSegmentTests` and `MissingUrl_ThrowsArgumentException_SendsNothing` have no spec trait.
- Assembly and MCP client version `0.6.0`. Redirects are off. A call does not retry.

## What not to touch

- Secrets. Do not commit Member tokens or platform admin keys. Test fixtures use stand-in values such as `member-token` and `admin-key`.
- Generated output: `bin/`, `obj/`, `TestResults/`, `*.nupkg`, `*.snupkg`.
- The public API. Ask Victor before changing method names, the 19 tool names, the 33 REST paths, model shapes, or the strings in `LibraryMessages`.
- `IsPackable`, `GeneratePackageOnBuild`, package versions, and the `global.json` pin, unless Victor asks. `T-20` requires a `net10.0` DLL and forbids a package.
- `.grok/` and other agent instruction formats. Agent instructions are this file and `.cursor/rules/`.
