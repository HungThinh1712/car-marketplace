namespace CarMarketplace.Api.Services;

/// <summary>The system UTC clock.</summary>
public sealed class SystemClock : IClock
{
    /// <summary>Current UTC time, truncated to microseconds so it matches MySQL <c>datetime(6)</c>.</summary>
    public DateTimeOffset UtcNow
    {
        get
        {
            var ticks = DateTimeOffset.UtcNow.UtcTicks;
            ticks -= ticks % TimeSpan.TicksPerMicrosecond;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }
}
