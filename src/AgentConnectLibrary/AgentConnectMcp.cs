namespace AgentConnectLibrary;

/// <summary>
/// The 19 AgentConnect v9 MCP tools. Each method calls only the MCP server.
/// The server enforces roles, validation, and paging. This surface does not.
/// </summary>
public sealed class AgentConnectMcp
{
    internal AgentConnectMcp(AgentConnectClient client)
    {
        _client = client;
    }

    /// <summary>Calls <c>list_workspaces</c>.</summary>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call. This throws <see cref="OperationCanceledException"/>, not <see cref="AgentConnectException"/>.</param>
    /// <returns>The page, with a row on each Workspace.</returns>
    public Task<McpWorkspacePage> ListWorkspacesAsync(
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = new Dictionary<string, object?>();
        JsonWire.AddFilter(arguments, where, orderBy);
        JsonWire.AddPage(arguments, pageSize, continuation);
        return CallAsync("list_workspaces", arguments, JsonWire.ReadMcpWorkspaces, timeout, cancellationToken);
    }

    /// <summary>Calls <c>update_workspace</c>. A description that is not set is omitted. A null description clears it.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="name">The name, when the caller sets it.</param>
    /// <param name="description">The description, when the caller sets it. Null clears it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The updated Workspace.</returns>
    public Task<Workspace> UpdateWorkspaceAsync(
        string workspaceId,
        Optional<string> name = default,
        Optional<string> description = default,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
        };
        JsonWire.AddOptional(arguments, "name", name);
        JsonWire.AddOptional(arguments, "description", description);
        return CallAsync("update_workspace", arguments, JsonWire.Read<Workspace>, timeout, cancellationToken);
    }

    /// <summary>Calls <c>list_lists</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The page, with a row on each List.</returns>
    public Task<McpListPage> ListListsAsync(
        string workspaceId,
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
        };
        JsonWire.AddFilter(arguments, where, orderBy);
        JsonWire.AddPage(arguments, pageSize, continuation);
        return CallAsync("list_lists", arguments, JsonWire.ReadMcpLists, timeout, cancellationToken);
    }

    /// <summary>Calls <c>create_list</c>. The schema is sent as raw JSON.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="name">The List name.</param>
    /// <param name="schema">The EasyJsonSchema document.</param>
    /// <param name="description">The description. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new List.</returns>
    public Task<ListRecord> CreateListAsync(
        string workspaceId,
        string name,
        System.Text.Json.JsonElement schema,
        string? description = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(name);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["name"] = name,
        };
        JsonWire.AddRaw(arguments, "schema", JsonWire.Raw(schema));
        if (description is not null)
        {
            arguments["description"] = description;
        }

        return CallAsync("create_list", arguments, JsonWire.Read<ListRecord>, timeout, cancellationToken);
    }

    /// <summary>Calls <c>update_list</c>. A description that is not set is omitted. A null description clears it.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="name">The name, when the caller sets it.</param>
    /// <param name="description">The description, when the caller sets it. Null clears it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The updated List.</returns>
    public Task<ListRecord> UpdateListAsync(
        string workspaceId,
        string listId,
        Optional<string> name = default,
        Optional<string> description = default,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["listId"] = listId,
        };
        JsonWire.AddOptional(arguments, "name", name);
        JsonWire.AddOptional(arguments, "description", description);
        return CallAsync("update_list", arguments, JsonWire.Read<ListRecord>, timeout, cancellationToken);
    }

    /// <summary>Calls <c>delete_list</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteListAsync(
        string workspaceId,
        string listId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        return CallAsync(
            "delete_list",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["listId"] = listId,
            },
            JsonWire.Read<DeletedResult>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>get_schema</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The schema document and version.</returns>
    public Task<SchemaResult> GetSchemaAsync(
        string workspaceId,
        string listId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        return CallAsync(
            "get_schema",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["listId"] = listId,
            },
            JsonWire.Read<SchemaResult>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>update_schema</c>. The schema is sent as raw JSON.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="schema">The new EasyJsonSchema document.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The schema and its new version.</returns>
    public Task<SchemaResult> UpdateSchemaAsync(
        string workspaceId,
        string listId,
        System.Text.Json.JsonElement schema,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["listId"] = listId,
        };
        JsonWire.AddRaw(arguments, "schema", JsonWire.Raw(schema));
        return CallAsync("update_schema", arguments, JsonWire.Read<SchemaResult>, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls <c>query_items</c>. <paramref name="schemaVersion"/> is a top-level argument on MCP.
    /// </summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="shape"><c>full</c> or <c>ids</c>. Any other string is sent and judged by the server.</param>
    /// <param name="schemaVersion">The version filter. Null omits it.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The page, with a row on each Item.</returns>
    public Task<McpItemPage> QueryItemsAsync(
        string workspaceId,
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
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(shape);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["listId"] = listId,
            ["shape"] = shape,
        };
        if (schemaVersion is not null)
        {
            arguments["schemaVersion"] = JsonValues.Parse(schemaVersion.Json);
        }

        JsonWire.AddFilter(arguments, where, orderBy);
        JsonWire.AddPage(arguments, pageSize, continuation);
        return CallAsync("query_items", arguments, JsonWire.ReadMcpItems, timeout, cancellationToken);
    }

    /// <summary>Calls <c>get_item</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="itemId">The Item id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Item.</returns>
    public Task<Item> GetItemAsync(
        string workspaceId,
        string listId,
        string itemId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(itemId);
        return CallAsync(
            "get_item",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["listId"] = listId,
                ["itemId"] = itemId,
            },
            JsonWire.Read<Item>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>upsert_item</c>. Item <paramref name="data"/> is sent as raw JSON. A null <paramref name="itemId"/> is omitted and the server may generate an id.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="data">The Item JSON.</param>
    /// <param name="itemId">The Item id. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Item id and schema version.</returns>
    public Task<ItemUpserted> UpsertItemAsync(
        string workspaceId,
        string listId,
        System.Text.Json.JsonElement data,
        string? itemId = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["listId"] = listId,
        };
        if (itemId is not null)
        {
            arguments["itemId"] = itemId;
        }

        JsonWire.AddRaw(arguments, "data", JsonWire.Raw(data));
        return CallAsync("upsert_item", arguments, JsonWire.Read<ItemUpserted>, timeout, cancellationToken);
    }

    /// <summary>Calls <c>delete_item</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="listId">The List id.</param>
    /// <param name="itemId">The Item id.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> DeleteItemAsync(
        string workspaceId,
        string listId,
        string itemId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(listId);
        ArgumentNullException.ThrowIfNull(itemId);
        return CallAsync(
            "delete_item",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["listId"] = listId,
                ["itemId"] = itemId,
            },
            JsonWire.Read<DeletedResult>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>invite_member</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="email">The email.</param>
    /// <param name="role">The Workspace role.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The assignment, including whether the email was sent.</returns>
    public Task<RoleAssignment> InviteMemberAsync(
        string workspaceId,
        string email,
        WorkspaceRole role,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(email);
        return CallAsync(
            "invite_member",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["email"] = email,
                ["role"] = role.ToString(),
            },
            JsonWire.Read<RoleAssignment>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>change_workspace_role</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="email">The email.</param>
    /// <param name="role">The Workspace role.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The email and role.</returns>
    public Task<WorkspaceMember> ChangeWorkspaceRoleAsync(
        string workspaceId,
        string email,
        WorkspaceRole role,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(email);
        return CallAsync(
            "change_workspace_role",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["email"] = email,
                ["role"] = role.ToString(),
            },
            JsonWire.Read<WorkspaceMember>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>revoke_workspace_access</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="email">The email.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The confirmation.</returns>
    public Task<DeletedResult> RevokeWorkspaceAccessAsync(
        string workspaceId,
        string email,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(email);
        return CallAsync(
            "revoke_workspace_access",
            new Dictionary<string, object?>
            {
                ["workspaceId"] = workspaceId,
                ["email"] = email,
            },
            JsonWire.Read<DeletedResult>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>list_workspace_members</c>.</summary>
    /// <param name="workspaceId">The Workspace id.</param>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The page, with a row on each member.</returns>
    public Task<McpWorkspaceMemberPage> ListWorkspaceMembersAsync(
        string workspaceId,
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        var arguments = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
        };
        JsonWire.AddFilter(arguments, where, orderBy);
        JsonWire.AddPage(arguments, pageSize, continuation);
        return CallAsync("list_workspace_members", arguments, JsonWire.ReadMcpWorkspaceMembers, timeout, cancellationToken);
    }

    /// <summary>Calls <c>set_tenant_owner</c>.</summary>
    /// <param name="email">The Member email.</param>
    /// <param name="isTenantOwner">Whether the Member is a TenantOwner.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Member, without a token.</returns>
    public Task<Member> SetTenantOwnerAsync(
        string email,
        bool isTenantOwner,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return CallAsync(
            "set_tenant_owner",
            new Dictionary<string, object?>
            {
                ["email"] = email,
                ["isTenantOwner"] = isTenantOwner,
            },
            JsonWire.Read<Member>,
            timeout,
            cancellationToken);
    }

    /// <summary>Calls <c>query_members</c>.</summary>
    /// <param name="where">The filter. Null omits it.</param>
    /// <param name="orderBy">The sort. Null omits it.</param>
    /// <param name="pageSize">The page size. Null omits it.</param>
    /// <param name="continuation">The continuation token. Null omits it.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The page, with a row on each Member.</returns>
    public Task<McpMemberPage> QueryMembersAsync(
        IReadOnlyList<FilterCondition>? where = null,
        OrderBy? orderBy = null,
        int? pageSize = null,
        string? continuation = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = new Dictionary<string, object?>();
        JsonWire.AddFilter(arguments, where, orderBy);
        JsonWire.AddPage(arguments, pageSize, continuation);
        return CallAsync("query_members", arguments, JsonWire.ReadMcpMembers, timeout, cancellationToken);
    }

    /// <summary>Calls <c>batch</c>. An HTTP success with per-operation errors is returned, not thrown.</summary>
    /// <param name="operations">The operations, in order.</param>
    /// <param name="timeout">The timeout for this call. Null uses the client timeout.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One result per operation.</returns>
    public Task<BatchResult> BatchAsync(
        IReadOnlyList<BatchOperation> operations,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        var arguments = new Dictionary<string, object?>
        {
            ["operations"] = JsonValues.Parse(JsonWire.Operations(operations)),
        };
        return CallAsync("batch", arguments, JsonWire.Read<BatchResult>, timeout, cancellationToken);
    }

    #region private

    private readonly AgentConnectClient _client;

    private Task<T> CallAsync<T>(
        string tool,
        IReadOnlyDictionary<string, object?> arguments,
        Func<string, T> read,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var snapshot = _client.Snapshot();
        return InvokeAsync(snapshot, tool, arguments, read, timeout, cancellationToken);
    }

    private async Task<T> InvokeAsync<T>(
        ClientSnapshot snapshot,
        string tool,
        IReadOnlyDictionary<string, object?> arguments,
        Func<string, T> read,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var json = await _client.Transport.ToolAsync(snapshot, tool, arguments, timeout, cancellationToken).ConfigureAwait(false);
        return read(json);
    }

    #endregion
}
