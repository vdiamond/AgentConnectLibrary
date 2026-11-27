namespace AgentConnectLibrary;

/// <summary>
/// One entry in an MCP list result. The entity is the same type the REST surface returns.
/// REST list results do not use this wrapper and have no row.
/// </summary>
/// <typeparam name="T">The shared entity type.</typeparam>
public sealed record McpRow<T>
{
    /// <summary>Gets the entity. Its id is the id AgentConnect returned.</summary>
    public required T Entity { get; init; }

    /// <summary>Gets the row number AgentConnect assigned, starting at 1 for the first entry of the query.</summary>
    public required int Row { get; init; }
}
