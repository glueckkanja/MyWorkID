namespace MyWorkID.Server.IntegrationTests
{
    /// <summary>
    /// <see cref="TimeProvider"/> that always returns the same point in time, so validity windows can be tested deterministically.
    /// </summary>
    public sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow.ToUniversalTime();
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
