using System.Globalization;

namespace AgentConnectLibrary;

/// <summary>
/// The Item <c>schemaVersion</c> filter from AgentConnect.
/// It is <c>{"eq": n}</c> or <c>{"belowCurrent": true}</c>.
/// On REST it is sent inside the <c>query</c> parameter. On MCP it is a top-level argument.
/// </summary>
public sealed class SchemaVersionFilter
{
    private SchemaVersionFilter(string json)
    {
        Json = json;
    }

    /// <summary>Gets the JSON object sent to AgentConnect.</summary>
    internal string Json { get; }

    /// <summary>
    /// Items whose stored schema version equals <paramref name="version"/>.
    /// </summary>
    /// <param name="version">The version. The server requires a whole number of at least 1.</param>
    /// <returns>The filter.</returns>
    public static SchemaVersionFilter Eq(int version)
    {
        return new SchemaVersionFilter("{\"eq\":" + version.ToString(CultureInfo.InvariantCulture) + "}");
    }

    /// <summary>Items whose stored schema version is below the List's current version.</summary>
    public static SchemaVersionFilter BelowCurrent { get; } = new("{\"belowCurrent\":true}");
}
