namespace AgentConnectLibrary;

/// <summary>
/// A List: id, name, current schema version, and description when one is set.
/// </summary>
public sealed record ListRecord
{
    /// <summary>Gets the List id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the current schema version.</summary>
    public required int SchemaVersion { get; init; }

    /// <summary>Gets the description, when the List has one.</summary>
    public string? Description { get; init; }
}

/// <summary>One REST page of Lists. There is no row number.</summary>
public sealed record ListPage
{
    /// <summary>Gets the Lists on this page.</summary>
    public required IReadOnlyList<ListRecord> Lists { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>One MCP page of Lists. Each entry has a row number.</summary>
public sealed record McpListPage
{
    /// <summary>Gets the Lists on this page.</summary>
    public required IReadOnlyList<McpRow<ListRecord>> Lists { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}
