namespace MyWorkID.Server.UnitTests.TestModels;

/// <summary>
/// <see cref="TimeProvider"/> that always returns the same point in time, so validity windows can be tested deterministically.
/// Time never passes, so timers fire immediately instead of delaying the tests.
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        => base.CreateTimer(callback, state, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
}
