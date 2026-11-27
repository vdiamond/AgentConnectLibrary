using System.Net;
using System.Text;
using System.Text.Json;

namespace AgentConnectLibrary.Tests;

internal sealed record StandInRequest(HttpMethod Method, Uri Uri, string? Body, string? Authorization, string? AdminKey);

internal sealed class StandInHandler : HttpMessageHandler
{
    private readonly object _gate = new();
    private readonly List<StandInRequest> _requests = [];

    public Func<StandInRequest, CancellationToken, Task<HttpResponseMessage>>? OnRequest { get; set; }

    public IReadOnlyList<StandInRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return _requests.ToArray();
            }
        }
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK, string? retryAfter = null)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }

        return response;
    }

    public static HttpResponseMessage Error(int status, string text, string? retryAfter = null)
    {
        return Json("{\"error\":" + JsonSerializer.Serialize(text) + "}", (HttpStatusCode)status, retryAfter);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var captured = new StandInRequest(
            request.Method,
            request.RequestUri ?? new Uri("http://missing.test/"),
            body,
            Header(request, "Authorization"),
            Header(request, "X-Admin-Key"));
        lock (_gate)
        {
            _requests.Add(captured);
        }

        if (OnRequest is null)
        {
            return Json("{}");
        }

        return await OnRequest(captured, cancellationToken).ConfigureAwait(false);
    }

    #region private

    private static string? Header(HttpRequestMessage request, string name)
    {
        return request.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }

    #endregion
}

internal static class McpStub
{
    public static HttpResponseMessage Respond(string? body, Func<string, string> toolResult)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        var root = document.RootElement;
        var method = root.TryGetProperty("method", out var methodValue) ? methodValue.GetString() : "";
        var id = root.TryGetProperty("id", out var idValue) ? idValue.GetRawText() : "null";
        if (method == "server/discover")
        {
            return StandInHandler.Json(
                "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{\"supportedVersions\":[\"2026-07-28\"],\"capabilities\":{\"tools\":{}},\"instructions\":\"test\",\"ttlMs\":0,\"cacheScope\":\"private\"}}");
        }

        if (method == "tools/call")
        {
            var name = root.GetProperty("params").GetProperty("name").GetString() ?? "";
            var text = JsonSerializer.Serialize(toolResult(name));
            return StandInHandler.Json(
                "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{\"content\":[{\"type\":\"text\",\"text\":" + text + "}],\"isError\":false}}");
        }

        if (!root.TryGetProperty("id", out _))
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        return StandInHandler.Json("{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{}}");
    }

    public static HttpResponseMessage ToolText(string? body, string text, bool isError)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        var root = document.RootElement;
        var id = root.TryGetProperty("id", out var idValue) ? idValue.GetRawText() : "null";
        var method = root.TryGetProperty("method", out var methodValue) ? methodValue.GetString() : "";
        if (method == "server/discover")
        {
            return Respond(body, _ => "{}");
        }

        var encoded = JsonSerializer.Serialize(text);
        return StandInHandler.Json(
            "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{\"content\":[{\"type\":\"text\",\"text\":" + encoded + "}],\"isError\":" + (isError ? "true" : "false") + "}}");
    }

    public static IReadOnlyList<string> ToolNames(IEnumerable<StandInRequest> requests)
    {
        var names = new List<string>();
        foreach (var request in requests)
        {
            if (request.Body is null || !request.Body.Contains("\"tools/call\"", StringComparison.Ordinal))
            {
                continue;
            }

            using var document = JsonDocument.Parse(request.Body);
            names.Add(document.RootElement.GetProperty("params").GetProperty("name").GetString() ?? "");
        }

        return names;
    }
}

internal static class Payloads
{
    public const string RawJson = """{"amount":1.2300,"when":"2026-11-25T10:00:00"}""";

    public const string TenantCreated = """{"id":"t1","name":"Acme","status":"Active","pageSizeDefault":100,"pageSizeMax":1000,"firstMemberId":"m1","token":"once","emailSent":true}""";

    public const string TenantSettings = """{"pageSizeDefault":100,"pageSizeMax":1000}""";

    public const string EmailSent = """{"emailSent":true}""";

    public const string EmailNotSent = """{"emailSent":false}""";

    public const string Deleted = """{"deleted":true}""";

    public const string Suspended = """{"status":"Suspended"}""";

    public const string Active = """{"status":"Active"}""";

    public const string MemberWithToken = """{"id":"m1","email":"ann@example.com","isTenantOwner":false,"invitationSent":false,"token":"plain-token"}""";

    public const string Member = """{"id":"m1","email":"ann@example.com","isTenantOwner":true,"invitationSent":true}""";

    public const string MemberPage = """{"members":[{"id":"m1","email":"ann@example.com","isTenantOwner":true,"invitationSent":true}]}""";

    public const string McpMemberPage = """{"members":[{"id":"m1","email":"ann@example.com","isTenantOwner":true,"invitationSent":false,"row":1}]}""";

    public const string Token = """{"lastTokenIssued":"2026-11-27T18:00:00Z","token":"new-token"}""";

    public const string WorkspaceCreated = """{"id":"w1","name":"Ops","description":"Desk","members":[{"email":"ann@example.com","role":"WorkspaceWrite","emailSent":true}]}""";

    public const string Workspace = """{"id":"w1","name":"Ops","description":"Desk"}""";

    public const string WorkspacePage = """{"workspaces":[{"id":"w1","name":"Ops","description":"Desk","role":"WorkspaceOwner"}],"continuation":"next"}""";

    public const string McpWorkspacePage = """{"workspaces":[{"id":"w1","name":"Ops","description":"Desk","role":"WorkspaceOwner","row":1}],"continuation":"next"}""";

    public const string WorkspaceMember = """{"email":"ann@example.com","role":"WorkspaceRead"}""";

    public const string WorkspaceMemberPage = """{"members":[{"email":"ann@example.com","role":"WorkspaceRead"}]}""";

    public const string McpWorkspaceMemberPage = """{"members":[{"email":"ann@example.com","role":"WorkspaceOwner","row":2}]}""";

    public const string RoleAssignment = """{"email":"ann@example.com","role":"WorkspaceWrite","emailSent":true}""";

    public const string List = """{"id":"l1","name":"Tasks","schemaVersion":1,"description":"Work"}""";

    public const string ListPage = """{"lists":[{"id":"l1","name":"Tasks","schemaVersion":1}]}""";

    public const string McpListPage = """{"lists":[{"id":"l1","name":"Tasks","schemaVersion":1,"row":1}]}""";

    public const string Schema = """{"schema":{"type":"object"},"schemaVersion":1}""";

    public const string Item = """{"id":"i1","schemaVersion":1,"data":{"amount":1}}""";

    public const string ItemPage = """{"items":[{"id":"i1","schemaVersion":1,"data":{"amount":1}}]}""";

    public const string McpItemPage = """{"items":[{"id":"i1","schemaVersion":1,"data":{"amount":1},"row":1}]}""";

    public const string ItemUpserted = """{"id":"i1","schemaVersion":1}""";

    public const string Batch = """{"results":[{"index":0,"status":200,"result":{"id":"i1"}}]}""";

    public static HttpResponseMessage Mcp(string? body)
    {
        return McpStub.Respond(body, ToolJson);
    }

    public static HttpResponseMessage Rest(StandInRequest request)
    {
        var path = request.Uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
        var key = request.Method.Method + " " + path;
        var json = key switch
        {
            "POST tenants" => TenantCreated,
            "POST tenants/t1/send-invitation" => EmailSent,
            "POST tenants/t1/suspend" => Suspended,
            "POST tenants/t1/resume" => Active,
            "DELETE tenants/t1" => Deleted,
            "GET tenant" => TenantSettings,
            "PATCH tenant" => TenantSettings,
            "POST members" => MemberWithToken,
            "POST members/ann@example.com/send-invitation" => EmailSent,
            "DELETE members/ann@example.com" => Deleted,
            "PUT members/ann@example.com/tenant-owner" => Member,
            "POST members/ann@example.com/token/regenerate" => Token,
            "POST members/ann@example.com/token/send" => EmailSent,
            "GET members" => MemberPage,
            "POST workspaces" => WorkspaceCreated,
            "DELETE workspaces/w1" => Deleted,
            "GET workspaces" => WorkspacePage,
            "PATCH workspaces/w1" => Workspace,
            "GET workspaces/w1/members" => WorkspaceMemberPage,
            "POST workspaces/w1/members" => RoleAssignment,
            "DELETE workspaces/w1/members/ann@example.com" => Deleted,
            "PATCH workspaces/w1/members/ann@example.com" => WorkspaceMember,
            "POST workspaces/w1/lists" => List,
            "PATCH workspaces/w1/lists/l1" => List,
            "DELETE workspaces/w1/lists/l1" => Deleted,
            "GET workspaces/w1/lists" => ListPage,
            "PUT workspaces/w1/lists/l1/schema" => Schema,
            "GET workspaces/w1/lists/l1/schema" => Schema,
            "GET workspaces/w1/lists/l1/items" => ItemPage,
            "GET workspaces/w1/lists/l1/items/i1" => Item,
            "PUT workspaces/w1/lists/l1/items/i1" => ItemUpserted,
            "DELETE workspaces/w1/lists/l1/items/i1" => Deleted,
            "POST batch" => Batch,
            _ => throw new InvalidOperationException("No stand-in body for " + key),
        };
        return StandInHandler.Json(json);
    }

    public static string ToolJson(string name)
    {
        return name switch
        {
            "list_workspaces" => McpWorkspacePage,
            "update_workspace" => Workspace,
            "list_lists" => McpListPage,
            "create_list" => List,
            "update_list" => List,
            "delete_list" => Deleted,
            "get_schema" => Schema,
            "update_schema" => Schema,
            "query_items" => McpItemPage,
            "get_item" => Item,
            "upsert_item" => ItemUpserted,
            "delete_item" => Deleted,
            "invite_member" => RoleAssignment,
            "change_workspace_role" => WorkspaceMember,
            "revoke_workspace_access" => Deleted,
            "list_workspace_members" => McpWorkspaceMemberPage,
            "set_tenant_owner" => Member,
            "query_members" => McpMemberPage,
            "batch" => Batch,
            _ => throw new InvalidOperationException("No stand-in body for " + name),
        };
    }
}

internal static class Clients
{
    public static AgentConnectClient Create(
        HttpMessageHandler handler,
        string? token = "member-token",
        string? admin = "admin-key",
        string? mcp = "http://stand-in.test/mcp",
        string? rest = "http://stand-in.test")
    {
        return new AgentConnectClient(new AgentConnectClientOptions
        {
            McpServerUrl = mcp,
            RestBaseUrl = rest,
            MemberToken = token,
            AdminKey = admin,
            HttpMessageHandler = handler,
        });
    }

    public static Dictionary<string, string> Query(Uri uri)
    {
        var text = uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (text.Length == 0)
        {
            return result;
        }

        foreach (var part in text.Split('&'))
        {
            var split = part.IndexOf('=');
            var name = Uri.UnescapeDataString(split < 0 ? part : part[..split]);
            var value = split < 0 ? "" : Uri.UnescapeDataString(part[(split + 1)..]);
            result[name] = value;
        }

        return result;
    }

    public static string EscapedPath(Uri uri)
    {
        return uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
    }
}
