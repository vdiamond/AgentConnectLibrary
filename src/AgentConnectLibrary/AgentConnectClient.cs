namespace AgentConnectLibrary;

/// <summary>
/// Values used to construct an <see cref="AgentConnectClient"/>.
/// The URLs, Member token, admin key, and timeout can be changed later on the client.
/// A single call cannot override the URLs or credentials.
/// </summary>
public sealed class AgentConnectClientOptions
{
    /// <summary>Gets or sets the full MCP Streamable HTTP URL, including any path.</summary>
    public string? McpServerUrl { get; set; }

    /// <summary>Gets or sets the base URL to which AgentConnect REST paths are appended.</summary>
    public string? RestBaseUrl { get; set; }

    /// <summary>Gets or sets the Member token sent as <c>Authorization: Bearer</c>.</summary>
    public string? MemberToken { get; set; }

    /// <summary>Gets or sets the platform admin key sent as <c>X-Admin-Key</c> on the five platform-operator routes.</summary>
    public string? AdminKey { get; set; }

    /// <summary>Gets or sets the default timeout. Null uses 30 seconds.</summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the HTTP handler both surfaces use.
    /// Leave it unset to use a handler owned by the client. The client does not dispose a handler the caller supplies.
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}

/// <summary>
/// A client for the AgentConnect MCP server and REST API.
/// One instance can be used from several threads. A call keeps the credentials it started with.
/// </summary>
public sealed class AgentConnectClient : IDisposable
{
    /// <summary>The timeout used when the caller does not set one. It is 30 seconds.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Constructs a client with the two URLs, a Member token, and an optional platform admin key.
    /// </summary>
    /// <param name="mcpServerUrl">The full MCP Streamable HTTP URL.</param>
    /// <param name="restBaseUrl">The REST base URL.</param>
    /// <param name="memberToken">The Member token, or null when the caller will set it later.</param>
    /// <param name="adminKey">The platform admin key, or null when the caller will not use platform-operator routes.</param>
    public AgentConnectClient(string? mcpServerUrl, string? restBaseUrl, string? memberToken = null, string? adminKey = null)
        : this(new AgentConnectClientOptions
        {
            McpServerUrl = mcpServerUrl,
            RestBaseUrl = restBaseUrl,
            MemberToken = memberToken,
            AdminKey = adminKey,
        })
    {
    }

    /// <summary>
    /// Constructs a client from options.
    /// </summary>
    /// <param name="options">The URLs, credentials, timeout, and optional HTTP handler.</param>
    public AgentConnectClient(AgentConnectClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _mcpServerUrl = options.McpServerUrl;
        _restBaseUrl = options.RestBaseUrl;
        _memberToken = options.MemberToken;
        _adminKey = options.AdminKey;
        _timeout = options.Timeout ?? DefaultTimeout;
        _ownsHandler = options.HttpMessageHandler is null;
        _handler = options.HttpMessageHandler ?? new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
        };
        Transport = new AgentConnectTransport(_handler);
        Mcp = new AgentConnectMcp(this);
        Rest = new AgentConnectRest(this);
    }

    /// <summary>Gets the MCP surface. It has one method for each AgentConnect v9 tool.</summary>
    public AgentConnectMcp Mcp { get; }

    /// <summary>Gets the REST surface. It has one method for each AgentConnect v9 endpoint.</summary>
    public AgentConnectRest Rest { get; }

    /// <summary>Gets or sets the MCP server URL used by calls that start after the change.</summary>
    public string? McpServerUrl
    {
        get
        {
            lock (_gate)
            {
                return _mcpServerUrl;
            }
        }

        set
        {
            lock (_gate)
            {
                _mcpServerUrl = value;
            }
        }
    }

    /// <summary>Gets or sets the REST base URL used by calls that start after the change.</summary>
    public string? RestBaseUrl
    {
        get
        {
            lock (_gate)
            {
                return _restBaseUrl;
            }
        }

        set
        {
            lock (_gate)
            {
                _restBaseUrl = value;
            }
        }
    }

    /// <summary>Gets or sets the Member token used by calls that start after the change.</summary>
    public string? MemberToken
    {
        get
        {
            lock (_gate)
            {
                return _memberToken;
            }
        }

        set
        {
            lock (_gate)
            {
                _memberToken = value;
            }
        }
    }

    /// <summary>Gets or sets the platform admin key used by calls that start after the change.</summary>
    public string? AdminKey
    {
        get
        {
            lock (_gate)
            {
                return _adminKey;
            }
        }

        set
        {
            lock (_gate)
            {
                _adminKey = value;
            }
        }
    }

    /// <summary>Gets or sets the timeout used by calls that do not pass their own. The default is 30 seconds.</summary>
    public TimeSpan Timeout
    {
        get
        {
            lock (_gate)
            {
                return _timeout;
            }
        }

        set
        {
            lock (_gate)
            {
                _timeout = value;
            }
        }
    }

    /// <summary>Disposes the HTTP handler when this client created it.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        if (_ownsHandler)
        {
            _handler.Dispose();
        }
    }

    internal AgentConnectTransport Transport { get; }

    internal ClientSnapshot Snapshot()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return new ClientSnapshot(_mcpServerUrl, _restBaseUrl, _memberToken, _adminKey, _timeout);
        }
    }

    #region private

    private readonly Lock _gate = new();
    private readonly bool _ownsHandler;
    private readonly HttpMessageHandler _handler;
    private string? _mcpServerUrl;
    private string? _restBaseUrl;
    private string? _memberToken;
    private string? _adminKey;
    private TimeSpan _timeout;
    private bool _disposed;

    #endregion
}
