using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>
/// One <c>where</c> condition: <c>{"path", "op", "value"}</c>.
/// Every condition must match. This is the AgentConnect filter, not a new language.
/// </summary>
public sealed record FilterCondition
{
    /// <summary>Gets the property path.</summary>
    public required string Path { get; init; }

    /// <summary>Gets the operator name, such as <c>eq</c>, <c>startsWith</c>, or <c>in</c>.</summary>
    public required string Op { get; init; }

    /// <summary>Gets the comparison value as raw JSON, so numbers and strings are not rewritten.</summary>
    public required JsonElement Value { get; init; }
}

/// <summary>
/// An <c>orderBy</c> value: <c>{"path", "desc"}</c>. <c>desc</c> is omitted unless the caller sets it.
/// </summary>
public sealed record OrderBy
{
    /// <summary>Gets the property path to sort on.</summary>
    public required string Path { get; init; }

    /// <summary>Gets whether to sort descending. Null leaves <c>desc</c> out of the JSON.</summary>
    public bool? Desc { get; init; }
}
