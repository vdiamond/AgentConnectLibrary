namespace AgentConnectLibrary;

/// <summary>A Member who holds a role on a Workspace.</summary>
public sealed record WorkspaceMember
{
    /// <summary>Gets the email.</summary>
    public required string Email { get; init; }

    /// <summary>Gets the role: <c>WorkspaceOwner</c>, <c>WorkspaceWrite</c>, or <c>WorkspaceRead</c>.</summary>
    public required string Role { get; init; }
}

/// <summary>One REST page of Workspace members. There is no row number.</summary>
public sealed record WorkspaceMemberPage
{
    /// <summary>Gets the members on this page.</summary>
    public required IReadOnlyList<WorkspaceMember> Members { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>One MCP page of Workspace members. Each entry has a row number.</summary>
public sealed record McpWorkspaceMemberPage
{
    /// <summary>Gets the members on this page.</summary>
    public required IReadOnlyList<McpRow<WorkspaceMember>> Members { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>A Workspace role assignment, including whether the email was sent.</summary>
public sealed record RoleAssignment
{
    /// <summary>Gets the email.</summary>
    public required string Email { get; init; }

    /// <summary>Gets the role.</summary>
    public required string Role { get; init; }

    /// <summary>Gets whether the invitation or Workspace-access email was sent.</summary>
    public required bool EmailSent { get; init; }
}
