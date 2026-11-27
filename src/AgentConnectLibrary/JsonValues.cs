using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>
/// Builds a <see cref="JsonElement"/> the caller can put in a filter, a schema, or Item data.
/// <see cref="Parse"/> keeps the JSON text as it was written.
/// </summary>
public static class JsonValues
{
    /// <summary>A JSON string.</summary>
    /// <param name="value">The string.</param>
    /// <returns>The JSON string element.</returns>
    public static JsonElement String(string value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    /// <summary>A JSON number.</summary>
    /// <param name="value">The number.</param>
    /// <returns>The JSON number element.</returns>
    public static JsonElement Number(int value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    /// <summary>A JSON boolean.</summary>
    /// <param name="value">The boolean.</param>
    /// <returns>The JSON boolean element.</returns>
    public static JsonElement Boolean(bool value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    /// <summary>
    /// Parses JSON and returns an element that outlives the parser.
    /// The text is kept, so a decimal is not shortened and a DateTime string is not rewritten.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The parsed element.</returns>
    public static JsonElement Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
