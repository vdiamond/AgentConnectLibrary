namespace AgentConnectLibrary;

/// <summary>
/// A Workspace role, sent and returned as <c>WorkspaceOwner</c>, <c>WorkspaceWrite</c>, or <c>WorkspaceRead</c>.
/// </summary>
public enum WorkspaceRole
{
    /// <summary>The WorkspaceOwner role.</summary>
    WorkspaceOwner,

    /// <summary>The WorkspaceWrite role.</summary>
    WorkspaceWrite,

    /// <summary>The WorkspaceRead role.</summary>
    WorkspaceRead,
}
