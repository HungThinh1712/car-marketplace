namespace CarMarketplace.Api.Services;

/// <summary>Clock used for publication time and the moving year limit.</summary>
public interface IClock
{
    /// <summary>Current UTC time.</summary>
    DateTimeOffset UtcNow { get; }
}
