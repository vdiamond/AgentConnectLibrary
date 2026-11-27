# AgentConnectLibrary

A .NET 10 class library that calls [AgentConnect](https://github.com/vdiamond/AgentConnect) Approved v9. It is a DLL. It is not a NuGet package.

Guidance for Cursor agents is in [AGENTS.md](AGENTS.md).

The library has two surfaces on one client:

- `Mcp` has one async method for each of the 19 MCP tools.
- `Rest` has one async method for each of the 33 REST endpoints.

You pass the MCP URL, the REST base URL, a Member token, and an optional platform admin key once. Each call is asynchronous, uses a 30 second timeout unless you change it, and returns a typed model. AgentConnect still enforces its own rules. This library does not.

## Build and test

```bash
dotnet build
dotnet test
```

Both commands use the repo root. Tests talk to stubbed HTTP and MCP endpoints. They do not need a live AgentConnect server.

The library project is `src/AgentConnectLibrary`. Its output is `AgentConnectLibrary.dll`.

## Use the library

Reference `AgentConnectLibrary.dll`, or a project reference to `src/AgentConnectLibrary`.

```csharp
using AgentConnectLibrary;

using var client = new AgentConnectClient(
    mcpServerUrl: "https://connect.example/mcp",
    restBaseUrl: "https://connect.example",
    memberToken: "the-member-token",
    adminKey: "the-platform-admin-key");

var workspaces = await client.Mcp.ListWorkspacesAsync();
foreach (var entry in workspaces.Workspaces)
{
    Console.WriteLine($"{entry.Row}: {entry.Entity.Name} ({entry.Entity.Id})");
}

var settings = await client.Rest.GetTenantAsync();

var items = await client.Rest.QueryItemsAsync(
    "workspace-id",
    "list-id",
    shape: "full",
    schemaVersion: SchemaVersionFilter.BelowCurrent,
    where:
    [
        new FilterCondition
        {
            Path = "status",
            Op = "eq",
            Value = JsonValues.String("open"),
        },
    ],
    pageSize: 50);
```

`schemaVersion`, `where`, and `orderBy` on `Rest.QueryItemsAsync` are sent together as one URL-encoded JSON `query` parameter. `shape`, `pageSize`, and `continuation` are plain query parameters.

Leave `description` unset to omit it. Pass null to clear it:

```csharp
await client.Rest.UpdateWorkspaceAsync("workspace-id", description: null);
```

A list call returns one page. When `Continuation` is set, pass it on the next call. The library does not walk pages and does not retry.

Change `client.MemberToken`, `client.AdminKey`, `client.McpServerUrl`, `client.RestBaseUrl`, or `client.Timeout` for calls that have not started. A call already running keeps the credentials it started with. One instance can be used from several threads.

Platform-operator calls (`CreateTenantAsync`, `SendTenantInvitationAsync`, `SuspendTenantAsync`, `ResumeTenantAsync`, `DeleteTenantAsync`) send `X-Admin-Key` and do not send the Member token. Every other call sends `Authorization: Bearer` and does not send the admin key.

```csharp
client.Timeout = TimeSpan.FromSeconds(60);
await client.Mcp.ListWorkspacesAsync(timeout: TimeSpan.FromSeconds(10), cancellationToken: CancellationToken.None);
```

Canceling the token throws `OperationCanceledException`. The library timeout throws `AgentConnectException` with `Kind` `Timeout`. A connection failure uses `Kind` `Transport`. An AgentConnect error uses `Kind` `Server`, with the HTTP status when there is one and the server's error text unchanged. `RetryAfter` is the raw `Retry-After` header when the server sends one.

Schema documents, Item `data`, and batch `result` values are `JsonElement`. They are sent and returned as JSON, not mapped to other C# types. `JsonValues.Parse` keeps the text you wrote.
