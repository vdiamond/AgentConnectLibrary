using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AgentConnectLibrary.Tests;

public sealed class BehaviorTests
{
    [Fact]
    [Trait("Spec", "T-1")]
    public async Task T_1_ListWorkspacesAsync_DeserializesMcpPage()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Mcp(request.Body)),
        };
        using var client = Clients.Create(handler);
        var page = await client.Mcp.ListWorkspacesAsync();
        Assert.Equal("w1", page.Workspaces[0].Entity.Id);
        Assert.Equal("Ops", page.Workspaces[0].Entity.Name);
        Assert.Equal(1, page.Workspaces[0].Row);
        Assert.Equal("next", page.Continuation);
    }

    [Fact]
    [Trait("Spec", "T-2")]
    public async Task T_2_SameInstance_RestListWorkspaces_DeserializesPage()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(
                request.Uri.AbsolutePath.Contains("/mcp", StringComparison.Ordinal)
                    ? Payloads.Mcp(request.Body)
                    : Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler, mcp: "http://stand-in.test/mcp", rest: "http://stand-in.test");
        var mcp = await client.Mcp.ListWorkspacesAsync();
        var rest = await client.Rest.ListWorkspacesAsync();
        Assert.Equal(mcp.Workspaces[0].Entity.Id, rest.Workspaces[0].Id);
        Assert.Equal("next", rest.Continuation);
        Assert.Equal("WorkspaceOwner", rest.Workspaces[0].Role);
    }

    [Fact]
    [Trait("Spec", "T-5")]
    public async Task T_5_McpListWorkspaces_SendsBearerAndNoAdminKey()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Mcp(request.Body)),
        };
        using var client = Clients.Create(handler, token: "member-token", admin: "admin-key");
        await client.Mcp.ListWorkspacesAsync();
        Assert.NotEmpty(handler.Requests);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Bearer member-token", request.Authorization);
            Assert.Null(request.AdminKey);
        });
    }

    [Fact]
    [Trait("Spec", "T-6")]
    public async Task T_6_GetTenant_SendsBearerAndNoAdminKey()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler, token: "member-token", admin: "admin-key");
        var settings = await client.Rest.GetTenantAsync();
        Assert.Equal(100, settings.PageSizeDefault);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bearer member-token", request.Authorization);
        Assert.Null(request.AdminKey);
    }

    [Fact]
    [Trait("Spec", "T-7")]
    public async Task T_7_CreateTenant_SendsAdminKeyAndNoAuthorization()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler, token: "member-token", admin: "admin-key");
        var created = await client.Rest.CreateTenantAsync("Acme", "ann@example.com");
        Assert.Equal("once", created.Token);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("admin-key", request.AdminKey);
        Assert.Null(request.Authorization);
    }

    [Fact]
    [Trait("Spec", "T-9")]
    public async Task T_9_ServerRejectsCall_ExceptionCarriesStatusAndText()
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Error(403, "$: not allowed")),
        };
        using var client = Clients.Create(handler);
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.UpdateWorkspaceAsync("w1", name: "Ops"));
        Assert.Equal(AgentConnectErrorKind.Server, exception.Kind);
        Assert.Equal(403, exception.StatusCode);
        Assert.Equal("$: not allowed", exception.Message);
        Assert.Single(handler.Requests);
    }

    [Fact]
    [Trait("Spec", "T-10")]
    public async Task T_10_MissingMemberToken_ThrowsArgumentException_SendsNothing()
    {
        var handler = new StandInHandler();
        using var client = Clients.Create(handler, token: null, admin: "admin-key-sentinel");
        var mcp = await Assert.ThrowsAsync<ArgumentException>(() => client.Mcp.ListWorkspacesAsync());
        var rest = await Assert.ThrowsAsync<ArgumentException>(() => client.Rest.GetTenantAsync());
        Assert.Empty(handler.Requests);
        Assert.DoesNotContain("admin-key-sentinel", mcp.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("admin-key-sentinel", rest.ToString(), StringComparison.Ordinal);
        Assert.IsType<ArgumentException>(mcp);
    }

    [Fact]
    [Trait("Spec", "T-11")]
    public async Task T_11_MissingAdminKey_ThrowsArgumentException_SendsNothing()
    {
        var handler = new StandInHandler();
        using var client = Clients.Create(handler, token: "member-token-sentinel", admin: null);
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => client.Rest.CreateTenantAsync("Acme", "ann@example.com"));
        Assert.Empty(handler.Requests);
        Assert.DoesNotContain("member-token-sentinel", exception.ToString(), StringComparison.Ordinal);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    [Trait("Spec", "T-12")]
    public async Task T_12_UpdateSchema_BodyIsTheSchemaItself()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler);
        const string Schema = """{"type":"object","properties":{"name":{"type":"string"}}}""";
        await client.Rest.UpdateSchemaAsync("w1", "l1", JsonValues.Parse(Schema));
        var request = Assert.Single(handler.Requests);
        Assert.Equal(Schema, request.Body);
        Assert.DoesNotContain("\"schema\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-13")]
    public async Task T_13_UpsertItem_BodyIsTheItemDataItself()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler);
        const string Data = """{"name":"Ada","count":2}""";
        await client.Rest.UpsertItemAsync("w1", "l1", "i1", JsonValues.Parse(Data));
        var request = Assert.Single(handler.Requests);
        Assert.Equal(Data, request.Body);
        Assert.DoesNotContain("\"data\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-14")]
    public async Task T_14_DeleteMember_EmailIsOneEncodedSegment()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler);
        await client.Rest.DeleteMemberAsync("ann@example.com");
        var request = Assert.Single(handler.Requests);
        Assert.Equal("members/ann%40example.com", Clients.EscapedPath(request.Uri));
    }

    [Fact]
    [Trait("Spec", "T-15")]
    public async Task T_15_BaseUrlTrailingSlash_DoesNotDoubleSlash()
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Json(Payloads.TenantSettings)),
        };
        using var client = Clients.Create(handler, rest: "http://stand-in.test/api/");
        await client.Rest.GetTenantAsync();
        var request = Assert.Single(handler.Requests);
        Assert.Equal("http://stand-in.test/api/tenant", request.Uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
        Assert.DoesNotContain("//tenant", request.Uri.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-16")]
    public async Task T_16_ListPage_ReturnsContinuation_DoesNotRequestAnotherPage()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(StandInHandler.Json(Payloads.WorkspacePage)),
        };
        using var client = Clients.Create(handler);
        var page = await client.Rest.ListWorkspacesAsync();
        Assert.Equal("next", page.Continuation);
        Assert.Equal("w1", Assert.Single(page.Workspaces).Id);
        Assert.Single(handler.Requests);
    }

    [Fact]
    [Trait("Spec", "T-17")]
    public async Task T_17_LogsTracesAndFailure_DoNotContainSecrets()
    {
        const string Token = "member-token-T17-9f3c2a";
        const string Admin = "admin-key-T17-9f3c2a";
        var observed = new StringBuilder();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                observed.Append(activity.DisplayName);
                observed.Append(activity.OperationName);
                foreach (var tag in activity.TagObjects)
                {
                    observed.Append(tag.Key);
                    observed.Append(tag.Value);
                }

                foreach (var pair in activity.Baggage)
                {
                    observed.Append(pair.Key);
                    observed.Append(pair.Value);
                }
            },
        };
        ActivitySource.AddActivityListener(activityListener);
        using var diagnostics = new DiagnosticObserver(observed);
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Mcp(request.Body)),
        };
        using var client = Clients.Create(handler, token: Token, admin: Admin);
        await client.Mcp.ListWorkspacesAsync();
        client.MemberToken = null;
        var failure = await Assert.ThrowsAsync<ArgumentException>(() => client.Mcp.ListWorkspacesAsync());
        observed.Append(failure);
        Assert.DoesNotContain(Token, observed.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Admin, observed.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-18")]
    public async Task T_18_InvalidTokenAndSuspendedTenant_KeepStatusAndText()
    {
        await AssertServerErrorAsync(false, 401, "$: invalid token", client => client.Rest.GetTenantAsync());
        await AssertServerErrorAsync(true, 401, "$: invalid token", client => client.Mcp.ListWorkspacesAsync());
        await AssertServerErrorAsync(true, 403, "$: tenant suspended", client => client.Mcp.ListWorkspacesAsync());
    }

    [Fact]
    [Trait("Spec", "T-19")]
    public async Task T_19_ListWorkspaces_WhereAndOrderByAreTheQueryParameter()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(StandInHandler.Json(Payloads.WorkspacePage)),
        };
        using var client = Clients.Create(handler);
        await client.Rest.ListWorkspacesAsync(
            where:
            [
                new FilterCondition
                {
                    Path = "name",
                    Op = "startsWith",
                    Value = JsonValues.String("Ops"),
                },
            ],
            orderBy: new OrderBy { Path = "created", Desc = true });
        var request = Assert.Single(handler.Requests);
        var query = Clients.Query(request.Uri);
        Assert.True(query.ContainsKey("query"));
        Assert.False(query.ContainsKey("where"));
        Assert.False(query.ContainsKey("orderBy"));
        using var document = JsonDocument.Parse(query["query"]);
        Assert.Equal("name", document.RootElement.GetProperty("where")[0].GetProperty("path").GetString());
        Assert.Equal("startsWith", document.RootElement.GetProperty("where")[0].GetProperty("op").GetString());
        Assert.Equal("Ops", document.RootElement.GetProperty("where")[0].GetProperty("value").GetString());
        Assert.Equal("created", document.RootElement.GetProperty("orderBy").GetProperty("path").GetString());
        Assert.True(document.RootElement.GetProperty("orderBy").GetProperty("desc").GetBoolean());
    }

    [Fact]
    [Trait("Spec", "T-20")]
    public void T_20_TargetsNet10_AndShipsADllNotAPackage()
    {
        var assembly = typeof(AgentConnectClient).Assembly;
        Assert.EndsWith("AgentConnectLibrary.dll", assembly.Location, StringComparison.OrdinalIgnoreCase);
        var framework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
        Assert.NotNull(framework);
        Assert.Contains("Version=v10.0", framework.FrameworkName, StringComparison.Ordinal);
        var root = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(root, "src", "AgentConnectLibrary", "AgentConnectLibrary.csproj"));
        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", project, StringComparison.Ordinal);
        Assert.Contains("<IsPackable>false</IsPackable>", project, StringComparison.Ordinal);
        Assert.Contains("<GeneratePackageOnBuild>false</GeneratePackageOnBuild>", project, StringComparison.Ordinal);
        Assert.Empty(Directory.GetFiles(root, "*.nupkg", SearchOption.AllDirectories));
    }

    [Fact]
    [Trait("Spec", "T-21")]
    public async Task T_21_UpdatedToken_IsUsedByTheNextCall()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(Payloads.Rest(request)),
        };
        using var client = Clients.Create(handler, token: "first-token");
        await client.Rest.GetTenantAsync();
        client.MemberToken = "second-token";
        await client.Rest.GetTenantAsync();
        Assert.Equal("Bearer first-token", handler.Requests[0].Authorization);
        Assert.Equal("Bearer second-token", handler.Requests[1].Authorization);
    }

    [Fact]
    [Trait("Spec", "T-22")]
    public async Task T_22_InstanceTimeout_FailsAtThatTimeout()
    {
        var handler = new StandInHandler
        {
            OnRequest = async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return StandInHandler.Json("{}");
            },
        };
        using var client = Clients.Create(handler);
        client.Timeout = TimeSpan.FromMilliseconds(200);
        var watch = Stopwatch.StartNew();
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.GetTenantAsync());
        watch.Stop();
        Assert.Equal(AgentConnectErrorKind.Timeout, exception.Kind);
        Assert.Equal(LibraryMessages.Timeout, exception.Message);
        Assert.Null(exception.StatusCode);
        Assert.InRange(watch.ElapsedMilliseconds, 100, 5000);
    }

    [Fact]
    [Trait("Spec", "T-23")]
    public async Task T_23_PerCallTimeout_FailsAtThatTimeout()
    {
        var handler = new StandInHandler
        {
            OnRequest = async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return StandInHandler.Json("{}");
            },
        };
        using var client = Clients.Create(handler);
        Assert.Equal(AgentConnectClient.DefaultTimeout, client.Timeout);
        var watch = Stopwatch.StartNew();
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.GetTenantAsync(timeout: TimeSpan.FromMilliseconds(200)));
        watch.Stop();
        Assert.Equal(AgentConnectErrorKind.Timeout, exception.Kind);
        Assert.Equal(LibraryMessages.Timeout, exception.Message);
        Assert.InRange(watch.ElapsedMilliseconds, 100, 5000);
        Assert.Equal(AgentConnectClient.DefaultTimeout, client.Timeout);
    }

    [Fact]
    [Trait("Spec", "T-24")]
    public async Task T_24_CallerCancel_ThrowsOperationCanceledException()
    {
        var handler = new StandInHandler
        {
            OnRequest = async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                return StandInHandler.Json("{}");
            },
        };
        using var client = Clients.Create(handler);
        using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Rest.GetTenantAsync(cancellationToken: source.Token));
        Assert.IsType<OperationCanceledException>(exception);
    }

    [Fact]
    [Trait("Spec", "T-25")]
    public async Task T_25_ServerError_SendsOneRequest()
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Error(500, "$: internal error")),
        };
        using var client = Clients.Create(handler);
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.GetTenantAsync());
        Assert.Equal(AgentConnectErrorKind.Server, exception.Kind);
        Assert.Equal(500, exception.StatusCode);
        Assert.Equal("$: internal error", exception.Message);
        Assert.Single(handler.Requests);
    }

    [Fact]
    [Trait("Spec", "T-26")]
    public async Task T_26_McpWrapsWorkspaceWithRow_RestUsesTheSameEntity()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) =>
            {
                var json = request.Body is not null && request.Body.Contains("\"tools/call\"", StringComparison.Ordinal)
                    ? null
                    : Payloads.WorkspacePage;
                var response = request.Body is not null && request.Body.Contains("server/discover", StringComparison.Ordinal)
                    ? Payloads.Mcp(request.Body)
                    : json is null
                        ? McpStub.Respond(request.Body, _ => Payloads.McpWorkspacePage)
                        : StandInHandler.Json(json);
                return Task.FromResult(response);
            },
        };
        using var client = Clients.Create(handler);
        var mcp = await client.Mcp.ListWorkspacesAsync();
        var rest = await client.Rest.ListWorkspacesAsync();
        Assert.IsType<Workspace>(mcp.Workspaces[0].Entity);
        Assert.IsType<Workspace>(rest.Workspaces[0]);
        Assert.Equal(mcp.Workspaces[0].Entity.Id, rest.Workspaces[0].Id);
        Assert.Equal(1, mcp.Workspaces[0].Row);
        Assert.Null(typeof(Workspace).GetProperty("Row"));
    }

    [Fact]
    [Trait("Spec", "T-27")]
    public async Task T_27_McpEntry_ExposesIdAndRow()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(McpStub.Respond(request.Body, _ => """{"workspaces":[{"id":"w1","name":"Ops","role":"WorkspaceRead","row":2}]}""")),
        };
        using var client = Clients.Create(handler);
        var page = await client.Mcp.ListWorkspacesAsync();
        var entry = Assert.Single(page.Workspaces);
        Assert.Equal("w1", entry.Entity.Id);
        Assert.Equal(2, entry.Row);
    }

    [Fact]
    [Trait("Spec", "T-28")]
    public async Task T_28_UnreachableHost_IsTransport_WithoutServerText()
    {
        using var client = new AgentConnectClient("http://127.0.0.1:1/mcp", "http://127.0.0.1:1", "member-token");
        client.Timeout = TimeSpan.FromSeconds(5);
        var rest = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.GetTenantAsync());
        var mcp = await Assert.ThrowsAsync<AgentConnectException>(() => client.Mcp.ListWorkspacesAsync());
        Assert.Equal(AgentConnectErrorKind.Transport, rest.Kind);
        Assert.Equal(AgentConnectErrorKind.Transport, mcp.Kind);
        Assert.Null(rest.StatusCode);
        Assert.Null(mcp.StatusCode);
        Assert.Equal(LibraryMessages.Transport, rest.Message);
        Assert.Equal(LibraryMessages.Transport, mcp.Message);
        Assert.DoesNotContain("$:", rest.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("$:", mcp.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-29")]
    public async Task T_29_OmittedDescriptionIsAbsent_ClearedDescriptionIsNull()
    {
        var handler = new StandInHandler
        {
            OnRequest = (request, _) => Task.FromResult(request.Body?.Contains("\"method\"", StringComparison.Ordinal) == true
                ? McpStub.Respond(request.Body, _ => Payloads.Workspace)
                : StandInHandler.Json(Payloads.Workspace)),
        };
        using var client = Clients.Create(handler);
        await client.Rest.UpdateWorkspaceAsync("w1", name: "Ops");
        await client.Rest.UpdateWorkspaceAsync("w1", description: null);
        await client.Mcp.UpdateWorkspaceAsync("w1", name: "Ops");
        await client.Mcp.UpdateWorkspaceAsync("w1", description: null);

        using var first = JsonDocument.Parse(handler.Requests[0].Body!);
        Assert.Equal("Ops", first.RootElement.GetProperty("name").GetString());
        Assert.False(first.RootElement.TryGetProperty("description", out _));

        using var second = JsonDocument.Parse(handler.Requests[1].Body!);
        Assert.False(second.RootElement.TryGetProperty("name", out _));
        Assert.Equal(JsonValueKind.Null, second.RootElement.GetProperty("description").ValueKind);

        var toolCalls = handler.Requests.Where(request => request.Body?.Contains("\"tools/call\"", StringComparison.Ordinal) == true).ToArray();
        var third = ToolArguments(toolCalls[0].Body!);
        Assert.Equal("Ops", third.GetProperty("name").GetString());
        Assert.False(third.TryGetProperty("description", out _));

        var fourth = ToolArguments(toolCalls[1].Body!);
        Assert.False(fourth.TryGetProperty("name", out _));
        Assert.Equal(JsonValueKind.Null, fourth.GetProperty("description").ValueKind);
    }

    [Fact]
    [Trait("Spec", "T-30")]
    public async Task T_30_RawJson_KeepsDecimalAndDateTime()
    {
        var raw = JsonValues.Parse(Payloads.RawJson);
        var handler = new StandInHandler
        {
            OnRequest = (request, _) =>
            {
                if (request.Uri.AbsolutePath.EndsWith("/schema", StringComparison.Ordinal))
                {
                    return Task.FromResult(StandInHandler.Json("{\"schema\":" + request.Body + ",\"schemaVersion\":2}"));
                }

                if (request.Uri.AbsolutePath.EndsWith("/batch", StringComparison.Ordinal))
                {
                    using var document = JsonDocument.Parse(request.Body!);
                    var data = document.RootElement.GetProperty("operations")[0].GetProperty("data").GetRawText();
                    return Task.FromResult(StandInHandler.Json("{\"results\":[{\"index\":0,\"status\":200,\"result\":" + data + "}]}"));
                }

                return Task.FromResult(StandInHandler.Json(Payloads.ItemUpserted));
            },
        };
        using var client = Clients.Create(handler);
        var schema = await client.Rest.UpdateSchemaAsync("w1", "l1", raw);
        Assert.Equal(Payloads.RawJson, handler.Requests[0].Body);
        Assert.Equal(Payloads.RawJson, schema.Schema.GetRawText());

        await client.Rest.UpsertItemAsync("w1", "l1", "i1", raw);
        Assert.Equal(Payloads.RawJson, handler.Requests[1].Body);

        var batch = await client.Rest.BatchAsync(
        [
            new BatchOperation
            {
                Op = "upsert",
                Type = "item",
                WorkspaceId = "w1",
                ListId = "l1",
                ItemId = "i1",
                Data = raw,
            },
        ]);
        Assert.Contains(Payloads.RawJson, handler.Requests[2].Body, StringComparison.Ordinal);
        Assert.Equal(Payloads.RawJson, batch.Results[0].Result!.Value.GetRawText());
        Assert.Contains("1.2300", schema.Schema.GetRawText(), StringComparison.Ordinal);
        Assert.Contains("2026-11-25T10:00:00", batch.Results[0].Result!.Value.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "T-31")]
    public async Task T_31_BatchPartialFailure_ReturnsTheModel()
    {
        const string Body = """{"results":[{"index":0,"status":400,"error":"name: required"},{"index":1,"status":424,"error":"$: not applied"},{"index":2,"status":424,"error":"$: not applied"}]}""";
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Json(Body)),
        };
        using var client = Clients.Create(handler);
        var result = await client.Rest.BatchAsync(
        [
            new BatchOperation { Op = "upsert", Type = "workspace", Name = "Ops" },
            new BatchOperation { Op = "upsert", Type = "workspace", Name = "Sales" },
            new BatchOperation { Op = "get", Type = "workspace", WorkspaceId = "w1" },
        ]);
        Assert.Equal(400, result.Results[0].Status);
        Assert.Equal("name: required", result.Results[0].Error);
        Assert.Equal(424, result.Results[1].Status);
        Assert.Equal("$: not applied", result.Results[2].Error);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    [Trait("Spec", "T-32")]
    public async Task T_32_EmailNotSent_ReturnsTheModel()
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Json(Payloads.EmailNotSent)),
        };
        using var client = Clients.Create(handler);
        var result = await client.Rest.SendTokenAsync("ann@example.com");
        Assert.False(result.EmailSent);
    }

    [Fact]
    [Trait("Spec", "T-33")]
    public async Task T_33_RetryAfter_IsExposed_AndNotRetried()
    {
        await AssertRetryAfterAsync(429, "$: too many requests", "3");
        await AssertRetryAfterAsync(503, "$: service unavailable", "7");
    }

    [Fact]
    [Trait("Spec", "T-34")]
    public async Task T_34_RunningCallKeepsTheCredentialsItStartedWith()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var held = 0;
        var handler = new StandInHandler
        {
            OnRequest = async (request, cancellationToken) =>
            {
                var count = Interlocked.Increment(ref held);
                if (count <= 2)
                {
                    if (count == 2)
                    {
                        started.TrySetResult();
                    }

                    await release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                return Payloads.Rest(request);
            },
        };
        using var client = Clients.Create(handler, token: "old-token", admin: "old-admin");
        var member = Task.Run(() => client.Rest.GetTenantAsync());
        var admin = Task.Run(() => client.Rest.CreateTenantAsync("Acme", "ann@example.com"));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        client.MemberToken = "new-token";
        client.AdminKey = "new-admin";
        var nextMember = Task.Run(() => client.Rest.GetTenantAsync());
        var nextAdmin = Task.Run(() => client.Rest.CreateTenantAsync("Acme", "ann@example.com"));
        await nextMember.WaitAsync(TimeSpan.FromSeconds(5));
        await nextAdmin.WaitAsync(TimeSpan.FromSeconds(5));
        release.TrySetResult();
        await member.WaitAsync(TimeSpan.FromSeconds(5));
        await admin.WaitAsync(TimeSpan.FromSeconds(5));

        var oldMember = Assert.Single(handler.Requests, request => request.Authorization == "Bearer old-token");
        Assert.Null(oldMember.AdminKey);
        var oldAdmin = Assert.Single(handler.Requests, request => request.AdminKey == "old-admin");
        Assert.Null(oldAdmin.Authorization);
        var newMember = Assert.Single(handler.Requests, request => request.Authorization == "Bearer new-token");
        Assert.Null(newMember.AdminKey);
        var newAdmin = Assert.Single(handler.Requests, request => request.AdminKey == "new-admin");
        Assert.Null(newAdmin.Authorization);
        Assert.Equal(4, handler.Requests.Count);
    }

    [Fact]
    [Trait("Spec", "T-35")]
    public async Task T_35_QueryItems_SchemaVersionWhereAndOrderByAreInsideQuery()
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Json(Payloads.ItemPage)),
        };
        using var client = Clients.Create(handler);
        await client.Rest.QueryItemsAsync(
            "w1",
            "l1",
            "full",
            SchemaVersionFilter.Eq(2),
            where:
            [
                new FilterCondition
                {
                    Path = "status",
                    Op = "eq",
                    Value = JsonValues.String("open"),
                },
            ],
            orderBy: new OrderBy { Path = "due", Desc = false },
            pageSize: 25,
            continuation: "page-2");
        await client.Rest.QueryItemsAsync(
            "w1",
            "l1",
            "ids",
            SchemaVersionFilter.BelowCurrent);

        var first = Clients.Query(handler.Requests[0].Uri);
        Assert.Equal("full", first["shape"]);
        Assert.Equal("25", first["pageSize"]);
        Assert.Equal("page-2", first["continuation"]);
        Assert.False(first.ContainsKey("schemaVersion"));
        Assert.False(first.ContainsKey("where"));
        Assert.False(first.ContainsKey("orderBy"));
        using (var document = JsonDocument.Parse(first["query"]))
        {
            Assert.Equal(2, document.RootElement.GetProperty("schemaVersion").GetProperty("eq").GetInt32());
            Assert.Equal("status", document.RootElement.GetProperty("where")[0].GetProperty("path").GetString());
            Assert.Equal("due", document.RootElement.GetProperty("orderBy").GetProperty("path").GetString());
            Assert.False(document.RootElement.GetProperty("orderBy").GetProperty("desc").GetBoolean());
        }

        var second = Clients.Query(handler.Requests[1].Uri);
        Assert.Equal("ids", second["shape"]);
        Assert.False(second.ContainsKey("schemaVersion"));
        using var below = JsonDocument.Parse(second["query"]);
        Assert.True(below.RootElement.GetProperty("schemaVersion").GetProperty("belowCurrent").GetBoolean());
        Assert.False(below.RootElement.TryGetProperty("where", out _));
    }

    [Fact]
    public async Task MissingUrl_ThrowsArgumentException_SendsNothing()
    {
        var handler = new StandInHandler();
        using var client = Clients.Create(handler, mcp: null, rest: "   ");
        var mcp = await Assert.ThrowsAsync<ArgumentException>(() => client.Mcp.ListWorkspacesAsync());
        var rest = await Assert.ThrowsAsync<ArgumentException>(() => client.Rest.GetTenantAsync());
        Assert.Empty(handler.Requests);
        Assert.DoesNotContain("member-token", mcp.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("admin-key", rest.ToString(), StringComparison.Ordinal);
    }

    #region private

    private static async Task AssertServerErrorAsync(bool mcp, int status, string text, Func<AgentConnectClient, Task> call)
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Error(status, text)),
        };
        using var client = Clients.Create(handler);
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => call(client));
        Assert.Equal(AgentConnectErrorKind.Server, exception.Kind);
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal(text, exception.Message);
        Assert.Single(handler.Requests);
        Assert.Equal(mcp, handler.Requests[0].Uri.AbsolutePath.Contains("mcp", StringComparison.Ordinal));
    }

    private static async Task AssertRetryAfterAsync(int status, string text, string retryAfter)
    {
        var handler = new StandInHandler
        {
            OnRequest = (_, _) => Task.FromResult(StandInHandler.Error(status, text, retryAfter)),
        };
        using var client = Clients.Create(handler);
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.ListWorkspacesAsync());
        Assert.Equal(AgentConnectErrorKind.Server, exception.Kind);
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal(text, exception.Message);
        Assert.Equal(retryAfter, exception.RetryAfter);
        Assert.Single(handler.Requests);
    }

    private static JsonElement ToolArguments(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("params").GetProperty("arguments").Clone();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "global.json")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("The repository root was not found.");
    }

    #endregion
}

internal sealed class DiagnosticObserver : IObserver<DiagnosticListener>, IDisposable
{
    private readonly StringBuilder _observed;
    private readonly List<IDisposable> _subscriptions = [];

    public DiagnosticObserver(StringBuilder observed)
    {
        _observed = observed;
        _subscriptions.Add(DiagnosticListener.AllListeners.Subscribe(this));
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(DiagnosticListener value)
    {
        _subscriptions.Add(value.Subscribe(new EventObserver(_observed)));
    }

    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
    }

    #region private

    private sealed class EventObserver : IObserver<KeyValuePair<string, object?>>
    {
        private readonly StringBuilder _observed;

        public EventObserver(StringBuilder observed)
        {
            _observed = observed;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            _observed.Append(value.Key);
            _observed.Append(value.Value);
        }
    }

    #endregion
}
