using System.Diagnostics;

namespace AgentConnectLibrary.Tests;

public sealed class TimeoutTests
{
    [Fact(Timeout = 60000)]
    [Trait("Spec", "T-8")]
    public async Task T_8_DefaultTimeout_FailsAfterThirtySeconds()
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
        Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);
        var watch = Stopwatch.StartNew();
        var exception = await Assert.ThrowsAsync<AgentConnectException>(() => client.Rest.GetTenantAsync());
        watch.Stop();
        Assert.Equal(AgentConnectErrorKind.Timeout, exception.Kind);
        Assert.Null(exception.StatusCode);
        Assert.Equal(LibraryMessages.Timeout, exception.Message);
        Assert.DoesNotContain("$:", exception.Message, StringComparison.Ordinal);
        Assert.InRange(watch.Elapsed.TotalSeconds, 28, 40);
    }
}
