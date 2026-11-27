namespace AgentConnectLibrary;

/// <summary>
/// A call failed for a reason other than library misuse.
/// Library misuse (a missing URL or credential) throws <see cref="ArgumentException"/> instead.
/// </summary>
public sealed class AgentConnectException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentConnectException"/> class.
    /// </summary>
    /// <param name="kind">Whether the server, the timeout, or the transport failed.</param>
    /// <param name="message">The server's error text, or a library message that is not an AgentConnect error text.</param>
    /// <param name="statusCode">The HTTP status, when the server sent one.</param>
    /// <param name="retryAfter">The raw <c>Retry-After</c> header, when the server sent one.</param>
    /// <param name="innerException">The transport or protocol exception, when there is one.</param>
    public AgentConnectException(
        AgentConnectErrorKind kind,
        string message,
        int? statusCode = null,
        string? retryAfter = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
        StatusCode = statusCode;
        RetryAfter = retryAfter;
    }

    /// <summary>Gets whether the server, the timeout, or the transport failed.</summary>
    public AgentConnectErrorKind Kind { get; }

    /// <summary>Gets the HTTP status when the server sent one.</summary>
    public int? StatusCode { get; }

    /// <summary>Gets the raw <c>Retry-After</c> header value when the server sent one.</summary>
    public string? RetryAfter { get; }
}

/// <summary>
/// Why an <see cref="AgentConnectException"/> was thrown.
/// </summary>
public enum AgentConnectErrorKind
{
    /// <summary>AgentConnect returned an error. <see cref="Exception.Message"/> is the server's text, unchanged.</summary>
    Server,

    /// <summary>The library timeout elapsed. A canceled <see cref="CancellationToken"/> is not this kind.</summary>
    Timeout,

    /// <summary>The host could not be reached, or the connection failed.</summary>
    Transport,
}
