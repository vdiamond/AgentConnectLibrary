namespace AgentConnectLibrary;

internal static class LibraryMessages
{
    public const string Timeout = "The call timed out.";

    public const string Transport = "The host could not be reached.";

    public const string MissingMcpUrl = "An MCP server URL is required for this call.";

    public const string MissingRestUrl = "A REST base URL is required for this call.";

    public const string MissingToken = "A Member token is required for this call.";

    public const string MissingAdminKey = "A platform admin key is required for this call.";

    public const string BadUrl = "The URL must be an absolute HTTP or HTTPS URL.";

    public const string Unreadable = "The response could not be read.";

    public const string BadTimeout = "The timeout must be greater than zero.";
}
