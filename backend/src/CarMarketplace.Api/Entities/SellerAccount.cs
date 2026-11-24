namespace CarMarketplace.Api.Entities;

/// <summary>A person who can publish and maintain listings.</summary>
public sealed class SellerAccount
{
    /// <summary>Primary key.</summary>
    public string Id { get; set; } = "";

    /// <summary>Account email, stored trimmed and lowercased.</summary>
    public string Email { get; set; } = "";

    /// <summary>Name shown on the seller's listings.</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>Password hash. Never copied into an API response.</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>UTC time the account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC time of the latest successful sign-in, including registration.</summary>
    public DateTime LastSignedInAt { get; set; }

    /// <summary>Listings owned by this seller.</summary>
    public List<Listing> Listings { get; set; } = [];
}
