namespace AgentConnectLibrary;

/// <summary>The Tenant created by <c>POST /tenants</c>, including the first Member's token once.</summary>
public sealed record TenantCreated
{
    /// <summary>Gets the Tenant id.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the status, <c>Active</c> or <c>Suspended</c>.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the default page size.</summary>
    public required int PageSizeDefault { get; init; }

    /// <summary>Gets the maximum page size.</summary>
    public required int PageSizeMax { get; init; }

    /// <summary>Gets the first Member's id.</summary>
    public required string FirstMemberId { get; init; }

    /// <summary>Gets the first Member's plaintext token. It is returned once.</summary>
    public required string Token { get; init; }

    /// <summary>Gets whether the invitation email was sent.</summary>
    public required bool EmailSent { get; init; }
}

/// <summary>A Tenant's page sizes.</summary>
public sealed record TenantSettings
{
    /// <summary>Gets the default page size.</summary>
    public required int PageSizeDefault { get; init; }

    /// <summary>Gets the maximum page size.</summary>
    public required int PageSizeMax { get; init; }
}

/// <summary>A Tenant status after suspend or resume.</summary>
public sealed record TenantStatus
{
    /// <summary>Gets the status, <c>Active</c> or <c>Suspended</c>.</summary>
    public required string Status { get; init; }
}

/// <summary>Whether an email was sent. <c>false</c> is a successful result, not an exception.</summary>
public sealed record EmailResult
{
    /// <summary>Gets whether the email was sent.</summary>
    public required bool EmailSent { get; init; }
}

/// <summary>A successful delete.</summary>
public sealed record DeletedResult
{
    /// <summary>Gets whether the resource was deleted. AgentConnect returns true.</summary>
    public required bool Deleted { get; init; }
}

/// <summary>A regenerated Member token. The plaintext token is returned once.</summary>
public sealed record TokenRegenerated
{
    /// <summary>Gets when the new token was issued.</summary>
    public required DateTimeOffset LastTokenIssued { get; init; }

    /// <summary>Gets the new plaintext token.</summary>
    public required string Token { get; init; }
}
