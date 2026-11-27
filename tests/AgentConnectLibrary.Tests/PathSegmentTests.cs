namespace AgentConnectLibrary.Tests;

/// <summary>
/// A path parameter of ".", "..", or empty must not be sent. System.Uri would otherwise
/// turn the call into a request on the parent resource.
/// </summary>
public sealed class PathSegmentTests
{
    private static readonly string[] BadSegments = [".", "..", ""];

    [Fact]
    public async Task EveryRestPathParameter_DotDotEmpty_ThrowsAndSendsNothing()
    {
        var schema = JsonValues.Parse("""{"type":"object"}""");
        var data = JsonValues.Parse("""{"name":"Ada"}""");
        foreach (var bad in BadSegments)
        {
            await RejectAsync("SendTenantInvitationAsync", client => client.Rest.SendTenantInvitationAsync(bad));
            await RejectAsync("SuspendTenantAsync", client => client.Rest.SuspendTenantAsync(bad));
            await RejectAsync("ResumeTenantAsync", client => client.Rest.ResumeTenantAsync(bad));
            await RejectAsync("DeleteTenantAsync", client => client.Rest.DeleteTenantAsync(bad));
            await RejectAsync("SendMemberInvitationAsync", client => client.Rest.SendMemberInvitationAsync(bad));
            await RejectAsync("DeleteMemberAsync", client => client.Rest.DeleteMemberAsync(bad));
            await RejectAsync("SetTenantOwnerAsync", client => client.Rest.SetTenantOwnerAsync(bad, true));
            await RejectAsync("RegenerateTokenAsync", client => client.Rest.RegenerateTokenAsync(bad));
            await RejectAsync("SendTokenAsync", client => client.Rest.SendTokenAsync(bad));
            await RejectAsync("DeleteWorkspaceAsync", client => client.Rest.DeleteWorkspaceAsync(bad));
            await RejectAsync("UpdateWorkspaceAsync", client => client.Rest.UpdateWorkspaceAsync(bad, name: "Ops"));
            await RejectAsync("ListWorkspaceMembersAsync", client => client.Rest.ListWorkspaceMembersAsync(bad));
            await RejectAsync("AddWorkspaceMemberAsync", client => client.Rest.AddWorkspaceMemberAsync(bad, "ann@example.com", WorkspaceRole.WorkspaceRead));
            await RejectAsync("RemoveWorkspaceMemberAsync id", client => client.Rest.RemoveWorkspaceMemberAsync(bad, "ann@example.com"));
            await RejectAsync("RemoveWorkspaceMemberAsync email", client => client.Rest.RemoveWorkspaceMemberAsync("w1", bad));
            await RejectAsync("ChangeWorkspaceRoleAsync id", client => client.Rest.ChangeWorkspaceRoleAsync(bad, "ann@example.com", WorkspaceRole.WorkspaceRead));
            await RejectAsync("ChangeWorkspaceRoleAsync email", client => client.Rest.ChangeWorkspaceRoleAsync("w1", bad, WorkspaceRole.WorkspaceRead));
            await RejectAsync("CreateListAsync", client => client.Rest.CreateListAsync(bad, "Tasks", schema));
            await RejectAsync("UpdateListAsync id", client => client.Rest.UpdateListAsync(bad, "l1", name: "Tasks"));
            await RejectAsync("UpdateListAsync listId", client => client.Rest.UpdateListAsync("w1", bad, name: "Tasks"));
            await RejectAsync("DeleteListAsync id", client => client.Rest.DeleteListAsync(bad, "l1"));
            await RejectAsync("DeleteListAsync listId", client => client.Rest.DeleteListAsync("w1", bad));
            await RejectAsync("ListListsAsync", client => client.Rest.ListListsAsync(bad));
            await RejectAsync("UpdateSchemaAsync id", client => client.Rest.UpdateSchemaAsync(bad, "l1", schema));
            await RejectAsync("UpdateSchemaAsync listId", client => client.Rest.UpdateSchemaAsync("w1", bad, schema));
            await RejectAsync("GetSchemaAsync id", client => client.Rest.GetSchemaAsync(bad, "l1"));
            await RejectAsync("GetSchemaAsync listId", client => client.Rest.GetSchemaAsync("w1", bad));
            await RejectAsync("QueryItemsAsync id", client => client.Rest.QueryItemsAsync(bad, "l1", "full"));
            await RejectAsync("QueryItemsAsync listId", client => client.Rest.QueryItemsAsync("w1", bad, "full"));
            await RejectAsync("GetItemAsync id", client => client.Rest.GetItemAsync(bad, "l1", "i1"));
            await RejectAsync("GetItemAsync listId", client => client.Rest.GetItemAsync("w1", bad, "i1"));
            await RejectAsync("GetItemAsync itemId", client => client.Rest.GetItemAsync("w1", "l1", bad));
            await RejectAsync("UpsertItemAsync id", client => client.Rest.UpsertItemAsync(bad, "l1", "i1", data));
            await RejectAsync("UpsertItemAsync listId", client => client.Rest.UpsertItemAsync("w1", bad, "i1", data));
            await RejectAsync("UpsertItemAsync itemId", client => client.Rest.UpsertItemAsync("w1", "l1", bad, data));
            await RejectAsync("DeleteItemAsync id", client => client.Rest.DeleteItemAsync(bad, "l1", "i1"));
            await RejectAsync("DeleteItemAsync listId", client => client.Rest.DeleteItemAsync("w1", bad, "i1"));
            await RejectAsync("DeleteItemAsync itemId", client => client.Rest.DeleteItemAsync("w1", "l1", bad));
        }
    }

    [Fact]
    public async Task DottedItemIdThatIsOneSegment_IsSentUnchanged()
    {
        foreach (var itemId in new[] { "foo.bar", "...", ".hidden", "foo..bar" })
        {
            var handler = new StandInHandler
            {
                OnRequest = (_, _) => Task.FromResult(StandInHandler.Json(Payloads.Deleted)),
            };
            using var client = Clients.Create(handler);
            await client.Rest.DeleteItemAsync("w1", "l1", itemId);
            var request = Assert.Single(handler.Requests);
            Assert.Equal("DELETE", request.Method.Method);
            Assert.Equal("workspaces/w1/lists/l1/items/" + Uri.EscapeDataString(itemId), Clients.EscapedPath(request.Uri));
        }
    }

    #region private

    private static async Task RejectAsync(string name, Func<AgentConnectClient, Task> call)
    {
        var handler = new StandInHandler();
        using var client = Clients.Create(handler, token: "member-token-secret", admin: "admin-key-secret");
        var exception = await Record.ExceptionAsync(() => call(client));
        Assert.True(exception is ArgumentException, name);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal(LibraryMessages.BadPathSegment, exception.Message);
        Assert.True(handler.Requests.Count == 0, name);
        Assert.DoesNotContain("member-token-secret", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("admin-key-secret", exception.ToString(), StringComparison.Ordinal);
    }

    #endregion
}
