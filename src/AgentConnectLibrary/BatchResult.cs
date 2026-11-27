using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>
/// One operation in a batch. Fields the caller does not set are left out of the JSON.
/// <see cref="Name"/> and <see cref="Description"/> can be set to null, which sends JSON null.
/// </summary>
public sealed record BatchOperation
{
    /// <summary>Gets the operation: <c>upsert</c>, <c>delete</c>, or <c>get</c>.</summary>
    public required string Op { get; init; }

    /// <summary>Gets the entity type: <c>member</c>, <c>workspace</c>, <c>list</c>, or <c>item</c>.</summary>
    public required string Type { get; init; }

    /// <summary>Gets the email, when this operation uses one.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the TenantOwner flag, when this operation sets one.</summary>
    public bool? IsTenantOwner { get; init; }

    /// <summary>Gets the Workspace id, when this operation uses one.</summary>
    public string? WorkspaceId { get; init; }

    /// <summary>Gets the name, when the caller sets it. A set null is sent as JSON null.</summary>
    public Optional<string> Name { get; init; }

    /// <summary>Gets the description, when the caller sets it. A set null clears the description.</summary>
    public Optional<string> Description { get; init; }

    /// <summary>Gets the List id, when this operation uses one.</summary>
    public string? ListId { get; init; }

    /// <summary>Gets the Item id, when this operation uses one.</summary>
    public string? ItemId { get; init; }

    /// <summary>Gets the schema JSON, when this operation sends one.</summary>
    public JsonElement? Schema { get; init; }

    /// <summary>Gets the Item JSON, when this operation sends some.</summary>
    public JsonElement? Data { get; init; }
}

/// <summary>The batch response. HTTP 200 with a failing operation is returned, not thrown.</summary>
public sealed record BatchResult
{
    /// <summary>Gets one entry per operation, in order.</summary>
    public required IReadOnlyList<BatchOperationResult> Results { get; init; }
}

/// <summary>One operation's result. <see cref="Result"/> is raw JSON when the status is 200.</summary>
public sealed record BatchOperationResult
{
    /// <summary>Gets the 0-based operation index.</summary>
    public required int Index { get; init; }

    /// <summary>Gets the status the single call would return, or 424 when the operation was not applied.</summary>
    public required int Status { get; init; }

    /// <summary>Gets the success body when <see cref="Status"/> is 200.</summary>
    public JsonElement? Result { get; init; }

    /// <summary>Gets the error text when the operation failed.</summary>
    public string? Error { get; init; }
}
