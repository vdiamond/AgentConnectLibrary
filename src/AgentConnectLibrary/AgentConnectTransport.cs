using System.Net;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AgentConnectLibrary;

internal readonly record struct ClientSnapshot(
    string? McpServerUrl,
    string? RestBaseUrl,
    string? MemberToken,
    string? AdminKey,
    TimeSpan Timeout);

/// <summary>
/// Sends one REST request or one MCP tool call. A call does not retry.
/// </summary>
internal sealed class AgentConnectTransport
{
    public AgentConnectTransport(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public async Task<T> RestAsync<T>(
        ClientSnapshot snapshot,
        bool admin,
        HttpMethod method,
        IReadOnlyList<string> segments,
        string? body,
        IReadOnlyList<KeyValuePair<string, string>>? query,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var json = await SendRestAsync(snapshot, admin, method, segments, body, query, timeout, cancellationToken).ConfigureAwait(false);
        return JsonWire.Read<T>(json);
    }

    public async Task<string> ToolAsync(
        ClientSnapshot snapshot,
        string tool,
        IReadOnlyDictionary<string, object?> arguments,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        RequireUrl(snapshot.McpServerUrl, LibraryMessages.MissingMcpUrl);
        RequireCredential(snapshot.MemberToken, LibraryMessages.MissingToken);
        var limit = ResolveTimeout(timeout, snapshot.Timeout);
        using var timeoutSource = new CancellationTokenSource(limit);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
        var capture = new CapturingHandler(_handler);
        var http = new HttpClient(capture, disposeHandler: false)
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

        try
        {
            var transport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(snapshot.McpServerUrl!, UriKind.Absolute),
                    TransportMode = HttpTransportMode.StreamableHttp,
                    EnableStandaloneGetStream = false,
                    OwnsSession = false,
                    MaxReconnectionAttempts = 0,
                    ConnectionTimeout = limit,
                    Name = "AgentConnectLibrary",
                    AdditionalHeaders = new Dictionary<string, string>
                    {
                        ["Authorization"] = "Bearer " + snapshot.MemberToken,
                    },
                },
                http);

            await using var client = await McpClient.CreateAsync(transport, ClientOptions, cancellationToken: linked.Token).ConfigureAwait(false);
            var result = await client.CallToolAsync(tool, arguments, cancellationToken: linked.Token).ConfigureAwait(false);
            if (result.IsError == true)
            {
                throw new AgentConnectException(AgentConnectErrorKind.Server, ToolText(result));
            }

            var text = ToolText(result);
            if (text.Length == 0)
            {
                throw new AgentConnectException(AgentConnectErrorKind.Server, LibraryMessages.Unreadable);
            }

            return text;
        }
        catch (Exception exception) when (exception is not AgentConnectException)
        {
            throw Translate(exception, cancellationToken, capture);
        }
        finally
        {
            http.Dispose();
        }
    }

    #region private

    private static readonly McpClientOptions ClientOptions = new()
    {
        ClientInfo = new Implementation
        {
            Name = "AgentConnectLibrary",
            Version = "0.6.0",
        },
    };

    private readonly HttpMessageHandler _handler;

    private async Task<string> SendRestAsync(
        ClientSnapshot snapshot,
        bool admin,
        HttpMethod method,
        IReadOnlyList<string> segments,
        string? body,
        IReadOnlyList<KeyValuePair<string, string>>? query,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        RequireUrl(snapshot.RestBaseUrl, LibraryMessages.MissingRestUrl);
        if (admin)
        {
            RequireCredential(snapshot.AdminKey, LibraryMessages.MissingAdminKey);
        }
        else
        {
            RequireCredential(snapshot.MemberToken, LibraryMessages.MissingToken);
        }

        var limit = ResolveTimeout(timeout, snapshot.Timeout);
        using var timeoutSource = new CancellationTokenSource(limit);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
        using var http = new HttpClient(_handler, disposeHandler: false)
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        using var request = new HttpRequestMessage(method, JsonWire.BuildUri(snapshot.RestBaseUrl!, segments, query));
        if (admin)
        {
            request.Headers.TryAddWithoutValidation("X-Admin-Key", snapshot.AdminKey);
        }
        else
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + snapshot.MemberToken);
        }

        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        try
        {
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linked.Token).ConfigureAwait(false);
            var responseBody = response.Content is null
                ? ""
                : await response.Content.ReadAsStringAsync(linked.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                string? retryAfter = null;
                if (response.Headers.TryGetValues("Retry-After", out var values))
                {
                    retryAfter = values.FirstOrDefault();
                }

                throw new AgentConnectException(
                    AgentConnectErrorKind.Server,
                    ErrorText(responseBody),
                    (int)response.StatusCode,
                    retryAfter);
            }

            return responseBody;
        }
        catch (Exception exception) when (exception is not AgentConnectException)
        {
            throw Translate(exception, cancellationToken, capture: null);
        }
    }

    private static Exception Translate(Exception exception, CancellationToken cancellationToken, CapturingHandler? capture)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return new OperationCanceledException(cancellationToken);
        }

        if (exception is OperationCanceledException or TimeoutException)
        {
            return new AgentConnectException(AgentConnectErrorKind.Timeout, LibraryMessages.Timeout, innerException: exception);
        }

        if (capture is { HasError: true })
        {
            return new AgentConnectException(
                AgentConnectErrorKind.Server,
                ErrorText(capture.Body),
                capture.StatusCode,
                capture.RetryAfter,
                exception);
        }

        if (exception is HttpRequestException http && http.StatusCode is HttpStatusCode status)
        {
            return new AgentConnectException(
                AgentConnectErrorKind.Server,
                ErrorText(http.Message),
                (int)status,
                innerException: exception);
        }

        if (exception is HttpRequestException or HttpIOException or System.Net.Sockets.SocketException)
        {
            return new AgentConnectException(AgentConnectErrorKind.Transport, LibraryMessages.Transport, innerException: exception);
        }

        return exception;
    }

    private static string ToolText(CallToolResult result)
    {
        var builder = new StringBuilder();
        foreach (var block in result.Content)
        {
            if (block is TextContentBlock text && text.Text is not null)
            {
                builder.Append(text.Text);
            }
        }

        return builder.ToString();
    }

    private static string ErrorText(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return "";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.String)
            {
                return error.GetString() ?? "";
            }
        }
        catch (JsonException)
        {
            // The body is not an AgentConnect error object. Return it unchanged.
        }

        var marker = "Response body: ";
        var index = body.LastIndexOf(marker, StringComparison.Ordinal);
        if (index >= 0)
        {
            return ErrorText(body[(index + marker.Length)..]);
        }

        return body;
    }

    private static void RequireUrl(string? url, string missingMessage)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException(missingMessage);
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(LibraryMessages.BadUrl);
        }
    }

    private static void RequireCredential(string? secret, string message)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException(message);
        }
    }

    private static TimeSpan ResolveTimeout(TimeSpan? callTimeout, TimeSpan instanceTimeout)
    {
        var timeout = callTimeout ?? instanceTimeout;
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(callTimeout), LibraryMessages.BadTimeout);
        }

        return timeout;
    }

    #endregion
}

internal sealed class CapturingHandler : DelegatingHandler
{
    public CapturingHandler(HttpMessageHandler inner)
        : base(inner)
    {
    }

    public bool HasError { get; private set; }

    public int StatusCode { get; private set; }

    public string? Body { get; private set; }

    public string? RetryAfter { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if ((int)response.StatusCode < 400)
        {
            return response;
        }

        HasError = true;
        StatusCode = (int)response.StatusCode;
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            RetryAfter = values.FirstOrDefault();
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        Body = body;
        var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        response.Content = new StringContent(body, Encoding.UTF8, mediaType);
        return response;
    }
}
