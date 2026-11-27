using System.Reflection;

namespace AgentConnectLibrary.Tests;

public sealed class CatalogTests
{
    private static readonly string[] McpMethods =
    [
        "BatchAsync",
        "ChangeWorkspaceRoleAsync",
        "CreateListAsync",
        "DeleteItemAsync",
        "DeleteListAsync",
        "GetItemAsync",
        "GetSchemaAsync",
        "InviteMemberAsync",
        "ListListsAsync",
        "ListWorkspaceMembersAsync",
        "ListWorkspacesAsync",
        "QueryItemsAsync",
        "QueryMembersAsync",
        "RevokeWorkspaceAccessAsync",
        "SetTenantOwnerAsync",
        "UpdateListAsync",
        "UpdateSchemaAsync",
        "UpdateWorkspaceAsync",
        "UpsertItemAsync",
    ];

    private static readonly string[] RestMethods =
    [
        "AddMemberAsync",
        "AddWorkspaceMemberAsync",
        "BatchAsync",
        "ChangeWorkspaceRoleAsync",
        "CreateListAsync",
        "CreateTenantAsync",
        "CreateWorkspaceAsync",
        "DeleteItemAsync",
        "DeleteListAsync",
        "DeleteMemberAsync",
        "DeleteTenantAsync",
        "DeleteWorkspaceAsync",
        "GetItemAsync",
        "GetSchemaAsync",
        "GetTenantAsync",
        "ListListsAsync",
        "ListMembersAsync",
        "ListWorkspaceMembersAsync",
        "ListWorkspacesAsync",
        "QueryItemsAsync",
        "RegenerateTokenAsync",
        "RemoveWorkspaceMemberAsync",
        "ResumeTenantAsync",
        "SendMemberInvitationAsync",
        "SendTenantInvitationAsync",
        "SendTokenAsync",
        "SetTenantOwnerAsync",
        "SuspendTenantAsync",
        "UpdateListAsync",
        "UpdateSchemaAsync",
        "UpdateTenantAsync",
        "UpdateWorkspaceAsync",
        "UpsertItemAsync",
    ];

    [Fact]
    [Trait("Spec", "T-3")]
    public async Task T_3_McpSurface_HasOneMethodPerTool()
    {
        Assert.Equal(McpMethods, AsyncMethodNames(typeof(AgentConnectMcp)));
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Mcp(request.Body)),
        };
        using var client = Clients.Create(handler);
        var schema = JsonValues.Parse("""{"type":"object"}""");
        var data = JsonValues.Parse("""{"name":"Ada"}""");
        await client.Mcp.ListWorkspacesAsync();
        await client.Mcp.UpdateWorkspaceAsync("w1", name: "Ops");
        await client.Mcp.ListListsAsync("w1");
        await client.Mcp.CreateListAsync("w1", "Tasks", schema);
        await client.Mcp.UpdateListAsync("w1", "l1", name: "Tasks");
        await client.Mcp.DeleteListAsync("w1", "l1");
        await client.Mcp.GetSchemaAsync("w1", "l1");
        await client.Mcp.UpdateSchemaAsync("w1", "l1", schema);
        await client.Mcp.QueryItemsAsync("w1", "l1", "full");
        await client.Mcp.GetItemAsync("w1", "l1", "i1");
        await client.Mcp.UpsertItemAsync("w1", "l1", data, "i1");
        await client.Mcp.DeleteItemAsync("w1", "l1", "i1");
        await client.Mcp.InviteMemberAsync("w1", "ann@example.com", WorkspaceRole.WorkspaceWrite);
        await client.Mcp.ChangeWorkspaceRoleAsync("w1", "ann@example.com", WorkspaceRole.WorkspaceRead);
        await client.Mcp.RevokeWorkspaceAccessAsync("w1", "ann@example.com");
        await client.Mcp.ListWorkspaceMembersAsync("w1");
        await client.Mcp.SetTenantOwnerAsync("ann@example.com", true);
        await client.Mcp.QueryMembersAsync();
        await client.Mcp.BatchAsync([new BatchOperation { Op = "get", Type = "workspace", WorkspaceId = "w1" }]);

        Assert.Equal(
            [
                "list_workspaces",
                "update_workspace",
                "list_lists",
                "create_list",
                "update_list",
                "delete_list",
                "get_schema",
                "update_schema",
                "query_items",
                "get_item",
                "upsert_item",
                "delete_item",
                "invite_member",
                "change_workspace_role",
                "revoke_workspace_access",
                "list_workspace_members",
                "set_tenant_owner",
                "query_members",
                "batch",
            ],
            McpStub.ToolNames(handler.Requests));
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Bearer member-token", request.Authorization);
            Assert.Null(request.AdminKey);
        });
    }

    [Fact]
    [Trait("Spec", "T-4")]
    public async Task T_4_RestSurface_HasOneMethodPerEndpoint()
    {
        Assert.Equal(RestMethods, AsyncMethodNames(typeof(AgentConnectRest)));
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler);
        var schema = JsonValues.Parse("""{"type":"object"}""");
        var data = JsonValues.Parse("""{"name":"Ada"}""");
        await client.Rest.CreateTenantAsync("Acme", "ann@example.com");
        await client.Rest.SendTenantInvitationAsync("t1");
        await client.Rest.SuspendTenantAsync("t1");
        await client.Rest.ResumeTenantAsync("t1");
        await client.Rest.DeleteTenantAsync("t1");
        await client.Rest.GetTenantAsync();
        await client.Rest.UpdateTenantAsync(pageSizeDefault: 50);
        await client.Rest.AddMemberAsync("ann@example.com");
        await client.Rest.SendMemberInvitationAsync("ann@example.com");
        await client.Rest.DeleteMemberAsync("ann@example.com");
        await client.Rest.SetTenantOwnerAsync("ann@example.com", true);
        await client.Rest.RegenerateTokenAsync("ann@example.com");
        await client.Rest.SendTokenAsync("ann@example.com");
        await client.Rest.ListMembersAsync();
        await client.Rest.CreateWorkspaceAsync("Ops", "Desk", [new WorkspaceMemberInput { Email = "ann@example.com", Role = WorkspaceRole.WorkspaceWrite }]);
        await client.Rest.DeleteWorkspaceAsync("w1");
        await client.Rest.ListWorkspacesAsync();
        await client.Rest.UpdateWorkspaceAsync("w1", name: "Ops");
        await client.Rest.ListWorkspaceMembersAsync("w1");
        await client.Rest.AddWorkspaceMemberAsync("w1", "ann@example.com", WorkspaceRole.WorkspaceWrite);
        await client.Rest.RemoveWorkspaceMemberAsync("w1", "ann@example.com");
        await client.Rest.ChangeWorkspaceRoleAsync("w1", "ann@example.com", WorkspaceRole.WorkspaceRead);
        await client.Rest.CreateListAsync("w1", "Tasks", schema, "Work");
        await client.Rest.UpdateListAsync("w1", "l1", name: "Tasks");
        await client.Rest.DeleteListAsync("w1", "l1");
        await client.Rest.ListListsAsync("w1");
        await client.Rest.UpdateSchemaAsync("w1", "l1", schema);
        await client.Rest.GetSchemaAsync("w1", "l1");
        await client.Rest.QueryItemsAsync("w1", "l1", "full");
        await client.Rest.GetItemAsync("w1", "l1", "i1");
        await client.Rest.UpsertItemAsync("w1", "l1", "i1", data);
        await client.Rest.DeleteItemAsync("w1", "l1", "i1");
        await client.Rest.BatchAsync([new BatchOperation { Op = "get", Type = "item", WorkspaceId = "w1", ListId = "l1", ItemId = "i1" }]);

        var actual = handler.Requests.Select(request => request.Method.Method + " " + Clients.EscapedPath(request.Uri)).ToArray();
        Assert.Equal(
            [
                "POST tenants",
                "POST tenants/t1/send-invitation",
                "POST tenants/t1/suspend",
                "POST tenants/t1/resume",
                "DELETE tenants/t1",
                "GET tenant",
                "PATCH tenant",
                "POST members",
                "POST members/ann%40example.com/send-invitation",
                "DELETE members/ann%40example.com",
                "PUT members/ann%40example.com/tenant-owner",
                "POST members/ann%40example.com/token/regenerate",
                "POST members/ann%40example.com/token/send",
                "GET members",
                "POST workspaces",
                "DELETE workspaces/w1",
                "GET workspaces",
                "PATCH workspaces/w1",
                "GET workspaces/w1/members",
                "POST workspaces/w1/members",
                "DELETE workspaces/w1/members/ann%40example.com",
                "PATCH workspaces/w1/members/ann%40example.com",
                "POST workspaces/w1/lists",
                "PATCH workspaces/w1/lists/l1",
                "DELETE workspaces/w1/lists/l1",
                "GET workspaces/w1/lists",
                "PUT workspaces/w1/lists/l1/schema",
                "GET workspaces/w1/lists/l1/schema",
                "GET workspaces/w1/lists/l1/items",
                "GET workspaces/w1/lists/l1/items/i1",
                "PUT workspaces/w1/lists/l1/items/i1",
                "DELETE workspaces/w1/lists/l1/items/i1",
                "POST batch",
            ],
            actual);

        foreach (var request in handler.Requests.Take(5))
        {
            Assert.Equal("admin-key", request.AdminKey);
            Assert.Null(request.Authorization);
        }

        foreach (var request in handler.Requests.Skip(5))
        {
            Assert.Equal("Bearer member-token", request.Authorization);
            Assert.Null(request.AdminKey);
        }
    }

    #region private

    private static string[] AsyncMethodNames(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => typeof(Task).IsAssignableFrom(method.ReturnType))
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    #endregion
}
