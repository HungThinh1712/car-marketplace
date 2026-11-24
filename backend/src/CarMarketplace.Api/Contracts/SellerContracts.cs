namespace CarMarketplace.Api.Contracts;

/// <summary>Payload for creating a seller account.</summary>
public sealed record RegisterSellerRequest
{
    /// <summary>Display name, 1 to 80 characters.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Email address. Comparison ignores letter case.</summary>
    public string? Email { get; init; }

    /// <summary>Password, 8 to 128 characters.</summary>
    public string? Password { get; init; }
}

/// <summary>Payload for signing in. A failure does not say which field was wrong.</summary>
public sealed record SignInRequest
{
    /// <summary>Account email.</summary>
    public string? Email { get; init; }

    /// <summary>Account password.</summary>
    public string? Password { get; init; }
}

/// <summary>The signed-in seller. This email is the caller's own and is not part of a public listing.</summary>
/// <param name="Id">Seller id.</param>
/// <param name="DisplayName">Display name.</param>
/// <param name="Email">Account email.</param>
public sealed record SellerResponse(string Id, string DisplayName, string Email);
