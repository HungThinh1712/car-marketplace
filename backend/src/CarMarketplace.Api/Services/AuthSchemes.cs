namespace CarMarketplace.Api.Services;

/// <summary>Cookie authentication scheme for a seller session.</summary>
public static class AuthSchemes
{
    /// <summary>Scheme and cookie name. The cookie expires 14 days after sign-in and does not slide.</summary>
    public const string Session = "marketplace_session";

    /// <summary>Fixed session lifetime measured from sign-in.</summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(14);
}
