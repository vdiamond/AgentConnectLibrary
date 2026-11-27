using System.Globalization;
using System.Text;
using System.Text.Json;

namespace AgentConnectLibrary;

/// <summary>
/// Writes request JSON and reads response JSON without reformatting raw values.
/// </summary>
internal static class JsonWire
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string? Raw(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined)
        {
            return null;
        }

        return element.GetRawText();
    }

    public static string? Raw(JsonElement? element)
    {
        return element is { } value ? Raw(value) : null;
    }

    public static string Object(Action<JsonObject> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            var body = new JsonObject(writer);
            writer.WriteStartObject();
            write(body);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string? Query(SchemaVersionFilter? schemaVersion, IReadOnlyList<FilterCondition>? where, OrderBy? orderBy)
    {
        if (schemaVersion is null && where is null && orderBy is null)
        {
            return null;
        }

        return Object(writer =>
        {
            if (schemaVersion is not null)
            {
                writer.Raw("schemaVersion", schemaVersion.Json);
            }

            if (where is not null)
            {
                writer.Raw("where", Where(where));
            }

            if (orderBy is not null)
            {
                writer.Raw("orderBy", Sort(orderBy));
            }
        });
    }

    public static void AddFilter(IDictionary<string, object?> arguments, IReadOnlyList<FilterCondition>? where, OrderBy? orderBy)
    {
        if (where is not null)
        {
            arguments["where"] = JsonValues.Parse(Where(where));
        }

        if (orderBy is not null)
        {
            arguments["orderBy"] = JsonValues.Parse(Sort(orderBy));
        }
    }

    public static void AddPage(IDictionary<string, object?> arguments, int? pageSize, string? continuation)
    {
        if (pageSize is int size)
        {
            arguments["pageSize"] = size;
        }

        if (continuation is not null)
        {
            arguments["continuation"] = continuation;
        }
    }

    public static void AddOptional(IDictionary<string, object?> arguments, string name, Optional<string> value)
    {
        if (!value.IsSpecified)
        {
            return;
        }

        arguments[name] = value.Value;
    }

    public static void AddRaw(IDictionary<string, object?> arguments, string name, string? raw)
    {
        if (raw is null)
        {
            return;
        }

        arguments[name] = JsonValues.Parse(raw);
    }

    public static string Operations(IReadOnlyList<BatchOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartArray();
            foreach (var operation in operations)
            {
                WriteOperation(writer, operation);
            }

            writer.WriteEndArray();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static List<KeyValuePair<string, string>> PageQuery(int? pageSize, string? continuation, string? query)
    {
        var pairs = new List<KeyValuePair<string, string>>();
        if (pageSize is int size)
        {
            pairs.Add(new KeyValuePair<string, string>("pageSize", size.ToString(CultureInfo.InvariantCulture)));
        }

        if (continuation is not null)
        {
            pairs.Add(new KeyValuePair<string, string>("continuation", continuation));
        }

        if (query is not null)
        {
            pairs.Add(new KeyValuePair<string, string>("query", query));
        }

        return pairs;
    }

    public static T Read<T>(string json)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(json, ReadOptions);
            if (value is null)
            {
                throw new JsonException("The response was empty.");
            }

            return value;
        }
        catch (JsonException exception)
        {
            throw new AgentConnectException(AgentConnectErrorKind.Server, LibraryMessages.Unreadable, innerException: exception);
        }
    }

    public static McpWorkspacePage ReadMcpWorkspaces(string json)
    {
        var (rows, continuation) = ReadRows<Workspace>(json, "workspaces");
        return new McpWorkspacePage
        {
            Workspaces = rows,
            Continuation = continuation,
        };
    }

    public static McpListPage ReadMcpLists(string json)
    {
        var (rows, continuation) = ReadRows<ListRecord>(json, "lists");
        return new McpListPage
        {
            Lists = rows,
            Continuation = continuation,
        };
    }

    public static McpMemberPage ReadMcpMembers(string json)
    {
        var (rows, continuation) = ReadRows<Member>(json, "members");
        return new McpMemberPage
        {
            Members = rows,
            Continuation = continuation,
        };
    }

    public static McpWorkspaceMemberPage ReadMcpWorkspaceMembers(string json)
    {
        var (rows, continuation) = ReadRows<WorkspaceMember>(json, "members");
        return new McpWorkspaceMemberPage
        {
            Members = rows,
            Continuation = continuation,
        };
    }

    public static McpItemPage ReadMcpItems(string json)
    {
        var (rows, continuation) = ReadRows<Item>(json, "items");
        return new McpItemPage
        {
            Items = rows,
            Continuation = continuation,
        };
    }

    public static Uri BuildUri(string baseUrl, IReadOnlyList<string> segments, IReadOnlyList<KeyValuePair<string, string>>? query)
    {
        var builder = new StringBuilder();
        builder.Append(baseUrl.TrimEnd('/'));
        foreach (var segment in segments)
        {
            builder.Append('/');
            builder.Append(Uri.EscapeDataString(segment));
        }

        if (query is { Count: > 0 })
        {
            builder.Append('?');
            for (var index = 0; index < query.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('&');
                }

                builder.Append(Uri.EscapeDataString(query[index].Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(query[index].Value));
            }
        }

        return new Uri(builder.ToString(), UriKind.Absolute);
    }

    #region private

    private static string Where(IReadOnlyList<FilterCondition> where)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartArray();
            foreach (var condition in where)
            {
                if (condition.Value.ValueKind is JsonValueKind.Undefined)
                {
                    throw new ArgumentException("A filter condition value is required.", nameof(where));
                }

                writer.WriteStartObject();
                writer.WriteString("path", condition.Path);
                writer.WriteString("op", condition.Op);
                writer.WritePropertyName("value");
                condition.Value.WriteTo(writer);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string Sort(OrderBy orderBy)
    {
        return Object(writer =>
        {
            writer.String("path", orderBy.Path);
            if (orderBy.Desc is bool descending)
            {
                writer.Boolean("desc", descending);
            }
        });
    }

    private static void WriteOperation(Utf8JsonWriter writer, BatchOperation operation)
    {
        writer.WriteStartObject();
        writer.WriteString("op", operation.Op);
        writer.WriteString("type", operation.Type);
        WriteString(writer, "email", operation.Email);
        if (operation.IsTenantOwner is bool owner)
        {
            writer.WriteBoolean("isTenantOwner", owner);
        }

        WriteString(writer, "workspaceId", operation.WorkspaceId);
        WriteOptional(writer, "name", operation.Name);
        WriteOptional(writer, "description", operation.Description);
        WriteString(writer, "listId", operation.ListId);
        WriteString(writer, "itemId", operation.ItemId);
        WriteRaw(writer, "schema", Raw(operation.Schema));
        WriteRaw(writer, "data", Raw(operation.Data));
        writer.WriteEndObject();
    }

    private static void WriteString(Utf8JsonWriter writer, string name, string? value)
    {
        if (value is null)
        {
            return;
        }

        writer.WriteString(name, value);
    }

    private static void WriteOptional(Utf8JsonWriter writer, string name, Optional<string> value)
    {
        if (!value.IsSpecified)
        {
            return;
        }

        if (value.Value is null)
        {
            writer.WriteNull(name);
            return;
        }

        writer.WriteString(name, value.Value);
    }

    private static void WriteRaw(Utf8JsonWriter writer, string name, string? raw)
    {
        if (raw is null)
        {
            return;
        }

        writer.WritePropertyName(name);
        writer.WriteRawValue(raw);
    }

    private static (IReadOnlyList<McpRow<T>> Rows, string? Continuation) ReadRows<T>(string json, string arrayName)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty(arrayName, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The response did not include " + arrayName + ".");
            }

            var rows = new List<McpRow<T>>();
            foreach (var entry in array.EnumerateArray())
            {
                if (!entry.TryGetProperty("row", out var row) || row.ValueKind != JsonValueKind.Number)
                {
                    throw new JsonException("An MCP list entry did not include row.");
                }

                rows.Add(new McpRow<T>
                {
                    Entity = Read<T>(WithoutRow(entry)),
                    Row = row.GetInt32(),
                });
            }

            string? continuation = null;
            if (root.TryGetProperty("continuation", out var token) && token.ValueKind == JsonValueKind.String)
            {
                continuation = token.GetString();
            }

            return (rows, continuation);
        }
        catch (AgentConnectException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new AgentConnectException(AgentConnectErrorKind.Server, LibraryMessages.Unreadable, innerException: exception);
        }
    }

    private static string WithoutRow(JsonElement entry)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in entry.EnumerateObject())
            {
                if (property.NameEquals("row"))
                {
                    continue;
                }

                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    #endregion
}

/// <summary>Writes one JSON object. Callers use it only while <see cref="JsonWire.Object"/> is running.</summary>
internal sealed class JsonObject
{
    private readonly Utf8JsonWriter _writer;

    public JsonObject(Utf8JsonWriter writer)
    {
        _writer = writer;
    }

    public void String(string name, string value)
    {
        _writer.WriteString(name, value);
    }

    public void StringIfSet(string name, string? value)
    {
        if (value is null)
        {
            return;
        }

        _writer.WriteString(name, value);
    }

    public void NumberIfSet(string name, int? value)
    {
        if (value is not int number)
        {
            return;
        }

        _writer.WriteNumber(name, number);
    }

    public void Boolean(string name, bool value)
    {
        _writer.WriteBoolean(name, value);
    }

    public void Optional(string name, Optional<string> value)
    {
        if (!value.IsSpecified)
        {
            return;
        }

        if (value.Value is null)
        {
            _writer.WriteNull(name);
            return;
        }

        _writer.WriteString(name, value.Value);
    }

    public void Raw(string name, string json)
    {
        _writer.WritePropertyName(name);
        _writer.WriteRawValue(json);
    }

    public void RawIfSet(string name, string? json)
    {
        if (json is null)
        {
            return;
        }

        Raw(name, json);
    }

    public void Members(IReadOnlyList<WorkspaceMemberInput>? members)
    {
        if (members is null)
        {
            return;
        }

        _writer.WritePropertyName("members");
        _writer.WriteStartArray();
        foreach (var member in members)
        {
            _writer.WriteStartObject();
            _writer.WriteString("email", member.Email);
            _writer.WriteString("role", member.Role.ToString());
            _writer.WriteEndObject();
        }

        _writer.WriteEndArray();
    }
}
