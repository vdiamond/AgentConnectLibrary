using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>A List schema document and its version. The schema JSON is not mapped to a C# type.</summary>
public sealed record SchemaResult
{
    /// <summary>Gets the EasyJsonSchema document.</summary>
    public required JsonElement Schema { get; init; }

    /// <summary>Gets the schema version.</summary>
    public required int SchemaVersion { get; init; }
}
