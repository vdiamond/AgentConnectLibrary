namespace AgentConnectLibrary;

/// <summary>
/// The 33 AgentConnect v9 REST endpoints. Each method calls only the REST API.
/// The server enforces roles, validation, and paging. This surface does not.
/// </summary>
public sealed class AgentConnectRest
{
    internal AgentConnectRest(AgentConnectClient client)
    {
        _client = client;
    }

    /// <summary>Calls <c>POST /tenants</c> with the platform admin key.</summary>
    /// <param name="name">The Tenant name.</param>
    /// <param name="email">The first Member's email.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Tenant, including the first Member's token once.</returns>
    public Task<TenantCreated> CreateTenantAsync(string name, string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        var body = JsonWire.Object(writer =>
        {
            writer.String("name", name);
            writer.String("email", email);
        });
        return SendAsync<TenantCreated>(true, HttpMethod.Post, ["tenants"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /tenants/{id}/send-invitation</c> with the platform admin key.</summary>
    /// <param name="id">The Tenant id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Whether the email was sent.</returns>
    public Task<EmailResult> SendTenantInvitationAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return SendAsync<EmailResult>(true, HttpMethod.Post, ["tenants", id, "send-invitation"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /tenants/{id}/suspend</c> with the platform admin key.</summary>
    /// <param name="id">The Tenant id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Tenant status.</returns>
    public Task<TenantStatus> SuspendTenantAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return SendAsync<TenantStatus>(true, HttpMethod.Post, ["tenants", id, "suspend"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /tenants/{id}/resume</c> with the platform admin key.</summary>
    /// <param name="id">The Tenant id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Tenant status.</returns>
    public Task<TenantStatus> ResumeTenantAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return SendAsync<TenantStatus>(true, HttpMethod.Post, ["tenants", id, "resume"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /tenants/{id}</c> with the platform admin key.</summary>
    /// <param name="id">The Tenant id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteTenantAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return SendAsync<DeletedResult>(true, HttpMethod.Delete, ["tenants", id], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /tenant</c>.</summary>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The caller's Tenant page sizes.</returns>
    public Task<TenantSettings> GetTenantAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return SendAsync<TenantSettings>(false, HttpMethod.Get, ["tenant"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PATCH /tenant</c>. A null page size is omitted, which leaves the stored value.</summary>
    /// <param name="pageSizeDefault">The default page size. Null omits it.</param>
    /// <param name="pageSizeMax">The maximum page size. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The page sizes after the change.</returns>
    public Task<TenantSettings> UpdateTenantAsync(int? pageSizeDefault = null, int? pageSizeMax = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var body = JsonWire.Object(writer =>
        {
            writer.NumberIfSet("pageSizeDefault", pageSizeDefault);
            writer.NumberIfSet("pageSizeMax", pageSizeMax);
        });
        return SendAsync<TenantSettings>(false, HttpMethod.Patch, ["tenant"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /members</c>.</summary>
    /// <param name="email">The email.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Member, including the plaintext token once.</returns>
    public Task<Member> AddMemberAsync(string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        var body = JsonWire.Object(writer => writer.String("email", email));
        return SendAsync<Member>(false, HttpMethod.Post, ["members"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /members/{email}/send-invitation</c>.</summary>
    /// <param name="email">The Member email. It is sent as one path segment.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Whether the email was sent. <c>false</c> does not throw.</returns>
    public Task<EmailResult> SendMemberInvitationAsync(string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return SendAsync<EmailResult>(false, HttpMethod.Post, ["members", email, "send-invitation"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /members/{email}</c>.</summary>
    /// <param name="email">The Member email. It is sent as one path segment.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteMemberAsync(string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return SendAsync<DeletedResult>(false, HttpMethod.Delete, ["members", email], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PUT /members/{email}/tenant-owner</c>.</summary>
    /// <param name="email">The Member email. It is sent as one path segment.</param>
    /// <param name="isTenantOwner">Whether the Member is a TenantOwner.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Member, without a token.</returns>
    public Task<Member> SetTenantOwnerAsync(string email, bool isTenantOwner, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        var body = JsonWire.Object(writer => writer.Boolean("isTenantOwner", isTenantOwner));
        return SendAsync<Member>(false, HttpMethod.Put, ["members", email, "tenant-owner"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /members/{email}/token/regenerate</c>.</summary>
    /// <param name="email">The Member email. It is sent as one path segment.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new token and when it was issued.</returns>
    public Task<TokenRegenerated> RegenerateTokenAsync(string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return SendAsync<TokenRegenerated>(false, HttpMethod.Post, ["members", email, "token", "regenerate"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /members/{email}/token/send</c>.</summary>
    /// <param name="email">The Member email. It is sent as one path segment.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Whether the email was sent. <c>false</c> does not throw.</returns>
    public Task<EmailResult> SendTokenAsync(string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return SendAsync<EmailResult>(false, HttpMethod.Post, ["members", email, "token", "send"], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /members</c>. <paramref name="where"/> and <paramref name="orderBy"/> go in the <c>query</c> parameter.</summary>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of Members. The library does not follow the continuation token.</returns>
    public Task<MemberPage> ListMembersAsync(
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var query = JsonWire.PageQuery(pageSize, continuation, JsonWire.Query(null, where, orderBy));
        return SendAsync<MemberPage>(false, HttpMethod.Get, ["members"], null, query, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /workspaces</c>.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description. Null omits it.</param>
    /// <param name="members">The initial role assignments. Null omits them.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Workspace and its initial assignments.</returns>
    public Task<WorkspaceCreated> CreateWorkspaceAsync(
        string name,
        string? description = null,
        IReadOnlyList<WorkspaceMemberInput>? members = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var body = JsonWire.Object(writer =>
        {
            writer.String("name", name);
            writer.StringIfSet("description", description);
            writer.Members(members);
        });
        return SendAsync<WorkspaceCreated>(false, HttpMethod.Post, ["workspaces"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /workspaces/{id}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteWorkspaceAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return SendAsync<DeletedResult>(false, HttpMethod.Delete, ["workspaces", id], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /workspaces</c>. <paramref name="where"/> and <paramref name="orderBy"/> go in the <c>query</c> parameter.</summary>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of Workspaces, with no row numbers.</returns>
    public Task<WorkspacePage> ListWorkspacesAsync(
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var query = JsonWire.PageQuery(pageSize, continuation, JsonWire.Query(null, where, orderBy));
        return SendAsync<WorkspacePage>(false, HttpMethod.Get, ["workspaces"], null, query, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PATCH /workspaces/{id}</c>. A description that is not set is omitted. A null description clears it.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="name">The name, when the caller sets it.</param>
    /// <param name="description">The description, when the caller sets it. Null clears it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The updated Workspace.</returns>
    public Task<Workspace> UpdateWorkspaceAsync(
        string id,
        Optional<string> name = default,
        Optional<string> description = default,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        var body = JsonWire.Object(writer =>
        {
            writer.Optional("name", name);
            writer.Optional("description", description);
        });
        return SendAsync<Workspace>(false, HttpMethod.Patch, ["workspaces", id], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /workspaces/{id}/members</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of Workspace members.</returns>
    public Task<WorkspaceMemberPage> ListWorkspaceMembersAsync(
        string id,
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        var query = JsonWire.PageQuery(pageSize, continuation, JsonWire.Query(null, where, orderBy));
        return SendAsync<WorkspaceMemberPage>(false, HttpMethod.Get, ["workspaces", id, "members"], null, query, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /workspaces/{id}/members</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="email">The email.</param>
    /// <param name="role">The Workspace role.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The assignment, including whether the email was sent.</returns>
    public Task<RoleAssignment> AddWorkspaceMemberAsync(string id, string email, WorkspaceRole role, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(email);
        var body = JsonWire.Object(writer =>
        {
            writer.String("email", email);
            writer.String("role", role.ToString());
        });
        return SendAsync<RoleAssignment>(false, HttpMethod.Post, ["workspaces", id, "members"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /workspaces/{id}/members/{email}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="email">The email. It is sent as one path segment.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> RemoveWorkspaceMemberAsync(string id, string email, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(email);
        return SendAsync<DeletedResult>(false, HttpMethod.Delete, ["workspaces", id, "members", email], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PATCH /workspaces/{id}/members/{email}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="email">The email. It is sent as one path segment.</param>
    /// <param name="role">The Workspace role.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The email and role.</returns>
    public Task<WorkspaceMember> ChangeWorkspaceRoleAsync(string id, string email, WorkspaceRole role, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(email);
        var body = JsonWire.Object(writer => writer.String("role", role.ToString()));
        return SendAsync<WorkspaceMember>(false, HttpMethod.Patch, ["workspaces", id, "members", email], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /workspaces/{id}/lists</c>. The schema is nested as raw JSON.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="name">The List name.</param>
    /// <param name="schema">The EasyJsonSchema document.</param>
    /// <param name="description">The description. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new List.</returns>
    public Task<ListRecord> CreateListAsync(
        string id,
        string name,
        System.Text.Json.JsonElement schema,
        string? description = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        var schemaJson = JsonWire.Raw(schema);
        var body = JsonWire.Object(writer =>
        {
            writer.String("name", name);
            writer.StringIfSet("description", description);
            writer.RawIfSet("schema", schemaJson);
        });
        return SendAsync<ListRecord>(false, HttpMethod.Post, ["workspaces", id, "lists"], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PATCH /workspaces/{id}/lists/{listId}</c>. A description that is not set is omitted. A null description clears it.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="name">The name, when the caller sets it.</param>
    /// <param name="description">The description, when the caller sets it. Null clears it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The updated List.</returns>
    public Task<ListRecord> UpdateListAsync(
        string id,
        string listId,
        Optional<string> name = default,
        Optional<string> description = default,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        var body = JsonWire.Object(writer =>
        {
            writer.Optional("name", name);
            writer.Optional("description", description);
        });
        return SendAsync<ListRecord>(false, HttpMethod.Patch, ["workspaces", id, "lists", listId], body, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /workspaces/{id}/lists/{listId}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteListAsync(string id, string listId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        return SendAsync<DeletedResult>(false, HttpMethod.Delete, ["workspaces", id, "lists", listId], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /workspaces/{id}/lists</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of Lists.</returns>
    public Task<ListPage> ListListsAsync(
        string id,
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        var query = JsonWire.PageQuery(pageSize, continuation, JsonWire.Query(null, where, orderBy));
        return SendAsync<ListPage>(false, HttpMethod.Get, ["workspaces", id, "lists"], null, query, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PUT /workspaces/{id}/lists/{listId}/schema</c>. The body is the schema JSON itself.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="schema">The EasyJsonSchema document.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The schema and its new version.</returns>
    public Task<SchemaResult> UpdateSchemaAsync(string id, string listId, System.Text.Json.JsonElement schema, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        return SendAsync<SchemaResult>(false, HttpMethod.Put, ["workspaces", id, "lists", listId, "schema"], JsonWire.Raw(schema), null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /workspaces/{id}/lists/{listId}/schema</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The schema and version.</returns>
    public Task<SchemaResult> GetSchemaAsync(string id, string listId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        return SendAsync<SchemaResult>(false, HttpMethod.Get, ["workspaces", id, "lists", listId, "schema"], null, null, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls <c>GET /workspaces/{id}/lists/{listId}/items</c>.
    /// <paramref name="schemaVersion"/>, <paramref name="where"/>, and <paramref name="orderBy"/> are sent together in the <c>query</c> parameter.
    /// <paramref name="shape"/>, <paramref name="pageSize"/>, and <paramref name="continuation"/> are plain query parameters.
    /// </summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="shape"><c>full</c> or <c>ids</c>. Any other string is sent and judged by the server.</param>
    /// <param name="schemaVersion">The version filter. It is never sent as its own query parameter.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One page of Items. The library does not follow the continuation token.</returns>
    public Task<ItemPage> QueryItemsAsync(
        string id,
        string listId,
        string shape,
        SchemaVersionFilter? schemaVersion = null,
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(shape);
        var query = new List<KeyValuePair<string, string>>
        {
            new("shape", shape),
        };
        query.AddRange(JsonWire.PageQuery(pageSize, continuation, JsonWire.Query(schemaVersion, where, orderBy)));
        return SendAsync<ItemPage>(false, HttpMethod.Get, ["workspaces", id, "lists", listId, "items"], null, query, timeout, cancellationToken);
    }

    /// <summary>Calls <c>GET /workspaces/{id}/lists/{listId}/items/{itemId}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="itemId">The Item id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Item.</returns>
    public Task<Item> GetItemAsync(string id, string listId, string itemId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(itemId);
        return SendAsync<Item>(false, HttpMethod.Get, ["workspaces", id, "lists", listId, "items", itemId], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>PUT /workspaces/{id}/lists/{listId}/items/{itemId}</c>. The body is the Item JSON itself.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="itemId">The Item id.</param>
    /// <param name="data">The Item JSON.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Item id and schema version.</returns>
    public Task<ItemUpserted> UpsertItemAsync(string id, string listId, string itemId, System.Text.Json.JsonElement data, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(itemId);
        return SendAsync<ItemUpserted>(false, HttpMethod.Put, ["workspaces", id, "lists", listId, "items", itemId], JsonWire.Raw(data), null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>DELETE /workspaces/{id}/lists/{listId}/items/{itemId}</c>.</summary>
    /// <param name="id">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="itemId">The Item id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteItemAsync(string id, string listId, string itemId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(itemId);
        return SendAsync<DeletedResult>(false, HttpMethod.Delete, ["workspaces", id, "lists", listId, "items", itemId], null, null, timeout, cancellationToken);
    }

    /// <summary>Calls <c>POST /batch</c>. HTTP 200 with per-operation errors is returned, not thrown.</summary>
    /// <param name="operations">The operations, in order.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One result per operation.</returns>
    public Task<BatchResult> BatchAsync(IReadOnlyList<BatchOperation> operations, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        var body = JsonWire.Object(writer => writer.Raw("operations", JsonWire.Operations(operations)));
        return SendAsync<BatchResult>(false, HttpMethod.Post, ["batch"], body, null, timeout, cancellationToken);
    }

    #region private

    private readonly AgentConnectClient _client;

    private Task<T> SendAsync<T>(
        bool admin,
        HttpMethod method,
        string[] segments,
        string? body,
        IReadOnlyList<KeyValuePair<string, string>>? query,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var snapshot = _client.Snapshot();
        return _client.Transport.RestAsync<T>(snapshot, admin, method, segments, body, query, timeout, cancellationToken);
    }

    #endregion
}
