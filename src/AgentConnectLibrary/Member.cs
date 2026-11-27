namespace AgentConnectLibrary;

/// <summary>
/// A Member. <see cref="Token"/> is set only when AgentConnect returns the plaintext token.
/// </summary>
public sealed record Member
{
    /// <summary>Gets the Member id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the email.</summary>
    public required string Email { get; init; }

    /// <summary>Gets whether the Member is a TenantOwner.</summary>
    public required bool IsTenantOwner { get; init; }

    /// <summary>Gets whether an invitation email has been sent.</summary>
    public required bool InvitationSent { get; init; }

    /// <summary>Gets the plaintext token when this response returns it once.</summary>
    public string? Token { get; init; }
}

/// <summary>One REST page of Members. There is no row number. Tokens are not included.</summary>
public sealed record MemberPage
{
    /// <summary>Gets the Members on this page.</summary>
    public required IReadOnlyList<Member> Members { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}

/// <summary>One MCP page of Members. Each entry has a row number.</summary>
public sealed record McpMemberPage
{
    /// <summary>Gets the Members on this page.</summary>
    public required IReadOnlyList<McpRow<Member>> Members { get; init; }

    /// <summary>Gets the continuation token when another page exists.</summary>
    public string? Continuation { get; init; }
}
