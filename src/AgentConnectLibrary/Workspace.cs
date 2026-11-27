namespace AgentConnectLibrary;

/// <summary>
/// A Workspace. List entries include <see cref="Role"/> (null when the caller has none).
/// An update response has no role, so <see cref="Role"/> is null there.
/// </summary>
public sealed record Workspace
{
    /// <summary>Gets the Workspace id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the description, when the Workspace has one.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the caller's role on a list entry, or null when the response has no role.</summary>
    public string? Role { get; init; }
}

/// <summary>One REST page of Workspaces. There is no row number.</summary>
public sealed record WorkspacePage
{
    /// <summary>Gets the Workspaces on this page.</summary>
    public required IReadOnlyList<Workspace> Workspaces { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>One MCP page of Workspaces. Each entry has a row number.</summary>
public sealed record McpWorkspacePage
{
    /// <summary>Gets the Workspaces on this page.</summary>
    public required IReadOnlyList<McpRow<Workspace>> Workspaces { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>A Workspace that was just created, including the initial role assignments.</summary>
public sealed record WorkspaceCreated
{
    /// <summary>Gets the Workspace id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the description, when one was stored.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the initial role assignments.</summary>
    public required IReadOnlyList<RoleAssignment> Members { get; init; }
}

/// <summary>An email and role sent when creating a Workspace or adding a Workspace member.</summary>
public sealed record WorkspaceMemberInput
{
    /// <summary>Gets the email.</summary>
    public required string Email { get; init; }

    /// <summary>Gets the role.</summary>
    public required WorkspaceRole Role { get; init; }
}
