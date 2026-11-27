using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>
/// An Item. <see cref="Data"/> is the stored JSON when the response includes it, and null for <c>shape</c> <c>ids</c>.
/// The JSON is not mapped to a C# type.
/// </summary>
public sealed record Item
{
    /// <summary>Gets the Item id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the schema version the Item was last validated against.</summary>
    public required int SchemaVersion { get; init; }

    /// <summary>Gets the stored JSON, when this response includes it.</summary>
    public JsonElement? Data { get; init; }
}

/// <summary>One REST page of Items. There is no row number.</summary>
public sealed record ItemPage
{
    /// <summary>Gets the Items on this page.</summary>
    public required IReadOnlyList<Item> Items { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>One MCP page of Items. Each entry has a row number.</summary>
public sealed record McpItemPage
{
    /// <summary>Gets the Items on this page.</summary>
    public required IReadOnlyList<McpRow<Item>> Items { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>The id and schema version returned by an Item upsert.</summary>
public sealed record ItemUpserted
{
    /// <summary>Gets the Item id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the schema version the Item was validated against.</summary>
    public required int SchemaVersion { get; init; }
}
